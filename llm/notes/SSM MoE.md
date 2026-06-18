# SSM(State Space Model)과 MoE(Mixture of Experts) 정리

---

## Part 1. SSM — State Space Model

### 1. SSM이 뭔가

SSM은 원래 제어 이론(Control Theory)에서 온 개념이다. 연속 시간 시스템의 상태 변화를 수학적으로 모델링하는 방식인데, 이걸 시퀀스 모델링(텍스트, 오디오 등)에 가져온 것이 딥러닝에서의 SSM이다.

핵심 아이디어는 **"숨겨진 상태(hidden state)를 선형 미분방정식으로 업데이트하면서 시퀀스를 처리한다"**는 것이다.

### 2. 수학적 구조

연속 시간 SSM의 기본 형태:

```
h'(t) = A·h(t) + B·x(t)    ← 상태 업데이트 (state equation)
y(t)  = C·h(t) + D·x(t)    ← 출력 (output equation)
```

- `x(t)`: 입력 신호
- `h(t)`: 숨겨진 상태 (hidden state)
- `y(t)`: 출력
- `A`: 상태 전이 행렬 — 이전 상태가 다음 상태에 어떻게 영향을 주는지
- `B`: 입력 행렬 — 입력이 상태에 어떻게 반영되는지
- `C`: 출력 행렬 — 상태에서 출력을 어떻게 뽑는지
- `D`: 직접 전달 행렬 (보통 생략)

실제 디지털 시퀀스에 적용하려면 이걸 **이산화(discretize)**한다:

```
h[k] = Ā·h[k-1] + B̄·x[k]
y[k] = C·h[k]
```

여기서 `Ā`, `B̄`는 A, B를 이산화한 버전이다 (ZOH, bilinear 등 방법 사용).

### 3. 왜 Transformer의 대안인가

**Transformer의 Self-Attention:**

```
Attention(Q, K, V) = softmax(QK^T / √d)·V
```

- 모든 토큰 쌍의 관계를 계산 → **O(n²)** 시간/메모리 복잡도
- 시퀀스 길이 n이 길어질수록 비용이 급증
- 장점: 임의 위치 간 정보 접근(global attention)이 강력

**SSM:**

- 고정 크기 상태 `h`를 순차적으로 업데이트 → **O(n)** 선형 복잡도
- 상태 크기가 시퀀스 길이에 무관하게 고정 → 메모리 효율적
- 단점: "3번째 토큰에서 언급한 특정 단어를 1000번째 토큰에서 정확히 회수"하는 **정밀 연상 검색(associative recall)**에 약함

비유하면:

- **Transformer** = 시험 중에 전체 교재를 펴놓고 아무 페이지나 볼 수 있음 (느리지만 정확)
- **SSM** = 교재를 한 번 읽으면서 요약 노트만 유지 (빠르지만 특정 세부사항 놓칠 수 있음)

### 4. Mamba 시리즈의 진화

#### Mamba-1 (2023.12)

- Albert Gu & Tri Dao가 제안
- 핵심 혁신: **Selective State Space** — 입력에 따라 A, B, C 파라미터를 동적으로 조절
- 기존 SSM(S4 등)은 A, B, C가 고정이었음 → 입력에 무관하게 같은 방식으로 상태 업데이트
- Mamba-1은 "어떤 정보를 기억하고 어떤 정보를 잊을지"를 **입력 의존적으로** 결정

```
# 기존 SSM (S4)
B, C = 고정 파라미터

# Mamba-1 (Selective SSM)
B = Linear(x)   ← 입력에서 B를 계산
C = Linear(x)   ← 입력에서 C를 계산
Δ = softplus(Linear(x))  ← 이산화 스텝도 입력 의존적
```

- 이 "선택성"이 Transformer의 어텐션이 하는 역할(중요한 것에 집중)을 근사

#### Mamba-2 (2024 중반)

- 핵심 목표: **학습 속도 개선**
- SSM 메커니즘을 단순화하여 Mamba-1 대비 2~8배 빠른 학습
- 대부분의 후속 아키텍처가 Mamba-1 대신 Mamba-2를 채택
- 트레이드오프: 학습은 빨라졌지만 추론 효율의 최적화는 부차적이었음

#### Mamba-3 (2026.03.17)

- 핵심 목표: **추론 효율** — 에이전틱 AI 시대에 디코딩이 병목
- Mamba-2 대비 **2배 작은 상태 크기** → 메모리 절약
- **MIMO(Multiple-Input Multiple-Output) 디코딩** 하드웨어 효율성 향상
- Transformer 베이스라인 대비 약 4% 성능 우위, 긴 시퀀스에서 최대 7배 빠른 추론
- Apache 2.0, ICLR 2026 채택

```
Mamba-1: "선택적 SSM으로 Transformer를 대체할 수 있다"
Mamba-2: "학습을 빠르게 해서 대규모로 쓸 수 있게 하자"
Mamba-3: "추론을 빠르게 해서 실제 배포에서 이기자"
```

### 5. SSM의 핵심 장단점 정리

**장점:**

- 선형 시간 복잡도 → 긴 시퀀스(100K~1M 토큰)에서 실용적
- 고정 크기 상태 → 추론 시 KV 캐시 불필요, 메모리 예측 가능
- 지속적 토큰 생성(에이전트의 긴 대화)에 유리

**단점:**

- 정밀 연상 검색(needle-in-a-haystack)이 약함
- 학습 시 병렬화가 Transformer만큼 직관적이지 않음 (개선 중)
- 생태계/도구 성숙도가 Transformer 대비 부족

---

## Part 2. MoE — Mixture of Experts

### 1. MoE가 뭔가

MoE는 "전체 네트워크를 다 쓰지 말고, 입력에 따라 **일부 전문가(Expert)만 활성화**하자"는 아이디어다.

핵심 동기: 모델 파라미터 수(=지식 용량)는 크게 유지하면서도 실제 연산량은 적게 쓰고 싶다.

### 2. 구조

일반적인 Transformer 블록:

```
Input → Attention → FFN(Feed-Forward Network) → Output
```

MoE Transformer 블록:

```
Input → Attention → Router → [Expert 1, Expert 2, ..., Expert N] → Output
                      ↓
              상위 K개 Expert만 선택
```

- **Expert:** 각각 독립적인 FFN(Feed-Forward Network). 구조는 동일하지만 가중치가 다름
- **Router (Gate):** 입력 토큰을 보고 "이 토큰은 어떤 Expert가 처리해야 하는가"를 결정

Router의 수학:

```
g(x) = softmax(W_g · x)           ← 각 Expert에 대한 점수 계산
G(x) = TopK(g(x))                 ← 상위 K개만 선택
y = Σ_i [G(x)_i · Expert_i(x)]   ← 선택된 Expert 출력의 가중합
```

### 3. 핵심 수치로 이해하기

Nemotron 3 Super를 예로 들면:

```
총 파라미터:    120B (1200억)
활성 파라미터:   12B (120억)   ← 실제 추론 시 사용
Expert 수:      N개
활성 Expert:    4개            ← Latent MoE로 "4개 비용으로 1개 가격"
```

즉, 120B 모델의 **지식 용량**을 가지면서 12B 모델의 **연산 비용**으로 실행된다. 이게 MoE의 핵심 가치다.

비유하면:

- **Dense 모델** = 모든 질문에 100명의 직원이 전부 달라붙어 답변 (비용 큼, 확실)
- **MoE 모델** = 100명 중 질문 분야에 맞는 4명만 골라서 답변 (비용 적음, 전문성 유지)

### 4. Router 설계의 도전과제

#### Load Balancing 문제

일부 Expert에만 토큰이 몰리면 → 특정 Expert 과부하, 나머지 유휴 → 학습 불균형

해결 방법:

- **Auxiliary Loss:** Expert 간 토큰 분배가 균등해지도록 보조 손실 추가
- **Expert Capacity:** 각 Expert가 처리할 수 있는 토큰 수에 상한 설정
- **Hash Routing:** 학습 가능한 라우터 대신 해시 함수로 분배 (Switch Transformer)

#### Token Dropping

용량 초과 시 토큰을 버리는 문제 → 정보 손실

#### Expert Collapse

학습 중 소수 Expert만 활성화되고 나머지가 죽는 현상 → 실질적으로 Dense 모델과 같아짐

### 5. MoE의 변형들

| 변형                                  | 핵심 아이디어                                                              |
| ------------------------------------- | -------------------------------------------------------------------------- |
| **Switch Transformer** (Google, 2021) | K=1, 토큰당 Expert 1개만 선택. 극한의 희소성                               |
| **Expert Choice** (2022)              | 토큰이 Expert를 고르는 게 아니라, Expert가 자기가 처리할 토큰을 고름       |
| **Soft MoE** (2023)                   | Hard routing 대신 모든 Expert에 soft weight 배분                           |
| **Latent MoE** (Nemotron 3 Super)     | 잠재 공간에서 라우팅, 4개 Expert 호출 비용으로 1개 비용만 사용             |
| **DeepSeek-MoE**                      | Fine-grained Expert (더 작은 Expert를 더 많이) + Shared Expert (항상 활성) |

### 6. MoE의 장단점

**장점:**

- 파라미터 효율: 동일 성능에서 Dense 대비 연산량 수분의 일
- 스케일링: 파라미터를 늘려도 추론 비용이 비례 증가하지 않음
- 전문화: 각 Expert가 특정 도메인/패턴에 특화될 수 있음

**단점:**

- 메모리: 비활성 Expert도 GPU 메모리에 적재되어야 함 (총 파라미터 크기만큼)
- 통신 오버헤드: 분산 학습 시 Expert 간 토큰 셔플링 비용
- 학습 불안정: 라우터 학습이 어렵고, Expert 붕괴 위험
- 배포 복잡성: 단일 GPU에 모든 Expert가 안 들어가면 Expert Parallelism 필요

---

## Part 3. 하이브리드 아키텍처 — SSM + Transformer + MoE

### 1. 왜 합치나

2026년 3월 기준 최전선은 **"세 가지를 조합하는 것"**이다.

```
순수 Transformer:  정밀 검색 ◎, 긴 컨텍스트 ✗, 추론 비용 ✗
순수 SSM:         정밀 검색 △, 긴 컨텍스트 ◎, 추론 비용 ◎
MoE:              파라미터 효율 ◎, 메모리 비용 △
```

조합하면:

```
SSM 레이어:        시퀀스의 대부분을 선형 시간에 처리 (긴 컨텍스트 담당)
Transformer 레이어: 핵심 위치에서 정밀 어텐션 수행 (정확한 검색 담당)
MoE 구조:          각 레이어의 FFN을 Expert로 분리 (연산 효율 담당)
```

### 2. Nemotron 3 Super의 구체적 구조

```
[입력 토큰 시퀀스]
       ↓
┌─────────────────────┐
│  Mamba-2 레이어      │  ← 시퀀스 처리의 대부분 담당
│  (SSM, 선형 복잡도)   │     1M 토큰 컨텍스트를 실용적으로 만듦
├─────────────────────┤
│  Transformer 레이어   │  ← 핵심 깊이에 간헐적으로 배치
│  (글로벌 어텐션)      │     정밀 연상 검색 담당
├─────────────────────┤
│  Latent MoE FFN      │  ← 각 레이어의 FFN을 Expert로 분리
│  (120B 중 12B 활성)   │     4개 Expert 비용으로 1개 가격
└─────────────────────┘
       ↓
[출력 토큰] + Multi-Token Prediction (MTP)
```

- **MTP(Multi-Token Prediction):** 한 번의 forward pass에서 여러 토큰을 동시 예측 → 토큰 생성 속도 50%+ 향상

### 3. 하이브리드의 실증 결과

Mamba-3 논문의 핵심 발견:

> "하이브리드 모델(SSM + 글로벌 어텐션)이 순수 Transformer보다 **검색 태스크에서도 우수**하면서 효율성도 유지한다."

이것이 의미하는 바: SSM의 유일한 약점이었던 정밀 검색마저 하이브리드로 해결 가능. 순수 Transformer를 고집할 이유가 줄어들고 있다.

---

## Part 4. 실무 관점 정리

### 언제 뭘 쓰나

| 시나리오                                 | 추천 아키텍처              | 이유                             |
| ---------------------------------------- | -------------------------- | -------------------------------- |
| 짧은 프롬프트, 높은 정확도 필요          | Dense Transformer          | 검색 정확도 최고, 비용 감당 가능 |
| 긴 컨텍스트 (코드베이스 전체, 문서 다수) | SSM-Transformer 하이브리드 | 1M 토큰까지 실용적 처리          |
| 고처리량 서빙 (API, 챗봇)                | MoE                        | 동일 품질에서 연산 비용 절감     |
| 에이전트 (지속적 토큰 생성)              | SSM + MoE 하이브리드       | 긴 대화에서의 추론 효율 극대화   |
| 에지/모바일                              | 소형 SSM 또는 소형 MoE     | 메모리/연산 제약 환경            |

### 2026년 3월 기준 방향성

1. **Transformer의 "독점"은 끝나가고 있다** — 하이브리드가 새 기본값으로 부상
2. **추론 효율이 학습 효율보다 중요해졌다** — 에이전트가 생성하는 토큰량이 폭증
3. **MoE는 "선택"이 아니라 "기본"이 되고 있다** — GPT-4, Gemini, Nemotron 모두 MoE 기반
4. **오픈소스 SSM 생태계가 급성장** — Mamba-3(Apache 2.0), Nemotron(NVIDIA Open License)
