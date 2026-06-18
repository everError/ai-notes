# 40 · 에이전트 루프 패턴 — ReAct vs Plan-Execute-Reflect

## "에이전트"란?

여기서 **에이전트**는 LLM이 단순 한 번 답하는 게 아니라, **여러 단계를 거쳐 답을 만드는 시스템**:

- 도구 호출 (검색·계산·외부 API)
- 결과 보고 다음 행동 결정
- 필요 시 재시도·자기 정정

여러 패턴이 있는데, RAG 시스템 관점에서 두 큰 흐름을 비교합니다.

## 패턴 1 — ReAct

**Re**asoning + **Act**ing 루프. LLM이 매 스텝마다 "다음에 뭘 해야 하지?"를 직접 결정.

```
LLM ─→ "다음 행동: search('X')"
  ↓ 도구 호출
결과 ─→ LLM
  ↓
LLM ─→ "다음 행동: search('Y')"
  ↓
... → 만족하면 최종 답변
```

장점:
- 유연함 — 새로운 상황에 자가 적응
- 도구 카탈로그가 풍부할 때 강력

단점:
- **작은 모델에서 도구 선택 실수 잦음**
- 호출 횟수 가변 → 응답 시간 들쑥날쑥
- 토큰 비용 ↑ (매 스텝마다 도구 카탈로그·이전 결과 다 prompt에 들어감)
- 무한 루프 위험 (도구 호출 한도로 막아야)

## 패턴 2 — Plan-Execute-Reflect

LLM이 **시작할 때 한 번 청사진을 짜고**, 그 다음은 코드가 결정론적으로 실행. 마지막에 검증.

```
1. Plan:    LLM이 sub-question 리스트 + 어떤 도구를 호출할지 청사진
2. Execute: 코드가 plan대로 도구 N번 호출 (LLM 자율 호출 X)
3. Compose: LLM이 모든 컨텍스트를 받아 구조화 답변
4. Reflect: 답변이 잘 됐는지 검증 (휴리스틱 + 옵션 LLM)
            부족하면 부족한 부분만 다시 검색·합성
```

장점:
- 호출 횟수 명확 (단순 1~2회, 복합 3~4회)
- 디버그 쉬움 — Plan/Execute/Compose 각 단계 분리
- 작은 모델에서 안정적 (LLM 자유도가 낮음)

단점:
- Plan이 잘못되면 전체 망가짐
- 동적 적응 약함

## 비교표

| 기준 | ReAct | Plan-Execute-Reflect |
|---|---|---|
| LLM 호출 횟수 | 가변 (5~10회) | 고정 (단순 2, 복합 3~4) |
| 다음 행동 결정 | 매 턴 LLM | 시작 시 한 번 |
| 작은 모델 안정성 | △ | ◎ |
| 단순 질문 효율 | ○ (적당) | ◎ (자동 짧은 경로) |
| 복합 질문 강력함 | ○ | ◎ (sub-question 분해) |
| 토큰 비용 | 가변·높음 | 고정·낮음 |
| 디버그 용이성 | △ | ◎ |
| 동적 상황 적응 | ◎ | △ |

## 하이브리드 — Plan에서 자동 분기

같은 코드 골격으로 단순/복합 질문을 자동 분기:

```
 Plan (LLM 1회) — mode 분류 포함
    ↓
    ├─ chitchat       → 검색·합성 모두 스킵, 짧은 직답
    ├─ out_of_scope   → 거절 메시지
    ├─ clarify        → 사용자에게 되묻기
    ├─ single_pass    → 검색 1번 → Compose
    └─ multi_step     → sub_questions 분해
                        → 각 검색 → union (dedupe)
                        → Compose
                        → LLM verify
                        → 부족하면 1라운드 repair
```

핵심: **`mode` 분류 자체가 Plan의 일부**라 정규식·룰 기반 라우터 불필요. LLM이 의도와 복잡도를 한 번에 판단.

호출 횟수 추정:

| 시나리오 | LLM 호출 |
|---|---|
| 인사 (chitchat) | 2회 (Plan 1 + 직답 1) |
| 단일 사실 질문 (single_pass) | 2회 (Plan + Compose) |
| 사례 비교 (multi_step, 검증 통과) | 3회 (Plan + Compose + Verify) |
| 보강 필요 | 4회 (+ 재합성) |

## Plan 출력 예시

질문: `"최근 게이지 박리 사례 비교해줘"`

```json
{
  "mode": "multi_step",
  "sub_questions": [
    {"query": "게이지 박리 원인", "aspect": "원인"},
    {"query": "게이지 박리 대책", "aspect": "대책"},
    {"query": "최근 검사 지그 개선 사례", "aspect": "관련 사례"}
  ],
  "expected_components": ["data-table", "data-document"]
}
```

질문: `"안녕"`

```json
{
  "mode": "chitchat",
  "sub_questions": [],
  "expected_components": []
}
```

## Reflect의 종류 (되돌아가는 거리 순)

| 단계 | 트리거 | 비용 | 효과 |
|---|---|---|---|
| **휴리스틱 검증** | 항상 | 0 (코드) | 환각 인용 차단, 빈 component 제거 |
| **검색만 재시도** | 빈 hit | +0~1 (rewrite LLM) | 동의어로 재검색 |
| **합성만 재시도** | 형식 깨짐 | LLM 1회 | 답변 재작성 |
| **Plan 재작성** | 거의 없음 | LLM 2~3회 | 위험 (무한루프) |

휴리스틱 검증이 가장 ROI 높은 단계 — LLM 호출 0회로 환각 인용을 차단합니다.

## 무한루프 방지

Reflect의 무한루프 위험은 max-iter로 차단:

```python
MAX_REPAIR_ROUNDS = 1   # 한 번만 되돌림
```

→ 최악 케이스도 LLM 호출이 5회 안에 끝남. 단순 질문 1회.

## 다른 패턴 (참고)

- **Self-Ask**: LLM이 sub-question을 만들고 자기가 답함 (도구 사용 X)
- **Tool-augmented LLM**: 단일 도구 결과를 받아 답하는 가벼운 케이스
- **Multi-agent**: 여러 에이전트가 협업 (router + worker, 또는 토론)
- **Reflexion**: Plan-Execute-Reflect의 학술적 형식화 (재학습 포함)

## 외부 학습 자원

- [ReAct 논문 (2022)](https://arxiv.org/abs/2210.03629) — Yao et al.
- [Plan-and-Solve Prompting (2023)](https://arxiv.org/abs/2305.04091)
- [Anthropic — Building effective agents (2024)](https://www.anthropic.com/research/building-effective-agents) — 단순한 게 강하다는 디자인 철학
- [Reflexion (2023)](https://arxiv.org/abs/2303.11366)
- [LangGraph — 에이전트 프레임워크](https://langchain-ai.github.io/langgraph/)
- [PydanticAI](https://ai.pydantic.dev/) — 타입 안전 에이전트 프레임워크
