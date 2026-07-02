# Advanced Search Techniques in RAG

이 문서는 RAG(정보 검색 증강 생성)에서 사용되는 다양한 검색 기법에 대해 다룹니다. 각각의 기법은 특정한 사용 사례와 요구 사항에 맞게 설계되었으며, 검색 성능과 효율성을 극대화합니다.

---

## 1. Semantic Search: 의미 기반 검색

### 1.1 개요

Semantic Search는 쿼리의 문자 그대로의 의미를 넘어, 의도와 맥락을 이해하여 검색을 수행합니다. 텍스트를 벡터 공간에 매핑하고, 벡터 간의 유사성을 계산하여 관련성 높은 결과를 반환합니다.

### 1.2 작동 원리

1. **문서 임베딩**:

   - 모든 문서를 벡터로 변환하여 벡터 저장소에 저장.
   - 사전 훈련된 언어 모델(예: BERT, GPT)을 사용하여 임베딩 생성.

2. **쿼리 임베딩**:

   - 사용자의 검색 쿼리를 동일한 방식으로 벡터로 변환.

3. **유사도 계산**:

   - 쿼리 벡터와 문서 벡터 간의 유사도를 계산.
   - 주로 **코사인 유사도**, **유클리디안 거리**, 또는 **Jaccard 유사도**를 사용.

4. **결과 반환**:
   - 가장 유사한 문서들을 검색 결과로 반환.

### 1.3 장점

- 동의어, 관련어 등을 고려한 더 정확한 검색 결과 제공.
- 언어의 뉘앙스와 맥락을 이해하여 검색 수행.
- 키워드 기반 검색에서 놓칠 수 있는 관련 정보를 검색 가능.

### 1.4 한계

- 계산 비용이 높을 수 있음 (특히 대규모 데이터셋에서).
- 임베딩 모델의 품질에 크게 의존함.
- 매우 특정한 키워드 검색에서는 성능이 떨어질 수 있음.

### 1.5 임베딩 모델 비교

| **모델**                  | **장점**                                  | **단점**                                     |
| ------------------------- | ----------------------------------------- | -------------------------------------------- |
| **BERT**                  | 문맥 이해에 강함, 다양한 NLP 작업 지원    | 처리 속도가 느릴 수 있음                     |
| **GPT**                   | 언어 생성과 유연한 작업에 적합            | 비용이 높고 대규모 데이터에 부적합           |
| **Sentence Transformers** | 문장 레벨 임베딩에 최적화, 빠른 속도 제공 | 문장 이외의 텍스트 처리에서 제한적일 수 있음 |

### 1.6 LangChain을 활용한 실습 예제

```python
from langchain.embeddings import OpenAIEmbeddings
from langchain.vectorstores import FAISS

# 벡터 저장소 초기화
embedding_model = OpenAIEmbeddings()
vectorstore = FAISS.from_documents(documents, embedding_model)

# 검색 수행
query = "What is the capital of France?"
results = vectorstore.similarity_search(query, k=5)

for result in results:
    print(f"Document: {result['content']}")
```

---

## 2. Keyword Search: 키워드 기반 검색

### 2.1 개요

Keyword Search는 사용자가 입력한 특정 단어나 구문을 문서 내에서 직접 찾는 전통적인 방식입니다. 단순하지만 효과적인 검색 방법입니다.

### 2.2 BM25 알고리즘

BM25는 키워드 검색을 더욱 효과적으로 만드는 랭킹 함수로, TF-IDF의 한계를 보완한 방식입니다.

#### 주요 특징:

1. **용어 빈도 (TF)**:
   - 단어가 문서 내에 자주 등장할수록 점수가 높아지지만, 증가율이 감소하여 과도한 반복의 영향을 제한.
2. **문서 길이 정규화**:
   - 긴 문서에서 용어가 더 자주 나타날 가능성을 고려하여 점수를 조정.

### 2.3 한계 해결 방안

- **키워드 확장 기법**:
  - 동의어 사전 활용.
  - 단어 스테밍 및 표제어 추출 적용.
- **고급 알고리즘 적용**:
  - BM25+ 및 BM25L과 같은 고급 랭킹 함수 활용.

### 2.4 LangChain을 활용한 실습 예제

```python
from langchain.retrievers import BM25Retriever

# BM25 검색기 생성
documents = ["Python is a programming language.", "France is in Europe."]
retriever = BM25Retriever.from_documents(documents)

# 검색 수행
query = "Where is France?"
results = retriever.get_relevant_documents(query)

for result in results:
    print(f"Document: {result['content']}")
```

---

## 3. Hybrid Search: 하이브리드 검색

### 3.1 개요

하이브리드 검색은 의미 기반 검색과 키워드 기반 검색을 결합한 방식으로, 두 방식의 장점을 모두 활용합니다.

### 3.2 작동 원리

1. **다중 검색기 통합**:
   - 의미 기반 검색기와 키워드 검색기의 결과를 결합.
2. **결과 집계**:
   - Reciprocal Rank Fusion (RRF) 알고리즘을 사용하여 최종 순위를 결정.
   - 공식: `score = Σ 1 / (rank + k)`, 여기서 `rank`는 검색 순위, `k`는 조정 상수.

### 3.3 장점

- 정확성과 다양성을 동시에 제공.
- 다양한 관점에서 검색 결과 제공 가능.
- 한 방식의 약점을 다른 방식으로 보완.

### 3.4 성능 최적화 방법

- 검색 결과에 가중치를 부여하여 특정 검색 방식을 더 강조.
- 검색 순서를 조정하여 빠른 결과 반환.

### 3.5 LangChain을 활용한 실습 예제

```python
from langchain.vectorstores import FAISS
from langchain.retrievers import BM25Retriever
from langchain.retrievers.ensemble import EnsembleRetriever

# 벡터 저장소 및 키워드 검색기 초기화
embedding_model = OpenAIEmbeddings()
vectorstore = FAISS.from_documents(documents, embedding_model)
vector_retriever = vectorstore.as_retriever()
keyword_retriever = BM25Retriever.from_documents(documents)

# 하이브리드 검색기 생성
hybrid_retriever = EnsembleRetriever(retrievers=[vector_retriever, keyword_retriever])

# 검색 수행
query = "Tell me about Europe."
results = hybrid_retriever.get_relevant_documents(query)

for result in results:
    print(result['content'])
```

---

## 4. Ensemble Search: 앙상블 검색

### 4.1 개요

Ensemble Search는 여러 검색 방법의 결과를 결합하여 더 정교하고 정확한 결과를 제공합니다. 하이브리드 검색의 확장형으로 볼 수 있습니다.

### 4.2 작동 방식

1. 여러 검색기의 결과를 동시에 생성.
2. 결과를 RRF와 같은 알고리즘으로 정렬.
3. 최종 결합된 결과를 사용자에게 반환.

### 4.3 실제 사용 사례

- 전자상거래에서 제품 추천 시스템 구축.
- 학술 논문 검색 엔진에서 다양한 검색 기법 통합.

---

## 요약

검색 방식은 애플리케이션의 요구 사항에 따라 선택해야 합니다. **Semantic Search**는 의미적 검색에 강점이 있고, **Keyword Search**는 빠르고 단순하며, **Hybrid Search**와 **Ensemble Search**는 복합적인 검색 요구를 충족합니다. LangChain은 이러한 검색 방식을 쉽게 구현할 수 있는 도구를 제공하며, 최적의 검색 성능을 발휘할 수 있습니다.
