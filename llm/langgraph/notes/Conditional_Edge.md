# LangGraph 조건 분기 및 고급 구성 가이드

LangGraph는 상태 기반(State-based)의 노드 그래프를 구성할 수 있는 강력한 프레임워크로, 다양한 흐름 제어(조건 분기, 반복, 병렬 실행 등)를 시각적이고 선언적으로 정의할 수 있습니다.

이 문서에서는 LangGraph의 핵심 기능인 **조건 분기(Conditional Branching)**와 함께 **고급 구성(루프, 다중 조건 흐름)**을 단계적으로 설명합니다.

---

## ✅ 1. 기본 구성 흐름

1. **State 정의**: 그래프의 상태(State) 구조 정의 (TypedDict, dataclass 등)
2. **Node 정의**: 상태의 입력 필드를 기반으로 출력 상태를 생성하는 함수 또는 Runnable
3. **Edge 정의**: 노드 간의 흐름 (`add_edge`, `add_conditional_edges`)
4. **Compile**: 실행 가능한 그래프로 변환 (`compile()`)
5. **Invoke**: 그래프 실행 (`invoke()`, `astream()`)

---

## 🌿 2. 조건 분기 (`add_conditional_edges`)

### 📌 사용 목적

노드 실행 결과나 상태의 특정 값에 따라 다음 실행 경로를 동적으로 선택할 수 있습니다.

### ✅ 사용 구조

```python
builder.add_conditional_edges(
    name="choose_node",                # 분기를 발생시킬 노드
    condition=condition_function,      # 분기 키를 반환하는 함수
    path_map={                         # 반환 키에 따른 다음 노드 매핑
        "A": "node_a",
        "B": "node_b",
        "default": "node_default"
    }
)
```

### 🧠 조건 함수 규칙

- `condition(state: StateType) -> str`
- `path_map`의 키와 대응되는 문자열을 반환해야 함

### ✅ 예제

```python
class AgentState(TypedDict):
    input: str
    tool_choice: Optional[str]
    result: Optional[str]

def choose_tool(state: AgentState) -> AgentState:
    if "날씨" in state["input"]:
        return {"tool_choice": "weather"}
    elif "뉴스" in state["input"]:
        return {"tool_choice": "news"}
    return {"tool_choice": "unknown"}

def condition(state: AgentState) -> str:
    return state["tool_choice"] or "unknown"
```

```python
builder.add_conditional_edges(
    "choose_tool",
    condition,
    path_map={
        "weather": "run_weather",
        "news": "run_news",
        "unknown": "handle_unknown"
    }
)
```

---

## 🔁 3. 고급 구성

### 🌀 A. 루프 처리 (Looping)

#### ✅ 예시: 특정 조건이 만족될 때까지 반복

```python
builder.add_conditional_edges(
    name="validate",
    condition=lambda state: "done" if state["valid"] else "retry",
    path_map={
        "done": "finalize",
        "retry": "validate"  # 자기 자신으로 다시 연결
    }
)
```

- 조건이 만족되지 않으면 다시 `validate` 노드로 되돌아가며 루프를 구성
- 무한 루프 방지를 위한 max retry 횟수 등은 상태에서 직접 관리 가능

### 🌲 B. 다단계 조건 분기

```python
builder.add_conditional_edges(
    "check_intent",
    condition=lambda s: s["intent"],
    path_map={
        "search": "search_handler",
        "book": "booking_handler",
        "cancel": "cancel_handler"
    }
)
```

- 사용자의 의도(intent)에 따라 다양한 작업 흐름 분기 가능

### ⚙️ C. 조건 + 연속 Edge 혼합

```python
builder.add_node("preprocess", preprocess)

builder.add_edge("entry", "preprocess")
builder.add_conditional_edges("preprocess", cond_fn, path_map)
builder.add_edge("run_tool", "summarize")
builder.set_finish_point("summarize")
```

- 조건 분기를 중간에 삽입하고, 이후는 순차 edge로 이어서 구성 가능

---

## 📌 요약

| 기능                    | 설명                                |
| ----------------------- | ----------------------------------- |
| `add_conditional_edges` | 조건 기반 분기 처리                 |
| 루프 구성               | 자기 자신으로 edge 연결해 반복 실행 |
| 다단계 분기             | 다양한 조건값에 따른 분기 처리      |
| 혼합형 그래프           | 분기 흐름 + 순차 흐름 조합 가능     |
