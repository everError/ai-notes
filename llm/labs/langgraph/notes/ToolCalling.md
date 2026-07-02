# Tool Calling 개요

## 1. Tool Calling이란?

Tool Calling은 LLM(Language Model)이 **외부 도구(API, DB, 계산기 등)를 필요할 때만 호출**하여 보다 정교한 응답을 생성하는 기능입니다. OpenAI의 GPT-4 Turbo에서 Function Calling의 발전된 개념으로 도입되었습니다.

## 2. Tool Calling의 주요 특징

- **모델이 필요할 때만 특정 도구를 호출**하여 데이터를 가져옴.
- **함수를 강제로 호출하지 않음**, 대신 모델이 상황을 판단하여 적절한 경우에만 사용.
- **다양한 API, DB, 계산기 등을 연동**하여 실시간 데이터 활용 가능.
- 기존 Function Calling 대비 **더 유연한 동작 방식**을 가짐.

## 3. Tool Calling과 Function Calling의 차이점

| 비교 항목          | Function Calling                 | Tool Calling                            |
| ------------------ | -------------------------------- | --------------------------------------- |
| **도구 실행 방식** | 개발자가 특정 함수를 강제로 실행 | 모델이 필요할 때만 적절한 도구를 선택   |
| **강제성 여부**    | 반드시 함수 실행 후 응답         | 필요하면 실행, 필요 없으면 자체 응답    |
| **유연성**         | 지정된 함수만 실행 가능          | 여러 개의 도구 중 적절한 것만 선택 가능 |
| **실제 활용 예시** | 데이터베이스 조회가 필수인 경우  | 검색, 뉴스 요약, 실시간 데이터 조회 등  |

## 4. Tool Calling 예제

### ✅ Python을 이용한 Tool Calling 예제

```python
import openai

client = openai.OpenAI()

response = client.chat.completions.create(
    model="gpt-4-turbo",
    messages=[{"role": "user", "content": "오늘 뉴욕 날씨 알려줘."}],
    tools=[
        {
            "type": "function",
            "function": {
                "name": "get_weather",
                "description": "주어진 도시의 날씨 정보를 조회합니다.",
                "parameters": {
                    "city": "string"
                }
            }
        }
    ]
)

print(response)
```

- 모델은 질문을 보고 **날씨 데이터가 필요하면 `get_weather` 함수를 호출**하고, 필요 없으면 자체적으로 응답.
- Function Calling과 달리, **API를 호출할 필요가 없으면 실행하지 않음**.

## 5. Tool Calling이 RAG에서 활용되는 방식

RAG(Retrieval-Augmented Generation) 시스템에서 Tool Calling을 활용하면 **벡터 검색 기반의 문서 검색뿐만 아니라 실시간 데이터 조회 기능**을 추가할 수 있습니다.

### ✅ RAG + Tool Calling 활용 예시

1. **실시간 데이터 활용**: 벡터 검색 후 최신 뉴스, 주식 가격, 날씨 등 API를 호출하여 정보를 업데이트.
2. **계산 및 데이터 가공**: 검색된 데이터를 기반으로 수치 계산, 단위 변환 등을 수행.
3. **혼합형 질의 응답**: 벡터 검색 결과와 API 데이터를 결합하여 보다 풍부한 답변 제공.

## 6. 결론

Tool Calling은 Function Calling보다 **더 유연하고 동적인 방식으로 모델이 API, DB 등을 호출**할 수 있도록 설계되었습니다. 이를 활용하면 기존의 단순한 LLM 응답을 넘어 **실시간 데이터를 반영한 강력한 RAG 시스템**을 구축할 수 있습니다. 🚀
