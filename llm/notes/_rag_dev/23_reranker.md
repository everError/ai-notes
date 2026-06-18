# 23 · Reranker (cross-encoder)

## 왜 reranker가 필요한가

Dense + Hybrid 검색이 top-50을 빠르게 가져옵니다. 하지만:
- top-50 안에 정답이 있어도 top-1이 아닐 수 있음
- LLM 합성 단계에는 보통 5~10개만 보내야 (컨텍스트 한계 + 노이즈)

**Reranker가 top-50을 정밀 재정렬해서 진짜 좋은 5~8개만 추려냅니다.**

## Bi-encoder vs Cross-encoder

```
Bi-encoder (검색용 — 일반 임베더)
   질문 ──→ encoder ──→ 벡터 q
   문서 ──→ encoder ──→ 벡터 d  (미리 인덱싱)
                       ↓
                 cos(q, d)  =  점수

   특성: 문서를 미리 임베딩 가능 → 검색 빠름
        but 점수가 거칠다 (질문↔문서 상호작용 못 봄)
```

```
Cross-encoder (reranker)
   [질문, 문서] ──→ encoder ──→ 점수

   특성: 질문과 문서를 한 번에 보고 직접 점수 매김 → 정확
        but 모든 후보 문서마다 호출 → 비쌈
```

조합:
1. Bi-encoder로 1차 50개 거름 (빠름)
2. Cross-encoder로 그 50개를 정밀 정렬 → top-8
3. LLM에 top-8 전달

각 단계 비용 분담이 적절해 시간/품질 균형이 잘 맞습니다.

## Reranker 모델 카탈로그

| 모델 | 라이선스 | 특성 |
|---|---|---|
| **BAAI/bge-reranker-v2-m3** | Apache 2.0 | 다국어 ★, 한국어/일본어/중국어 양호 |
| **BAAI/bge-reranker-base** | Apache 2.0 | 영어, 가벼움 |
| **dragonkue/bge-reranker-v2-m3-ko** | MIT | bge-m3에 한국어 fine-tune. 한국어 ★ |
| **mixedbread-ai/mxbai-rerank-large-v1** | Apache 2.0 | 영어 ★ |
| **Cohere `rerank-3.5`** | SaaS API | 다국어 ★, 사내 데이터 외부 송신 주의 |
| **jinaai/jina-reranker-v2** | Apache 2.0 | 다국어 |
| **mlx-community/Qwen-Reranker** | Apache 2.0 | LLM-as-reranker |

선택 가이드:
- 영어 위주 → bge-reranker-base, mxbai
- 다국어 자체 호스팅 → bge-reranker-v2-m3
- 한국어 ★ 자체 호스팅 → dragonkue ko
- 외부 API OK + 최강 품질 → Cohere

## 사용 흐름 (예시)

```python
from FlagEmbedding import FlagReranker

reranker = FlagReranker("BAAI/bge-reranker-v2-m3", use_fp16=True, devices=["cuda"])

# 1차 검색 결과 (top-50)
hits = vector_db.search(query, top_k=50)

# 점수 매기기
pairs = [[query, hit.text] for hit in hits]
scores = reranker.compute_score(pairs, normalize=True)  # 0~1로 정규화

# 정렬
for hit, score in zip(hits, scores):
    hit.score = score
hits.sort(key=lambda h: h.score, reverse=True)

top_8 = hits[:8]
```

## 성능 영향

벤치마크 일반 통계 (BEIR·MTEB-rerank 등):
- Dense only nDCG@10 ≈ 0.55~0.65
- Dense + Reranker nDCG@10 ≈ 0.70~0.85 → **+15~25%p 향상**

대부분의 도메인에서 reranker가 단일 단계로 가장 ROI 높습니다.

## Cross-encoder vs LLM-as-Judge

요즘은 LLM(예: Claude, GPT-4)을 reranker로 쓰는 사례도 있습니다 ("이 문서가 질문과 얼마나 관련 있나?"를 LLM이 1~10 점수로):

| | Cross-encoder (BERT 계열) | LLM-as-Judge |
|---|---|---|
| 속도 | 50개 ~100ms | 50개 ~수십 초 |
| 비용 | 작은 GPU 한 장 | 토큰 비용 |
| 정확도 | 강력 (rerank 전용 학습) | 더 강력하지만 트레이드오프 큼 |
| 구조화 출력 | 그냥 점수 | JSON 분류·이유 함께 |

대부분 운영은 cross-encoder. LLM-as-Judge는 평가·offline 단계에 사용.

## 폴백 패턴

reranker는 GPU 메모리 부족·CUDA 미설정·모델 다운로드 실패 등으로 못 뜰 수 있습니다. 안정적 시스템은:

```python
try:
    reranker = FlagReranker(...)
except Exception as e:
    log.warning("reranker unavailable: %s", e)
    reranker = None

# 사용 시
if reranker:
    hits = reranker_sort(hits)
else:
    hits = hits[:top_k]   # 1차 검색 순서 그대로
```

→ 검색은 동작하되 품질만 약간 떨어짐.

## 도메인 fine-tune의 가치

baseline reranker(예: bge-reranker-v2-m3)는 일반 도메인용. 특정 언어/도메인에 fine-tune된 변형이 있으면 큰 향상이 있을 수 있습니다.

예시 (한국어):

| 모델 | 한국어 nDCG@10 |
|---|---|
| BAAI base (다국어) | ~0.69 |
| dragonkue ko fine-tune | ~0.81 |

차이는 보통 **학습 데이터의 언어/도메인 비중**에서 옵니다. 가능하면 도메인에 가까운 fine-tune 변형을 검토.

## 외부 학습 자원

- [Sentence Transformers — Cross-Encoder 튜토리얼](https://sbert.net/examples/applications/cross-encoder/README.html)
- [BAAI/bge-reranker-v2-m3 모델 카드](https://huggingface.co/BAAI/bge-reranker-v2-m3)
- [Cohere Rerank 가이드](https://cohere.com/rerank)
- [Reranker Leaderboard](https://agentset.ai/rerankers)
- [Pinecone — Cross-Encoder 가이드](https://www.pinecone.io/learn/series/rag/rerankers/)
