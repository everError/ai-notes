# LangChain의 Tool 개념 및 활용

LangChain에서 제공하는 **Tool**(도구)은 LLM(대형 언어 모델)이 외부 시스템과 상호작용할 수 있도록 하는 기능입니다. Tool을 활용하면 AI Agent가 단순한 텍스트 처리뿐만 아니라 **검색, 계산, API 호출, 데이터베이스 조회 등 다양한 작업을 수행할 수 있습니다**.

---

## 📌 **1. LangChain의 Tool 개념**

LangChain에서 Tool은 **외부 서비스나 기능을 LLM이 직접 호출할 수 있도록 하는 인터페이스**입니다.

✅ **주요 개념**

- **LLM과 외부 세계를 연결하는 인터페이스**
- **함수 형태로 정의되며, LLM이 해당 함수를 호출할 수 있음**
- **API 호출, 데이터 검색, 계산, 파일 읽기/쓰기 등 다양한 작업 수행 가능**

✅ **Tool이 필요한 이유**

- LLM은 자체적으로 웹 검색, 계산, 데이터베이스 조회 등을 수행할 수 없음
- Tool을 사용하면 **LLM이 명령을 내리고 실제 실행은 외부 시스템에서 처리 가능**
- 이를 통해 AI Agent가 단순 질의응답을 넘어 **실제 업무 수행이 가능한 AI 시스템**으로 확장됨

---

## 🏗 **2. Tool의 주요 속성**

각 Tool은 특정한 속성을 가지며, 이를 통해 LLM과 효과적으로 상호작용할 수 있습니다.

| 속성            | 설명                                                               |
| --------------- | ------------------------------------------------------------------ |
| `name`          | Tool의 고유한 이름                                                 |
| `description`   | Tool의 역할과 기능 설명 (LLM이 이를 참고하여 Tool을 선택)          |
| `args_schema`   | Tool에 전달할 인자의 형식 정의 (Pydantic 사용 가능)                |
| `func`          | Tool이 실행할 실제 함수 (API 호출, 계산, 데이터 검색 등)           |
| `return_direct` | True이면 LLM이 추가적인 응답 생성 없이 Tool의 반환값을 그대로 출력 |

✅ **예제** (간단한 검색 Tool 정의)

```python
from langchain.tools import Tool
import requests

def search(query: str):
    response = requests.get(f"https://api.example.com/search?q={query}")
    return response.json()

search_tool = Tool(
    name="WebSearch",
    description="사용자의 질의에 대해 웹 검색을 수행하고 결과를 반환합니다.",
    func=search
)
```

---

## 🛠 **3. Tool의 주요 기능 및 특징**

### 🔹 **1) LLM이 직접 Tool을 호출 가능**

- LLM은 프롬프트를 분석하여 적절한 Tool을 선택하여 실행 가능
- 예: "2023년 최신 AI 기술 트렌드를 검색해줘" → `WebSearch` Tool 호출

### 🔹 **2) 복잡한 연산을 Tool을 통해 수행**

- 수학 계산, 코드 실행 등의 작업을 수행할 수 있음
- 예: Python REPL(코드 실행) Tool을 활용하여 수식 계산 가능

### 🔹 **3) API 호출을 통한 외부 시스템 연동**

- REST API 또는 GraphQL API 호출 가능
- 외부 데이터베이스, 검색 엔진, 파일 시스템과 연동 가능

### 🔹 **4) Tool을 여러 개 조합하여 다중 작업 수행**

- 여러 개의 Tool을 등록하고, LLM이 상황에 맞게 적절한 Tool을 선택하여 사용 가능
- 예: "이 기사 요약하고 관련된 최신 뉴스 검색해줘" → 요약 Tool + 검색 Tool 조합 사용

---

## 🚀 **4. LangChain Tool 사용 방법**

### **1️⃣ Tool 단독 사용**

Tool을 직접 실행하여 특정 기능을 수행할 수 있음.

```python
result = search_tool.run("LangChain 최신 업데이트")
print(result)
```

### **2️⃣ LLM과 연동하여 자동 호출**

LLM이 사용자 질문을 분석하고 적절한 Tool을 선택하여 실행하도록 설정 가능.

```python
from langchain.chat_models import ChatOpenAI
from langchain.agents import initialize_agent
from langchain.agents import AgentType

llm = ChatOpenAI(model_name="gpt-4")
agent = initialize_agent([search_tool], llm, agent=AgentType.ZERO_SHOT_REACT_DESCRIPTION, verbose=True)

response = agent.run("LangChain의 최신 기능을 검색해줘")
print(response)
```

✅ 위와 같이 설정하면 LLM이 적절한 Tool을 자동으로 선택하여 실행하게 됨.

---

## 📚 **5. 주요 내장 Tool 예시**

LangChain에서는 여러 개의 기본 제공 Tool을 사용할 수 있습니다.

| Tool                | 설명                                         |
| ------------------- | -------------------------------------------- |
| `SerpAPIWrapper`    | Google 검색 API를 통해 웹 검색 수행          |
| `PythonREPLTool`    | Python 코드를 실행하여 계산 수행             |
| `RequestsGetTool`   | HTTP GET 요청을 수행하여 API 데이터 가져오기 |
| `SQLDatabaseTool`   | SQL 쿼리를 실행하여 데이터베이스 조회        |
| `LLMTextSummarizer` | 긴 문서를 요약                               |

---

## 🎯 **6. 결론**

LangChain의 **Tool** 개념을 활용하면 **AI Agent가 단순한 대화형 모델을 넘어서, 실제 작업을 수행하는 강력한 시스템**으로 확장될 수 있습니다.

✔ LLM이 직접 외부 시스템과 연결되어 실질적인 업무를 수행 가능
✔ API, 계산, 검색, 데이터 조회 등 다양한 작업을 자동화 가능
✔ 여러 개의 Tool을 조합하여 복잡한 에이전트 워크플로우 구축 가능
