# LangGraph Subgraphs 가이드

LangGraph의 `Subgraph`는 복잡한 워크플로우를 **모듈화하고 재사용 가능하게 만드는 기능**입니다. 여러 노드로 구성된 상태 그래프(StateGraph)를 하나의 서브 그래프로 묶어, 상위 그래프에서 **하나의 노드처럼 사용**할 수 있습니다. 이를 통해 시스템을 더 읽기 쉽고 유지보수 가능하게 만들 수 있습니다.

---

## ✅ Subgraph란?

| 항목    | 설명                                                                               |
| ------- | ---------------------------------------------------------------------------------- |
| 정의    | 여러 노드로 구성된 내부 그래프를 외부에서 하나의 노드처럼 호출할 수 있게 만든 구조 |
| 목적    | 코드 재사용성, 복잡도 분리, 논리 모듈화                                            |
| 활용 예 | 반복적인 처리 흐름, 특정 도메인 전용 처리기, 평가 루프 등                          |

---

## 🔧 Subgraph 생성 예시

```python
from langgraph.graph import StateGraph, END

# 1. 내부 상태 정의
class InnerState(TypedDict):
    text: str

# 2. 서브그래프 구성
inner = StateGraph(InnerState)
inner.add_node("a", lambda x: x)
inner.set_entry_point("a")
inner.set_finish_point("a")

# 3. 서브그래프 컴파일
subgraph = inner.compile_to_node()
```

- `compile_to_node()` 를 호출하면 이 서브그래프는 **상위 그래프에서 하나의 노드처럼 사용** 가능해집니다.

---

## 🧩 Subgraph를 상위 그래프에서 사용하는 방법

```python
from langgraph.graph import StateGraph

class OuterState(TypedDict):
    text: str

# 상위 그래프 생성
outer = StateGraph(OuterState)
outer.add_node("preprocess", lambda x: x)
outer.add_node("subgraph_block", subgraph)
outer.set_entry_point("preprocess")
outer.add_edge("preprocess", "subgraph_block")
outer.set_finish_point("subgraph_block")

graph = outer.compile()
graph.invoke({"text": "hello"})
```

- `subgraph_block` 노드는 내부적으로 `inner` 그래프를 실행함
- 상태 타입이 맞아야 동작 (입출력 키/값 매핑 유의)

---

## 🤝 병렬 실행 (Parallel Nodes)과 Subgraph

LangGraph에서는 `StateGraph.add_parallel()` 메서드를 통해 여러 노드(또는 서브그래프)를 **병렬로 실행**할 수 있습니다. 각 병렬 노드는 동일한 상태를 입력받고, 동시에 실행되어 결과를 병합합니다.

### ✅ 병렬 실행 예시

```python
builder.add_parallel(
    name="parallel_block",
    branches={
        "branch1": subgraph1,
        "branch2": subgraph2,
    },
)
```

- `branch1`, `branch2`는 모두 `compile_to_node()`로 만들어진 Subgraph
- 병렬 실행 후, 각 결과는 병합되어 다음 노드에 전달됨
- 상태 충돌 방지를 위해 각 Subgraph는 서로 다른 상태 키를 사용하는 것이 좋습니다

### 병합 예시 결과 형태

```json
{
  "branch1": {"result": ...},
  "branch2": {"result": ...}
}
```

- 이후 노드에서 `state["branch1"]["result"]` 와 같이 접근 가능

---

## 🔁 Subgraph의 유용한 활용 사례

| 시나리오           | 설명                                                    |
| ------------------ | ------------------------------------------------------- |
| 🔄 반복 루프 처리  | 재귀 또는 반복 평가 루프를 서브그래프로 캡슐화          |
| 🧪 평가 파이프라인 | LLM 응답 평가 및 수정 흐름을 모듈화                     |
| 📦 도메인 처리기   | 도메인별 특화된 응답 생성 서브그래프 구성               |
| 🧠 Self-RAG 루프   | Retriever → Generator → Evaluator → Reflector 구조 구현 |
| ⚙️ 병렬 실행 블록  | 여러 검색기, 요약기 등을 병렬로 실행하여 비교           |

---

## ⚠️ 주의사항

| 항목           | 내용                                                           |
| -------------- | -------------------------------------------------------------- |
| 상태 구조      | 서브그래프의 입력/출력 상태 타입은 상위 그래프와 호환되어야 함 |
| 상태 공유      | 서브그래프 내부에서 상태 변경 시 상위 상태에도 반영됨          |
| 상태 키 충돌   | 동일 키 사용 시 예상치 못한 덮어쓰기 주의                      |
| 루프 제한      | 무한 루프 방지를 위한 명시적 종료 조건 필요                    |
| 병렬 결과 병합 | 병렬 실행 후 결과 병합 구조 확인 필요                          |

---

## ✅ 정리

- `Subgraph`는 LangGraph에서 **복잡한 노드 흐름을 하나의 블록으로 묶어 사용하는 모듈화 도구**입니다.
- 유지보수성, 재사용성, 추론 흐름 분리에 매우 효과적입니다.
- `StateGraph`를 정의하고 `.compile_to_node()`로 변환 후 상위 그래프에 삽입하는 방식으로 구현됩니다.
- 병렬 실행 시에도 Subgraph를 활용하면 여러 흐름을 동시에 처리하고 비교하는 데 매우 유용합니다.

---

LangGraph의 Subgraph 기능은 복잡한 AI 시스템에서 각 파트의 책임을 분리하고, 테스트 및 디버깅 효율을 크게 향상시켜줍니다. ReAct, Self-RAG, 평가 기반 루프, 병렬 검색기/요약기 비교 구성 등에서 특히 유용합니다.
