# TimesFM

TimesFM(Time Series Foundation Model)은 Google Research에서 공개한 시계열(Time Series) 예측 모델이다.

기존의 시계열 예측 모델은 데이터셋마다 별도의 학습이 필요했지만, TimesFM은 대규모 시계열 데이터로 사전 학습된 Foundation Model을 활용하여 다양한 시계열 예측 문제에 적용할 수 있도록 설계되었다.

---

## Time Series Forecasting

시계열 예측(Time Series Forecasting)은 과거 데이터를 기반으로 미래 값을 예측하는 작업이다.

예시:

```text
매출
100
120
140
160
?
```

```text
주가
50000
51000
53000
52000
?
```

```text
전력 사용량
100
120
110
130
?
```

---

## 기존 접근 방식

전통적인 시계열 예측은 데이터셋마다 모델을 별도로 학습한다.

```text
데이터
 ↓
ARIMA
 ↓
예측
```

```text
데이터
 ↓
LSTM
 ↓
예측
```

새로운 데이터가 생기면 모델을 다시 학습해야 한다.

---

## Foundation Model 접근 방식

TimesFM은 대규모 시계열 데이터를 사전 학습한 후 다양한 예측 작업에 활용한다.

```text
대규모 시계열 데이터
          ↓
      TimesFM
          ↓
다양한 예측 작업
```

즉, 특정 데이터셋만을 위한 모델이 아니라 범용 시계열 예측 모델을 목표로 한다.

---

## 특징

### Pre-trained Model

대량의 시계열 데이터로 사전 학습되어 있다.

### Zero-shot Forecasting

추가 학습 없이 새로운 시계열 데이터에 적용할 수 있다.

### Transformer 기반

Transformer 아키텍처를 기반으로 시계열 데이터를 처리한다.

### 범용성

다양한 도메인의 시계열 데이터에 활용할 수 있다.

예시:

- 매출 예측
- 트래픽 예측
- 센서 데이터 예측
- 전력 수요 예측
- 금융 데이터 예측

---

## 기존 모델과 비교

| 구분        | ARIMA     | LSTM   | TimesFM            |
| ----------- | --------- | ------ | ------------------ |
| 모델 유형   | 통계 모델 | 딥러닝 | Foundation Model   |
| 재학습 필요 | O         | O      | 경우에 따라 불필요 |
| 범용성      | 낮음      | 중간   | 높음               |
| 사전 학습   | X         | X      | O                  |

---

## 학습 로드맵

1. Time Series Forecasting 개념
2. ARIMA
3. Prophet
4. LSTM
5. Transformer
6. TimesFM 구조 분석
7. TimesFM 실습

---

## References

- Google Research TimesFM
- Hugging Face TimesFM
- TimesFM Paper
