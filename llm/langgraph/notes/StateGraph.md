# LangGraph 학습 정리: StateGraph 기반 설계 흐름

LangGraph는 LangChain과 달리 **데이터 상태(State)** 를 중심으로 워크플로우를 구성하는 프레임워크입니다. 이 문서에서는 LangGraph 개발 시 핵심 개념인 `StateGraph`를 중심으로 전체적인 개발 흐름을 정리합니다.

---

## ✅ 개발 순서 요약

1. **State 정의**  
   전체 그래프의 데이터를 정의하는 구조체 (`TypedDict`, `dataclass`, `Pydantic` 등 사용)

2. **Node 정의**  
   하나 이상의 `state` 속성을 입력으로 받아 새로운 속성을 "채우는" 단위 함수 또는 Runnable

3. **Edge(엣지) 정의**  
   노드 간 흐름을 정의. 순차, 조건 분기 등 포함 (`add_edge`, `add_conditional_edges`)

4. **Compile**  
   정의된 그래프를 실행 가능한 객체로 컴파일 (`compile()`)

5. **Invoke**  
   초기 state를 입력으로 그래프 실행 (`invoke()`, `astream()`)

---

## 🧠 왜 State 중심인가?

LangGraph는 "데이터 플로우 기반" 설계를 지향합니다.

- 각 노드는 State의 일부 필드를 **소비(입력)** 하고 다른 필드를 **생성(출력)** 합니다.
- 따라서 먼저 `State`를 정의한 후, 그 속성을 채우는 방식으로 `Node`를 만들게 됩니다.

예:

```python
class AgentState(TypedDict):
    input: str
    parsed_query: Optional[dict]
    tool_choice: Optional[str]
    tool_result: Optional[str]
    output: Optional[str]
```

---

## ⚙️ 주요 API 요약

| 메서드                                                | 설명                            |
| ----------------------------------------------------- | ------------------------------- |
| `StateGraph(StateType)`                               | 전체 그래프 빌더 생성           |
| `add_node(name, fn)`                                  | 노드 등록                       |
| `add_edge(from_node, to_node)`                        | 노드 간 순차 실행 흐름 정의     |
| `add_conditional_edges(node, condition_fn, path_map)` | 조건 분기 흐름 정의             |
| `set_entry_point(name)`                               | 그래프 시작 노드 지정           |
| `set_finish_point(name)`                              | 그래프 종료 노드 지정           |
| `compile()`                                           | 실행 가능한 그래프 객체 생성    |
| `invoke(input_state)`                                 | 그래프 실행 (sync)              |
| `astream(input_state)`                                | 그래프 실행 (async + streaming) |

---

## 🔁 전체 예시 흐름

```python
from langgraph.graph import StateGraph

# 1. 상태 정의
class AgentState(TypedDict):
    input: str
    output: Optional[str]

# 2. 그래프 빌더 생성
builder = StateGraph(AgentState)

# 3. 노드 정의 및 등록
def run_llm(state: AgentState) -> AgentState:
    output = "LLM 처리 결과: " + state["input"]
    return {"output": output}

builder.add_node("llm_node", run_llm)

# 4. 엣지 및 흐름 정의
builder.set_entry_point("llm_node")
builder.set_finish_point("llm_node")

# 5. 컴파일 및 실행
graph = builder.compile()
result = graph.invoke({"input": "안녕"})
print(result)
```

---

## 📌 요약: StateGraph란?

> LangGraph의 `StateGraph`는 "Node 중심"이 아닌 **State 중심**의 DAG입니다. 각 노드는 상태를 읽고 새로운 상태를 반환하며, 그래프는 이 흐름을 기반으로 동작합니다.
