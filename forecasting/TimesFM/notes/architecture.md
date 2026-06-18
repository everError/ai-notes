# TimesFM Architecture

## 개요

TimesFM(Time Series Foundation Model)은 Google Research가 개발한 시계열 예측용 Foundation Model이다.

아키텍처는 Transformer를 기반으로 하지만, 일반적인 LLM과는 차이가 있다.

핵심 특징은 다음과 같다.

- Decoder-Only Transformer
- Patch-Based Processing
- Autoregressive Forecasting
- Time Series Foundation Model
- Zero-shot Forecasting 지원

TimesFM은 GPT처럼 시퀀스를 순차적으로 예측하지만, 텍스트 토큰 대신 시계열 데이터를 처리한다.

---

# 전체 구조

```text
Time Series
      ↓
Patching
      ↓
Patch Embedding
      ↓
Transformer Decoder
      ↓
Forecast Head
      ↓
Prediction
```

---

# 1. Patch-Based Processing

TimesFM의 가장 중요한 특징이다.

일반적인 시계열 모델은

```text
100
102
105
110
```

각 시점을 하나의 토큰처럼 처리한다.

하지만 TimesFM은 여러 시점을 하나의 Patch로 묶는다.

예시

```text
Patch 1
[100,102,105,110]

Patch 2
[111,113,115,118]

Patch 3
[120,122,125,128]
```

---

## 왜 Patch를 사용하는가?

### 긴 시계열 처리

예를 들어

```text
16,384개의 시점
```

을 처리한다고 가정하자.

일반 Transformer

```text
16,384 Tokens
```

TimesFM

```text
Patch Size = 32

16,384 / 32

= 512 Tokens
```

Transformer가 처리해야 할 토큰 수가 크게 감소한다.

---

## 기본 설정

대표 모델 기준

```text
Input Patch Length = 32
Output Patch Length = 128
```

즉

```text
32개 입력
↓
128개 미래값 예측
```

구조를 사용한다.

---

# 2. Patch Embedding

LLM에서는

```text
"hello"
```

가

```text
Embedding Vector
```

로 변환된다.

TimesFM에서는

```text
[100,102,105,...]
```

Patch 전체가 하나의 벡터로 변환된다.

```text
Patch
 ↓
Residual MLP
 ↓
Embedding
```

Transformer는 이 임베딩 벡터를 입력으로 사용한다.

---

# 3. Decoder-Only Transformer

TimesFM은 Encoder-Decoder 구조가 아니다.

```text
Encoder
 ↓
Decoder
```

구조를 사용하지 않는다.

대신

```text
Decoder Only
```

구조를 사용한다.

LLM으로 비교하면

```text
GPT
Llama
Qwen
```

와 비슷한 계열이다.

---

## 왜 Decoder Only 인가?

시계열 예측은 본질적으로

```text
과거
 ↓
미래
```

를 예측하는 문제다.

따라서

```text
현재 시점까지 관측
↓
다음 값 생성
```

이라는 GPT 스타일 구조가 잘 맞는다.

---

# 4. Causal Self-Attention

LLM과 동일하게 미래를 볼 수 없다.

예를 들어

```text
100
102
105
?
```

를 예측할 때

```text
100
102
105
110
```

을 미리 보면 안 된다.

따라서

```text
Causal Mask
```

를 사용한다.

---

# 5. Positional Information

시계열에서는 순서가 매우 중요하다.

```text
100 → 120 → 140
```

와

```text
140 → 120 → 100
```

은 완전히 다르다.

그래서 Patch Embedding에 위치 정보를 추가한다.

```text
Patch Embedding
 +
Positional Encoding
```

최근 버전은 Rotary Position Embedding(RoPE)을 사용한다.

---

# 6. Forecast Head

Transformer 출력은 바로 숫자가 아니다.

마지막에 Forecast Head가 붙는다.

```text
Transformer Output
        ↓
Forecast Head
        ↓
Future Values
```

---

## Point Forecast

하나의 예측값 생성

예시

```text
110
115
120
125
```

---

## Quantile Forecast

불확실성까지 예측

예시

```text
10%  = 105
50%  = 110
90%  = 118
```

즉

```text
110이 예상값
105~118 범위 가능
```

과 같은 해석이 가능하다.

---

# 7. Autoregressive Forecasting

TimesFM은 GPT처럼 자기 출력을 다시 입력으로 사용할 수 있다.

예를 들어

```text
입력
512개 시점
```

↓

```text
128개 예측
```

↓

```text
예측 결과 일부를 다시 입력
```

↓

```text
다음 128개 예측
```

이 과정을 반복하여 긴 미래 구간을 생성한다.

---

# LLM과 비교

| 항목      | LLM                 | TimesFM             |
| --------- | ------------------- | ------------------- |
| 입력      | 텍스트              | 시계열 데이터       |
| 토큰      | 단어/서브워드       | Patch               |
| 모델      | Decoder Transformer | Decoder Transformer |
| 학습 목표 | 다음 토큰 예측      | 미래 시점 예측      |
| 출력      | 텍스트              | 숫자                |

---

# 핵심 아이디어

TimesFM의 가장 중요한 아이디어는

```text
시계열 데이터
      ↓
Patch Tokenization
      ↓
Decoder-Only Transformer
      ↓
Forecasting
```

이다.

즉 Google은 NLP에서 성공한 GPT 스타일 구조를 그대로 가져오되, 텍스트 토큰 대신 시계열 Patch를 입력으로 사용하는 방식으로 범용 시계열 Foundation Model을 구현했다.
