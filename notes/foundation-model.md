# Foundation Model

## 개요

Foundation Model은 대규모 데이터로 사전 학습(pre-training)된 범용 AI 모델을 의미한다.

특정 작업만 수행하도록 학습된 모델과 달리, 다양한 다운스트림 작업에 활용할 수 있으며 추가 학습(Fine-tuning) 또는 프롬프트만으로 새로운 작업을 수행할 수 있다.

---

## 특징

### 대규모 사전 학습

인터넷 문서, 이미지, 시계열 데이터 등 방대한 데이터를 사용하여 학습한다.

예시:

- 텍스트 → LLM
- 이미지 + 텍스트 → VLM
- 시계열 데이터 → Time Series Foundation Model

---

### 범용성

하나의 모델을 다양한 작업에 활용할 수 있다.

예시:

LLM

- 질의응답
- 요약
- 번역
- 코드 생성

VLM

- 이미지 설명
- 객체 인식
- 문서 이해

Time Series Foundation Model

- 매출 예측
- 전력 수요 예측
- 센서 데이터 예측

---

### Transfer Learning

사전 학습된 지식을 새로운 문제에 재사용할 수 있다.

```text
Pre-training
      ↓
Fine-tuning
      ↓
Specific Task
```

---

## 종류

### Large Language Model (LLM)

텍스트 데이터를 학습한 언어 모델

예시:

- GPT
- Llama
- Qwen
- Gemma

---

### Vision Language Model (VLM)

이미지와 텍스트를 함께 처리하는 멀티모달 모델

예시:

- Qwen-VL
- GPT-4o
- Gemini

---

### Time Series Foundation Model

시계열 데이터를 학습한 범용 예측 모델

예시:

- TimesFM
- Chronos
- Moirai

---

## Foundation Model과 기존 모델의 차이

기존 방식

```text
데이터
 ↓
특정 모델 학습
 ↓
특정 문제 해결
```

Foundation Model 방식

```text
대규모 사전학습
 ↓
Foundation Model
 ↓
다양한 문제 적용
```

---

## 정리

Foundation Model은 다양한 작업에 재사용할 수 있도록 대규모 데이터로 사전 학습된 범용 AI 모델이다.

LLM, VLM, Time Series Foundation Model은 모두 Foundation Model의 하위 개념으로 볼 수 있다.
