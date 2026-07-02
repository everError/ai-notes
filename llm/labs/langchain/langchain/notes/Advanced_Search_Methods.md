# 고급 검색 기법

## 1. 쿼리(Query) 확장

사용자의 원래 쿼리를 확장하여 더 관련성 있는 결과를 얻는 기술

### 1.1 Multi Query (다중 쿼리 확장)

- 하나의 원본 쿼리로부터 여러 개의 관련 쿼리를 생성하여 검색의 범위와 정확도를 높이는 방법

#### **작동 원리**

1. Retriever에 쿼리를 생성할 LLM(Large Language Model)을 지정
2. 지정된 LLM이 원본 쿼리를 분석하고, 다양한 관점에서 여러 개의 관련 쿼리를 생성
3. 생성된 모든 쿼리를 사용하여 검색을 수행
4. 각 검색 결과를 취합하여 가장 적합한 문서를 반환

#### **장점**

- **검색의 다양성 증가**: 하나의 질문에서 다각도의 답변을 확보 가능
- **누락된 정보 감소**: 하나의 쿼리로 찾기 어려운 정보도 포착 가능
- **컨텍스트 확장**: 원본 질문의 맥락을 더 넓게 고려하여 검색 결과를 강화

#### **예제 코드**

```python
from langchain.chains import MultiQueryRetriever
from langchain.chat_models import ChatOpenAI
from langchain.vectorstores import FAISS

llm = ChatOpenAI(model_name="gpt-4")
vectorstore = FAISS.load_local("path_to_vectorstore")
retriever = MultiQueryRetriever(llm=llm, retriever=vectorstore.as_retriever())

query = "세계에서 가장 큰 강은?"
results = retriever.get_relevant_documents(query)
print(results)
```

### 1.2 Decomposition (질문 분해)

- 복잡한 질문을 여러 개의 간단한 하위 질문으로 분해하여 처리하는 방식

#### **작동 원리**

1. LLM을 활용하여 입력된 복잡한 질문을 여러 개의 하위 질문으로 변환
2. 각 하위 질문에 대해 개별 검색 수행
3. 검색된 답변을 조합하여 최종 응답을 생성

#### **장점**

- **복잡한 문제 해결**: 단계별 접근을 통해 복잡한 질문도 효과적으로 다룸
- **정확성 향상**: 하위 질문을 개별적으로 처리하여 오류 가능성을 줄임
- **설명 가능성**: 각 단계의 결과를 기록하여 검색 프로세스를 추적 가능

#### **예제 코드**

```python
from langchain.chains import DecomposableRetriever

decomposer = DecomposableRetriever(llm=llm, retriever=vectorstore.as_retriever())
query = "로마제국의 몰락 이유와 그 영향은?"
results = decomposer.get_relevant_documents(query)
print(results)
```

---

## 2. Re-rank (재순위화)

초기 검색 결과를 다시 순위를 매기는 과정으로, 더 정확하고 관련성 높은 결과를 상위로 올리는 것이 목적

### 2.1 Cross Encoder Reranker 사용

- Cross Encoder는 쿼리와 문서 쌍의 관련성을 직접 평가하는 강력한 모델
- 쿼리와 문서 간의 직접적인 관련성을 평가하여 높은 정확도를 제공

#### **작동 원리**

1. 쿼리와 각 문서를 쌍으로 입력
2. 모델이 이 쌍을 동시에 처리하여 관련성 점수를 출력
3. 점수에 따라 문서들을 재정렬

#### **장점**

- **높은 정확도**: 문서와 쿼리를 함께 평가하여 최적의 결과 제공
- **컨텍스트 이해**: 쿼리와 문서 간의 복잡한 관계를 분석하여 최적의 순위 결정

### 2.2 LLM Reranker 사용

- LLM을 활용한 재순위화는 모델의 추론 능력을 활용하여 검색 결과를 개선
- 단순한 키워드 매칭을 넘어 문맥을 고려한 순위 조정 가능

#### **작동 원리**

1. 초기 검색 결과를 LLM에 입력으로 제공
2. LLM이 각 문서의 관련성을 평가하고 순위를 조정
3. 조정된 순위에 따라 문서를 재정렬

#### **장점**

- **맥락 이해**: LLM의 언어 이해 능력을 활용하여 문맥적 적합성을 고려
- **유연성**: 다양한 기준으로 재순위화 가능
- **설명 가능성**: LLM이 순위 조정 이유를 설명할 수 있음

---

## 3. Contextual Compression (맥락 압축)

- 검색된 문서를 쿼리의 맥락에 맞게 압축하거나 필터링하는 기법
- 긴 문서에서 핵심 정보만을 효과적으로 활용하여 LLM에 전달하는 정보의 양을 줄임

### **3.1 LLMChainFilter**

- LLM을 사용하여 검색된 문서 중 관련성 높은 문서만을 선택

#### **작동 원리**

1. 각 문서에 대해 LLM에게 관련성을 평가하도록 요청
2. LLM의 판단에 따라 문서를 유지하거나 제거

### **3.2 LLMChainExtractor**

- LLM을 사용하여 각 문서에서 쿼리와 관련된 부분만을 추출

#### **작동 원리**

1. 각 문서를 LLM에 입력으로 제공
2. LLM이 쿼리와 관련된 내용만을 추출하여 요약

### **3.3 EmbeddingsFilter**

- 임베딩을 사용하여 문서를 필터링
- LLM을 사용하지 않아 빠르고 비용 효율적

#### **작동 원리**

1. 쿼리와 문서를 벡터로 임베딩
2. 쿼리 벡터와 문서 벡터 간의 유사도를 계산
3. 유사도가 임계값을 넘는 문서만 유지

### **3.4 DocumentCompressorPipeline**

- 여러 압축 기법을 순차적으로 적용하는 파이프라인 방식

#### **작동 원리**

1. 여러 압축기를 순서대로 연결
2. 각 압축기의 출력이 다음 압축기의 입력으로 전달

#### **추가 개선 사항**

- **다중 필터링 기법 적용**: LLM 기반과 임베딩 기반을 병행하여 최적의 성능 도출
- **비용 대비 효율적인 압축 방식 선택**: 처리 속도와 정확도를 균형 있게 고려

---

이 문서는 고급 검색 기법을 정리한 것으로, 쿼리 확장, 재순위화, 맥락 압축 등의 기법을 활용하여 검색 성능을 향상시킬 수 있습니다. 각 기법의 적용은 도메인과 시스템의 요구 사항에 따라 선택적으로 사용될 수 있습니다.
