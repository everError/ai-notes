## vLLM: 고성능 LLM 추론 및 서빙을 위한 프레임워크

vLLM(Virtual Large Language Model)은 대규모 언어 모델(LLM)의 추론 및 서빙을 가속화하고 최적화하기 위해 설계된 오픈소스 라이브러리입니다. 특히 \*\*처리량(throughput)\*\*을 극대화하고 **메모리 관리**를 효율화하는 데 중점을 둡니다.

---

### 주요 특징

vLLM은 여러 혁신적인 기술을 통해 LLM 서빙의 성능을 획기적으로 개선합니다.

#### **1. PagedAttention**

vLLM의 핵심 기술인 **PagedAttention**은 운영 체제의 가상 메모리 및 페이징 기술에서 영감을 받았습니다. 기존의 LLM 서빙 시스템에서는 `Key-Value (KV) 캐시`가 연속적인 메모리 블록에 저장되어야 했기 때문에, 메모리 단편화와 낭비가 심했습니다.

PagedAttention은 KV 캐시를 **블록(페이지)** 단위로 분할하여 비연속적인 메모리 공간에 저장합니다. 이를 통해 다음과 같은 이점을 얻습니다.

- **메모리 낭비 최소화**: 필요한 만큼만 메모리 블록을 할당하여 낭비되는 메모리를 크게 줄입니다.
- **유연한 메모리 관리**: 긴 시퀀스나 복잡한 샘플링에서도 유연하게 메모리를 공유하고 관리할 수 있습니다. 예를 들어, 병렬 샘플링 시 여러 결과가 동일한 프롬프트의 KV 캐시를 공유하여 메모리 사용량을 줄일 수 있습니다.

#### **2. 연속적인 배치 처리 (Continuous Batching)**

기존의 정적 배치(static batching) 방식은 모든 요청이 끝날 때까지 기다려야 다음 배치를 처리할 수 있어 GPU가 유휴 상태에 있는 시간이 길었습니다. vLLM은 **연속적인 배치 처리**를 통해 요청이 도착하는 즉시 동적으로 배치에 추가합니다.

이 방식은 GPU 활용률을 극대화하여 시스템의 전체 처리량을 크게 향상시키고, 개별 요청의 대기 시간을 줄여줍니다.

#### **3. 최적화된 CUDA 커널**

vLLM은 `FlashAttention`과 같은 최적화된 CUDA 커널을 통합하여 어텐션 계산 속도를 가속화합니다. 이를 통해 GPU의 연산 능력을 최대한 활용하여 더 빠른 추론 속도를 제공합니다.

#### **4. 모델 양자화 지원**

정확도 손실을 최소화하면서 모델의 크기를 줄이고 추론 속도를 높이기 위해 다양한 양자화 기법을 지원합니다. 지원하는 방식에는 `GPTQ`, `AWQ`, `SqueezeLLM` 등이 포함됩니다.

---

### vLLM의 장점

- **높은 처리량**: PagedAttention과 연속적인 배치 처리를 통해 Hugging Face Transformers 대비 최대 24배 높은 처리량을 달성할 수 있습니다.
- **효율적인 메모리 사용**: 메모리 낭비를 약 4% 수준으로 줄여 동일한 하드웨어에서 더 큰 모델을 서빙하거나 더 많은 동시 요청을 처리할 수 있습니다.
- **유연성 및 확장성**: 분산 추론을 지원하여 여러 GPU에 걸쳐 모델을 실행할 수 있으며, LoRA(Low-Rank Adaptation)와 같은 어댑터를 동적으로 로드할 수 있습니다.
- **OpenAI 호환 API**: OpenAI API와 동일한 형식의 인터페이스를 제공하여 기존 애플리케이션에 쉽게 통합할 수 있습니다.

---

### 사용법 및 설치

vLLM은 `pip`을 통해 간단하게 설치할 수 있습니다. (현재 Linux 환경을 공식 지원합니다.)

```bash
pip install vllm
```

Python 코드 내에서 LLM 모델을 로드하고 추론을 실행하는 것은 매우 직관적입니다.

```python
from vllm import LLM, SamplingParams

# 모델 로드 (Hugging Face 허브 또는 로컬 경로)
llm = LLM(model="meta-llama/Meta-Llama-3-8B-Instruct", tensor_parallel_size=4)

# 샘플링 파라미터 설정
sampling_params = SamplingParams(temperature=0.7, top_p=0.95, max_tokens=1024)

# 프롬프트 리스트
prompts = [
    "대한민국의 수도는 어디인가요?",
    "vLLM에 대해서 설명해주세요.",
]

# 추론 실행
outputs = llm.generate(prompts, sampling_params)

# 결과 출력
for output in outputs:
    prompt = output.prompt
    generated_text = output.outputs[0].text
    print(f"Prompt: {prompt!r}, Generated text: {generated_text!r}")
```

또한, 다음 명령어를 통해 OpenAI 호환 API 서버를 쉽게 실행할 수 있습니다.

```bash
python -m vllm.entrypoints.openai.api_server \
    --model meta-llama/Meta-Llama-3-8B-Instruct \
    --tensor-parallel-size 4
```

---

### 결론

vLLM은 **PagedAttention**과 **연속적인 배치 처리**라는 핵심 기술을 바탕으로 LLM 서빙의 효율성을 크게 향상시킨 강력한 도구입니다. 높은 처리량과 효율적인 메모리 관리를 통해 비용을 절감하고 사용자에게 더 빠른 응답을 제공할 수 있어, 실시간 AI 서비스 및 대규모 LLM 애플리케이션 구축에 필수적인 프레임워크로 자리 잡고 있습니다.
