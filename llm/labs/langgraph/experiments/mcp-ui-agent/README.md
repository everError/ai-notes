# mcp-ui-agent 실습

**MCP 도구로 데이터를 얻고, 데이터 형태에 따라 다른 UI 페이로드(`{type, props}`)를 스트리밍으로 반환**하는 generative UI 패턴 실습입니다.

```
질문 → 에이전트 → MCP 도구 → 결과 형태 판별(코드)
                                ├ list[dict] → {type: "data-table", props: {columns, rows}}   (A)
                                └ dict(날씨)  → {type: "weather-card", props: {...}}           (B)
스트림: token 이벤트(요약 텍스트) + ui 이벤트(type+props)
```

## 구성

```
mcp-ui-agent/
├── environment.yml        # mamba 환경 (mcp-ui-lab)
├── mcp_server/
│   └── server.py          # 데모 MCP 서버 (도서 목록=표, 날씨=단일 레코드)
└── notebooks/
    ├── 01_mcp_ui_agent.ipynb   # create_agent(프리빌트)로 기본 패턴
    └── 02_custom_graph.ipynb   # 직접 StateGraph 구성 — 판단 루프에 ui_builder 노드 삽입, 인터리빙
```

## 셋업

```bash
mamba env create -f environment.yml
mamba activate mcp-ui-lab
jupyter lab
```

이미 `langgraph-lab` 환경이 있으면 새로 만드는 대신:

```bash
mamba activate langgraph-lab
pip install mcp langchain-mcp-adapters
```

전제: Ollama + `gemma4:e4b`.

**MCP 서버 실행** (별도 터미널에서 먼저 띄울 것):

```bash
mamba activate mcp-ui-lab
python mcp_server/server.py --http
# → http://localhost:8000/mcp 에서 대기
```

> 원래는 stdio(클라이언트가 서버를 서브프로세스로 자동 실행)가 기본이지만,
> **Windows + Jupyter 조합에서는 stdio 서브프로세스 생성이 막힌다**
> (ipykernel 이벤트 루프의 서브프로세스 미지원 + 가짜 stderr 에 fileno 없음).
> 그래서 이 실습은 streamable-http transport 를 사용한다.

## 확인 포인트

- [ ] 도서 질문 → `data-table`, 날씨 질문 → `weather-card` 로 페이로드가 갈리는가
- [ ] 요약 텍스트가 토큰 단위로 실시간 출력되는가 (스트리밍)
- [ ] 형태 판별이 LLM 출력이 아니라 코드(`build_ui_payload`)에서 일어나는 구조를 이해했는가
- [ ] 새 UI 타입(예: 차트)을 추가하려면 어디를 고치면 되는지 말할 수 있는가

## 확장 아이디어

- FastAPI + SSE 로 실제 서버화 (`event: token` / `event: ui` 계약 그대로)
- 프론트(Vue)에서 type → 컴포넌트 registry 로 동적 렌더링
- 도구가 여러 번 호출되는 질문에서 ui 이벤트를 여러 개 방출하도록 확장
