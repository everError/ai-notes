# generative-ui

에이전트가 사용자 화면을 만들어 내려보내는 방식을 정리하는 랩.

- `notes/` — 개념 분류와 기법 정리
- `experiments/` — 실습

## 문서

| 문서 | 내용 |
| --- | --- |
| `notes/00-taxonomy.md` | 용어 정리. 서버 주도 UI, 생성형 UI, 전송 규격의 구분 |
| `notes/01-techniques.md` | 기법 여섯 가지의 구조와 대가 |
| `notes/02-transport-and-state.md` | 전송 계층, 상태 동기화, 상호작용, 다시 열기 |
| `notes/03-security.md` | 격리 수단과 위험 |

## 실습

| 폴더 | 내용 |
| --- | --- |
| `experiments/mcp-ui-agent` | MCP 도구 결과의 형태를 판별해 선언적 화면 기술을 스트리밍으로 반환. `notes/01-techniques.md` 의 5번과 4번을 결합한 구성 |

## 관련

- `../mcp` — 도구 결과에 화면을 실어 보내는 경로
- `../langgraph` — 실습이 쓰는 그래프 구성
- `../../notes/agent-streaming.md` — 여러 단계 루프를 끊기지 않게 내보내는 문제
