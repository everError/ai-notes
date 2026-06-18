# Semantic Kernel 에이전트 아키텍처 요약

Semantic Kernel의 에이전트 아키텍처는 Microsoft가 개발한 Semantic Kernel 내부 기능 중 하나로, 대형 언어 모델(LLM)과 도구, 기억(Memory), 플래너(Planner) 등을 조합하여 목적 지향적인 에이전트를 구성할 수 있도록 지원합니다.

이 구조는 다양한 컴포넌트 간의 메시지 교환을 통해 복잡한 작업을 자동화하거나 협업형 워크플로우를 구성하는 데 활용됩니다.

---

## 주요 개념

### ✅ Agent

- 고유한 **목표(goal)**, **역할(role)**, **상태(state)** 를 갖는 실행 단위
- 내부적으로 하나 이상의 스킬(Skill)을 호출하거나, 다른 Agent와 상호작용 가능

### ✅ Kernel

- Semantic Kernel의 중심 객체로, 에이전트가 사용하는 **LLM**, **메모리**, **스킬**, **플러그인** 등을 포함

### ✅ Skill / Plugin

- 재사용 가능한 기능 단위 (예: 요약, 번역, 계산 등)
- C# 메서드 또는 프롬프트로 정의하여 플러그인처럼 등록 가능

### ✅ Planner

- 주어진 목표(goal)를 분석하여 자동으로 실행 계획을 생성
- `SequentialPlanner`, `ActionPlanner` 등이 존재하며, 여러 스킬을 연결해 목적을 달성

### ✅ Memory

- 세션 간 또는 에이전트 간 공유 상태 저장소
- 벡터 임베딩 기반 검색, 텍스트 저장 등 다양한 방식 지원

---

## 특징 및 장점

- .NET 기반으로 강력한 확장성과 통합성 제공
- OpenAI, Azure OpenAI, HuggingFace 등 다양한 모델 연동 가능
- 플러그인 기반 구조로 기능 단위 분리 및 재사용 용이
- MCP 메시지 포맷을 통해 명확한 에이전트 간 메시지 전송 구조 제공

---

## 사용 예시

- 문서 요약 → 피드백 → 수정 등 다중 에이전트 협업 흐름 구성
- 사용자의 자연어 요청을 기반으로 Planner가 실행 계획을 생성하고 Skill들을 순차적으로 호출
- 정보 검색 + 요약 + 리포트 작성 자동화

---

## 참고 링크

- [Semantic Kernel GitHub](https://github.com/microsoft/semantic-kernel)
- [Semantic Kernel 공식 문서](https://learn.microsoft.com/en-us/semantic-kernel/)
