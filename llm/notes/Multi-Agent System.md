# 멀티에이전트 시스템 (Multi-Agent System, MAS)

## 1. 정의

복수의 자율적 AI 에이전트가 공유 환경 내에서 상호작용하며, 개별 또는 공동의 목표를 달성하는 분산형 시스템이다. 단일 에이전트로는 처리하기 어려운 대규모·복잡한 작업을 역할 분담과 협력을 통해 해결하는 것이 핵심 목적이다. (IBM, Google Cloud, SAP, Salesforce 등 공통)

## 2. 핵심 특성

Michael Wooldridge의 _Introduction to MultiAgent Systems_(2002)에서 정리하고, 이후 Wikipedia·IBM 등에서 널리 인용되는 세 가지 특성이 있다.

- **자율성(Autonomy)**: 각 에이전트가 독립적으로 판단하고 행동한다. 중앙 기관의 확인 없이 데이터를 수집·처리·실행할 수 있다.
- **로컬 뷰(Local View)**: 어떤 에이전트도 시스템 전체의 글로벌 뷰를 갖지 않는다. 각자 자신의 범위 내에서 정보를 인식한다.
- **탈중앙화(Decentralization)**: 특정 에이전트가 전체를 통제하는 구조가 아니라, 에이전트들의 상호작용으로 시스템 행동이 결정된다.

Salesforce는 여기에 **조정(Coordination)**을 추가한다. 에이전트들이 동기화 상태를 유지하며, 다른 에이전트의 작업에 따라 업데이트를 공유하고 계획을 조정하는 것이다.

## 3. 장점

- **확장성**: 에이전트를 추가·제거하여 시스템 규모를 유연하게 조절할 수 있다.
- **장애 내성**: 한 에이전트가 실패해도 다른 에이전트가 역할을 대신하거나 조정할 수 있어 전체 시스템이 유지된다.
- **모듈성**: 각 에이전트가 독립적으로 개발·유지보수·업그레이드 가능하다.
- **보안 강화**: 에이전트별로 접근 가능한 데이터와 작업 범위를 분리하여 권한 관리를 강화할 수 있다.
- **전문성**: 각 에이전트가 특정 영역에 특화되어 개별 성능 최적화가 가능하다.

## 4. 네트워크 구조

- **중앙집중형**: 중앙 장치가 글로벌 지식 기반을 포함하고 에이전트를 연결·감독한다. 커뮤니케이션이 용이하고 지식이 통일되지만, 중앙 유닛이 실패하면 전체 시스템이 실패한다. (IBM)
- **분산형**: 에이전트들이 인접 에이전트와 직접 정보를 공유한다. 견고성과 모듈성이 장점이지만 조율 복잡도가 높다. (IBM)
- **계층형**: 상위 에이전트가 하위 에이전트를 관리·위임하는 구조. 중앙집중형과 분산형의 중간 지점이다.

## 5. 에이전트 관계 유형

- **협력형(Cooperative)**: 공통 목표를 위해 정보·자원을 공유한다.
- **경쟁형(Competitive)**: 제한된 자원을 두고 상충하는 목표를 가진다.
- **이종형(Heterogeneous)**: 서로 다른 기술·역할·능력을 가진 에이전트가 혼합된다.

## 6. MAS와 에이전틱 워크플로우의 관계

MAS와 에이전틱 워크플로우(Agentic Workflow)는 별개의 개념이다.

- **MAS**: 독립된 에이전트들이 분산 배포·실행·통신하는 **시스템 아키텍처**
- **에이전틱 워크플로우**: 특정 목표 달성을 위해 에이전트가 자율적으로 실행하는 **구조화된 비즈니스 로직** (계획 수립 → 도구 사용 → 실행 → 결과 평가 → 피드백 수정)

IBM은 멀티에이전트 협업을 에이전틱 워크플로우의 구성요소 중 하나로 보고 있으며, UiPath는 에이전틱 워크플로우를 에이전트의 적응성과 워크플로우의 구조를 결합한 하이브리드 접근이라 정의한다.

### 하이브리드 접근

ScienceDirect에 게재된 스마트 제조 논문(Farahani et al., 2026)에서 제시된 구조로, MAS 아키텍처 위에서 에이전틱 워크플로우를 구동하는 방식이다. 전통적 MAS의 경직성을 극복하면서도 단일 에이전트 에이전틱 AI의 병목을 피할 수 있다.

- **MAS 레이어**: 각 에이전트가 독립된 서비스로 배포·실행되는 분산 인프라
- **워크플로우 레이어**: 각 에이전트 내부에서 LLM 기반 추론·계획·도구 호출 등의 실행 로직이 동작
- **통신 레이어**: 독립된 워크플로우들이 A2A 등 표준 프로토콜로 서로 발견·통신

즉, 독립된 에이전틱 워크플로우들이 각각 A2A 서버로 존재하면서 서로 통신하는 구조가 되어야 MAS + 에이전틱 워크플로우 + A2A가 모두 성립한다.

## 7. 통신 프로토콜

### MCP (Model Context Protocol)

Anthropic이 제안. 에이전트와 외부 도구·API 간 연결을 표준화하는 프로토콜이다. 에이전트가 DB, 웹 검색, 파일 시스템 등을 일관된 인터페이스로 호출할 수 있게 해준다.

### A2A (Agent-to-Agent Protocol)

Google이 제안하고 Linux Foundation에 기증. 에이전트 간 통신을 표준화하는 프로토콜이다. 2026년 4월 기준 150개 이상 조직이 지지하며, Azure AI Foundry·Amazon Bedrock AgentCore에 통합되었다. IBM ACP도 A2A에 합류하여 사실상 표준이다.

**핵심 구성요소:**

- **Agent Card**: 에이전트의 이름·기능·인증 방식을 기술한 JSON. 상호 발견 수단.
- **Task**: 에이전트 간 작업 단위. 생명주기 상태를 가짐.
- **Message**: user 또는 agent 역할의 메시지. 텍스트·바이너리·파일·구조화 데이터 혼합 가능.
- **Artifact**: Task의 결과물 (PDF, JSON, 이미지 등).

**MCP와의 관계:** 경쟁이 아닌 보완. MCP는 에이전트가 도구를 쓰는 것(수직), A2A는 에이전트끼리 대화하는 것(수평). 잡코리아 기술블로그에서 정리한 대로 "도구 활용에 MCP, 에이전트 간 소통에 A2A"가 사실상 표준이 되어가고 있다.

## 8. 주요 프레임워크 (2026년 기준)

| 프레임워크             | 특징                                                     |
| ---------------------- | -------------------------------------------------------- |
| **LangGraph**          | 그래프 기반 워크플로우, 세밀한 흐름 제어, 가장 널리 사용 |
| **CrewAI**             | 역할 기반 팀 구성, 쉬운 입문, 빠른 프로토타이핑          |
| **AutoGen**            | 대화 중심 협업, Microsoft Agent Framework로 통합 중      |
| **Google ADK**         | 계층적 에이전트 트리, A2A 네이티브 지원, 멀티모달        |
| **Strands Agents SDK** | AWS 기반, Graph 패턴 + Agents-as-Tools 패턴 조합         |

## 9. 실무 고려사항

### 언제 MAS가 필요한가

- 단일 에이전트의 컨텍스트 윈도우로 감당 불가능한 복잡도
- 서로 다른 보안/권한 경계를 넘나드는 작업
- 여러 팀이 독립적으로 에이전트를 개발·유지보수해야 하는 경우
- 병렬 실행으로 지연 시간을 줄여야 하는 경우

### 언제 싱글에이전트로 충분한가

- 워크플로우가 예측 가능한 패턴을 따르는 경우
- 빠른 검증과 프로토타이핑이 필요한 경우
- 비용과 지연을 최소화해야 하는 경우

### 비용 주의

에이전트 간 상호작용이 깊어질수록 LLM 호출이 증가한다. Google Research에 따르면 멀티에이전트 조율은 순차 추론 작업에서 싱글에이전트 대비 39~70% 성능 저하를 보인다는 분석도 있다. 작업 특성에 맞는 구조 선택이 중요하다.

## 10. 참고 자료

- IBM, "다중 에이전트 시스템이란 무엇인가요?" — https://www.ibm.com/kr-ko/think/topics/multiagent-system
- IBM, "What are Agentic Workflows?" — https://www.ibm.com/think/topics/agentic-workflows
- Google Cloud, "AI에서 멀티 에이전트 시스템이란 무엇인가요?" — https://cloud.google.com/discover/what-is-a-multi-agent-system?hl=ko
- Salesforce, "멀티 에이전트 시스템이란 무엇인가요?" — https://www.salesforce.com/kr/agentforce/ai-agents/multi-agent-systems/
- SAP, "다중 에이전트 시스템이란 무엇인가요?" — https://www.sap.com/korea/resources/what-are-multi-agent-systems
- SK AX, "2026년 기업 AI 전환의 해답, 멀티에이전트 시스템이 필요한 이유" — https://www.skax.co.kr/insight/trend/3626
- 카카오클라우드, "멀티 에이전트 시스템의 원리와 구현" — https://blog.kakaocloud.com/152
- 잡코리아 기술블로그, "멀티에이전트 시스템(MAS) 실전 가이드" — https://techblog.jobkorea.co.kr/
- UiPath, "Agents and workflows" — https://docs.uipath.com/agents/automation-cloud/latest/user-guide/agent-vs-workflows
- Farahani et al., "Hybrid Agentic AI and Multi-Agent Systems in Smart Manufacturing" (2026) — https://arxiv.org/abs/2511.18258
- A2A Protocol 공식 문서 — https://a2a-protocol.org
- AWS 기술 블로그, "Deep Insight 아키텍처로 배우는 실전 설계" — https://aws.amazon.com/ko/blogs/tech/practical-design-lessons-from-the-deep-insight-arch/
