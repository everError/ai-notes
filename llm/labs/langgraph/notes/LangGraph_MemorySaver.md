# LangGraph MemorySaver 가이드

`MemorySaver`는 LangGraph에서 제공하는 **in-memory 기반의 Checkpointer 구현체**입니다. 즉, LangGraph 그래프의 상태(GraphState)를 Python 메모리(RAM) 상에 저장하고, 같은 세션(thread\_id)으로 후속 호출 시 해당 상태를 복원할 수 있게 해주는 저장소입니다.

> **주의**: 이름과는 다르게, `MemorySaver`는 메모리 사용량을 줄이는 기능(memory optimization)을 의미하지 않습니다.

---

## ✅ MemorySaver란?

| 항목    | 설명                                                |
| ----- | ------------------------------------------------- |
| 정의    | LangGraph 상태를 Python 메모리에 저장하는 체크포인터 구현체          |
| 역할    | `graph.invoke(..., thread_id=...)`를 사용할 때 상태를 유지함 |
| 저장 위치 | 디스크가 아닌 RAM (휘발성)                                 |
| 지속성   | 없음 – 프로세스 종료 시 저장된 상태 모두 사라짐                      |
| 주요 용도 | 빠른 테스트, 데모, 멀티 스레드 세션 시뮬레이션 등에 사용                 |

---

## 🛠️ 기본 사용 예시

```python
from langgraph.checkpoint.memory import MemorySaver
from langgraph.graph import StateGraph

memory = MemorySaver()
builder = StateGraph(GraphState)

# 노드와 엣지 구성 등...
graph = builder.compile(checkpointer=memory)

# 상태 저장 및 복원
graph.invoke({"query": "안녕"}, thread_id="session-1")
graph.invoke({"query": "오늘 날씨 어때?"}, thread_id="session-1")
```

* 동일한 `thread_id`를 지정하면 이전 대화 상태가 자동으로 이어짐
* 여러 사용자/세션을 동시에 테스트할 수 있음

---

## 🔍 내부 동작 방식

| 단계                                     | 설명 |
| -------------------------------------- | -- |
| 1. `graph.invoke()` 호출 시 thread\_id 확인 |    |
| 2. 해당 ID에 대한 상태가 메모리에 존재하면 복원          |    |
| 3. 없으면 초기 상태 사용                        |    |
| 4. 처리 후 결과 상태를 메모리에 저장                 |    |

> 저장된 상태는 `MemorySaver` 객체의 Python 딕셔너리에 유지됨

---

## 🧠 언제 MemorySaver를 사용하면 좋은가?

| 상황          | 추천 여부 | 설명                                |
| ----------- | ----- | --------------------------------- |
| ✅ 빠른 로컬 테스트 | 매우 적합 | 파일 저장 없이 빠르게 상태 저장/복원 가능          |
| ✅ 여러 세션 관리  | 적합    | `thread_id` 기반 분기 처리에 유용          |
| ❌ 장기 세션 유지  | 부적합   | Python 프로세스 종료 시 상태 유실            |
| ❌ 서비스 환경    | 부적합   | Redis, SQLite, S3 등 지속 가능한 저장소 권장 |

---

## 🔄 `MemorySaver` vs 메모리 최적화 전략

| 항목       | MemorySaver                   | 메모리 절약 전략 (Memory Saving) |
| -------- | ----------------------------- | ------------------------- |
| 기능 목적    | 상태 저장/복원 (in-memory)          | 상태 자체를 줄임 (요약, 삭제 등)      |
| 관련 모듈    | `langgraph.checkpoint.memory` | `StateReducer`, 요약 노드 등   |
| 상태 유지 방식 | RAM에 저장                       | 상태 크기를 줄이거나 context를 압축   |
| 영속성      | ❌ 없음                          | 전략에 따라 외부 DB 등 가능         |

---

## 📌 정리

| 항목       | 설명                                      |
| -------- | --------------------------------------- |
| ✅ 무엇인가   | LangGraph 상태를 메모리에 저장하는 Checkpointer    |
| ✅ 언제 쓰나  | 빠른 실험, 세션 단위 흐름 유지 시                    |
| ❌ 무엇이 아님 | 메모리 사용량 최적화 기능은 아님                      |
| ✅ 대안     | 장기 상태 저장 시 SqliteSaver, RedisSaver 등 사용 |

---

`MemorySaver`는 LangGraph의 세션 흐름 유지 기능을 간단하게 실험해볼 수 있게 해주는 **비휘발성 상태 저장소의 테스트 버전**입니다.
실제 서비스에는 외부 저장소를 사용하는 Checkpointer로 교체하는 것이 좋습니다.
