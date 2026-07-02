# LangChain 도구 정의 가이드 (Tools Guide)

LangChain에서는 AI Agent가 외부 기능을 사용할 수 있도록 **도구(Tool)**를 정의하는 기능을 제공합니다. 이 문서는 LangChain에서 도구를 정의하는 방법, 구조, 사용 가이드라인, 데코레이터 방식까지 포괄적으로 설명합니다.

---

## 📌 1. Tool이란?

Tool은 **LLM Agent가 직접 호출하여 실행할 수 있는 외부 함수 또는 기능**입니다. 예를 들어, 웹 검색, 계산, API 호출, DB 질의 등의 작업을 Tool로 정의하면, LLM은 자연어 명령을 통해 해당 Tool을 사용할 수 있습니다.

Tool은 다음과 같은 경우에 유용합니다:

- LLM이 직접 처리할 수 없는 외부 동작이 필요한 경우
- 실제 작업 처리(ex. 요약, 번역, 파일 처리 등)
- 멀티에이전트 구성 시 모듈화된 작업 수행

---

## 📐 2. Tool 정의 방식

LangChain에서 Tool을 정의하는 방법은 크게 두 가지입니다:

### ✅ (1) 클래식 방식 - `Tool` 객체 생성

```python
from langchain.tools import Tool

def say_hello(name: str) -> str:
    return f"Hello, {name}!"

hello_tool = Tool(
    name="HelloTool",
    description="사용자 이름을 받아 인사하는 도구",
    func=say_hello
)
```

### ✅ (2) 데코레이터 방식 - `@tool` 사용 (간결하고 가독성 좋음)

```python
from langchain.tools import tool

@tool
def say_hello(name: str) -> str:
    """사용자 이름을 받아 인사하는 도구"""
    return f"Hello, {name}!"
```

- 데코레이터는 내부적으로 `Tool` 객체를 생성합니다.
- 함수 설명 문자열(도큐스트링)이 description 역할을 함.

---

## 🏗 3. 고급 사용: `args_schema` 지정하기

복잡한 입력값 구조가 필요한 경우, `Pydantic` 모델을 활용할 수 있습니다.

```python
from langchain.tools import tool
from pydantic import BaseModel

class CalcInput(BaseModel):
    a: int
    b: int

@tool(args_schema=CalcInput)
def add_numbers(a: int, b: int) -> int:
    """두 숫자를 더합니다."""
    return a + b
```

이렇게 하면 LLM은 어떤 입력값이 필요한지 명확하게 이해하고 정확하게 Tool을 사용할 수 있습니다.

---

## ⚙ 4. Agent와 함께 사용하기

Tool은 `initialize_agent()`를 통해 LLM과 함께 사용할 수 있습니다.

```python
from langchain.chat_models import ChatOpenAI
from langchain.agents import initialize_agent, AgentType

tools = [hello_tool, add_numbers]
llm = ChatOpenAI(model_name="gpt-3.5-turbo")

agent = initialize_agent(tools, llm, agent=AgentType.ZERO_SHOT_REACT_DESCRIPTION, verbose=True)

response = agent.run("10과 20을 더해줘")
print(response)
```

LLM은 description을 참고해 적절한 Tool을 선택하고, 자동으로 실행합니다.

---

## 📂 5. Tool 관리 구조 예시

```bash
project/
├── tools/
│   ├── __init__.py         # tool 모음 관리
│   ├── greeting_tools.py   # 인사 관련 도구들
│   ├── math_tools.py       # 계산 관련 도구들
│   └── api_tools.py        # 외부 API 관련 도구들
```

**tools/**init**.py** 예시:

```python
from .greeting_tools import say_hello
from .math_tools import add_numbers

all_tools = [say_hello, add_numbers]
```

---

## ✅ 6. Tool 정의 시 가이드라인

| 항목              | 가이드                                                     |
| ----------------- | ---------------------------------------------------------- |
| 이름(name)        | 명확하고 짧게 (예: "SearchTool")                           |
| 설명(description) | LLM이 이해할 수 있도록 자연어로 구체적으로 작성            |
| 인자(args_schema) | 가능하면 Pydantic 모델 사용하여 명시적으로 정의            |
| 함수(func)        | 순수 함수로 작성 (입력 → 출력), 외부 상태 의존 최소화      |
| 테스트            | 단위 테스트 작성 권장 (Tool 단독 실행 가능하므로 쉽습니다) |

---

## 📎 7. 참고 사항

- Tool을 너무 많이 등록하면 LLM이 선택에 혼란을 느낄 수 있습니다 → 주요 기능 중심으로 관리
- Tool 간 **입력/출력 포맷을 일관성 있게 유지** → 파이프라인화 가능
- `args_schema`가 있으면 LLM의 사용 정확도가 높아짐
- 데코레이터 방식은 간단하지만, 복잡한 경우는 클래식 방식도 고려

---
