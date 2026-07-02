# PDF 기반 범용 분기형 RAG 검색 시스템 설계 가이드

이 문서는 PDF로 수집된 다양한 주제의 문서를 벡터DB로 전처리하고, 사용자 질의에 따라 **도메인별로 분기된 검색 흐름**을 구성한 후, LangChain의 도구 체계 및 LLM 기반 Agent에 통합하는 설계 방식을 정리합니다. 특히, **CrossEncoder 기반 재정렬(reranker)** 기법을 적용하여 검색 정밀도를 향상시키고, 다양한 분야의 문서 기반 RAG 검색 시스템을 일반화된 구조로 구현하는 데 중점을 둡니다. 학습과 재사용이 가능한 구조로 정리되어 있어, 다양한 주제에 응용 가능합니다.

---

## 🎯 시스템 구성 개요 (확장 설명)

| 단계 | 구성 요소                    | 설명                                                                                                                                     |
| ---- | ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| 1    | PDF 문서 로딩 및 텍스트 추출 | PyPDFLoader로 페이지 단위 문서를 추출하고, LangChain `Document` 형태로 변환합니다. 메타데이터(출처, 페이지 번호 포함)를 함께 저장합니다. |
| 2    | 텍스트 청크 분리             | RecursiveCharacterTextSplitter를 통해 문장을 일정 크기로 나눠 벡터화 효율을 높입니다.                                                    |
| 3    | 벡터화 및 저장               | Ollama 임베딩 모델(`bge-m3`)을 사용하여 문서 청크를 벡터로 변환하고 Chroma에 저장합니다.                                                 |
| 4    | 고도화 검색                  | Chroma에서 기본 검색 후 CrossEncoder를 통해 재정렬(reranking)하여 검색 품질을 높입니다.                                                  |
| 5    | 검색 도구화                  | 검색 기능을 LangChain `@tool` 데코레이터로 LLM Tool로 등록하여 자동 호출이 가능하게 만듭니다.                                            |
| 6    | 도메인 라우팅                | 쿼리 내용을 기반으로 적절한 검색 도구나 벡터DB로 분기하는 함수를 구성합니다. (선택)                                                      |
| 7    | 에이전트 구성                | LangGraph를 통해 검색 → 평가 → 재질의 → 응답 생성을 상태 기반 그래프로 구성합니다.                                                       |

---

## 📁 1. PDF 문서 로딩 및 콘텐츠 재구성

```python
# 역할: PDF 파일을 LangChain 문서 객체로 변환
from langchain_community.document_loaders import PyPDFLoader
from langchain_core.documents import Document

loader = PyPDFLoader(pdf_file)
pages = loader.load()

final_docs = []
for p in pages:
    metadata = {"source": pdf_file, "page": p.metadata.get("page", 0)}
    content = f"[출처: {pdf_file}]\n\n{p.page_content}"
    final_docs.append(Document(page_content=content, metadata=metadata))
```

---

## 📎 2. 텍스트 청크 분리

```python
# 역할: 긴 문서를 의미 단위로 분리하여 임베딩 처리 용이하게 구성
from langchain.text_splitter import RecursiveCharacterTextSplitter

splitter = RecursiveCharacterTextSplitter(chunk_size=500, chunk_overlap=50)
split_docs = splitter.split_documents(final_docs)
```

---

## 💾 3. 벡터 DB에 저장 (Chroma + Ollama 기반 임베딩)

```python
# 역할: 청크를 벡터로 변환 후 Chroma에 저장
from langchain_chroma import Chroma
from langchain_ollama import OllamaEmbeddings

embedding_model = OllamaEmbeddings(model="bge-m3")

vector_db = Chroma.from_documents(
    documents=split_docs,
    embedding=embedding_model,
    collection_name="generic_documents",
    persist_directory="./chroma_db"
)
```

---

## 🔍 4. CrossEncoder 기반 Reranker 구성

```python
# 역할: 단순 임베딩 유사도 검색 결과를 의미 기반으로 재정렬
from langchain.retrievers import ContextualCompressionRetriever
from langchain.retrievers.document_compressors import CrossEncoderReranker

reranker = CrossEncoderReranker(model="cross-encoder/ms-marco-MiniLM-L-6-v2", top_n=3)
retriever = ContextualCompressionRetriever(
    base_retriever=vector_db.as_retriever(search_kwargs={"k":5}),
    base_compressor=reranker
)
```

---

## 🛠️ 5. LangChain Tool로 검색 기능 등록

```python
# 역할: 검색 기능을 LangChain Tool로 등록하여 LLM이 tool_call을 통해 호출 가능하게 만듦
from langchain.tools import tool
from langchain_core.documents import Document

@tool
def generic_document_search(query: str) -> List[Document]:
    """범용 문서에서 검색을 수행합니다."""
    docs = retriever.invoke(query)
    return docs or [Document(page_content="관련 정보를 찾을 수 없습니다.")]
```

---

## 🔀 6. 다중 도메인 라우팅 예시 (선택)

```python
# 역할: 도메인에 따라 다른 retriever/tool을 선택할 수 있도록 분기 처리
from langchain.schema.runnable import RunnableLambda

def domain_router(query: str) -> str:
    if "기술" in query:
        return "tech"
    elif "절차" in query:
        return "procedure"
    return "generic"

router = RunnableLambda(domain_router)
```

---

## 🤖 7. LangGraph 기반 Agent 구현

### 📌 주요 클래스 설계 개요 (개인 프로젝트 설계 기준)

LangGraph는 상태 기반의 데이터 흐름을 설계하는 프레임워크로, 각 상태를 표현하고 유지할 수 있는 구조가 필요합니다.

#### ✅ `InformationStrip`

- **역할**: 추출된 정보의 내용을 저장하고, 관련성과 충실성 점수를 함께 기록합니다.
- **이유**: 단순한 문자열로 된 추출 정보가 아니라, 질의와의 관계성과 정확성(신뢰도)을 평가하여 후속 처리에서 필터링, 평가 등에 활용하기 위해 필요합니다.

#### ✅ `ExtractedInformation`

- **역할**: `InformationStrip` 리스트와 전체 문서에 대한 질의 응답 가능성 점수를 함께 포함한 구조입니다.
- **이유**: 문서 전체에서 추출된 정보들을 묶어 하나의 의미 있는 평가 단위로 관리하며, 쿼리 개선 등 후속 작업에서 사용됩니다.

#### ✅ `RefinedQuestion`

- **역할**: 기존 질문을 더 나은 정보 검색을 위해 개선한 질의와 그 이유를 포함합니다.
- **이유**: 초기 질문이 모호하거나 정보 검색에 비효율적인 경우, 모델이 스스로 개선된 질의를 생성할 수 있도록 돕습니다.

#### ✅ `CorrectiveRagState`

- **역할**: LangGraph 흐름 전반에 걸쳐 공유되는 상태 정보를 저장합니다. 원 질문, 생성된 응답, 검색된 문서, 반복 횟수를 포함합니다.
- **이유**: 각 노드가 공유 상태를 기반으로 동작해야 하므로, 반복 제어(`num_generations`) 등 제어 흐름을 유지하기 위해 필요합니다.

#### ✅ `GeneralRagState`

- **역할**: `CorrectiveRagState`를 확장하여 쿼리 재작성 결과, 추출된 정보, 최종 응답까지 포함할 수 있는 구조입니다.
- **이유**: RAG 흐름에서 상태가 점차 확장되기 때문에, 초기에 정의된 상태를 기반으로 필요한 필드를 확장하여 사용합니다.

LangGraph는 상태 기반 흐름을 설계하는 데 적합한 프레임워크입니다. 다음은 문서 검색 → 정보 추출 → 쿼리 개선 → 응답 생성 흐름을 나타냅니다.

### 상태 정의 및 평가 로직

```python
from pydantic import BaseModel, Field
from typing import List, Optional, TypedDict
from langchain_core.documents import Document

class InformationStrip(BaseModel):
    content: str
    source: str
    relevance_score: float = Field(..., ge=0, le=1)
    faithfulness_score: float = Field(..., ge=0, le=1)

class ExtractedInformation(BaseModel):
    strips: List[InformationStrip]
    query_relevance: float = Field(..., ge=0, le=1)

class RefinedQuestion(BaseModel):
    question_refined: str
    reason: str

class CorrectiveRagState(TypedDict):
    question: str
    generation: str
    documents: List[Document]
    num_generations: int

class GeneralRagState(CorrectiveRagState):
    rewritten_query: str
    extracted_info: Optional[ExtractedInformation]
    node_answer: Optional[str]
```

### LangGraph 노드 및 흐름 구성

#### ✅ `retrieve_documents`

- **역할**: 현재 질문 또는 재작성된 질문을 기반으로 벡터DB에서 관련 문서를 검색합니다.
- **설계 배경**: 질문이 초기 상태이든 개선된 쿼리이든 동일한 검색 흐름을 적용하기 위해 재사용 가능한 구조로 설계했습니다.

#### ✅ `extract_and_evaluate_information`

- **역할**: 검색된 문서에서 핵심 정보를 추출하고 관련성 및 충실성 점수를 부여합니다.
- **설계 이유**: 단순한 문서 출력이 아니라 의미 있는 정보를 선별해낼 수 있도록 하기 위해 점수 기반 필터링 구조를 적용했습니다. RAG의 ‘G(rounding)’을 강화하는 단계입니다.

#### ✅ `rewrite_query`

- **역할**: 기존 질문과 추출된 정보를 바탕으로 더 정밀한 검색이 가능하도록 질문을 개선합니다.
- **설계 이유**: 초기 질문이 애매하거나 문서 검색과 일치하지 않을 경우, 반복 검색 루프의 품질을 높이기 위해 필요했습니다. 추출 정보 기반의 개선이 핵심입니다.

#### ✅ `generate_node_answer`

- **역할**: 추출된 정보를 기반으로 최종 사용자 응답을 생성합니다. 정보 출처와 구조화된 설명 포함.
- **설계 이유**: 신뢰 가능한 출처 기반 응답 생성을 목표로 하며, Markdown 형태로 출력하여 후처리나 프론트렌더링에 적합하게 만들기 위해 설계되었습니다.

#### ✅ `should_continue`

- **역할**: 정보가 부족하거나 반복 가능성이 있을 경우 루프를 계속할지 여부를 판단합니다.
- **설계 이유**: 루프 횟수 제한과 추출 정보 존재 여부를 기반으로 흐름을 제어하여 무한 반복을 방지하고 효율적인 응답 흐름을 유지하기 위함입니다.

```python
from langgraph.graph import StateGraph, START, END

workflow = StateGraph(GeneralRagState)
workflow.add_node("retrieve", retrieve_documents)
workflow.add_node("extract", extract_and_evaluate_information)
workflow.add_node("rewrite", rewrite_query)
workflow.add_node("answer", generate_node_answer)

workflow.add_edge(START, "retrieve")
workflow.add_edge("retrieve", "extract")
workflow.add_conditional_edges("extract", should_continue, {
    "계속": "rewrite",
    "종료": "answer"
})
workflow.add_edge("rewrite", "retrieve")
workflow.add_edge("answer", END)

agent = workflow.compile()
```

### 실행 예시

```python
# 역할: 전체 LangGraph 실행 흐름을 순차적으로 보여주는 예시
inputs = {"question": "프로젝트 관리 시 유의할 절차는 무엇인가요?"}

for output in agent.stream(inputs):
    for key, value in output.items():
        print(f"[Node: {key}]\n{value}\n")

print("\n최종 답변:")
print(output["answer"])
```

---

## ✅ 요약

| 구성 요소   | 설명                                                   |
| ----------- | ------------------------------------------------------ |
| 문서 처리   | PDF → Document 변환 후 청크 분리 및 메타데이터 구성    |
| 벡터 저장   | Chroma + Ollama 임베딩                                 |
| 검색 고도화 | CrossEncoder 기반 Reranker 적용                        |
| 도구화      | LangChain Tool 또는 LangGraph 구성으로 호출 가능       |
| 분기 구성   | 질의 기반 조건부 흐름 (도메인 분기 또는 반복 평가)     |
| Agent 구현  | LangGraph를 통해 검색 → 추출 → 재작성 → 답변 흐름 구성 |
| 학습 목적   | 각 단계별 구성 요소의 역할과 동작을 명확히 학습 가능   |
