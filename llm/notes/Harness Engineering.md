# Harness Engineering 종합 가이드

## 1. 정의

**Harness Engineering**은 AI 에이전트가 장시간 복잡한 작업을 자율적으로 수행할 때, 안정적으로 성공하도록 감싸주는 전체 인프라를 설계하는 엔지니어링 분야다.

핵심 공식: **Agent = Model + Harness**

모델 자체가 아닌, 모델을 둘러싼 모든 것 — 시스템 프롬프트, 도구 오케스트레이션, 실행 흐름, 미들웨어 훅, 상태 관리, 피드백 루프 — 이 harness의 영역이다.

"harness"라는 용어는 마구(馬具)에서 왔다. 말은 강력하고 빠르지만, 고삐와 안장 없이는 원하는 곳으로 가지 않는다. LLM이 말이라면, harness는 그 힘을 생산적으로 방향 잡아주는 장비이고, 엔지니어는 방향을 제시하는 기수다.

> "하네스 엔지니어링은 AI 에이전트를 제어하기 위한 도구와 관행이다." — Martin Fowler

---

## 2. 왜 필요한가

LLM은 단일 프롬프트에서는 인상적이지만, 복잡한 장시간 작업에서는 반복적으로 실패한다.

### 핵심 실패 모드 4가지

| 실패 모드                       | 설명                                                     | 예시                                                                 |
| ------------------------------- | -------------------------------------------------------- | -------------------------------------------------------------------- |
| **한 번에 다 하려는 경향**      | 전체 프로젝트를 한 세션에 시도하다 구조가 무너짐         | "쇼핑몰 만들어줘" → 로그인, 상품, 결제를 동시에 시도하다 중간에 꼬임 |
| **컨텍스트 소실**               | 토큰이 쌓이면 앞에서 한 작업을 잊거나 맥락이 흐려짐      | 앞에서 만든 API 스키마를 뒤에서 다른 형태로 만듦                     |
| **자기 평가 실패**              | 자기가 만든 결과물을 거의 항상 "잘 됐다"고 평가          | 버그 있는 코드를 검토시키면 "잘 작동합니다"라고 답변                 |
| **조기 종료 (Context Anxiety)** | 컨텍스트 한계가 다가온다고 판단하면 작업을 서둘러 마무리 | 기능 3개 남았는데 "여기까지가 적당합니다"라며 종료                   |

### 수치로 보는 harness의 효과

LangChain은 모델을 전혀 바꾸지 않고 harness만 개선해서 Terminal Bench 2.0 벤치마크에서 **52.8% → 66.5%** (Top 30 → Top 5)로 도약했다. 실패율은 62% 감소하고, 멀티스텝 작업 완수율은 약 80% 증가했다. 같은 모델(GPT-5.2-Codex)이 harness 하나로 완전히 다른 성능을 보여준 사례다.

---

## 3. 핵심 구성요소 5가지

### 3.1 도구 오케스트레이션 (Tool Orchestration)

에이전트의 역량은 접근 가능한 도구로 정의된다. 파일시스템, 셸 명령, API 호출, DB 질의, 외부 서비스 등을 어떤 권한으로 어떻게 호출할지 설계한다.

**실무 예시 — LangChain DeepAgents의 LocalContextMiddleware:**
에이전트 시작 시 작업 디렉토리를 자동 탐색하고, Python 설치 경로, 사용 가능한 도구 등을 미리 매핑해서 에이전트 컨텍스트에 주입한다.

```python
class LocalContextMiddleware:
    def on_agent_start(self, context):
        context["cwd_structure"] = map_directory(".")
        context["python_path"] = find_tool("python3")
        context["available_tools"] = discover_tools()
```

### 3.2 가드레일과 안전 제약 (Guardrails & Safety)

에이전트가 해서는 안 되는 행동을 구조적으로 막는다.

- **권한 경계**: 접근 가능한 파일, 디렉토리, 명령어 제한
- **검증 체크**: 린터, 타입 체커, 테스트 스위트로 출력 검증
- **아키텍처 제약**: 의존성 경계, 네이밍 규칙, 디렉토리 구조 강제
- **루프 감지**: 에이전트가 같은 행동을 반복하는 무한 루프 방지

**핵심 통찰**: 제약이 많을수록 신뢰성이 올라간다. OpenAI의 Codex 팀은 엄격한 아키텍처 경계를 린터와 검증기로 강제했을 때 에이전트 성능이 더 좋아졌다고 보고했다.

```python
class SafetyMiddleware:
    BLOCKED_COMMANDS = ["rm -rf", "DROP TABLE", "curl | bash"]
    ALLOWED_DIRS = ["./src", "./tests", "./docs"]

    def before_tool_call(self, tool_name, args):
        if tool_name == "shell":
            for cmd in self.BLOCKED_COMMANDS:
                if cmd in args["command"]:
                    return BLOCK("위험한 명령어 차단됨")
        if tool_name == "file_write":
            if not any(args["path"].startswith(d) for d in self.ALLOWED_DIRS):
                return BLOCK("허용되지 않은 디렉토리")
        return ALLOW
```

### 3.3 에러 복구와 피드백 루프 (Error Recovery & Feedback Loops)

에이전트는 반드시 실패한다. 핵심은 우아하게 실패하고 자동으로 복구하는 것이다.

- **자기 검증 루프**: 코드 작성 → 테스트 실행 → 실패 시 수정 → 재테스트
- **롤백 메커니즘**: 변경이 기존 기능을 깨뜨리면 이전 상태로 복구
- **루프 감지**: 같은 실패를 3회 이상 반복하면 전략 전환

**LangChain의 핵심 발견**: 에이전트의 가장 흔한 실패 패턴은 코드를 작성하고, 자기 코드를 다시 읽고, 괜찮아 보인다고 판단하고 멈추는 것이다. "네 코드를 리뷰해"가 아니라 "원본 스펙 대비 검증해"로 바꾸자 성능이 크게 개선되었다.

### 3.4 관찰 가능성 (Observability)

보이지 않는 것은 개선할 수 없다. 에이전트의 모든 행동을 로깅하고 추적한다. 매 도구 호출과 결과, 토큰 사용량과 비용, 의사결정 근거, 재시도 횟수와 실패 패턴, 작업 소요 시간을 기록한다.

LangChain은 LangSmith를 사용해 에이전트의 모든 행동을 trace로 기록하고, 이 trace를 분석해서 harness 개선 포인트를 찾는 반복 사이클을 돌렸다.

### 3.5 Human-in-the-Loop 체크포인트

완전한 자율은 거의 적절하지 않다. 실수의 비용이 가장 높은 지점에 사람의 판단을 배치한다. 프로덕션 데이터 변경, 인프라 수정, 보안 설정 변경, 새 패키지 설치 등에 승인 게이트를 건다.

---

## 4. 설계 원칙: 실패 모드별 해법

### 4.1 작업 분해 → "한 번에 다 하려는 경향" 해결

Planner 에이전트가 한 줄짜리 프롬프트를 200개 이상의 feature 체크리스트로 분해하고, Generator 에이전트가 하나씩 구현한다.

```
사용자: "2D 레트로 게임 메이커 만들어줘"

Planner 출력:
├── Sprint 1: 프로젝트 대시보드 (5 features)
├── Sprint 2: 타일 기반 레벨 에디터 (27 criteria)
├── Sprint 3: 픽셀아트 스프라이트 에디터
├── Sprint 4~10: 애니메이션, 행동시스템, 플레이모드, 사운드, AI생성, 내보내기
```

### 4.2 세션 간 상태 전달 → "컨텍스트 소실" 해결

| 전략                        | 설명                                                 | 장점               | 단점                   |
| --------------------------- | ---------------------------------------------------- | ------------------ | ---------------------- |
| **Compaction**              | 오래된 대화를 요약해서 같은 세션 내에서 축약         | 연속성 유지        | Context anxiety 미해소 |
| **Context Reset + Handoff** | 세션을 완전히 끊고, 핸드오프 아티팩트로 새 세션 시작 | 깨끗한 상태 재시작 | 오케스트레이션 복잡성  |

**핸드오프 아티팩트 예시 (progress.md):**

```markdown
## 프로젝트 상태: 쇼핑몰 웹앱

### 완료된 Feature

- [x] 사용자 인증 (JWT 기반)
- [x] 상품 목록 페이지 (페이지네이션 포함)

### 다음 에이전트가 알아야 할 것

- DB는 PostgreSQL, 스키마는 backend/migrations/ 참조
- 프론트엔드 상태관리는 Zustand 사용 중

### 남은 작업

- [ ] 장바구니 API → 결제 연동 → 주문 내역
```

### 4.3 생성-평가 분리 → "자기 평가 실패" 해결

GAN에서 영감을 받은 구조로, 만드는 에이전트와 평가하는 에이전트를 분리한다.

```
Planner → [Sprint Contract 협상] → Generator 구현 → Evaluator 테스트
                                         ↑                    │
                                         └── 기준 미달 시 ────┘
```

**Evaluator가 실제로 잡아낸 버그 (Anthropic 실험):**

| 기준                                     | 판정                                                                                    |
| ---------------------------------------- | --------------------------------------------------------------------------------------- |
| 사각형 채우기 도구로 드래그 시 영역 채움 | **FAIL** — 시작/끝 지점에만 타일 배치. fillRectangle이 mouseUp에서 미트리거             |
| 엔티티 스폰 포인트 선택 후 삭제          | **FAIL** — Delete 핸들러가 selection+selectedEntityId 모두 요구하나 클릭 시 하나만 설정 |
| API로 애니메이션 프레임 순서 변경        | **FAIL** — /frames/reorder가 /{frame_id} 뒤에 정의. 'reorder'를 정수 파싱 시도하여 422  |

### 4.4 규칙과 제약 → 일관성 확보

| 플랫폼       | 규약 파일     | 역할                                          |
| ------------ | ------------- | --------------------------------------------- |
| Claude Code  | CLAUDE.md     | 프로젝트 컨벤션, 디렉토리 구조, 아키텍처 제약 |
| OpenAI Codex | AGENTS.md     | 목차 역할. 실제 지식은 docs/에 저장           |
| Cursor       | .cursor/rules | 파일 패턴별 마크다운 설정, 버전 관리          |
| DeepAgents   | system prompt | Claude Code 방식을 모델링한 상세 프롬프트     |

---

## 5. 실전 사례

### 5.1 Anthropic — Planner → Generator → Evaluator

"2D 레트로 게임 메이커" 한 줄 프롬프트로 전체 앱 생성:

| 구성                    | 시간  | 비용 | 결과                                         |
| ----------------------- | ----- | ---- | -------------------------------------------- |
| Solo (1 agent)          | 20분  | $9   | 기본 UI만 동작, 게임 플레이 불가             |
| Full Harness (3 agents) | 6시간 | $200 | 16 features, 게임 플레이 + AI 생성 기능 포함 |

후속 DAW 실험 (Opus 4.6, 간소화 harness): 3시간 50분, $124.70. sprint 구조 제거했지만 QA 에이전트는 여전히 "기능은 있지만 인터랙션이 안 되는" 문제를 잡아냄.

### 5.2 OpenAI — Repository-as-Knowledge-Base

Codex 에이전트로 100만 줄 코드베이스를 인간 작성 코드 0줄로 구축:

- AGENTS.md는 ~100줄 목차. 실제 지식은 docs/에 저장
- "에이전트가 접근할 수 없는 것은 존재하지 않는 것" 원칙
- 3명 엔지니어 → 하루 3.5 PR 머지. 7명으로 성장 시 throughput 더 증가

Martin Fowler의 분류로 보면: Context Engineering + Architectural Constraints + Garbage Collection(주기적 정리 에이전트)

### 5.3 LangChain — Terminal Bench 2.0 개선

GPT-5.2-Codex 고정, harness만 변경. 최적화 대상은 System Prompt, Tools, Middleware 세 가지.

핵심 인사이트:

- **자기 검증 vs 자기 리뷰**: "코드 리뷰해" → "괜찮습니다". "원본 스펙 대비 검증해" → 누락 발견
- **환경 탐색 비용**: 미리 주입하면 에이전트가 환경 파악에 쓰는 시간/토큰 제거
- **추론 예산 역설**: xhigh(최대) 53.9% < high 63.6%. 계획/검증에만 고추론, 구현은 절약하는 "추론 샌드위치" 효과적

---

## 6. 관련 개념 비교

| 구분 | 에이전트 프레임워크   | 에이전트 하네스                  |
| ---- | --------------------- | -------------------------------- |
| 역할 | 에이전트 부품 제공    | 완성된 실행 시스템               |
| 비유 | 공구함                | 시공 설계도 + 공구함 + 검수 체계 |
| 예시 | LangChain, LlamaIndex | Claude Agent SDK, DeepAgents     |

- **프롬프트 엔지니어링** = "우회전" 명령 → **하네스 엔지니어링** = 도로 + 가드레일 + 표지판 + 교통 시스템
- **오케스트레이터** = 뇌 (언제/어떻게 호출할지) → **하네스** = 손과 환경 (도구 제공 + 입출력 관리)

---

## 7. 첫 하네스 구축 6단계

1. **스코프 정의** — 할 수 있는 것 / 절대 안 되는 것 문서화
2. **규약 파일 작성** — CLAUDE.md / AGENTS.md / .cursorrules 등
3. **피드백 루프** — 최소한 write → test → fix 사이클 강제
4. **가드레일** — 파일 접근 제한, 린팅 필수, 파괴적 명령어 차단
5. **관찰 가능성** — 행동, 도구 호출, 토큰 사용량 로깅
6. **Human 체크포인트** — 고위험 행동에 승인 게이트

---

## 8. LangGraph 구현 예시

```python
from langgraph.graph import StateGraph, END
from langgraph.checkpoint.memory import MemorySaver

workflow = StateGraph(AppState)

workflow.add_node("planner", planner_agent)      # 요청 → feature 분해
workflow.add_node("generator", generator_agent)   # feature 1개 구현
workflow.add_node("evaluator", evaluator_agent)   # 실제 테스트 + 점수
workflow.add_node("update_progress", update_log)  # progress.md 갱신

workflow.set_entry_point("planner")
workflow.add_edge("planner", "generator")
workflow.add_edge("generator", "evaluator")
workflow.add_conditional_edges("evaluator", route_after_eval, {
    "retry": "generator",          # 점수 미달 → 피드백과 함께 재시도
    "next_feature": "update_progress"  # 통과 → 다음 feature
})
workflow.add_conditional_edges("update_progress", check_remaining, {
    "continue": "generator",
    "done": END
})

app = workflow.compile(checkpointer=MemorySaver())
```

---

## 9. 도구 생태계 (2026)

| 도구                 | 유형            | 특징                                   |
| -------------------- | --------------- | -------------------------------------- |
| Claude Agent SDK     | 내장형 하네스   | 권한 모델, 훅 시스템, 자동 compaction  |
| LangChain DeepAgents | 오픈소스 하네스 | MIT, 모델 무관, 계획/파일/서브에이전트 |
| OpenAI Codex SDK     | 내장형 하네스   | OS 샌드박싱, AGENTS.md 패턴            |
| LangGraph            | 런타임          | 상태 그래프, 체크포인터, 멀티스텝      |
| CrewAI               | 멀티에이전트    | 전문 에이전트 협업, Flows 파이프라인   |
| Cursor               | IDE 통합        | .cursor/rules, 루프 감지               |

---

## 10. 핵심 교훈

**모델이 아니라 하네스가 성능을 결정한다.** 같은 Opus 4.6도 하네스에 따라 점수가 크게 다르다.

**하네스는 모델과 함께 진화한다.** 새 모델이 나오면 불필요한 부분을 제거하고, 새 영역에 구조를 추가한다. "흥미로운 하네스 조합의 공간은 줄어들지 않는다. 이동할 뿐이다." — Anthropic

**다른 모델에는 다른 하네스가 필요하다.** GPT용 하네스를 Claude에 그대로 적용하면 성능이 떨어진다. 모델별 실패 패턴이 다르기 때문이다.

**가장 단순한 것부터 시작한다.** "가능한 가장 단순한 해결책을 찾고, 필요할 때만 복잡성을 추가하라." — Anthropic, Building Effective Agents

---

## 참고 자료

| 자료                                                       | URL                                                                                  |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Anthropic — Effective Harnesses for Long-Running Agents    | https://www.anthropic.com/engineering/effective-harnesses-for-long-running-agents    |
| Anthropic — Harness Design for Long-Running Apps           | https://www.anthropic.com/engineering/harness-design-long-running-apps               |
| OpenAI — Harness Engineering (Codex)                       | https://openai.com/index/harness-engineering/                                        |
| Martin Fowler — Harness Engineering                        | https://martinfowler.com/articles/exploring-gen-ai/harness-engineering.html          |
| LangChain — Improving Deep Agents with Harness Engineering | https://blog.langchain.com/improving-deep-agents-with-harness-engineering/           |
| LangChain — DeepAgents (오픈소스)                          | https://github.com/langchain-ai/deepagents                                           |
| Parallel Web Systems — What Is an Agent Harness?           | https://parallel.ai/articles/what-is-an-agent-harness                                |
| NxCode — Harness Engineering Complete Guide 2026           | https://www.nxcode.io/resources/news/what-is-harness-engineering-complete-guide-2026 |
| arXiv — OpenDev: AI Coding Agents for the Terminal         | https://arxiv.org/abs/2603.05344                                                     |
