# AI 에이전트 Skills 학습 정리

## 1. Skills란 무엇인가

### 정의

Skills는 AI 에이전트에게 **특정 작업에 대한 전문성**을 부여하는 구조화된 지침 모듈이다. 단순한 프롬프트가 아니라 "지시사항(instructions) + 도구(tools) + 참조 지식(knowledge) + 정책(policies)"이 하나의 패키지로 묶인 형태이다.

핵심 파일은 **SKILL.md**이며, 이 안에 특정 태스크에 대한 베스트 프랙티스가 담겨 있다.

### MCP와의 차이

| 구분      | MCP (Model Context Protocol)                     | Skills                                   |
| --------- | ------------------------------------------------ | ---------------------------------------- |
| 역할      | "뭘 할 수 있느냐" (능력 확장)                    | "얼마나 잘 하느냐" (품질 향상)           |
| 예시      | DB 연결, API 호출, 파일시스템 접근               | 문서 포맷팅 규칙, 코드 컨벤션, 분석 절차 |
| 작동 방식 | 도구 호출 → JSON 스키마 → 응답 파싱 왕복         | 프롬프트에 한 번 로드 → LLM이 내재화     |
| 토큰 효율 | 매 호출마다 tool description 토큰 소비           | 한 번 로드 후 외부 호출 왕복 없음        |
| 비유      | 레시피를 검색하며 한 단계씩 따라하는 초보 요리사 | 레시피를 체화한 숙련된 요리사            |

**Skills와 MCP는 대체 관계가 아니라 상호 보완 관계이다.**

- MCP → 외부 데이터/서비스 연결 (능력)
- Skills → 작업 품질/일관성 보장 (전문성)
- Function Call → 둘을 연결하는 실행 메커니즘

---

## 2. Skills의 작동 흐름

```
1. 사용자 프롬프트 입력
       ↓
2. 시스템 프롬프트에 스킬 목록(이름 + 설명)이 이미 포함되어 있음
   → 검색이 아니라 "매칭"에 가까움
       ↓
3. LLM이 요청을 보고 어떤 스킬이 필요한지 판단
       ↓
4. 해당 SKILL.md 파일을 읽어옴 (Lazy Loading)
   → 전체 스킬을 다 로드하면 토큰 낭비이므로, 목록만 보여주고 필요한 것만 로드
       ↓
5. 읽은 내용을 컨텍스트에 넣고 그 지침대로 작업 수행
   → "프롬프트에 변환"이 아니라 "그대로 읽고 따른다"
       ↓
6. 필요하면 MCP/도구 호출 (bash, 파일 생성 등)
       ↓
7. 결과물 출력
```

### 핵심 포인트

- **스킬 목록은 항상 컨텍스트에 존재**: 시스템 프롬프트의 `<available_skills>` 블록에 이름과 설명이 포함
- **Lazy Loading 구조**: 목록(가벼움)은 항상 로드, 상세 내용(무거움)은 필요할 때만 로드
- **LLM의 자율 판단**: 사용자가 스킬을 지정하는 게 아니라, AI가 자동으로 적절한 스킬을 선택

---

## 3. Skills에 필요한 실행 환경

Skills의 프롬프트 부분은 어디서든 재현 가능하지만, Claude 수준의 완전한 Skills 실행에는 다음 인프라가 필요하다.

### Claude가 갖추고 있는 인프라

| 구성 요소                 | 역할                                     | 예시                          |
| ------------------------- | ---------------------------------------- | ----------------------------- |
| **리눅스 컨테이너**       | bash 실행, 파일 생성/편집, pip/npm 설치  | python-docx, python-pptx 실행 |
| **파일시스템 파이프라인** | 사용자 파일 수신 → 처리 → 결과물 반환    | uploads → outputs 경로        |
| **아티팩트 렌더러**       | HTML, React, Mermaid, SVG 등 즉시 렌더링 | 프론트엔드 뷰어               |
| **외부 도구 연동**        | 웹 검색, 이미지 검색 등                  | 백엔드 API 연동               |

### 로컬에서 재현할 때 필요한 것

```
┌─────────────────────────────────────┐
│           프론트엔드 (WebUI)          │
│  파일 업/다운로드, 아티팩트 렌더링      │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│      에이전트 (LangGraph + LLM)      │
│  스킬 라우팅 → 프롬프트 주입 → 도구호출  │
└──────┬───────────┬──────────────────┘
       │           │
┌──────▼───┐ ┌─────▼─────────────────┐
│ MCP 서버  │ │ Docker 샌드박스        │
│ 검색/API  │ │ bash, python, npm     │
│ DB 접근   │ │ 파일 생성/편집          │
└──────────┘ └───────────────────────┘
```

- **코드 실행 샌드박스**: Docker 컨테이너, Open Interpreter, LangChain PythonREPLTool 등
- **파일 I/O 파이프라인**: 사용자 업로드 → 컨테이너 마운트 → 결과물 다운로드
- **프론트엔드 렌더링**: HTML/React/SVG를 브라우저에서 바로 보여줄 뷰어
- **외부 도구 연동**: SerpAPI, Tavily 등을 MCP 서버나 function call로 연결

---

## 4. 사람들이 Skills로 만드는 것들

### 대표적인 사용 사례

- **문서 생성 품질 관리**: 회사 보고서 포맷, 톤, 구조를 skill로 등록 → 매번 설명 불필요
- **코드 컨벤션**: 팀 프레임워크 패턴, 에러 처리 규칙 등 반복 설명이 필요한 지식
- **분석 워크플로우**: 재무 데이터 분석 순서, 필수 포함 지표 등 표준화된 절차
- **도메인 전문 지식**: 법률 문서 작성법, 의료 보고서 형식, 산업 용어 규칙
- **콘텐츠 작성**: 뉴스레터 톤앤보이스, 타겟 독자 정의, 구조 규칙

### Skills의 진짜 가치

- 한두 번 하는 일이면 그냥 프롬프트에 설명하면 됨
- **같은 유형의 작업을 반복적으로 할 때**, skill로 등록해두면 일관된 품질이 나옴
- "반복적으로 높은 품질이 필요한 작업"에서 효과가 극대화됨

---

## 5. 플랫폼별 Skills 현황

### Claude Skills (Anthropic) — 현재 가장 성숙

- 폴더 기반 (SKILL.md) 구조
- AI가 자동으로 필요한 스킬을 판단하고 활성화
- 여러 스킬 동시 사용 가능 (Composable)
- Claude 앱, Claude Code, API에서 동일 포맷 사용 (Portable)
- 오픈소스로 스킬 프레임워크 공개

### GPT Custom GPTs (OpenAI) — 기존 방식

- 사용자가 수동으로 어떤 GPT를 쓸지 선택해야 함
- 8,000자 커스텀 인스트럭션 제한
- GPT Store를 통한 배포 가능
- 하나의 GPT만 활성화 가능 (조합 불가)

### ChatGPT Skills (OpenAI) — 개발 중

- 내부 코드명 "Hazelnut"
- Claude Skills와 유사한 모듈식 접근
- 슬래시 커맨드로 접근, 스킬 에디터 제공 예정
- Custom GPT를 스킬로 변환하는 옵션 포함
- 합성 가능(composable), 이식 가능(portable), 코드 실행 지원
- OpenAI Codex에서는 이미 `.agents/skills/` 디렉토리 기반 스킬 로드를 지원

### 비교 요약

|        | Claude Skills        | GPT Custom GPTs      | ChatGPT Skills (개발중) |
| ------ | -------------------- | -------------------- | ----------------------- |
| 활성화 | AI 자동 판단         | 사용자 수동 선택     | AI 자동 + 슬래시 커맨드 |
| 구조   | 폴더 기반 (SKILL.md) | 프롬프트 + 지식 파일 | 모듈식, 코드 실행 포함  |
| 조합   | 여러 스킬 동시 사용  | 하나씩만             | 합성 가능               |
| 이식성 | 앱/Code/API 공유     | ChatGPT 내부만       | 웹/앱/API 공유 예정     |

---

## 6. Skills 생태계 — 마켓플레이스의 등장

### SKILL.md = 업계 공통 표준

Anthropic이 정의한 SKILL.md 포맷이 사실상의 표준(de facto standard)으로 자리잡고 있다. Claude Code, Cursor, OpenCode, Windsurf, Cline, Roo Code, Aide, Augment 등 대부분의 AI 코딩 에이전트가 이 포맷을 지원한다.

### 주요 마켓플레이스

**ClawHub (clawhub.ai)**

- OpenClaw의 공식 스킬 레지스트리
- 3,000개 이상의 커뮤니티 빌트 스킬
- 벡터 검색 기반 시맨틱 검색
- CLI 통합, 시맨틱 버저닝 지원
- "AI 에이전트의 npm"이라 불림

**SkillHub (skillhub.club)**

- 7,000개 이상의 AI 평가 스킬
- Claude, Codex, Gemini, OpenCode 등 다양한 에이전트 지원
- 5가지 차원(Practicality, Clarity, Automation, Quality, Impact) AI 평가
- S-rank, A-rank 등급 시스템

**SkillsMP (skillsmp.com)**

- GitHub 레포지토리에서 자동 수집
- 최소 2스타 이상 필터링
- 카테고리 분류, 인기도 정렬

### OpenClaw — 로컬 AI 어시스턴트 + Skills

- 오픈소스, 셀프호스팅 방식의 개인 AI 어시스턴트
- Anthropic Claude, OpenAI GPT, Ollama 로컬 모델 지원
- Ollama로 완전 오프라인 실행 가능
- 셸 명령 실행, 파일 읽기/쓰기, 웹 브라우저 제어, 50개 이상 서비스 통합
- ClawHub에서 스킬을 설치하여 기능 확장

### 보안 주의사항

- 2026년 2월 ClawHavoc 사건: 341개 악성 스킬이 멀웨어 유포
- npm 생태계 초기와 동일한 패턴의 보안 위협
- **스킬 코드 리뷰, 작성자 평판 확인, VirusTotal 보고서 점검 필수**

---

## 7. 로컬 환경에서 Skills 구현하기

### 방법 1: 프롬프트 기반 스킬 등록 (가장 간단)

```python
skills_registry = {
    "pptx": "skills/pptx/SKILL.md",
    "docx": "skills/docx/SKILL.md",
    "code_review": "skills/code_review/SKILL.md",
}

def load_skill(skill_name: str) -> str:
    with open(skills_registry[skill_name]) as f:
        return f.read()

# 시스템 프롬프트에 동적 주입
system_prompt = base_prompt + "\n" + load_skill("docx")
```

### 방법 2: LangGraph에서 스킬을 노드로 구성

```python
from langgraph.graph import StateGraph

def skill_router(state):
    query = state["query"]
    if "프레젠테이션" in query:
        return "pptx_skill"
    elif "문서" in query:
        return "docx_skill"
    return "general"

def pptx_skill_node(state):
    skill_prompt = load_skill("pptx")
    response = llm.invoke(skill_prompt + state["query"])
    return {"result": response}

graph = StateGraph(State)
graph.add_node("router", skill_router)
graph.add_node("pptx_skill", pptx_skill_node)
graph.add_conditional_edges("router", skill_router, {...})
```

### 방법 3: 도구로 등록하여 LLM이 자율 로드

```python
# Claude 방식과 가장 유사
SKILL_REGISTRY = {
    "pptx": {
        "description": "프레젠테이션 생성 시 사용",
        "path": "skills/pptx/SKILL.md"
    },
    "docx": {
        "description": "워드 문서 생성 시 사용",
        "path": "skills/docx/SKILL.md"
    },
}

# 시스템 프롬프트에 목록만 포함 (토큰 절약)
skill_list = "\n".join(
    f"- {k}: {v['description']}" for k, v in SKILL_REGISTRY.items()
)
system_prompt = f"""당신은 에이전트입니다.
사용 가능한 스킬:
{skill_list}
작업 전에 적절한 스킬을 load_skill 도구로 읽으세요."""

@tool
def load_skill(skill_name: str) -> str:
    """스킬의 상세 지침을 로드합니다"""
    path = SKILL_REGISTRY[skill_name]["path"]
    with open(path) as f:
        return f.read()
```

### 방법 4: OpenClaw + ClawHub 활용

로컬에 직접 인프라를 구축하지 않고, OpenClaw을 설치하면 ClawHub의 3,000개 이상의 스킬을 바로 활용할 수 있다. Ollama 로컬 모델도 지원하므로, 로컬 LLM + Skills 조합의 가장 현실적인 시작점이 될 수 있다.

### 로컬 모델 사용 시 고려사항

- **컨텍스트 윈도우 크기**: 스킬 문서가 길면 작은 모델에선 부담. 간결하게 유지하거나 RAG로 필요한 부분만 로드
- **모델 능력**: 7B~70B급 모델은 복잡한 스킬 지시사항을 정확하게 따르지 못할 수 있음. 스킬 문서를 단순명확하게 작성
- **스킬 라우팅**: LLM 판단에만 의존하지 말고, 키워드 매칭이나 classifier를 보조로 사용하면 안정적

---

## 8. 핵심 요약

> **Skills의 본질은 "적시에 로드되는 전문 프롬프트"이다.**

1. Skills는 특정 플랫폼에 종속된 기능이 아니라 **에이전트 아키텍처 패턴**이다
2. MCP가 능력을 주고, Skills가 품질을 준다 — 둘은 상호 보완 관계
3. SKILL.md 포맷이 업계 표준으로 수렴하고 있다
4. Claude, OpenAI(개발중), OpenClaw 등 주요 플랫폼이 모두 채택 중
5. ClawHub, SkillHub 같은 마켓플레이스에서 커뮤니티 스킬을 바로 활용 가능
6. 로컬 환경(Ollama + LangGraph)에서도 충분히 구현 가능하며, OpenClaw이 좋은 출발점

---

_작성일: 2026-02-13_
_학습 대화 기반 정리_
