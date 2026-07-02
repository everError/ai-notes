# RAG Search Engines: Features and Details

RAG(정보 검색 증강 생성) 검색기는 현대 NLP 시스템에서 중요한 정보 검색 방식을 제공합니다. 이 문서에서는 RAG 검색기의 핵심 방식인 **시멘틱 검색(Semantic Search)**, **키워드 검색(Keyword Search)**, 그리고 **하이브리드 검색(Hybrid Search)**에 대해 설명하고, LangChain을 활용한 방법까지 다룹니다.

---

## 1. 시멘틱 검색 (Semantic Search)
### 1.1 개요
시멘틱 검색은 쿼리의 단순한 키워드 매칭을 넘어, 사용자의 의도와 맥락을 이해하여 검색을 수행합니다. 텍스트를 벡터 공간으로 매핑하고, 벡터 간의 유사성을 계산하여 관련성 높은 결과를 반환합니다.

### 1.2 작동 원리
1. **문서 임베딩**:
   - 데이터베이스의 각 문서를 벡터 표현으로 변환합니다. 사전 훈련된 언어 모델(예: BERT, GPT)을 사용해 문서의 의미를 캡처합니다.

2. **쿼리 임베딩**:
   - 사용자의 검색 쿼리를 동일한 방식으로 벡터화합니다.

3. **유사도 계산**:
   - 벡터 간의 유사도를 계산하며, 주로 **코사인 유사도** 또는 **유클리디안 거리**를 사용합니다.

4. **결과 반환**:
   - 유사도가 가장 높은 문서가 검색 결과로 반환됩니다.

### 1.3 주요 특징
- **맥락적 이해**: 동의어, 관련 용어 등을 고려.
- **고급 매칭**: 쿼리의 의도와 의미를 캡처.
- **강건성**: 불완전하거나 모호한 쿼리에도 높은 정확도.

### 1.4 심화 내용
- **코사인 유사도**: 벡터 방향의 유사성을 기반으로 문서와 쿼리 간의 연관성을 측정합니다.
- **임베딩 모델 선택**: BERT, GPT, OpenAI 등 다양한 임베딩 모델이 제공되며, 작업 목적에 따라 적합한 모델을 선택해야 합니다.

### 1.5 LangChain을 활용한 시멘틱 검색 예제
```python
from langchain.vectorstores import FAISS
from langchain.embeddings import OpenAIEmbeddings
from langchain.chains import RetrievalQA

# 벡터 저장소 초기화
embedding_model = OpenAIEmbeddings()
vectorstore = FAISS.from_documents(documents, embedding_model)

# 검색 체인 생성
retriever = vectorstore.as_retriever()
qa_chain = RetrievalQA.from_chain_type(llm=llm, retriever=retriever)

# 쿼리 실행
query = "What is the capital of France?"
result = qa_chain.run(query)
print(result)
```

---

## 2. 키워드 검색 (Keyword Search)
### 2.1 개요
키워드 검색은 사용자가 입력한 특정 단어나 구문을 문서에서 직접 찾는 전통적인 검색 방식입니다. 이는 간단하면서도 효과적인 검색 방식을 제공합니다.

### 2.2 작동 원리
1. **단어 매칭**:
   - 입력된 키워드를 기반으로 문서를 검색.

2. **BM25 알고리즘**:
   - 키워드의 빈도와 문서 길이를 조정하여 더 정교한 검색 결과를 제공.

3. **결과 반환**:
   - 가장 높은 점수를 가진 문서가 반환됩니다.

### 2.3 주요 특징
- **단순성**: 구현 및 계산 비용이 낮음.
- **정확성**: 특정 키워드 검색에 적합.
- **빠른 속도**: 대규모 데이터에서도 효율적.

### 2.4 심화 내용
- **BM25 알고리즘의 특징**:
  - TF(용어 빈도): 단어가 문서에 나타나는 횟수에 따라 점수를 부여합니다.
  - IDF(역문서 빈도): 자주 등장하지 않는 단어에 더 높은 가중치를 부여.
  - 문서 길이 정규화: 긴 문서와 짧은 문서 간의 공정성을 확보.

### 2.5 LangChain을 활용한 키워드 검색 예제
```python
from langchain.retrievers import BM25Retriever
from langchain.chains import RetrievalQA

# 문서 리스트 준비
documents = ["Python is a programming language.", "France is in Europe."]

# BM25 검색기 생성
retriever = BM25Retriever.from_documents(documents)
qa_chain = RetrievalQA.from_chain_type(llm=llm, retriever=retriever)

# 쿼리 실행
query = "Where is France?"
result = qa_chain.run(query)
print(result)
```

---

## 3. 하이브리드 검색 (Hybrid Search)
### 3.1 개요
하이브리드 검색은 의미 기반 검색과 키워드 검색을 결합하여 각각의 장점을 활용하는 검색 방식입니다. 이를 통해 다양한 유형의 검색 쿼리에 대응할 수 있습니다.

### 3.2 작동 원리
1. **다중 검색기 통합**:
   - 의미 기반 검색기와 키워드 검색기의 결과를 결합.

2. **결과 집계**:
   - Reciprocal Rank Fusion (RRF) 알고리즘을 사용하여 최종 순위를 결정.

3. **최종 결과 반환**:
   - 두 방식의 결과를 혼합해 최상의 문서를 반환.

### 3.3 주요 특징
- **정확성 향상**: 키워드와 의미의 조합으로 더 높은 정확도 제공.
- **다양성 확보**: 다양한 관점에서 검색 결과 제공.
- **유연성**: 사용자 요구에 맞는 검색 결과 반환.

### 3.4 심화 내용
- **RRF 알고리즘**:
  - 검색기별 결과 순위를 고려하여 가중치를 부여한 점수 계산.
  - 공식: `score = Σ 1 / (rank + k)` (여기서 k는 조정 상수).
- **응용 분야**: 고객 서비스 챗봇, 전자상거래 추천 시스템 등에서 유용.

### 3.5 LangChain을 활용한 하이브리드 검색 예제
```python
from langchain.retrievers import BM25Retriever, VectorStoreRetriever
from langchain.retrievers.ensemble import EnsembleRetriever
from langchain.vectorstores import FAISS

# 벡터 저장소 및 BM25 초기화
embedding_model = OpenAIEmbeddings()
vectorstore = FAISS.from_documents(documents, embedding_model)
vector_retriever = vectorstore.as_retriever()
keyword_retriever = BM25Retriever.from_documents(documents)

# 하이브리드 검색기 생성
hybrid_retriever = EnsembleRetriever(retrievers=[vector_retriever, keyword_retriever])

# 쿼리 실행
query = "Tell me about Europe."
results = hybrid_retriever.get_relevant_documents(query)
for result in results:
    print(result.content)
```

---

## 검색 방식 비교표

| 검색 방식       | 주요 특징                                   | 장점                                  | 단점                              |
|----------------|------------------------------------------|-------------------------------------|----------------------------------|
| 시멘틱 검색      | 의도와 맥락을 이해하여 벡터 간 유사성 계산         | 맥락적 이해, 높은 정확도             | 계산 비용 높음, 모델 품질 의존         |
| 키워드 검색      | 특정 단어나 구문을 매칭                     | 빠른 속도, 단순 구현                 | 의미적 유사성 반영 불가, 동의어 처리 부족 |
| 하이브리드 검색   | 시멘틱 + 키워드 검색 결합                     | 정확성과 다양성 제공                 | 복잡한 구현, 결과 집계 알고리즘 필요      |

---

RAG 검색기는 고급 정보 검색 기능을 통해 다양한 응용 분야에서 강력한 성능을 제공합니다. LangChain을 활용하면 이러한 검색 방식을 손쉽게 구현할 수 있으며, 각각의 방법은 사용 사례에 따라 최적의 성능을 발휘합니다.
