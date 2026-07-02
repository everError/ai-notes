# LangGraph 상태 리듀서(State Reducer) 가이드

LangGraph의 핵심 철학은 **State 중심의 데이터 흐름 제어**입니다. 그중에서도 `State Reducer`는 각 노드의 실행 결과를 전체 상태에 병합하는 방식을 결정하는 중요한 역할을 합니다.

---

## ✅ State Reducer란?

> `State Reducer`는 상태 업데이트를 관리하는 함수로, 각 노드의 출력을 전체 상태에 통합하는 방식을 정의합니다.

### 📌 핵심 요약

- **정의**: 상태 업데이트를 관리하는 함수
- **목적**: 그래프의 각 노드의 출력을 전체 상태에 통합하는 전략 정의
- **반환**: 항상 전체 상태 (`dict`) 객체를 반환해야 함

### ⚙️ 동작 방식 비교

| 방식               | 설명                                                      |
| ------------------ | --------------------------------------------------------- |
| 기본 리듀서        | `dict.update()` 방식으로 필드를 단순 병합                 |
| 추가형 리듀서      | 리스트 필드에 `.append()` 또는 리스트 결합 수행           |
| 사용자 정의 리듀서 | 중복 제거, 조건 병합 등 로직 삽입 가능                    |
| Annotated 리듀서   | 특정 필드에만 개별 reducer를 지정 (예: `operator.add` 등) |

---

## 📦 기본 동작 방식

```python
def run_node(state):
    return {"output": "hello"}

# 내부적으로:
state.update({"output": "hello"})
```

- 기존 상태는 그대로 유지되고, 반환된 필드만 업데이트됩니다.

---

## 🧩 커스텀 Reducer 정의 및 적용

### ✅ 기본 구조

```python
def my_reducer(old_state: dict, updates: dict) -> dict:
    new_state = old_state.copy()
    new_state.update(updates)
    return new_state
```

### ✅ 사용 방법

```python
builder = StateGraph(AgentState, state_reducer=my_reducer)
```

---

## 🧠 커스텀 Reducer가 필요한 이유

| 필요 상황        | 설명                                            |
| ---------------- | ----------------------------------------------- |
| 상태 변경 추적   | 상태 변경 이력을 log에 누적                     |
| 불변성 보장      | 원본 상태 객체 변경 방지                        |
| 조건 기반 병합   | 특정 조건에서만 필드 병합 수행                  |
| 상태 필드 결합   | 여러 필드를 기반으로 파생 필드 생성             |
| 리스트 누적 처리 | 기존 리스트에 요소 append (예: message list 등) |

---

## 🔁 커스텀 Reducer 예제 모음

### 1. 상태 변경 로그 누적

```python
def reducer_with_log(old: dict, updates: dict) -> dict:
    log = old.get("log", [])
    log.append({"updates": updates})
    return {
        **old,
        **updates,
        "log": log
    }
```

### 2. 리스트 필드에 값 누적

```python
def append_reducer(old: dict, updates: dict) -> dict:
    new_state = old.copy()
    for key, value in updates.items():
        if isinstance(value, list):
            new_state[key] = old.get(key, []) + value
        else:
            new_state[key] = value
    return new_state
```

### 3. 덮어쓰기 방지

```python
def safe_reducer(old: dict, updates: dict) -> dict:
    if any(k in old for k in updates):
        raise ValueError("중복된 필드가 존재합니다.")
    return {**old, **updates}
```

---

## 🏷️ Annotated 리듀서 적용 (필드 단위)

LangGraph에서는 `Annotated` 타입 힌트를 사용해 **필드 단위 reducer**를 지정할 수 있습니다.
이때 reducer는 일반적으로 Python 내장 함수나 사용자 정의 병합 함수일 수 있습니다.

```python
from operator import add
from typing import Annotated, TypedDict

class ReducerState(TypedDict):
    query: str
    documents: Annotated[list[str], add]  # 리스트 병합
```

- 위 예시에서 `documents` 필드는 리스트를 병합할 때 `operator.add`가 적용됩니다.
- `query`는 기본 병합 방식 사용

이 방식은 전체 reducer를 지정하지 않고도 세부 제어가 가능하다는 장점이 있습니다.

---

## 📌 요약 정리

| 항목           | 설명                                                      |
| -------------- | --------------------------------------------------------- |
| 정의           | 상태 업데이트를 관리하는 함수                             |
| 목적           | 노드 실행 결과를 전체 상태에 통합하는 전략 정의           |
| 기본 방식      | `dict.update()` 기반 필드 병합                            |
| 커스텀 방식    | 조건 병합, 리스트 누적, 로그 추가 등 자유롭게 구현        |
| 필드 단위 설정 | `Annotated[field, 병합함수]` 방식으로 적용 가능           |
| 적용 방법      | `StateGraph(..., state_reducer=함수)` 또는 Annotated 활용 |
| 반환값         | 항상 전체 상태(dict)를 반환해야 함                        |

---

LangGraph의 상태 리듀서는 단순 병합을 넘어서 **데이터 흐름의 품질과 추적 가능성**을 높이는 중요한 도구입니다. 복잡한 AI 워크플로우에서 상태 관리 전략을 세밀하게 설계할 수 있도록 돕습니다.
