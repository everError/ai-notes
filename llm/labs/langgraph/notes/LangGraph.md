# LangGraph 개요

## 🔍 LangGraph란?

LangGraph는 LangChain에서 확장한 프레임워크로, LLM(대형 언어 모델)의 동작을 **상태 기반(Stateful)** **멀티에이전트 그래프(Graph)** 로 구성할 수 있게 해주는 오픈소스 도구입니다. LangGraph는 복잡한 추론 흐름, 멀티 스텝 워크플로우, 대화형 시스템 등을 보다 **명확하고 직관적인 그래프 기반 구조**로 표현할 수 있도록 설계되었습니다.

> LangGraph는 LangChain 및 LLM 오케스트레이션과 밀접하게 통합되어 있으며, LangChain 팀이 직접 개발한 프로젝트입니다.

---

## 🚦 왜 LangGraph를 사용할까?

### 🔸 기존의 LLM 체인(Chain) 방식의 한계

- 선형 흐름 (입력 → LLM → 출력) 에 한정됨
- 조건 분기, 반복 처리, 병렬 실행 등이 어려움

### 🔸 LangGraph의 장점

- 복잡한 LLM 기반 애플리케이션을 **상태 기계(State Machine)** 로 구성 가능
- 각 노드가 LLM, 도구 호출, 조건 판단 등의 역할을 수행
- **비동기 처리를 통한 고성능 분기 및 반복 처리** 가능
- 멀티 에이전트 시스템 설계에 유리함

---

## 🧩 핵심 개념 및 구성 요소

### 1. **Graph (그래프)**

- 전체 워크플로우를 정의하는 그래프 객체
- 노드(Node)와 간선(Transition)으로 구성됨

### 2. **Node (노드)**

- 그래프 내 하나의 처리 단위 (예: LLM 호출, 함수 실행 등)
- 일반적으로 LangChain Runnable 또는 사용자 정의 함수

### 3. **Edge / Transition (간선)**

- 하나의 노드에서 다음 노드로 넘어가는 조건 및 연결 정의
- 조건 분기 로직(if/else, switch 등)을 포함할 수 있음

### 4. **State (상태)**

- 각 실행 단계에서 공유되는 데이터
- 대화 컨텍스트, 도구 호출 결과 등이 저장됨

---

## ✨ 사용 예시 (간단 예)

```python
from langgraph.graph import StateGraph, END
from langchain.chat_models import ChatOpenAI
from langchain.schema.runnable import RunnableLambda

# 1. 노드 함수 정의
llm = ChatOpenAI()

@RunnableLambda
def process(input):
    return llm.invoke("사용자 입력: " + input)

# 2. 그래프 구성
builder = StateGraph()
builder.add_node("llm_node", process)
builder.set_entry_point("llm_node")
builder.add_edge("llm_node", END)

graph = builder.compile()

# 3. 실행
result = graph.invoke({"input": "안녕"})
print(result)
```

---

## 🔧 활용 사례

- 멀티 라운드 대화 흐름 관리 (메모리 포함)
- 조건에 따라 분기하는 복잡한 상담 챗봇
- LLM 기반 자동화 파이프라인 (e.g. 문서 요약 → 번역 → 저장)
- 멀티 에이전트 협업 시뮬레이션

---

## 📝 정리

LangGraph는 LangChain의 강력한 기능을 기반으로, 복잡한 LLM 기반 애플리케이션을 시각적이고 구조적인 그래프 형태로 구성할 수 있도록 도와주는 프레임워크입니다. 워크플로우를 시각화하고, 상태 기반 멀티에이전트 시스템을 설계하고자 한다면 LangGraph는 매우 유용한 선택이 될 수 있습니다.
