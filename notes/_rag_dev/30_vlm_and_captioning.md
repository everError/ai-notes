# 30 · VLM과 이미지 캡셔닝

## VLM이 뭐야?

**VLM = Vision-Language Model**. 이미지와 텍스트를 동시에 이해하는 LLM. 입력에 이미지를 함께 넣으면, 이미지를 보고 자연어로 답할 수 있습니다.

```
입력:  [이미지] + "이 사진 무슨 내용이야?"
출력:  "고양이가 창가에 앉아 있는 사진입니다..."
```

VLM은 보통 다음으로 구성:
1. **Vision Encoder** — 이미지를 벡터로 변환 (CLIP, SigLIP 같은 ViT 기반)
2. **Connector / Projector** — 이미지 벡터를 LLM이 읽을 수 있는 토큰으로 매핑
3. **LLM** — 이미지 토큰 + 텍스트 토큰을 같이 처리해서 답변 생성

```
       [이미지]
          │
          ▼
   Vision Encoder (예: ViT, SigLIP)
          │
          ▼
   Projector (Linear / MLP)
          │
          ▼
   ┌──── LLM ────┐
   │             │ ← + [텍스트 토큰]
   │   answer    │
   └─────────────┘
```

## VLM 모델 카탈로그 (2026년 기준)

| 모델 | 크기 | 라이선스 | 특성 |
|---|---|---|---|
| **GPT-4o / GPT-4-Turbo Vision** | API | SaaS | 영어 ★, 다국어 양호 |
| **Claude 3.5 Sonnet (Vision)** | API | SaaS | 텍스트 합성 강자 |
| **Qwen2-VL / Qwen2.5-VL** | 7B / 32B / 72B | Apache 2.0 | 다국어 ★, OCR 32개 언어 |
| **Qwen3-VL** (MoE) | 30B-A3B / 80B | Apache 2.0 | Qwen2.5-VL 후속 |
| **Llama 3.2 Vision** | 11B / 90B | Llama 라이선스 | 영어 ★ |
| **Gemma 3 (Vision)** | 4B / 27B | Gemma 라이선스 | 다국어 |
| **InternVL 2 / 3** | 1B~76B | Apache 2.0 | 학술 강자, OCR 강 |
| **MiniCPM-V** | 2B~8B | Apache 2.0 | 모바일·엣지 친화 |
| **LLaVA v1.6** | 7B / 13B | Apache 2.0 | 오래되지만 baseline |
| **Pixtral** (Mistral) | 12B | Apache 2.0 | 다국어 |

선택 가이드:
- 외부 API OK → Claude / GPT-4o
- 자체 호스팅 + 다국어 → Qwen2.5-VL, Qwen3-VL
- 가벼움 우선 → MiniCPM-V, Gemma 3 4B
- OCR 강조 → Qwen2.5-VL, InternVL 3

## 이미지 캡셔닝 (Image Captioning) 이 뭐야?

이미지를 보고 **자연어로 무슨 내용인지 묘사**하는 작업.

```
[고양이 사진]
   ↓ VLM
"갈색 줄무늬 고양이가 햇살 드는 창가의 쿠션 위에 누워 자고 있다."
```

활용:
- 시각장애인 보조 (alt text 자동 생성)
- 사진 라이브러리 검색
- **RAG에서 이미지 → 검색 가능한 텍스트로 변환** ← 우리 관심사

## RAG에서 캡션 활용

PPT·PDF 문서 안의 **다이어그램·사진·차트·도식**은 텍스트 추출에 안 잡힙니다. 일반 텍스트 검색에서 누락됩니다.

해결: VLM이 미리 캡션을 만들어 검색 인덱스에 추가:

```
인덱싱 시 (1회 비용):
  슬라이드 이미지 → VLM 캡션 → 텍스트 청크에 prepend → 임베딩
                                                        ↓
                                                    벡터 DB
검색 시 (운영):
  사용자 질문 → 임베딩 → 벡터 DB 검색 → hit (텍스트+캡션이 함께 임베딩되어 있어 매칭)
```

**핵심 효과**: 슬라이드 안의 그림·사진이 **텍스트 검색에서 hit 가능**해집니다. 별도 비전 인덱스 없이도 멀티모달 검색.

## 캡션 단위 — 페이지 전체 vs 개별 이미지

### 페이지/슬라이드 전체 PNG 캡션
- 페이지를 통째로 VLM에 주고 "이 페이지는 ~한 다이어그램과 ~한 사진 포함"
- 빠른 인덱싱 (페이지당 호출 1회)
- 검색에 prepend, 답변 카드에 페이지 썸네일 노출

### 개별 picture(figure) 캡션
- 페이지 안의 각 그림을 따로 VLM에 주고 짧은 캡션 + 종류 분류
- 인덱싱 비용 ↑ (그림 수만큼 호출)
- 답변 카드에 작은 figure 썸네일 strip을 보여줘 사용자가 정확히 어떤 그림을 인용했는지 노출

둘을 같이 쓰면: 검색은 페이지 캡션 매칭으로, 답변 카드 풍부화는 figure 캡션으로 — 텍스트 인덱스 1개로 멀티모달 RAG.

## 캡션 프롬프트 팁

### 추측 금지

> 이미지에 없는 내용은 절대 추측하지 마세요.

이게 없으면 VLM이 보지도 않은 이름·숫자·라벨을 지어냅니다 (환각).

### 무엇에 집중할지 명시

```
이 이미지의 시각 정보를 한국어로 3~5문장 묘사하세요.
텍스트 위주가 아니라 시각 요소에 집중:
- 다이어그램·도식의 종류와 의미
- 사진이라면 무엇을 보여주는지
- 화살표·강조 표시·라벨 등 주의를 끄는 단서
250자 이내.
```

### JSON 모드 강제

분류 + 캡션을 함께 받을 때:

```
출력은 다음 JSON 한 줄로만:
{"caption": "...", "kind": "photo|diagram|chart|screenshot|other"}
```

VLM의 JSON mode (Ollama format=json, OpenAI response_format) 활용.

### 출력 길이 제한

너무 길면 임베딩 토큰 한계 압박. 너무 짧으면 정보 부족. 가이드라인:
- 페이지 전체 캡션: 100~300자
- 단일 figure 캡션: 50~120자

## 비전 임베딩 — ColPali / ColQwen 가족

캡션 방식의 대안: **이미지를 직접 임베딩**해서 별도 인덱스에 저장.

```
페이지 PNG → Vision Encoder → 768~1024개 multi-vector / 페이지
                                ↓
                           Vespa/Qdrant 등 multi-vector 지원 DB
                                ↓
                  Late-interaction 검색 (질문 토큰별 ↔ 이미지 패치별 매칭)
```

| 캡션 방식 | ColPali 방식 |
|---|---|
| VLM 1회 호출/페이지 → 텍스트 인덱스 통합 | Vision encoder 1회/페이지 → 별도 multi-vector 인덱스 |
| 기존 텍스트 인프라 그대로 | multi-vector 지원 DB 필요 (Vespa, Qdrant) |
| 텍스트와 이미지 정보가 한 인덱스에 통합 | dual retrieval (텍스트 + 비전 인덱스) |
| 캡션 표현력의 한계 | 픽셀 단위 정밀 (다이어그램·차트 절대 강자) |
| 운영 단순 | 운영 복잡도 ↑↑ |

대표 모델:
- **ColPali** / **ColQwen2** / **ColQwen2.5** (Apache 2.0)
- **JinaCLIP v2**
- **NV-CLIP-v2** (NVIDIA)

## 외부 학습 자원

- [HuggingFace VLM 비교 2026](https://huggingface.co/blog/vlms-2026)
- [Qwen2-VL 논문 (2024)](https://arxiv.org/abs/2409.12191)
- [ColPali 논문 (2024)](https://arxiv.org/abs/2407.01449) — Late-interaction vision retrieval
- [Vespa ColPali 가이드](https://vespa-engine.github.io/pyvespa/examples/pdf-retrieval-with-ColQwen2-vlm_Vespa-cloud.html)
- [Ollama 멀티모달 가이드](https://ollama.com/blog/llava)
