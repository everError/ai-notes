# LangChain Expression Language (LCEL)

## LCEL이란?
**LangChain Expression Language**는 LangChain 컴포넌트를 조합하여 복잡한 작업 흐름을 구성할 수 있게 해주는 도구이자 문법 체계입니다. 이를 통해 사용자는 간단한 작업에서부터 복잡한 파이프라인까지 유연하게 설정할 수 있습니다.

---

## 주요 특징
1. **유연한 구성**
   - 다양한 LangChain 컴포넌트를 조합하여 작업 흐름을 설계할 수 있음.
2. **표준화된 문법**
   - 직관적인 문법 체계를 제공하여 작업 설정을 단순화.
3. **확장성**
   - 커스텀 컴포넌트 및 연산자를 추가하여 고유한 작업 흐름을 구성 가능.

---

## 동작 방식
1. **프롬프트 작성**
   - 템플릿 기반 또는 문자열 형태로 프롬프트를 구성.
2. **LLM과 연결**
   - 구성된 프롬프트를 LLM에 제공하여 작업을 수행.
3. **연산자 사용**
   - `|` (pipe) 연산자를 활용하여 컴포넌트를 체인 형태로 연결.

### 예제
```python
from langchain.prompts import PromptTemplate
from langchain.chains import LLMChain
from langchain.llms import OpenAI

# 프롬프트 템플릿 생성
prompt = PromptTemplate(template="Translate the following text to French: {text}", input_variables=["text"])

# LLM 연결
llm = OpenAI(temperature=0.7)
chain = LLMChain(llm=llm, prompt=prompt)

# 실행
output = chain.run({"text": "Hello, how are you?"})
print(output)  # 실행 결과 출력
```
---

## 주요 구성 요소

### 1. **프롬프트 (Prompt)**
- **템플릿 기반**으로 동작하며, 동적 입력을 지원.
- 예: `Translate the following text to French: {text}`

#### Prompt 구성 방법
1. **정적 문자열 사용**: 단순히 고정된 프롬프트를 작성.
2. **동적 템플릿**: 변수와 함께 동작하는 템플릿 사용. LangChain의 `PromptTemplate` 클래스를 활용.

### 2. **연산자 (Operators)**
- **파이프 연산자 (`|`)**
  - LangChain에서 컴포넌트를 연결하는 데 사용.
  - 입력 데이터를 가공하거나, 여러 단계를 연결하여 복잡한 작업 수행.

#### 파이프 연산자 예제
```python
# 데이터 가공
processed_data = data | preprocess | embed | retrieve | summarize
# 각 단계별로 데이터를 처리하고 마지막 결과를 summarize 단계에서 요약
```
- `preprocess`: 데이터 전처리
- `embed`: 데이터를 벡터화
- `retrieve`: 벡터화된 데이터를 검색
- `summarize`: 결과 요약

### 3. **LLM (Large Language Models)**
- 텍스트 생성의 핵심 엔진으로 사용.
- OpenAI, Anthropic 등 다양한 모델을 지원.

### 4. **Output Parser**
**Output Parser**는 LLM의 출력을 원하는 형식으로 변환하는 데 사용됩니다. 구조화된 데이터를 제공해 LLM의 출력을 더 유연하게 사용할 수 있도록 도와줍니다.

#### 주요 파서 유형 및 예제

1. **StrOutputParser**: 문자형 파싱
   - 비정형 출력을 문자열로 반환합니다.
   ```python
   from langchain_core.output_parsers import StrOutputParser
   chain = prompt | llm | StrOutputParser()  # 출력 결과를 문자열로 반환
   ```

2. **JsonOutputParser**: JSON 파싱
   - JSON 형식의 출력을 파싱하여 Python 딕셔너리로 변환합니다.

3. **PydanticOutputParser**: Pydantic 객체 파싱
   - 출력 데이터를 Pydantic 모델에 맞게 파싱하고 검증합니다.

#### 사용 사례 및 장점
- **주요 사용 사례**
   - 구조화된 데이터 추출
   - 형식화된 응답 생성
   - LLM 출력의 일관성 보장
   - 복잡한 객체 생성 및 검증
- **장점**
   - 출력 형식의 표준화
   - 에러 처리 및 검증 간소화
   - 다양한 데이터 형식 지원
   - 파이프라인 구성의 유연성 증가

---

## Runnable
**Runnable**은 LCEL의 핵심 개념으로, 다양한 컴포넌트를 연결하고 실행하는 인터페이스입니다. **Runnable**을 활용하면 서로 다른 작업을 결합하고 실행할 수 있으며, 주로 데이터를 전달하고 변환하는 역할을 합니다.

- **역할**: Runnable은 데이터 흐름을 제어하며 컴포넌트 간의 상호작용을 가능하게 합니다.
- **유용한 상황**:
   - 다중 작업의 동시 실행 (병렬 처리)
   - 데이터 전달 및 변환 파이프라인 구현
   - 개별 컴포넌트의 독립 실행 및 테스트

### 1. RunnableParallel: 병렬 실행
- 여러 작업을 동시에 실행하고 결과를 딕셔너리로 반환합니다.
- **작업 실행 순서 및 동기화**: RunnableParallel은 내부적으로 독립된 작업을 동시에 실행하므로, 작업 간 순서가 보장되지 않지만 각 결과를 키-값 쌍으로 반환합니다.
- **성능 주의사항**: 
   - 작업의 수가 많아지면 메모리와 CPU 사용량이 증가할 수 있음.
   - 네트워크 또는 외부 API 호출이 포함된 작업의 경우, 병렬 실행이 네트워크 병목을 초래할 수 있으므로 적절히 관리해야 합니다.

#### 예제 코드
```python
from langchain_core.runnables import RunnableParallel
from operator import itemgetter

# 병렬로 실행할 작업 설정
parallel_chain = RunnableParallel({
    "topic": question_chain,        # 질문 처리 체인
    "language": language_chain,    # 언어 감지 체인
    "question": itemgetter("question")  # 특정 데이터 가져오기
})

# 실행 결과를 딕셔너리 형태로 반환
output = parallel_chain.invoke({"question": "What is LangChain?"})
print(output)  # {'topic': ..., 'language': ..., 'question': ...}
```

#### 구조 다이어그램
- 입력 → **RunnableParallel** → topic_chain, language_chain, itemgetter → 결과 딕셔너리 → 출력

### 2. RunnablePassthrough & RunnableLambda: 데이터 전달 및 변환
- **RunnablePassthrough**: 입력을 그대로 전달. 주로 데이터 흐름을 확인하거나 디버깅에 유용합니다.
- **RunnableLambda**: 함수를 활용해 입력 데이터를 변환. 데이터 전처리, 변형 작업 등 유연한 데이터 처리가 가능합니다.

#### 예제 코드
```python
from langchain_core.runnables import RunnablePassthrough, RunnableLambda

# 데이터 전달 및 변환 설정
chain = RunnableParallel({
    "passed": RunnablePassthrough(),            # 데이터를 그대로 전달
    "modified": RunnableLambda(lambda x: x.upper())  # 데이터를 대문자로 변환
})

# 실행 결과
output = chain.invoke("hello")
print(output)  # {'passed': 'hello', 'modified': 'HELLO'}
```

#### 주요 사용 사례
- **RunnablePassthrough**
   - 데이터 흐름 확인 및 디버깅.
   - 중간 처리 없이 원본 데이터 전달.
- **RunnableLambda**
   - 데이터 전처리 및 가공.
   - 사용자 정의 함수를 활용해 복잡한 변환 로직 구현.

---

## Chat Completion Methods
**Chat Completion Methods**는 입력 데이터를 다양한 방식으로 처리하고 출력을 반환합니다.

1. **invoke**: 단일 입력에 대해 완성된 출력을 반환합니다.
   - **성능**: 가장 기본적인 메서드로, 빠르고 안정적인 결과를 제공합니다.
   - **사용 사례**: 단일 요청에 대한 처리.

2. **stream**: 입력에 대한 응답을 실시간 스트림으로 전달합니다.
   - **성능**: 결과를 점진적으로 반환하기 때문에 응답 시간이 중요한 경우에 유리합니다.
   - **사용 사례**: 실시간 채팅, 대화형 애플리케이션.

3. **batch**: 여러 입력을 동시에 처리하고 응답을 배치 단위로 반환합니다.
   - **성능**: 대량의 데이터를 효율적으로 처리할 수 있지만, 메모리 사용량이 증가할 수 있습니다.
   - **사용 사례**: 다수의 요청을 병렬 처리하여 대기 시간 단축.

#### 예제
```python
# 단일 입력 처리
output = chain.invoke(input_data)
print(output)  # 완성된 출력 반환

# 실시간 스트림 처리
for chunk in chain.stream(input_data):
    print(chunk)  # 스트림 결과를 점진적으로 출력

# 배치 단위 처리
batch_output = chain.batch([input_data1, input_data2])
print(batch_output)  # 입력 리스트에 대한 결과 반환
```
---

## 활용 사례
1. **다중 작업 체인 구성**
   - 예: 번역 → 요약 → 키워드 추출.
2. **프롬프트 설계 및 테스트**
   - 다양한 템플릿을 테스트하며 최적화된 프롬프트 설계 가능.
3. **복잡한 파이프라인 구현**
   - 여러 컴포넌트를 조합하여 데이터 전처리, 검색, 생성 등의 작업 흐름을 자동화.
4. **출력 데이터 가공**
   - Output Parser를 사용해 LLM 출력을 원하는 형태로 변환.
5. **병렬 실행**
   - RunnableParallel을 활용하여 작업을 동시에 실행.
6. **데이터 변환**
   - RunnablePassthrough 및 RunnableLambda를 사용하여 데이터를 전처리 및 변환.

---

## 요약
LangChain Expression Language (LCEL)는 다양한 LangChain 컴포넌트를 결합해 복잡한 작업을 효율적으로 구현할 수 있는 도구입니다. 주요 기능은 **프롬프트 관리**, **데이터 변환**, **병렬 실행** 및 **출력 가공** 등으로 요약되며, 이를 통해 사용자는 유연하고 강력한 LLM 기반 애플리케이션을 설계할 수 있습니다.

---

LangChain Expression Language는 직관적이고 유연한 문법 체계를 통해 복잡한 작업을 간단히 구성할 수 있도록 지원하며, LangChain의 기능을 극대화하는 데 중요한 역할을 합니다.
