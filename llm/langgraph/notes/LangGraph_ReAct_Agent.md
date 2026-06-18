# Langgraph React Agent 가이드

`ReAct`는 Reasoning(추론)과 Acting(행동)을 결합하여 **LLM이 환경과 상호작용하며 복잡한 작업을 수행**할 수 있도록 하는 접근 방식입니다. LangGraph는 이 ReAct 방식의 에이전트를 구조적으로 구현할 수 있는 기능을 제공합니다.

---

## ✅ ReAct란?

| 항목 | 설명                                                    |
| ---- | ------------------------------------------------------- |
| 정의 | 추론(Reasoning) + 행동(Acting)을 결합한 방식            |
| 목적 | 단순 응답 생성이 아닌, 도구를 활용하여 복잡한 문제 해결 |

### 📌 동작 사이클 구조

ReAct는 다음과 같은 반복적인 사이클을 구성합니다:

1. **행동 (Act)**: 도구 선택 및 실행
2. **관찰 (Observe)**: 도구 실행 결과를 관찰
3. **추론 (Reason)**: 현재 상황 분석 후 다음 행동 결정

> Thought → Action → Observation 구조를 기반으로 문제를 단계적으로 해결함

```text
Thought: 사용자 질문을 이해하고 도구 필요 판단
Action: 선택한 도구 호출
Observation: 도구의 실행 결과를 받아 확인
```

---

## ⚙️ LangGraph에서 ReAct 구현 방식

LangGraph는 ReAct 에이전트를 만들기 위해 **두 가지 방식**을 제공합니다:

- `create_react_agent()` 함수 사용 (간편 생성)
- `StateGraph` 구조를 이용한 수동 구성 (고급 제어)

---

## 🧪 방식 1: `create_react_agent()`로 빠르게 생성

```python
from langgraph.prebuilt import create_react_agent

graph = create_react_agent(
    llm=llm,                     # LLM 모델
    tools=tools,                # 도구 목록
    state_modifier=system_prompt
)

inputs = {"messages": [HumanMessage(content="스테이크 메뉴의 가격은 얼마인가요?")]}
messages = graph.invoke(inputs)
```

### 주요 구성 단계

1. LLM 설정
2. 사용할 도구 정의
3. 프롬프트 설정 (`state_modifier`)
4. `create_react_agent` 호출로 그래프 생성
5. 메시지를 전달하여 실행

### ✅ 고급 옵션 활용

- `prompt`: 사용자 정의 프롬프트 지정
- `tools_renderer`: 도구 설명 렌더링 방식 커스터마이징
- `output_parser`: 출력 파서 정의
- `stop_sequence`: 모델 응답 중단 시점 제어

> 이 방식은 복잡한 제어가 필요 없는 일반적인 ReAct 사용에 매우 효율적입니다.

---

## 🧩 방식 2: StateGraph로 ReAct Agent 수동 구성

LangGraph의 핵심인 `StateGraph`를 이용하면 에이전트의 작동 흐름을 세밀하게 제어할 수 있습니다.

### ✅ 조건부 엣지 함수 정의 예시

```python
def should_continue(state: GraphState):
    last = state["messages"][-1]
    if hasattr(last, 'tool_calls'):
        return "execute_tools"
    return END
```

### ✅ ToolNode 정의

```python
from langgraph.prebuilt import ToolNode

tools = [search_web, search_menu]
tool_node = ToolNode(tools)
```

### ✅ 그래프 구성 예시

```python
builder = StateGraph(GraphState)
builder.add_node("call_model", call_model)
builder.add_node("execute_tools", tool_node)

builder.add_edge(START, "call_model")
builder.add_conditional_edges("call_model", should_continue)
builder.add_edge("execute_tools", "call_model")

graph = builder.compile()
```

> 조건부 분기, 반복, 상태 병합 등을 유연하게 정의할 수 있습니다.

---

## 🛠️ `tools_condition()` 함수 사용

LangGraph는 조건부 분기를 위한 유틸 함수 `tools_condition()`을 제공합니다.

```python
from langgraph.prebuilt import tools_condition

builder.add_conditional_edges(
    "agent",
    tools_condition,  # 도구 호출 유무 확인
    path_map={
        "tools": "tools",
        "end": END
    }
)
```

- 최신 메시지에 도구 호출 요청이 포함되었는지 자동 판단하여 흐름을 제어합니다.

---

## 🎨 Mermaid 다이어그램 시각화

LangGraph의 그래프는 시각적으로 확인할 수 있습니다.

```python
from IPython.display import Image, display

display(Image(graph.get_graph().draw_mermaid_png()))
```

- 실행 흐름(모델 → 도구 → 모델 → 종료)을 시각적으로 확인 가능
- Mermaid.js 기반으로 렌더링됨

---

## 🧠 상태 관리 및 세션 지속

LangGraph는 대화 흐름을 유지하기 위한 다음 기능들을 제공합니다:

- **Checkpointer**: 상태를 저장하고 복원하여 멀티턴 대화를 지원
- **Thread ID**: 세션 단위 식별자 부여로 사용자별 대화 유지
- **StateReducer**: 메시지 병합 및 변경 히스토리 추적 가능

---

## 🙋 인간 개입 (Human-in-the-Loop) 처리

- `AskHuman` 과 같은 도구를 정의하면 특정 상황에서 모델의 판단이 아닌 사용자 확인을 유도할 수 있습니다.
- 실시간 피드백이 필요한 환경에 유용

---

## 🔄 LangChain initialize_agent와의 차이점

| 항목       | `initialize_agent` (LangChain) | `create_react_agent` (LangGraph)            |
| ---------- | ------------------------------ | ------------------------------------------- |
| 프레임워크 | LangChain Core                 | LangGraph (상태 기반 FSM)                   |
| 흐름 제어  | 프롬프트 기반                  | 명시적 상태 그래프 구성                     |
| 실행 구조  | 추론 루프 자동 반복            | 노드 간 상태 전이로 흐름 제어               |
| 시각화     | 지원 안됨                      | Mermaid로 그래프 시각화 가능                |
| 확장성     | 단순 챗봇에 적합               | 조건 분기, 도구 병렬 실행 등 고급 구성 가능 |
| 상태 관리  | 없음                           | 상태 리듀서, 체크포인트, 메시지 추적 가능   |

---

## 📌 정리

| 구성 요소            | 설명                                                    |
| -------------------- | ------------------------------------------------------- |
| `create_react_agent` | LLM + Tools 조합으로 빠르게 ReAct agent 생성            |
| `StateGraph`         | 고급 제어를 위한 수동 그래프 구성 방식                  |
| `ToolNode`           | 도구 실행용 노드, AIMessage의 tool_calls 필드 기반 실행 |
| `tools_condition`    | 도구 호출 여부 판단에 유용한 조건부 함수                |
| `checkpointer`       | 장기적 대화 상태 유지를 위한 상태 저장 기능             |
| `AskHuman`           | 사용자 개입을 트리거하는 도구 구성 가능                 |
| `draw_mermaid_png()` | Mermaid 기반 그래프 시각화 기능 제공                    |
| `StateReducer`       | 커스텀 병합 로직으로 메시지 및 상태 변경 관리           |

---

ReAct 방식은 단순 응답 생성이 아니라, 도구를 활용한 복합적 문제 해결을 가능하게 하며, LangGraph는 이를 구조적으로 지원하는 강력한 프레임워크입니다.
