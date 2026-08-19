# DeepSeek Harness (dsh)

DeepSeek이 2026-08-13에 공개한 오픈소스 에이전트 하네스. 저장소는
`deepseek-ai/deepseek-harness`, 라이선스는 MIT, 런타임은 Node.js.

- `notes/` — 개념과 구조 정리
- `experiments/` — 실행 절차와 설정 예시

## 정리

| 문서 | 내용 |
| --- | --- |
| `notes/01-overview.md` | 기본 정보, 플러그인 구조, 프로바이더, 자격 증명 처리 |
| `notes/02-dsh-home-layout.md` | 실행 후 확인한 `~/.dsh` 구조와 플러그인 설치 방식 |
| `notes/03-plugin-workflow.md` | 플러그인을 다루는 세 가지 층위. 패치 문법, 에이전트 프리셋 |
| `notes/04-cordis.md` | 기반 프레임워크 Cordis. 출처와 설계 축 |
| `notes/05-request-lifecycle.md` | 요청 하나가 처리되는 흐름. 등록형과 동작형 구분 |
| `notes/06-cordis-internals.md` | Cordis 소스를 읽고 정리한 구현 방식 |
| `notes/07-tech-stack.md` | 계층별로 쓰인 기술과 그 선택이 뜻하는 것 |

## 실험

| 문서 | 내용 |
| --- | --- |
| `experiments/01-setup-ollama.md` | 로컬 Ollama 연결 및 에이전트 루프 확인 |
| `experiments/02-troubleshooting.md` | 실행 중 겪은 오류 세 건과 원인 |

## 참고

- https://github.com/deepseek-ai/deepseek-harness
- https://deepseek-harness.github.io/deepseek-harness/en/guide/providers
