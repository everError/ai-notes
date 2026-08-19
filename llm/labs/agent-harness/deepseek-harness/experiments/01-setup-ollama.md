# 실험 01 — dsh 를 로컬 Ollama 에 연결하기

목표: DeepSeek API 키 없이 로컬 모델만으로 dsh 를 기동하고, 에이전트 루프가
실제로 도구를 호출하며 다단계 작업을 끝까지 수행하는지 확인한다.

## 사전 확인

환경 (2026-08-19 기준 확인값)

- Node.js v22.20.0 / npm 11.6.2
- Ollama 구동 중, 보유 모델 `gemma4:e4b`

Ollama 의 OpenAI 호환 엔드포인트 응답 확인.

```bash
curl -s http://127.0.0.1:11434/v1/models
```

`gemma4:e4b` 가 목록에 나오면 정상이다.

도구 호출 지원 여부 확인. 하네스는 도구 호출이 되지 않으면 아무 작업도
수행하지 못하므로 이 단계를 먼저 통과해야 한다.

```bash
curl -s http://127.0.0.1:11434/v1/chat/completions -H "Content-Type: application/json" -d "{\"model\":\"gemma4:e4b\",\"messages\":[{\"role\":\"user\",\"content\":\"What is the weather in Seoul? Use the tool.\"}],\"tools\":[{\"type\":\"function\",\"function\":{\"name\":\"get_weather\",\"description\":\"Get weather for a city\",\"parameters\":{\"type\":\"object\",\"properties\":{\"city\":{\"type\":\"string\"}},\"required\":[\"city\"]}}}],\"stream\":false}"
```

응답에 `"finish_reason":"tool_calls"` 와 `tool_calls` 배열이 포함되어야 한다.
확인 결과 정상 반환되었다.

## 1단계 — dsh 기동

dsh 의 실행 디렉터리가 곧 에이전트의 작업 공간이 된다. 노트와 실험 문서가 수정
대상이 되지 않도록 `workspace/` 에서 실행한다. 아래 경로는 저장소 루트 기준이다.

```bash
cd llm/labs/agent-harness/deepseek-harness/experiments/workspace
```

전역 설치 없이 실행한다. 첫 실행은 패키지 내려받기로 시간이 걸린다.

```bash
npx @deepseek-ai/dsh web
```

기본 주소는 http://127.0.0.1:3080 이다.

소스에서 직접 빌드하려면 저장소를 내려받아 `pnpm install`, `pnpm run build`,
`pnpm dsh web` 순으로 실행한다. 이 경우 작업 디렉터리 지정 방식이 달라지므로
저장소 문서를 확인한다.

```bash
git clone https://github.com/deepseek-ai/deepseek-harness.git
```

### 기동 확인

실행 디렉터리에는 파일이 생기지 않는다. 정상이다. 설정과 세션은 모두
`~/.dsh` 에 만들어지므로 그쪽을 확인한다. 구조는 `../notes/02-dsh-home-layout.md` 에 정리했다.

프로바이더를 등록하기 전 `~/.dsh/settings.yaml` 에는 온보딩 항목만 들어 있다.
등록이 끝났는지 판단할 때 이 파일을 보면 된다.

## 2단계 — Ollama 를 커스텀 프로바이더로 등록

웹 화면에서 Settings -> Models -> Add custom provider 를 선택하고 다음을 입력한다.

| 항목 | 값 |
| --- | --- |
| Provider ID | `ollama` |
| Display name | Ollama (local) |
| Base URL | `http://127.0.0.1:11434/v1` |
| API protocol | `openai-completions` |
| API credential | 임의 값 (예: `ollama`) |
| Models | `gemma4:e4b` |

Ollama 는 인증을 요구하지 않지만 프로바이더 스키마가 자격 증명 필드를 요구하므로
임의의 값을 넣는다. 저장하면 다음 요청부터 서버 재시작 없이 적용된다.

화면 대신 설정 파일로 처리하려면 `settings.sample.yaml` 의 내용을
`~/.dsh/settings.yaml` 에 병합하고 `~/.dsh/.env` 에 `OLLAMA_API_KEY=ollama` 를 추가한다.
자격 증명 저장 경로는 실행 디렉터리와 무관하게 `~/.dsh` 로 고정된다.

기본 화면의 DeepSeek 카드에 값을 넣으면 `.credentials.yaml` 에 `DEEPSEEK_API_KEY` 로
기록된다. 로컬 모델만 쓸 때는 이 항목이 필요 없으므로, 커스텀 프로바이더 쪽에
등록했는지 `settings.yaml` 로 확인한다.

## 3단계 — 모드와 권한 확인

화면 입력창 위에 두 가지 선택기가 있다.

| 선택기 | 뜻 |
| --- | --- |
| 모드 | 에이전트 프리셋. Standard, PTC, Minimal, Creator |
| 권한 | 권한 프리셋. Read Only 상태에서는 파일 수정과 명령 실행이 막힌다 |

모드 이름과 프리셋 식별자는 서로 다르다.

| 화면 표기 | 프리셋 식별자 | 내용 |
| --- | --- | --- |
| Standard mode | `standard` | 파일 편집, 셸, 검색, 스킬, 계획, 서브에이전트를 갖춘 기본 구성 |
| PTC mode | `code` | Standard 의 기능을 Code Mode SDK 로 노출해 여러 단계를 하나의 프로그램으로 묶는다 |
| Minimal mode | `minimal` | 지속 bash 와 str_replace_editor 두 가지만 |
| Creator mode | `cordis` | Standard 에 런타임 조회와 플러그인 실험, 프리셋 작성 안내를 더한 것 |

작업을 시키려면 권한 선택기를 Read Only 에서 바꿔야 한다.

## 4단계 — 확인 과제

작은 것부터 단계를 올리며 어디에서 끊기는지 기록한다. 입력 파일은 `sample/` 에 있다.

| 번호 | 지시 내용 | 확인 대상 |
| --- | --- | --- |
| 1 | 자기소개를 한 문단으로 작성하도록 지시 | 프로바이더 연결 자체 |
| 2 | `sample/inventory.csv` 를 읽고 항목 수를 답하도록 지시 | 도구 호출과 결과 반환 경로 |
| 3 | `sample/inventory.csv` 에서 category 별 재고 합계를 구해 `summary.md` 로 저장하도록 지시 | 읽기와 쓰기가 이어지는 다단계 호출 |
| 4 | `sample/` 전체를 훑어 재주문이 필요한 품목을 근거와 함께 정리하도록 지시 | 탐색과 종합의 결합, 종료 조건 판단 |

## 관찰 기록

| 과제 | 결과 | 비고 |
| --- | --- | --- |
| 1 | | |
| 2 | | |
| 3 | | |
| 4 | | |

## 예상되는 한계

`gemma4:e4b` 는 단일 도구 호출은 처리하지만, 여러 단계를 거치는 작업에서는
호출 인자 형식 오류나 종료 조건 판단 실패가 나타날 수 있다. 3단계 이후에서
루프가 반복되거나 중단되면 모델 능력의 문제이지 하네스 설정의 문제가 아닐 가능성이
높다. 이 경우 DeepSeek 또는 다른 상용 프로바이더 키로 같은 과제를 다시 수행해
하네스 자체의 동작과 모델 한계를 분리해 판단한다.
