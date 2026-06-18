# 21 · 임베딩과 벡터 DB

## 임베딩 (Embedding) 이 뭐야?

**텍스트를 의미 공간 안의 점으로 바꾸는 일.** 결과는 보통 384·768·1024·1536·3072 차원의 부동소수점 벡터입니다.

```
"고양이는 귀엽다"   →  [0.18, -0.05, 0.41, ..., 0.07]
"강아지는 사랑스럽다" →  [0.16, -0.04, 0.39, ..., 0.06]   (위와 가까움 — 의미 유사)
"양자역학"           →  [-0.32, 0.11, 0.02, ..., -0.08]  (멂)
```

핵심 성질: **의미적으로 비슷한 문장은 벡터 공간에서 가깝다.** 단순 단어 매칭이 아니라 의미 매칭.

## 거리 측정 — 코사인 유사도

```
similarity = cos(θ) = (A · B) / (|A| · |B|)

  값:  1.0  →  완전히 같은 방향 (의미 같음)
       0.0  →  무관
      -1.0  →  반대
```

대부분의 텍스트 임베딩은 코사인 유사도(또는 inner product) 사용. 이미지 임베딩은 L2 거리(유클리드)도 흔합니다.

## 임베딩 모델 카탈로그 (2026년 기준)

| 모델 | 차원 | 특성 |
|---|---|---|
| OpenAI `text-embedding-3-large` | 3072 (축약 가능) | 영어 ★, 다국어 양호. API 전용 (사내 데이터 외부 송신) |
| `BAAI/bge-m3` | 1024 | 다국어 ★, dense + sparse + multi-vector 지원, MIT |
| `BAAI/bge-large-en-v1.5` | 1024 | 영어 위주 |
| `intfloat/multilingual-e5-large` | 1024 | 100여개 언어 지원 |
| `jinaai/jina-embeddings-v3` | 1024 | 다국어, 긴 컨텍스트 (8K), Apache 2.0 |
| `nomic-embed-text-v1.5` | 768 (차원 가변) | 영어 위주, 무료 |
| `Snowflake/arctic-embed-l` | 1024 | 영어 ★, Apache 2.0 |
| `KR-SBERT` 계열 | 768 | 한국어 특화, 영어 약함 |

선택 가이드:
- 영어 위주 + 외부 API OK → OpenAI text-embedding-3
- 다국어 + 자체 호스팅 → bge-m3, multilingual-e5, jina v3
- 한국어 ★ + 영어 혼재 → bge-m3가 무난
- GPU 자원 적음 → smaller variant (`bge-base`, `e5-base`) 검토

## 벡터 DB가 뭐야?

벡터 수만~수십억 개를 저장하고 **"질문 벡터와 가장 가까운 N개"** 를 빠르게 찾는 DB. 일반 RDBMS는 모든 벡터를 비교해야 해서 느림.

벡터 DB는 **근사 최근접 이웃 (Approximate Nearest Neighbor, ANN)** 인덱스를 써서 N×log(N) 또는 그보다 빠르게 검색.

## 벡터 DB 카탈로그

| DB | 라이선스 | 특성 |
|---|---|---|
| **Milvus** | Apache 2.0 | GPU 가속, multi-vector, BM25 builtin, 대규모 |
| **Qdrant** | Apache 2.0 | 가벼움, 필터링 강력, multi-vector 지원 |
| **Weaviate** | BSD-3 | GraphQL API, 모듈식 |
| **Pinecone** | SaaS | 매니지드, 영어권 popular |
| **PostgreSQL + pgvector** | PostgreSQL | 기존 PG에 벡터 추가, 작은 규모에 적합 |
| **Vespa** | Apache 2.0 | 검색·랭킹 강력, ColPali 등 multi-vector 강자 |
| **FAISS** | MIT | DB 아닌 라이브러리. 단일 머신, 메모리 인덱스 |
| **Chroma** | Apache 2.0 | 로컬·임베디드용, Python 친화 |

선택 가이드:
- 대규모(수억+) + 운영 → Milvus, Vespa
- 중간(수백만) + 빠른 시작 → Qdrant, Weaviate
- 작은(수십만) + 단순 → pgvector, Chroma
- 오프라인 분석 → FAISS

## ANN 인덱스 — HNSW

**Hierarchical Navigable Small World**. 그래프 기반 ANN의 사실상 표준.

```
   상위 레이어 (sparse, 빠른 점프용)
      ●─────────●─────●
       \       / \   /
   중간  ●───●───●─●
   레이어 │   │   │ │
         │   │   │ │
   하위   ●─●─●─●─●─●─●─●─●  (모든 노드)
   레이어 (dense, 정밀 매칭용)

   질문 벡터 → 상위에서 시작 → 가까운 노드로 점프 → 하위로 내려가며 정밀
```

특성:
- **빠름**: 수백만 벡터에서도 수 ms
- **재현율**: 99% 가까이 (true top-k와 거의 같음)
- **메모리**: 벡터 + 그래프 (RAM 또는 GPU)
- **trade-off**: 조회 정확도 ↔ 메모리 ↔ 인덱스 빌드 시간

주요 파라미터:
- `M`: 한 노드의 최대 이웃 수 (보통 16~64)
- `ef_construction`: 빌드 시 탐색 폭 (높을수록 인덱스 품질 ↑, 빌드 시간 ↑)
- `ef`: 검색 시 탐색 폭 (높을수록 재현율 ↑, 속도 ↓)

## 다른 ANN 인덱스 패밀리

| 인덱스 | 특성 |
|---|---|
| **HNSW** | 그래프, 빠름·정확·메모리↑ — 대부분의 default |
| **IVF (FLAT/PQ/SQ)** | 클러스터링 + 양자화. 메모리 절약, 정확도↓ |
| **DiskANN** | SSD 기반, 메모리 절약 |
| **ScaNN** | Google 제품, 양자화 + 그래프 |

수만 청크 미만이면 HNSW가 거의 항상 정답. 수억 이상에서 IVF/DiskANN 고려.

## 검색 흐름 (단순화)

```python
# 1. 질문 임베딩
query_vec = embedder.embed("질문")   # 1024-dim

# 2. 벡터 DB 검색
hits = db.search(
    collection="my_docs",
    data=[query_vec],
    metric="COSINE",
    limit=50,                # 일단 50개 가져와서
    output_fields=["doc_id", "text", ...],
)

# 3. (다음 단계) reranker로 정밀 정렬
top_8 = rerank(query, hits, k=8)
```

## 흔한 오해

### "임베딩이 의미를 다 잡는다"
- 사실은 학습 데이터에 따라 잘 잡는 분야와 못 잡는 분야가 있음
- 일반 다국어 임베더는 학술 약어, 법률 용어, 부품번호에 약할 수 있음
- 보강: 키워드 매칭(BM25) hybrid ([22_hybrid_search_and_bm25.md](22_hybrid_search_and_bm25.md))

### "차원 높을수록 좋다"
- 차원 ×2 → 메모리 ×2, 속도 ×0.5, 정확도 보통 약간 ↑
- 1024 정도가 sweet spot. 3072는 영어 까다로운 도메인에서만 의미

### "코사인 vs L2 거리"
- 코사인 = 정규화된 방향. 텍스트 임베딩의 표준
- L2 = 절대 거리. 이미지 임베딩에서 더 흔함
- 모델 카드에 권장 메트릭 명시되어 있음

### "임베딩만 잘하면 검색 끝"
- 1차 ANN 결과는 거칠다. cross-encoder reranker가 정밀 정렬 ([23_reranker.md](23_reranker.md))

## 외부 학습 자원

- [Pinecone — Vector Embeddings 시각화](https://www.pinecone.io/learn/series/faiss/vector-embeddings/)
- [BGE-M3 paper (2024)](https://arxiv.org/abs/2402.03216)
- [HNSW 논문 (2016)](https://arxiv.org/abs/1603.09320) — Malkov & Yashunin
- [Milvus 공식 문서](https://milvus.io/docs/)
- [Sentence-Transformers MTEB 리더보드](https://huggingface.co/spaces/mteb/leaderboard)
