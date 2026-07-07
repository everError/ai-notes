# doc-embedding 실습

PDF·Excel 문서를 파싱 → 청킹 → 임베딩 → Chroma 검색까지 만드는 인제스트 파이프라인 실습.

먼저 [문서 임베딩 파이프라인 정리](../../notes/document-embedding-pipeline.md)를 읽을 것.

## 구성

```
doc-embedding/
├── environment.yml            # mamba 환경 (rag-lab)
├── notebooks/
│   ├── 01_parse_documents.ipynb    # 샘플 문서 생성 + PDF/Excel 파싱 → parsed.jsonl
│   └── 02_chunk_embed_search.ipynb # 청킹 → 배치 임베딩 → Chroma upsert → 검색
├── data/                      # 샘플 문서, 중간 산출물 (노트북이 생성)
└── chroma-db/                 # Chroma 영구 저장소 (노트북이 생성)
```

## 셋업

```bash
mamba env create -f environment.yml
mamba activate rag-lab
jupyter lab
```

노트북 01 → 02 순서로 진행. Docker 불필요 (Chroma는 로컬 파일 모드).

- 02 첫 실행 시 임베딩 모델(multilingual-e5-small, 약 470MB) 다운로드
- 02의 6절(Langfuse 트레이싱)만 선택 사항 — observability 실습 스택이 떠 있어야 동작

## 확인 포인트

- [ ] 같은 질문이 PDF 청크와 Excel 행을 **한 컬렉션에서 함께** 검색하는가
- [ ] 02의 3절 upsert 셀을 두 번 실행해도 count 가 안 늘어나는가 (해시 id 멱등성)
- [ ] e5 prefix(`query:` / `passage:`)를 빼면 검색 순위가 어떻게 달라지는가 (직접 실험)
- [ ] chunk_size 를 300 → 800 으로 바꾸면 검색 결과가 어떻게 달라지는가
