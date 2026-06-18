# 41 · Structured Output — LLM에게 정해진 모양으로 답하게 시키기

## 왜 필요한가

LLM은 자유 텍스트를 잘 만들지만, 후처리할 때는 **정해진 JSON 구조**가 훨씬 편합니다:

```
자유 텍스트:
"답변은 ... (긴 markdown) ... 출처: A.pdf 5페이지"
   ↓ 정규식으로 출처 파싱? — 깨짐 위험 ↑

structured:
{
  "answer_md": "답변은 ...",
  "citations": [{"file_name": "A.pdf", "page_no": 5, "snippet": "..."}],
  "confidence": "high"
}
   ↓ 직접 처리·검증·매핑 OK
```

활용:
- 답변 + 인용 + 메타를 한 번에
- 도구 호출 인자 강제
- 분류·라벨링·요약을 일관된 포맷으로

## 강제 방법 3가지

### 1. Prompt-only

LLM에게 "JSON으로 답해, 스키마는 ...":
- 가장 단순
- 작은 모델은 종종 형식 깨뜨림 (앞뒤 설명 붙임, 따옴표 빼먹음)

### 2. Constrained decoding (Grammar)

토큰 생성 단계에서 grammar에 안 맞는 토큰을 차단:
- llama.cpp의 GBNF
- Outlines, lm-format-enforcer
- vLLM의 guided decoding (XGrammar, llguidance)

특성: 거의 100% 형식 보장. 추론 백엔드 지원 필요.

### 3. JSON mode (sampling-time)

OpenAI / Anthropic / Ollama 등 API가 제공: `format: "json"` 또는 `format: <schema>`. 모델이 JSON 형식만 출력하게 sampling 단계에서 제약.

OpenAI:
```python
client.chat.completions.create(
    model="gpt-4o",
    messages=[...],
    response_format={"type": "json_schema", "json_schema": {...}},
)
```

Anthropic Claude (tool use 형태):
```python
client.messages.create(
    model="claude-3-5-sonnet-...",
    tools=[{"name": "format_answer", "input_schema": {...}}],
    tool_choice={"type": "tool", "name": "format_answer"},
)
```

Ollama:
```python
httpx.post(f"{OLLAMA_URL}/api/chat", json={
    "model": "...",
    "messages": [...],
    "format": json_schema,   # JSON Schema dict
})
```

## Pydantic으로 schema 정의

Python에서 JSON Schema를 직접 손으로 쓰기보다 Pydantic 모델로:

```python
from pydantic import BaseModel
from typing import Literal

class Citation(BaseModel):
    file_name: str
    page_no: int
    snippet: str = ""

class AgentAnswer(BaseModel):
    answer_md: str
    citations: list[Citation] = []
    confidence: Literal["high", "medium", "low"] = "high"

# JSON Schema 자동 생성
schema = AgentAnswer.model_json_schema()

# LLM에 전달
payload = {"format": schema, ...}

# 응답 검증
content = res.json()["message"]["content"]
answer = AgentAnswer.model_validate_json(content)
```

## 방어 코드

JSON mode가 켜져도 모델이 마크다운 코드 펜스를 붙이는 등 깨지는 경우:

```python
import json

try:
    obj = json.loads(content)
except json.JSONDecodeError:
    # 앞뒤 부산물 무시하고 { ... } 만 추출
    s, e = content.find("{"), content.rfind("}")
    if s != -1 and e > s:
        obj = json.loads(content[s : e + 1])
    else:
        raise
```

## 두 단계 검증 (Verification)

LLM 출력이 schema에는 맞아도 **내용이 부정확**할 수 있습니다 (환각 인용, 누락). 두 단계로 검증:

### 휴리스틱 검증 (코드, 비용 0)

```python
def verify_heuristic(answer, hits):
    # 1. 환각 인용 차단
    hit_keys = {(h.file_name, h.page_no) for h in hits}
    answer.citations = [c for c in answer.citations
                        if (c.file_name, c.page_no) in hit_keys]
    
    # 2. 빈 component 제거
    answer.components = [c for c in answer.components if has_real_data(c)]
    
    # 3. 인용이 모두 사라지면 confidence 자동 lowering
    if hits and not answer.citations and answer.answer_md.strip():
        answer.confidence = "medium"
    
    return answer
```

이 단계가 잡는 것:
- LLM이 만들어낸 가짜 file_name → 자동 폐기
- 빈 표/차트 카드 → UI에 빈 카드 안 뜨게
- 답변과 인용 일관성

### LLM 검증 (faithfulness + coverage)

검증자 LLM에게:

```
- 사용자 질문, sub_questions
- 작성된 답변, 인용 목록, 검색 컨텍스트

JSON으로 응답:
{
  "coverage":     {"ok": bool, "missing_aspects": [...]},
  "faithfulness": {"ok": bool, "unfaithful_citations": [...]},
  "overall_confidence": "high" | "medium" | "low"
}
```

- `unfaithful_citations` 에 들어간 건 코드가 답변에서 제거
- `missing_aspects` 가 있으면 그 측면들로 다시 검색 → 재합성 1회

비용: LLM 1회 호출(~수천 토큰). 토큰 비용 vs 답변 신뢰도 trade-off에 따라 항상/때때로/안 함 결정.

## 작은 모델에서 잘 작동시키는 팁

### 1. system prompt에 스키마 의미 설명
`format: schema` 만으로는 모델이 의미 잘 못 잡음. 자연어로도 한 번 더:

> "각 citation의 file_name과 unit_no는 검색 결과에 등장한 값을 그대로 복사해야 합니다."

### 2. temperature 낮게
0.0~0.1. 창의성 X, 일관성 ★.

### 3. few-shot 예시
스키마 매칭이 약하면 예시 1~2개를 prompt에 추가.

### 4. 재시도 코드
검증 실패 시 한 번 다시 호출. 무한루프는 max_attempts로 막기.

### 5. 필드 분리 — 작은 부분만 LLM에게 맡기기
LLM이 모든 필드를 채우게 하지 말고, 자연어로 결정해야 하는 것만(file_name, snippet 등) 채우게 하고 나머지(image_uri, doc_id, download_url)는 코드가 검색 메타에서 매핑.

## 외부 학습 자원

- [Pydantic 공식 문서](https://docs.pydantic.dev/) — JSON Schema 자동 생성
- [Ollama Structured Outputs](https://docs.ollama.com/capabilities/structured-outputs)
- [OpenAI structured outputs](https://platform.openai.com/docs/guides/structured-outputs)
- [Outlines (Constrained decoding)](https://github.com/dottxt-ai/outlines)
- [Instructor — Pydantic + structured output](https://github.com/jxnl/instructor)
- [Anthropic — Tool use & structured output](https://docs.anthropic.com/en/docs/build-with-claude/tool-use)
