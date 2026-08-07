# MCP Stateless 마이그레이션 — 2026-07-28 스펙 대응

기존 MCP 서버·클라이언트를 2026-07-28 스펙으로 옮기는 절차를 정리합니다. 개념 설명은 [MCP_Architecture](MCP_Architecture.md) 에 있고, 이 문서는 **무엇을 고쳐야 하는지**에 집중합니다.

> 폐기된 기능은 최소 12개월 동안 동작합니다. 서비스가 당장 멈추지는 않으므로 계획된 일정으로 진행하면 됩니다. 다만 세션 기반 구현은 새 클라이언트와 연결되지 않는 시점이 오므로 방치 대상은 아닙니다.

---

## 1. 영향 범위 판정

아래 항목 중 하나라도 해당하면 수정이 필요합니다.

- [ ] `initialize` / `initialized` 핸드셰이크를 처리하거나 기다린다
- [ ] `Mcp-Session-Id` 를 발급·검증하거나, 세션을 메모리·Redis 에 보관한다
- [ ] 로드밸런서에 sticky session 을 설정해 두었다
- [ ] 도구 실행 도중 사용자 확인이 필요할 때 SSE 스트림으로 클라이언트에 되묻는다
- [ ] HTTP + SSE 전송을 쓴다
- [ ] Dynamic Client Registration 으로 인가 서버에 클라이언트를 등록한다
- [ ] Roots, Sampling, Logging 기능에 의존한다
- [ ] `tasks/list` 를 호출한다

**stdio 전용 로컬 서버는 대부분 영향이 적습니다.** stdio 전송은 유지되고, 프로세스 수명이 곧 연결 수명이라 세션 제거의 영향이 작습니다. 그래도 핸드셰이크 제거와 `_meta` 처리는 SDK 업데이트로 따라가야 합니다.

---

## 2. 진행 순서

전체를 한 번에 바꾸지 말고 아래 순서로 나눕니다. 앞 단계가 끝나야 다음 단계가 안전합니다.

```text
1) SDK 업데이트 및 버전 협상 확인
2) 서버 — 핸드셰이크·세션 제거
3) 서버 — 상태 외부화 (MRTR 도입)
4) 클라이언트 — 자기기술 요청 + MRTR 재호출 루프
5) 인가 — DCR → CIMD
6) 인프라 — sticky session 해제, 헤더 기반 라우팅 적용
7) 폐기 기능 대체 경로 마련
```

---

## 3. 서버 측 작업

### 3.1 핸드셰이크와 세션 제거

`initialize` 핸들러와 세션 저장소를 걷어냅니다. 클라이언트 정보는 이제 매 요청의 `params._meta` 에서 읽습니다.

```python
# 이전 — 초기화 시점에 한 번 받아 세션에 보관
session.client_info = init_params.clientInfo

# 이후 — 요청마다 전달됨
client_info = params.get("_meta", {}).get(
    "io.modelcontextprotocol/clientInfo", {}
)
```

세션에 담아 두던 값(사용자 설정, 선택된 워크스페이스 등)은 세 가지 중 하나로 옮깁니다.

| 기존 보관 위치 | 옮길 곳                    | 판단 기준                          |
| -------------- | -------------------------- | ---------------------------------- |
| 세션 메모리    | 인가 토큰의 클레임         | 인증된 사용자에 귀속되는 값        |
| 세션 메모리    | 요청 파라미터              | 호출마다 달라질 수 있는 값         |
| 세션 메모리    | `requestState` (MRTR)      | 한 작업의 중간 단계에서만 쓰는 값  |

### 3.2 프로토콜 버전 확인

`MCP-Protocol-Version` 헤더로 들어옵니다. 구버전 클라이언트를 당분간 함께 받아야 한다면 이 값으로 분기합니다.

### 3.3 되묻기를 MRTR 로 재구현

SSE 스트림으로 확인을 요청하던 코드가 대상입니다.

```python
# 이전 — 열린 스트림으로 서버가 클라이언트에 요청
answer = await session.elicit("Delete 3 files?", schema=bool)
if answer:
    delete(files)

# 이후 — 상태를 반환하고 재호출을 받는 구조
def handle(params):
    state = params.get("requestState")

    if state is None:                        # 1차 호출
        files = resolve_targets(params)
        return {
            "resultType": "input_required",
            "inputRequests": {
                "confirm": {
                    "type": "elicitation",
                    "message": f"Delete {len(files)} files?",
                    "schema": {"type": "boolean"},
                }
            },
            "requestState": encode({"step": 1, "files": files}),
        }

    resumed = decode(state)                  # 2차 호출
    if params["inputResponses"]["confirm"]:
        delete(resumed["files"])
    return {"resultType": "result", "content": [...]}
```

설계 시 유의할 점입니다.

- **`requestState` 는 직렬화 가능해야 합니다.** 열린 파일 핸들, DB 커넥션, 소켓을 담을 수 없습니다. 재호출은 다른 인스턴스에 도착할 수 있습니다.
- **위조를 전제로 다뤄야 합니다.** 클라이언트를 거쳐 돌아오는 값이므로 서명하거나 암호화하고, 만료 시각을 함께 넣습니다. 검증 없이 신뢰하면 권한 상승 경로가 됩니다.
- **크기를 작게 유지합니다.** 큰 중간 결과는 서버 측 저장소에 두고 식별자만 담습니다.

### 3.4 목록 응답에 캐싱 정보 추가

`tools/list`, `prompts/list`, `resources/list` 응답에 `ttlMs` 와 `cacheScope` 를 넣습니다. 사용자·테넌트마다 목록이 달라진다면 `cacheScope` 를 반드시 좁게 잡습니다. 잘못 설정하면 노출되면 안 되는 도구 목록이 다른 사용자에게 공유됩니다.

---

## 4. 클라이언트 측 작업

### 4.1 요청 자기기술

연결 시 한 번 보내던 정보를 매 요청에 싣습니다.

- HTTP 헤더 `MCP-Protocol-Version: 2026-07-28`
- HTTP 헤더 `Mcp-Method`, `Mcp-Name`
- `params._meta` 의 `io.modelcontextprotocol/clientInfo`

### 4.2 MRTR 재호출 루프

`resultType` 을 확인하는 루프가 새로 필요합니다.

```python
state = None
responses = None

while True:
    result = call(method, name, args,
                  requestState=state, inputResponses=responses)

    if result["resultType"] != "input_required":
        return result

    responses = ask_user(result["inputRequests"])   # 사용자 승인 획득
    state = result["requestState"]                  # 그대로 되돌려 보냄
```

무한 루프 방지를 위해 왕복 횟수 상한을 둡니다.

### 4.3 목록 캐시 적용

`ttlMs` 동안 목록을 재사용하고, `cacheScope` 가 사용자 간 공유를 허용하지 않으면 캐시 키에 사용자 식별자를 포함합니다.

---

## 5. 인가 — DCR 에서 CIMD 로

Dynamic Client Registration 은 인가 서버마다 등록 요청을 보내고 불투명한 `client_id` 를 발급받는 방식이었습니다. CIMD 는 반대로 동작합니다.

1. 클라이언트가 고정된 HTTPS URL 에 메타데이터 문서를 게시한다
2. **그 URL 자체가 `client_id` 가 된다**
3. 인가 서버가 URL 을 조회해 메타데이터를 확인한다

등록 절차 없이 여러 인가 서버에 동일한 신원을 쓸 수 있습니다. DCR 은 CIMD 를 지원하지 않는 인가 서버와의 하위 호환용으로만 남습니다.

함께 확인할 항목입니다.

- **RFC 9207 issuer 검증** — 인가 응답의 issuer 를 검증해 인가 서버 혼동 공격을 차단합니다. 자격증명이 특정 인가 서버에 바인딩됩니다.
- **`application_type`** — CLI·데스크톱 클라이언트라면 이 값을 지정해 localhost 리다이렉트 문제를 해소합니다.

---

## 6. 인프라 작업

| 항목             | 변경 내용                                                          |
| ---------------- | ------------------------------------------------------------------ |
| 로드밸런서       | sticky session 해제. 일반 라운드로빈으로 전환                       |
| 스케일아웃       | 인스턴스 수를 자유롭게 조정 가능. 재시작이 진행 중 작업을 끊지 않음 |
| 게이트웨이       | `Mcp-Method`/`Mcp-Name` 헤더로 라우팅·인가·레이트리밋 구성          |
| 캐시 계층        | 목록 응답의 `ttlMs`/`cacheScope` 를 존중하도록 설정                 |
| 관측             | `_meta` 의 `traceparent`/`tracestate`/`baggage` 로 OpenTelemetry 연동 |

트레이스 컨텍스트 전파가 스펙에 공식 문서화되었으므로, 기존 관측 스택과 연결하기 쉬워졌습니다.

---

## 7. 폐기 기능 대체

| 폐기 기능      | 대체 방향                                                     |
| -------------- | ------------------------------------------------------------- |
| Roots          | 접근 범위를 인가 토큰 클레임이나 요청 파라미터로 전달          |
| Sampling       | 호스트 측 LLM 호출로 이동. 서버가 모델을 역호출하지 않는 구조로 |
| Logging        | 서버 자체 로깅 + OpenTelemetry 트레이스 전파                   |
| `tasks/list`   | 작업 식별자를 애플리케이션이 보관하고 `tasks/get` 으로 조회     |

Tasks 는 코어에서 `io.modelcontextprotocol/tasks` 확장으로 이동했습니다. 폴링 방식 `tasks/get` 과 `tasks/update` 를 사용하며, 확장을 사용하려면 명시적으로 선언해야 합니다.

---

## 8. 검증 체크리스트

- [ ] 서버 인스턴스를 2개 이상 띄우고 sticky session 없이 연속 요청이 정상 처리되는가
- [ ] MRTR 재호출이 **1차 호출과 다른 인스턴스**에 도착해도 이어서 처리되는가
- [ ] 인스턴스를 재시작한 뒤에도 진행 중이던 MRTR 작업이 재개되는가
- [ ] 변조한 `requestState` 를 보냈을 때 거부되는가
- [ ] 만료된 `requestState` 가 거부되는가
- [ ] 사용자별로 다른 도구 목록이 캐시를 통해 섞이지 않는가
- [ ] `Mcp-Method`/`Mcp-Name` 헤더만으로 게이트웨이 라우팅이 동작하는가
- [ ] 구버전 클라이언트가 연결될 때의 동작이 의도한 대로인가

---

## 9. 실습 아이디어

`llm-notes/llm/labs/mcp/` 아래 기존 실습 서버로 확인해 볼 수 있는 항목입니다.

1. 기존 서버를 새 SDK 로 올리고 세션 코드를 제거한 뒤, 인스턴스 2개를 nginx 라운드로빈 뒤에 배치해 정상 동작 확인
2. 파일 삭제처럼 확인이 필요한 도구를 MRTR 로 구현하고, 1차·2차 호출이 서로 다른 인스턴스로 가도록 강제해 검증
3. 게이트웨이에서 특정 도구(`Mcp-Name`)에만 레이트리밋을 걸어 헤더 기반 제어 확인 — 이 항목은 `llmops-notes/gateway/` 와 이어집니다

---

## 참고

| 자료                 | URL                                                                      |
| -------------------- | ------------------------------------------------------------------------ |
| 2026-07-28 스펙 공지 | https://blog.modelcontextprotocol.io/posts/2026-07-28/                    |
| 릴리스 후보 상세     | https://blog.modelcontextprotocol.io/posts/2026-07-28-release-candidate/  |
| 스펙 저장소 릴리스   | https://github.com/modelcontextprotocol/modelcontextprotocol/releases     |
