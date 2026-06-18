# LangChain 내장 도구 모음

LangChain은 다양한 내장 도구(Agents Tools)를 제공하여 LLM(대형 언어 모델)과 쉽게 연동할 수 있도록 지원합니다. 이 문서는 LangChain에서 기본적으로 제공하는 주요 도구들을 정리한 것입니다.

---

## 📌 **LangChain 내장 도구 개요**

LangChain의 내장 도구들은 AI Agent가 외부 데이터와 상호작용하거나 특정 작업을 수행할 수 있도록 돕습니다. 대표적인 도구들은 다음과 같습니다:

- 🔹 **검색 도구** → 웹 검색, 문서 검색
- 🔹 **계산 도구** → 수식 계산, 코드 실행
- 🔹 **데이터 처리 도구** → JSON, CSV, SQL 데이터베이스 처리
- 🔹 **API 호출 도구** → HTTP 요청, OpenAPI 연동
- 🔹 **파일 처리 도구** → 로컬 파일 읽기/쓰기
- 🔹 **LLM 보조 도구** → 프롬프트 엔지니어링, 텍스트 요약

이제 각 도구에 대해 상세히 알아보겠습니다.

---

## 🔍 **1. 검색(Search) 도구**

### **🔹 SerpAPIWrapper**

- Google Search API를 사용하여 웹 검색 수행
- API Key 필요
- 사용 예시:
  ```python
  from langchain.tools import SerpAPIWrapper
  search = SerpAPIWrapper()
  result = search.run("LangChain이란?")
  ```

### **🔹 Wikipedia API Wrapper**

- Wikipedia에서 정보를 검색하는 도구
- 사용 예시:
  ```python
  from langchain.tools import WikipediaQueryRun
  wiki = WikipediaQueryRun()
  result = wiki.run("LangChain")
  ```

### **🔹 Tavily Search**

- Tavily API를 이용한 강력한 검색 도구
- 웹 검색 결과를 효과적으로 수집 및 분석 가능
- API Key 필요
- 사용 예시:
  ```python
  from langchain.tools import TavilySearchAPIWrapper
  tavily_search = TavilySearchAPIWrapper()
  result = tavily_search.run("LangChain 최신 업데이트")
  ```

---

## 🧮 **2. 계산(Calculation) 도구**

### **🔹 Python REPL (Code Interpreter)**

- Python 코드 실행을 위한 내장 도구
- 사용 예시:
  ```python
  from langchain.tools import PythonREPLTool
  code_tool = PythonREPLTool()
  result = code_tool.run("print(2 + 2)")
  ```

### **🔹 Wolfram Alpha API**

- 수학 및 과학 문제 해결을 위한 API 도구
- API Key 필요
- 사용 예시:
  ```python
  from langchain.tools import WolframAlphaAPIWrapper
  wolfram = WolframAlphaAPIWrapper()
  result = wolfram.run("integrate x^2 dx")
  ```

---

## 📊 **3. 데이터 처리(Data Handling) 도구**

### **🔹 JSON 처리 도구**

- JSON 데이터를 파싱하는 도구
- 사용 예시:
  ```python
  from langchain.tools.json.tool import JsonSpec
  json_tool = JsonSpec()
  ```

### **🔹 SQL 데이터베이스 도구**

- SQL 쿼리를 실행하는 도구
- 예시:
  ```python
  from langchain.tools import SQLDatabaseTool
  sql_tool = SQLDatabaseTool(db_uri="sqlite:///mydb.db")
  ```

---

## 🌍 **4. API 호출 도구**

### **🔹 Requests (HTTP 요청 도구)**

- REST API 요청을 수행하는 도구
- 사용 예시:
  ```python
  from langchain.tools import RequestsGetTool
  requests_tool = RequestsGetTool()
  result = requests_tool.run("https://api.example.com/data")
  ```

### **🔹 OpenAPI Wrapper**

- OpenAPI 명세를 기반으로 API와 상호작용하는 도구
- 사용 예시:
  ```python
  from langchain.tools import OpenAPISpec
  openapi_tool = OpenAPISpec("https://api.example.com/openapi.json")
  ```

---

## 📁 **5. 파일 처리(File Handling) 도구**

### **🔹 파일 읽기/쓰기 도구**

- 로컬 파일을 읽고 쓰는 기능 제공
- 사용 예시:
  ```python
  from langchain.tools import ReadFileTool, WriteFileTool
  read_tool = ReadFileTool()
  write_tool = WriteFileTool()
  ```

---

## 📝 **6. LLM 보조 도구**

### **🔹 텍스트 요약 도구**

- 긴 문서를 요약하는 기능 제공
- 사용 예시:
  ```python
  from langchain.tools import LLMTextSummarizer
  summarizer = LLMTextSummarizer()
  ```

### **🔹 프롬프트 최적화 도구**

- LLM 성능을 최적화하기 위한 프롬프트 엔지니어링 기능 제공
