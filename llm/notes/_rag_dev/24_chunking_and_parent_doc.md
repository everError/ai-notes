# 24 · 청크 분할과 Parent-Doc Retrieval

## 청크가 뭐고 왜 자르나

LLM은 한 번에 받을 수 있는 토큰이 정해져 있습니다 (예: 8K, 32K, 128K). 또 임베딩 모델도 입력 길이 제한이 있습니다 (보통 512~8192 토큰). 그래서 긴 문서를 작은 단위로 자르고 그 단위로 임베딩+검색합니다.

```
원본 문서 (예: 30 페이지 PDF)
   ↓ 청크 분할
청크1, 청크2, 청크3, ... 청크N
   ↓ 각 청크 임베딩 (벡터 N개)
벡터 DB
```

검색 단위 = 청크. 즉 "이 청크가 질문과 가깝다"는 결과가 나옵니다.

## 청크 크기의 트레이드오프

| 너무 작은 청크 (~100 토큰) | 너무 큰 청크 (~3000 토큰) |
|---|---|
| 의미 토막남 | 한 청크에 여러 주제 섞임 |
| 검색 정확도 ↑, LLM 합성은 어려움 | 임베딩 흐려져 검색 정확도 ↓ |
| 청크 수 많아 인덱싱·저장 비용 ↑ | LLM 컨텍스트 한계 위험 |

일반적인 sweet spot은 **300~1500 토큰** (한국어 기준 600~3000자).

## 청크 분할 전략

### 1. 의미 단위 우선

```
PPT 1 슬라이드      = 1 청크 (사람이 묶은 자연 경계)
PDF 1 페이지        = 1 청크
Excel 1 시트        = 1 청크 (혹은 행 그룹)
markdown 헤딩 단위  = 1 청크
```

이런 자연 경계는 의미가 잘 보존되어 검색 매칭 품질이 좋습니다.

### 2. 길면 RecursiveCharacterTextSplitter

LangChain의 표준 splitter — 우선순위에 따라 분할:

```
1. 빈 줄 (\n\n)        ← 문단 경계
2. 줄바꿈 (\n)
3. 마침표/물음표/느낌표
4. 공백
5. 글자 단위 (최후)
```

오버랩(overlap)으로 청크 사이 경계 정보 손실 완화 (보통 청크 크기의 10~20%):

```python
from langchain_text_splitters import RecursiveCharacterTextSplitter

splitter = RecursiveCharacterTextSplitter(
    chunk_size=1200,
    chunk_overlap=180,
)
chunks = splitter.split_text(long_text)
```

### 3. 표는 별도 청크로

표(table)는 그 자체가 한 단위입니다. 본문 텍스트와 섞어 임베딩하면 표 의미가 흐려집니다. 표는 markdown 형식으로 보존하고 별도 청크에 넣는 게 좋습니다:

```
원본 표 (300행) 가 너무 길다면
   ↓
청크1: [헤더] + 1~50행
청크2: [헤더] + 51~100행
청크3: [헤더] + 101~150행
...
```

**헤더를 매 청크에 반복**하면 어떤 컬럼인지 LLM이 알 수 있어 인용·합성이 정확합니다.

### 4. 메타·맥락 prepend

각 청크의 임베딩 입력에 다음을 prepend하면 검색 점수 ↑:

```
[<file_name> / <섹션 또는 페이지 번호>]

(원본 텍스트 본문)
```

이걸 더 발전시킨 게 **Contextual Retrieval** (Anthropic, 아래 참고).

## Parent-Document Retrieval

### 문제

청크가 작으면 검색은 정확하지만 LLM이 답을 합성할 정보가 부족합니다:
- 청크 A: "원인: 부품 마모"
- 같은 페이지 청크 B: "현상: 누유"
- 같은 페이지 청크 C: "대책: 정기 점검"

LLM이 좋은 답을 쓰려면 세 청크를 다 봐야 하는데, 검색은 한 청크만 가져올 수 있습니다.

### 해법

작은 청크로 검색해서 hit이 잡히면, 그 청크가 속한 **부모 단위(페이지·슬라이드·섹션) 전체를 끌어와서** LLM에 보여줍니다.

```
검색: "누유 원인"
   ↓
hit: file=A.pdf, page=5, chunk_index=1 ("원인: 부품 마모...")
   ↓
parent_doc 확장:
   같은 file=A.pdf, page=5의 모든 청크를 chunk_index 순서로 join
   ↓
LLM은 페이지 5 전체를 보면서 답변 생성
```

구현(의사 코드):

```python
def expand_parent(client, hits):
    # 1. (doc_id, page_no)별로 dedup — 한 페이지의 여러 chunk hit은 1개로
    by_unit = {}
    for h in hits:
        key = (h.doc_id, h.page_no)
        if key not in by_unit or h.score > by_unit[key].score:
            by_unit[key] = h

    # 2. 각 unit의 모든 chunk를 끌어와 join
    for h in by_unit.values():
        rows = client.query(
            collection,
            filter=f'doc_id == "{h.doc_id}" and page_no == {h.page_no}',
            output_fields=["chunk_index", "text"],
        )
        rows.sort(key=lambda r: r["chunk_index"])
        h.unit_text = "\n\n".join(r["text"] for r in rows)
    return list(by_unit.values())
```

### 부수 효과 — 중복 hit 자동 통합

같은 단위에서 여러 청크가 hit된 경우 자동으로 1개로 통합. LLM이 "페이지 5는 ○○하고 또 페이지 5는 ○○하고…"식으로 중복 답하지 않음.

## Contextual Retrieval (Anthropic, 2024)

청크에 **상위 컨텍스트**(문서 제목, 섹션 헤더, 1줄 요약)를 LLM이 생성해 prepend하는 기법. 인덱싱 시 1회 비용으로 검색 정확도 큰 향상:

```
원본 청크: "원인: 부품 마모"
LLM이 추가:  "이 청크는 [Q3 품질 보고서]의 [원인 분석] 섹션에서 가져온 내용입니다."
   ↓
임베딩 입력 = prepend된 텍스트
```

Anthropic 발표:
- 검색 실패율 49% 감소
- reranker 결합 시 67% 감소

비용:
- 청크당 LLM 호출 1회 (50~100 토큰)
- prompt caching으로 90% 절약 가능

가벼운 흉내: **결정론적 메타 prepend** (파일명, 섹션명, 페이지 번호 등)는 LLM 호출 없이도 비슷한 효과 일부 얻을 수 있습니다.

## 외부 학습 자원

- [LangChain ParentDocumentRetriever 가이드](https://python.langchain.com/docs/how_to/parent_document_retriever/)
- [Anthropic Contextual Retrieval 블로그](https://www.anthropic.com/news/contextual-retrieval)
- [LangChain TextSplitter 비교](https://python.langchain.com/docs/concepts/text_splitters/)
- [Pinecone — Chunking strategies](https://www.pinecone.io/learn/chunking-strategies/)
- [LlamaIndex — Node Parsers](https://docs.llamaindex.ai/en/stable/module_guides/loading/node_parsers/)
