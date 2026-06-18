# LangChain Python v1.0 (Alpha)

> ⚠️ 현재 v1.0은 **알파(Alpha)** 버전입니다.  
> 전체 문서: [LangChain v1.0 Release Notes](https://docs.langchain.com/oss/python/releases/langchain-v1)

---

## What’s New

### 1. 메시지 구조 개선

- 모든 메시지에 `.content_blocks` 속성 추가  
  → reasoning, citations, tool call 등 **구조화된 응답**을 다루기 쉬움
- 여전히 `.content`는 유지되며, **호환성 보장**

### 2. 사전 구축 체인 / 에이전트

- `langgraph.prebuilts` → `langchain.agents`로 통합
- `create_react_agent`, `ToolNode`, `AgentState` 등 import 경로 변경
- 에이전트/체인의 **표준화된 빌딩 블록 제공**

### 3. 구조화된 출력 (Structured Output)

- 모델이 **도구 호출 + 구조화된 출력**을 함께 생성 가능
- 중복 LLM 호출 제거 → 비용 절감
- 출력 전략:
  - `Artificial tool calling` (기본)
  - `Provider implementations` (네이티브 지원 모델)

### 4. 오류 처리 강화

- `handle_errors`, `handle_tool_errors` 인자 추가
- 파싱 오류, 다중 도구 호출, 실행 실패 시 동작을 명시적으로 제어 가능
- 기본 동작: 실패 시 `ToolException` 발생 (무한 루프 방지)

---

## Breaking Changes

- **Python 3.9 지원 종료** → 최소 버전: **Python 3.10**
- 반환 타입 변경: Chat 모델 응답은 항상 `AIMessage`
- `langchain-openai`: `responses/v1` 형식이 **기본값**
  - 이전 형식이 필요하면 `output_version="v0"` 설정
- `langchain-anthropic`: `max_tokens` 기본값이 모델별로 상향 조정됨
- Deprecated API 전부 제거됨
- 일부 기능은 `langchain-legacy` 패키지로 이동

---

## 참고 링크

- [Release Notes](https://docs.langchain.com/oss/python/releases/langchain-v1)
- [LangChain GitHub](https://github.com/langchain-ai/langchain)
