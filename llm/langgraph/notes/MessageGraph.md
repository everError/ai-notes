# LangGraph MessageGraph 가이드

`MessageGraph`는 LangGraph에서 제공하는 메시지 기반 특수 그래프 유형으로, 상태를 **메시지 리스트**로 유지하며 챗봇, 에이전트, 도구 호출 등 LLM 기반 대화 흐름을 간결하게 구성할 수 있도록 설계되었습니다.

---

## ✅ MessageGraph란?

> `MessageGraph`는 LangChain의 `ChatModel`에 특화된 상태 기반 그래프로, 상태를 메시지 객체(HumanMessage, AIMessage 등)의 목록으로 유지합니다.

### 📌 핵심 개념

- **정의**: LangChain의 ChatModel을 위한 특수한 형태의 `StateGraph`
- **입출력 메시지 처리**: `BaseMessage` 하위 객체 목록을 입력 및 출력으로 처리
- **목적**: 자연스러운 대화 흐름과 문맥 유지가 필요한 상황에서 대화 기록을 효과적으로 관리
- **특징**:
  - 상태를 메시지 리스트로 관리 (`List[BaseMessage]`)
  - 메시지를 자동 병합하며 중복 제거
  - `Annotated` + `add_messages`로 메시지 상태 확장 가능

---

## 💬 메시지 타입

LangGraph에서 사용하는 메시지 타입은 `langchain_core.messages` 모듈 기반입니다:

| 메시지 타입       | 설명                |
| ----------------- | ------------------- |
| `HumanMessage`    | 사용자 입력         |
| `AIMessage`       | 모델 출력           |
| `SystemMessage`   | 시스템 명령 메시지  |
| `ToolMessage`     | 외부 도구 실행 결과 |
| `FunctionMessage` | 함수 실행 결과      |

---

## 📦 Messages State 정의

### ✅ 방법 1: 직접 정의

```python
from typing import Annotated, TypedDict
from langchain_core.messages import AnyMessage
from langgraph.graph.message import add_messages

class GraphState(TypedDict):
    messages: Annotated[list[AnyMessage], add_messages]
```

- `add_messages` 함수는 메시지 ID를 추적하여 병합, 중복 제거 기능을 포함합니다.
- 단순 리스트 병합에는 `operator.add`도 사용 가능하지만, ID 추적 기능은 없습니다.

### ✅ 방법 2: `MessagesState` 상속

```python
from langgraph.graph import MessagesState
from langchain_core.documents import Document
from typing import List

class GraphState(MessagesState):
    documents: List[Document]
    grade: float
    num_generation: int
```

- `messages` 필드는 `MessagesState`에 내장되어 있으므로 별도 정의 없이 사용 가능
- 추가 필드를 통해 응답 점수, 생성 횟수, 검색 문서 등 확장 가능

> 상태 업데이트 시 `messages` 외에 다른 필드를 사용하려면, 반드시 상태 정의에 해당 키를 포함해야 함

---

## 🔧 기본 사용 예시

```python
from langgraph.graph.message import MessageGraph
from langchain_core.messages import HumanMessage, AIMessage

builder = MessageGraph()

def chatbot_node(state):
    return [AIMessage(content="Hello!")]

builder.add_node("chatbot", chatbot_node)
builder.set_entry_point("chatbot")
builder.set_finish_point("chatbot")

graph = builder.compile()
result = graph.invoke([HumanMessage(content="Hi")])
print(result)
```

---

## 🔁 조건 분기 예시

```python
def should_continue(messages):
    if len(messages) > 6:
        return END
    return "reflect"

builder.add_conditional_edges("generate", should_continue)
```

메시지 수 또는 특정 메시지 내용을 기반으로 흐름을 제어할 수 있습니다.

---

## ⚠️ 주의사항

| 항목        | 설명                                                                |
| ----------- | ------------------------------------------------------------------- |
| 메모리 관리 | 대화 기록이 길어질 경우 오래된 메시지를 제거하는 로직 필요          |
| 프라이버시  | 민감한 정보가 포함된 메시지는 마스킹/제거 등의 처리가 필요          |
| 성능 최적화 | 메시지 처리 로직이 복잡해지면 실행 속도나 메모리 사용량에 영향 있음 |

---

## 🧩 활용 예시

| 시나리오           | 설명                                         |
| ------------------ | -------------------------------------------- |
| 챗봇               | 사용자와 단순한 대화 주고받기                |
| 반영 에이전트      | 모델 출력을 검토하고 수정하는 반영 과정 구성 |
| 도구 호출 에이전트 | 사용자 질문을 툴 실행으로 연결하고 응답 반환 |
| 대화 히스토리 관리 | 메시지 리스트 기반으로 대화 내용 전체 유지   |

---

`MessageGraph`는 LLM 기반의 대화 흐름, 에이전트 제어, 툴 호출 등에서 매우 직관적이고 유용한 구조를 제공합니다. 대화 메시지를 명시적으로 상태로 다루고 싶을 때 최적화된 그래프 구조입니다.
