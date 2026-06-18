# 학습 노트 — RAG / 에이전트 / 멀티모달 검색

이 디렉토리는 RAG·LLM 에이전트·문서 검색 시스템에서 자주 쓰이는 **개념과 기술**을 처음 또는 다시 학습하는 사람을 위해 정리한 모음입니다. 위키피디아 식으로 읽고 어디서든 적용할 수 있게 작성했습니다.

## 목차

| # | 파일 | 다루는 내용 |
|---|---|---|
| 20 | [rag_basics.md](20_rag_basics.md) | RAG가 뭐고 왜 필요한가 |
| 21 | [embeddings_and_vector_db.md](21_embeddings_and_vector_db.md) | 임베딩, 코사인 유사도, 벡터 DB, ANN 인덱스 |
| 22 | [hybrid_search_and_bm25.md](22_hybrid_search_and_bm25.md) | dense + sparse hybrid, BM25, RRF |
| 23 | [reranker.md](23_reranker.md) | cross-encoder reranker |
| 24 | [chunking_and_parent_doc.md](24_chunking_and_parent_doc.md) | 청크 분할 전략, parent-doc retrieval, contextual prepend |
| 30 | [vlm_and_captioning.md](30_vlm_and_captioning.md) | VLM(Vision-Language Model), 이미지 캡셔닝, ColPali |
| 31 | [document_extraction.md](31_document_extraction.md) | PPTX·PDF·Excel 추출 라이브러리 |
| 40 | [agent_loops.md](40_agent_loops.md) | ReAct vs Plan-Execute-Reflect 패턴 |
| 41 | [structured_output.md](41_structured_output.md) | LLM JSON 모드, schema 강제, 인용 검증 |
| 42 | [sdui_and_generative_ui.md](42_sdui_and_generative_ui.md) | SDUI / Generative UI 패턴 |
| 50 | [korean_nlp.md](50_korean_nlp.md) | 한국어 토크나이저, 형태소 기반 검색 |
| 99 | [glossary.md](99_glossary.md) | 약어·용어집 |

## 학습 진입 경로

### A. RAG가 처음
20 → 21 → 40

### B. 검색 품질 깊이
22 → 23 → 24 → 50

### C. 멀티모달(이미지) 관심
30 → 31 → 42

### D. 에이전트 구조 관심
40 → 41 → 42

각 노트 끝에 외부 학습 자원(공식 문서·블로그·논문) 링크가 있습니다.
