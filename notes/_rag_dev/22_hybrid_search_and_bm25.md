# 22 · Hybrid Search와 BM25

## 왜 dense 임베딩만으로는 부족한가

dense 임베딩은 **의미 매칭**에 강하지만 두 가지 약점이 있습니다:

1. **고유명사·코드·약어**에 약함
   - 부품번호 `25450-2MHA0` 는 임베딩이 잘 못 잡음 — 일반 학습 코퍼스에 안 등장
   - 회사명 마커("(주)", "Corp.") 같은 표시도 흐려질 수 있음
2. **희귀 키워드**는 의미 공간에서 묻힘
   - 도메인 특수 어휘는 학습 데이터 적어 좌표가 부정확

이런 케이스는 **단어 매칭(키워드 검색)** 이 더 정확합니다. 그래서 둘을 결합한 게 **Hybrid Search**.

## BM25 (Best Match 25)

검색 엔진에서 30년 넘게 쓰는 고전 점수 함수. 단어 매칭의 사실상 표준.

```
score(D, Q) = Σ_term IDF(term) · tf_BM25(term, D)

  IDF(term):  흔한 단어는 가중치 ↓ (예: "있다", "그리고")
              희귀한 단어는 가중치 ↑ (예: "에틸렌글리콜")
  tf_BM25:    문서 내 등장 빈도 (포화 곡선 — 너무 많이 나와도 무한정 안 오름)
              + 문서 길이 정규화 (긴 문서에 편향 X)
```

직관: "에틸렌글리콜" 같은 흔하지 않은 단어가 문서에 5번 등장하면 점수 ↑. "있다" 100번 등장은 거의 무시.

장점:
- 결정론적 (학습 안 함, 코퍼스 통계만 사용)
- 빠름
- 정확한 단어 매칭은 강자

단점:
- 동의어 못 봄 ("자동차 결함" ≠ "차량 불량")
- 어순·문맥 무시
- 형태소가 다른 언어(한국어, 일본어)에선 토큰화 전처리 필수

## Hybrid = Dense + Sparse

```
질문: "에틸렌글리콜 누유 원인"
   │
   ├─→ Dense embedder ───→ Vector DB top-50 ("의미적으로 비슷")
   │
   └─→ Sparse BM25     ───→ Vector DB top-50 ("정확한 단어 매칭")

   두 결과를 RRF(Reciprocal Rank Fusion) 또는 가중합으로 융합 → 최종 top-50
```

## RRF (Reciprocal Rank Fusion)

여러 ranker의 결과를 하나로 합치는 단순한 알고리즘:

```
RRF_score(d) = Σ_ranker  1 / (k + rank_in_ranker(d))

  k는 보통 60. rank는 1부터.
```

직관: "여러 ranker에서 동시에 위에 등장하는 문서가 좋다". 점수 정규화·가중치 학습 없이 단순 결합. 가중합보다 안정적.

## Sparse Vector

BM25 결과를 벡터 형태로 표현 (대부분 0, 일부 차원만 값):

```
원본 문서: "에틸렌글리콜 누유"
어휘 사전 100,000개 중:
  "에틸렌글리콜" (idx=42351) 가중치 4.2
  "누유"        (idx=18722) 가중치 3.8
  나머지 99998개 차원은 0
```

벡터 DB가 sparse vector field를 지원하면 dense + sparse를 한 인덱스에서 같이 운영 가능.

## 분석기 / 토크나이저

BM25 검색의 품질은 **토큰화**에 크게 좌우됩니다. 영어는 단순 공백 분리로 충분하지만:

- **CJK (한·중·일)**: 형태소 분석 필수 (Kiwi, Mecab, Jieba 등)
- **합성어 언어 (독일어 등)**: 분해 토큰화
- **약어/번호**: 토큰 보존 규칙

벡터 DB는 분석기를 collection schema에 등록하거나 외부에서 사전 토큰화한 텍스트를 받습니다.

자세히 (한국어): [50_korean_nlp.md](50_korean_nlp.md)

## Hybrid의 가치

벤치마크 일반 통계 (MTEB·BEIR 등):

| 구성 | nDCG@10 (대략적) |
|---|---|
| Dense only | 0.55~0.65 |
| BM25 only | 0.45~0.55 |
| Hybrid (Dense + BM25) | 0.65~0.75 |
| Hybrid + Reranker | 0.75~0.85 |

조합별 +5~10%p 향상이 일반적. 도메인 따라 hybrid 효과가 크게 다르고, 희귀 어휘·코드가 많은 도메인일수록 BM25 추가 가치 ↑.

## Hybrid의 한계 — Reranker가 그 다음

Hybrid라도 **top-50 안에 정답이 있지만 top-1이 아닐 수 있습니다.** 이걸 정밀 정렬하는 게 cross-encoder reranker. 다음 노트: [23_reranker.md](23_reranker.md)

## 외부 학습 자원

- [Anthropic — Contextual Retrieval](https://www.anthropic.com/news/contextual-retrieval) — BM25 + dense + LLM-context 조합 사례
- [BM25 — Wikipedia](https://en.wikipedia.org/wiki/Okapi_BM25)
- [Milvus 2.5 Hybrid Search 가이드](https://milvus.io/blog/get-started-with-hybrid-semantic-full-text-search-with-milvus-2-5.md)
- [Qdrant Hybrid Search 가이드](https://qdrant.tech/articles/hybrid-search/)
- [RRF 논문 (2009)](https://plg.uwaterloo.ca/~gvcormac/cormacksigir09-rrf.pdf) — Cormack et al.
