# 20 · RAG 기본기

## RAG가 뭐야?

**RAG = Retrieval-Augmented Generation** ("검색으로 보강된 생성").

LLM(예: ChatGPT, Llama, Qwen)은 학습할 때 본 지식만 알고 있어서:
- 학습 후에 만들어진 정보 모름
- 사내 문서·내부 시스템·도메인 자료 모름
- 자기가 모르는 걸 모르고 *그럴듯한 거짓말* 을 함 (환각 / hallucination)

RAG는 LLM에게 답변 *직전에* 관련 자료를 찾아서 같이 던져주는 패턴:

```
사용자 질문
    ↓
[Retriever] ─→ 자료 저장소에서 관련 섹션 N개 가져오기
    ↓
LLM에게 "이 N개 자료를 근거로 답해" 라고 시키기
    ↓
답변 (출처 인용 포함)
```

## 언제 RAG가 필요한가

- 사내 위키·매뉴얼·티켓 검색
- 법률·의료·과학 문헌 질의응답
- 제품 FAQ 봇
- 코드 베이스 질의응답
- 새 뉴스 / 시기 의존 정보

공통점: **LLM 학습 데이터에 없거나, 자주 갱신되거나, 출처를 명시해야 하는 정보**.

## RAG vs Fine-tuning

| | RAG | Fine-tuning |
|---|---|---|
| 새 지식 추가 | 인덱싱만 (분~시간) | 학습 (시간~일) |
| 출처 추적 | 가능 (어떤 문서에서 왔는지) | 어려움 (모델 가중치에 흡수됨) |
| 비용 | 추론 시 약간 ↑ (컨텍스트 길어짐) | 학습 비용 + 모델 관리 |
| 갱신 | 인덱스만 다시 빌드 | 재학습 |
| 도메인 스타일 | 약함 | 강함 (말투·포맷 학습 가능) |

대부분의 도메인 지식 QA는 RAG가 1순위. Fine-tuning은 어조·포맷이 핵심일 때.

## RAG 구성 4단계

```
1. 인덱싱 (Indexing)  ← 1회 또는 자료 추가 시
   원본 문서 → 텍스트 추출 → 청크 분할 → 임베딩 → 벡터DB

2. 검색 (Retrieval)   ← 매 질문마다 빠르게
   질문 → 임베딩 → 벡터DB top-k → (선택) 재정렬

3. 합성 (Generation)
   LLM에게 "질문 + 검색 결과" 함께 전달

4. 검증 (Verification, 옵션)
   답변이 실제 자료를 잘 근거 삼는지 확인 (인용 정확도, 누락 점검)
```

각 단계의 디테일은 다음 노트들에서:

- 인덱싱·청크: [24_chunking_and_parent_doc.md](24_chunking_and_parent_doc.md)
- 임베딩·벡터 DB: [21_embeddings_and_vector_db.md](21_embeddings_and_vector_db.md)
- 검색 품질: [22_hybrid_search_and_bm25.md](22_hybrid_search_and_bm25.md), [23_reranker.md](23_reranker.md)
- 합성/검증: [40_agent_loops.md](40_agent_loops.md), [41_structured_output.md](41_structured_output.md)

## 청크(chunk)가 뭐야?

LLM은 한 번에 받을 수 있는 텍스트 길이가 정해져 있습니다 (예: 32K 토큰). 100쪽 PDF를 통째로 던지면 못 받습니다. 그래서 **작게 자른 단위**를 청크라고 합니다.

검색은 청크 단위로 일어납니다. 슬라이드 1장, PDF 한 페이지, Excel 시트 1개를 보통 자연 청크로 사용.

자세히: [24_chunking_and_parent_doc.md](24_chunking_and_parent_doc.md)

## "벡터로 검색"은 어떻게 작동?

1. 텍스트를 **임베딩 모델**이 고차원 부동소수점 배열로 변환
2. 의미가 비슷한 텍스트는 벡터 공간에서 가까이 위치
3. 질문도 똑같이 벡터로 만들고, 가장 가까운 청크들을 찾아냄

```
"고양이의 특성"  →  [0.23, -0.11, 0.05, ...]
"feline traits" →  [0.21, -0.10, 0.06, ...]   (유사)
"자동차 정비"    →  [-0.41, 0.30, -0.02, ...]  (전혀 다름)
```

자세히: [21_embeddings_and_vector_db.md](21_embeddings_and_vector_db.md)

## RAG의 흔한 함정

### 1. 청크가 너무 작거나 너무 큼
- 작으면: 의미가 토막나서 LLM이 맥락 못 잡음
- 크면: 검색 정확도 ↓ (한 청크에 여러 주제가 섞여 임베딩이 흐려짐)
- 해법: parent-document retrieval (작은 청크로 검색하고 큰 부모 단위로 LLM에 전달)

### 2. 의미는 같은데 단어가 다름
- "버스 시간표" vs "노선 운행 정보"
- dense 임베딩이 어느 정도 잡지만 한계가 있음 → hybrid search

### 3. 환각 인용 (hallucinated citation)
- LLM이 "Document X 의 5장"을 인용했는데 그런 문서/장이 실제로 없음
- 해법: 휴리스틱 검증 — 인용한 file_name + section이 실제 검색 결과에 있는지 코드로 검사

### 4. 표/이미지의 정보 누락
- 슬라이드의 표를 단순 셀-pipe-join 텍스트로 임베딩하면 의미 손실
- 다이어그램·사진은 텍스트 추출에 안 잡힘
- 해법: 표는 markdown으로 보존, 이미지는 VLM 캡션 ([30_vlm_and_captioning.md](30_vlm_and_captioning.md))

### 5. 다국어/특수어휘 토큰화
- 동아시아어 어미 변형, 부품번호·약어 등이 토큰화 단계에서 분해 잘못
- 해법: 도메인 토크나이저 + 다국어 임베딩 모델

## 외부 학습 자원

- 입문 — [Anthropic, Contextual Retrieval](https://www.anthropic.com/news/contextual-retrieval)
- 영상 — Pinecone "Vector Database 101" 시리즈
- 책 — *Building LLM Powered Applications* (O'Reilly, 2024)
- 논문 — Lewis et al. 2020, ["Retrieval-Augmented Generation for Knowledge-Intensive NLP Tasks"](https://arxiv.org/abs/2005.11401) (RAG 용어의 출처)
