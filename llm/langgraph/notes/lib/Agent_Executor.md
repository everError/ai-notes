# LangChain 에이전트 구현 가이드

LangChain에서 에이전트를 정의하고 사용하는 방식 중 하나는 `create_tool_calling_agent` 함수를 통해 에이전트를 생성하고, `AgentExecutor`를 통해 실행하는 것입니다. 아래에 각 구성 요소와 개념적 배경을 자세히 설명합니다.

---

## ✅ 0. 에이전트 개념 이해

### 🔹 에이전트란?

LangChain에서의 **에이전트(Agent)**는 LLM이 외부 도구(API, 계산기, DB, 검색 엔진 등)를 상황에 맞게 선택하여 작업을 수행하도록 설계된 인터페이스입니다. 즉, **복잡한 질문을 분석하고 필요한 외부 도구를 순차적으로 사용하면서 목표에 도달하는 일련의 추론 시스템**입니다.

### 🔹 구성 요소

LangChain 에이전트는 다음 구성 요소로 이루어집니다:

- **LLM**: 추론과 응답 생성을 담당
- **프롬프트(Prompt)**: 작업 지시 및 컨텍스트 제공
- **도구(Tools)**: 외부 기능(API, 계산 등)을 래핑한 객체
- **에이전트 로직**: 어떤 도구를 언제 사용할지를 결정
- **실행기(Executor)**: 에이전트 실행을 조율하고 결과를 반환

### 🔹 대표 아키텍처 흐름

```
사용자 입력 → 프롬프트 생성 → LLM reasoning → 도구 선택 및 호출 → 결과 수집 및 재추론 → 최종 응답 생성
```

---

## ✅ 1. create_tool_calling_agent

LangChain에서는 사용자 정의 프롬프트를 기반으로 도구 기반 에이전트를 생성할 수 있습니다. 이때 사용되는 주요 구성 요소는 다음과 같습니다.

### 🔹 프롬프트 구성

프롬프트는 LLM이 작업을 이해하고 실행하기 위한 컨텍스트를 제공합니다. `agent_scratchpad`와 `input` 변수를 포함해야 하며, 프롬프트에는 일반적으로 시스템 역할, 사용자 메시지, 이전 대화 내역, 에이전트의 도구 호출 기록 등을 포함합니다.

```python
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder

agent_prompt = ChatPromptTemplate.from_messages([
    ("system", "당신은 레스토랑의 메뉴 정보를 제공하고, 관련된 블로그 글도 함께 추천하는 AI 어시스턴트입니다."),
    MessagesPlaceholder(variable_name="chat_history"),
    ("human", "{input}"),
    MessagesPlaceholder(variable_name="agent_scratchpad"),
])
```

> 💬 `input`: 사용자 초기 질의 및 지시사항 → Agent가 작업을 시작하는 출발점  
> 📜 `agent_scratchpad`: Agent의 사고 과정과 중간 단계 기록 → 이전 단계의 결과와 다음 단계를 계획하는 데 사용됨

### 🔹 에이전트 생성

```python
from langchain.agents import create_tool_calling_agent

tools = [web_search_tool, blog_search, search_menu]
agent = create_tool_calling_agent(llm, tools, agent_prompt)
```

> `create_tool_calling_agent`는 LLM과 도구, 프롬프트를 바탕으로 ToolCallingAgent를 생성합니다. 이 Agent는 사용자의 입력에 따라 적절한 도구를 선택하여 작업을 수행합니다.

---

## ✅ 2. AgentExecutor 실행

생성된 agent는 `AgentExecutor`를 통해 실제 사용자 입력을 받아 처리할 수 있습니다. `AgentExecutor`는 agent와 tools를 실행 환경에 통합하며, 입력값에 따라 프롬프트를 구성하고 반복적으로 도구를 호출하며 최종 응답을 생성합니다.

```python
from langchain.agents import AgentExecutor

agent_executor = AgentExecutor(agent=agent, tools=tools, verbose=True)

query = "시그니처 스테이크의 가격과 특징은 무엇인가요? 그리고 스테이크와 어울리는 와인 추천도 해주세요."
agent_response = agent_executor.invoke({"input": query})
```

> `verbose=True` 옵션을 통해 각 도구 호출 및 내부 동작 로그를 출력할 수 있습니다.

### 🔎 출력 예시

- `search_menu` 툴 실행 → 메뉴 데이터 검색
- `web_search` 툴 실행 → 스테이크와 와인에 대한 블로그 검색
- 중간 reasoning 로그는 `agent_scratchpad`로 기록
- 최종적으로 종합된 응답이 사용자에게 반환됨

---

## ✅ 3. 확장 정보

### 🔹 주요 개념 요약

| 항목                 | 설명                                                                         |
| -------------------- | ---------------------------------------------------------------------------- |
| `Tool`               | 외부 기능(API, 계산기, DB 등)을 래핑한 객체. 에이전트는 Tool을 선택해 사용함 |
| `agent_scratchpad`   | 이전 도구 호출 로그와 reasoning 내용 저장 공간                               |
| `AgentExecutor`      | agent + tools 실행 관리, 반복 추론 및 응답 구성                              |
| `ChatPromptTemplate` | 시스템 지시사항과 대화 흐름을 프롬프트 형태로 구성하는 객체                  |

### 🔹 자주 쓰는 Agent 유형

- `AgentType.ZERO_SHOT_REACT_DESCRIPTION`: 툴 설명 기반으로 reasoning + tool calling 진행 (기본값)
- `AgentType.CONVERSATIONAL_REACT_DESCRIPTION`: 이전 대화를 고려한 tool reasoning 포함
- `create_openai_functions_agent`: OpenAI function-calling 기반 에이전트 생성
- `create_react_agent`: 더 구체적인 ReAct 스타일 에이전트

### 🔹 LangChain 내부 구성 라이브러리 개요

| 모듈                     | 설명                                         |
| ------------------------ | -------------------------------------------- |
| `langchain.agents`       | Agent, Executor, Tool 관련 생성 및 실행 모듈 |
| `langchain_core.prompts` | 프롬프트 템플릿 구성용 핵심 모듈             |
| `langchain_core.tools`   | Tool 클래스, 호출 방식 정의                  |
| `langchain.chat_models`  | OpenAI 등 LLM 설정 클래스 (ChatOpenAI 등)    |

---

## 📝 정리

LangChain의 Agent 구조는 복잡한 요구를 충족할 수 있도록 **LLM 기반 reasoning**, **외부 도구 사용**, **프롬프트 기반 지시 구조**를 통합합니다. `create_tool_calling_agent`는 가장 단순하고 강력한 생성 방식 중 하나이며, `AgentExecutor`는 이 Agent를 실제 입력과 연결해 실행하는 엔진 역할을 합니다.

> 💡 복잡한 멀티스텝 작업, 도구 조합, 동적 질의 해석이 필요한 AI 앱에서 매우 유용하게 활용됩니다.
