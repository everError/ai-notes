# RAG

문서를 검색 가능한 형태로 만드는 **인제스트 파이프라인**(파싱 → 청킹 → 임베딩 → 벡터 저장)과 검색 품질을 다룹니다.

벡터 DB 엔진 자체(Milvus, Chroma 아키텍처)는 db-notes에서, 대량 인제스트 운영·트레이싱·평가는 llmops-notes에서 다루고, 여기서는 **문서를 잘 넣고 잘 찾는 방법**에 집중합니다.

## 노트

- [문서 임베딩 파이프라인 정리](./notes/document-embedding-pipeline.md) — PDF/Excel 파싱 전략, 청킹, 임베딩 모델 선택, 대량 인제스트 효율화

## 실습

- [doc-embedding](./experiments/doc-embedding/) — PDF·Excel 파싱부터 Chroma 검색까지 (Jupyter)

## 앞으로 다룰 주제

- [ ] Docling으로 복잡한 PDF(표·레이아웃) 구조 보존 파싱
- [ ] 검색 품질 측정 — Recall@k, 청킹 파라미터 튜닝
- [ ] 하이브리드 검색 — dense + BM25/sparse
- [ ] Milvus로 대규모 인제스트 (db-notes 연계)
