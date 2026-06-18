# LangGraph ToolNode 가이드

`ToolNode`는 LangGraph에서 **ReAct 에이전트**나 **도구 기반 에이전트**를 만들 때 핵심 역할을 수행하는 노드입니다. 모델이 생성한 `tool_calls` 요청을 실제로 실행하고, 그 결과를 LangChain 메시지 형식(`ToolMessage`)으로 반환해 추론을 이어가게 해줍니다.

---

## ✅ ToolNode란?

| 항목 | 설명                                                                              |
| ---- | --------------------------------------------------------------------------------- |
| 정의 | LLM이 요청한 도구 호출을 실행하고 그 결과를 모델에게 다시 전달하는 LangGraph 노드 |
| 기반 | `AIMessage.tool_calls` 필드 분석 → 도구 실행 → `ToolMessage` 생성                 |
| 목적 | 모델의 의도에 따라 외부 도구를 호출하고 실행 결과를 문맥에 반영                   |

---

## 🔁 동작 구조

```text
[AIMessage(tool_calls)] → ToolNode → [ToolMessage] → 모델
```

### 📌 실행 흐름

1. **도구 호출 요청 추출**: 모델이 생성한 `AIMessage.tool_calls` 확인
2. **도구 실행**: 호출된 이름에 매칭되는 실제 Python 함수 실행
3. **결과 구조화**: 실행 결과를 `ToolMessage`로 래핑
4. **반환**: 모델에게 Observation으로 결과 제공

---

## 🛠️ 예제 코드

### ✅ 도구 정의

```python
from langchain_core.tools import tool

@tool
def search_menu(query: str) -> str:
    # 예시 도구 - 식당 메뉴 검색
    return f"{query}에 대한 스테이크는 35,000원입니다."

@tool
def get_weather(city: str) -> str:
    return f"{city}의 현재 기온은 22도입니다."
```

### ✅ ToolNode 생성 및 사용

```python
from langgraph.prebuilt import ToolNode

# 도구 목록 구성
tools = [search_menu, get_weather]

# ToolNode 생성
tool_node = ToolNode(tools)

# 그래프에 등록
builder.add_node("tools", tool_node)
```

---

## ⚙️ 내부 작동 방식

| 단계                           | 설명                                              |
| ------------------------------ | ------------------------------------------------- |
| 1. `AIMessage.tool_calls` 분석 | JSON 형식의 호출 요청 추출 (도구명 + 파라미터)    |
| 2. 실행 대상 매핑              | 도구 목록에서 이름 일치 함수 찾기                 |
| 3. 동시 실행                   | 여러 도구 요청 시 병렬 실행 (async 지원)          |
| 4. 응답 구조화                 | `ToolMessage(tool_call_id=..., content=...)` 생성 |
| 5. 반환                        | 생성된 ToolMessage 리스트 반환                    |

---

## 🧠 예시 흐름 시나리오

```text
1. 사용자: "서울 날씨 알려줘"
2. 모델: Thought + Action → tool_calls = [get_weather(city="Seoul")]
3. ToolNode: get_weather 실행 → '22도입니다' 응답
4. ToolMessage: 모델에게 Observation 전달
5. 모델: Observation을 기반으로 최종 답변 생성
```

---

## 📚 관련 메시지 타입

| 메시지         | 설명                             |
| -------------- | -------------------------------- |
| `AIMessage`    | 모델 응답 + tool_calls 포함 가능 |
| `ToolMessage`  | 도구 실행 결과 포함된 메시지     |
| `HumanMessage` | 사용자 입력 메시지               |

---

## 🔐 주의사항 및 팁

| 항목              | 설명                                              |
| ----------------- | ------------------------------------------------- |
| 도구 이름 충돌    | `@tool` 이름이 중복되지 않도록 주의               |
| 도구 등록 누락    | `ToolNode([...])`에 모든 사용 도구 등록 필요      |
| 파라미터 미스매치 | 호출 파라미터와 함수 시그니처 불일치 시 에러 발생 |
| async 지원        | 비동기 도구도 지원 (async def 함수)               |

---

## 🧩 ToolNode 활용 시나리오

| 예시                 | 설명                                            |
| -------------------- | ----------------------------------------------- |
| 데이터 조회 에이전트 | 검색, DB 조회, API 호출 도구를 통해 데이터 수집 |
| 계산기 에이전트      | 수식 계산, 날짜 계산 등 복합 연산 도구 호출     |
| 외부 API 조작        | 스마트홈, IoT 기기, Slack 등 API 연동 도구 실행 |
| 사용자 개입 트리거   | `AskHuman` 도구 정의로 사람 개입 유도 가능      |

---

## 📌 요약

| 항목 | 설명                                               |
| ---- | -------------------------------------------------- |
| 목적 | 모델이 요청한 도구를 실행하고 결과를 반환하는 노드 |
| 입력 | `AIMessage` (tool_calls 포함)                      |
| 출력 | `ToolMessage`                                      |
| 장점 | 병렬 실행, 명확한 구조, LangChain 도구와 통합 쉬움 |

---

`ToolNode`는 ReAct 기반 LangGraph 흐름에서 모델이 실질적으로 외부 세계와 상호작용하는 중간 다리 역할을 합니다. 도구 정의와 함께 사용하면 매우 강력한 실행 기반 LLM 시스템을 구축할 수 있습니다.
