# skill-agent 실습

Claude Code 스타일의 **스킬 시스템**(progressive disclosure)을 LangGraph 에이전트로 재현합니다.

- 시스템 프롬프트에는 스킬의 이름+설명만 노출 → 에이전트가 필요할 때 `read_skill` 도구로 본문 로드 → 지침대로 수행
- 동시에 **절차형 스킬 vs 덕목형 스킬의 효과 차이**를 대조 실험으로 관찰합니다

## 구성

```
skill-agent/
├── environment.yml           # mamba 환경 (langgraph-lab)
├── skills/
│   ├── alarm-report/SKILL.md         # 절차형: 설비 알람 보고서 출력 형식 규정
│   └── karpathy-guidelines/SKILL.md  # 덕목형: 코딩 행동 지침
└── notebooks/
    └── 01_skill_agent.ipynb
```

## 셋업

```bash
mamba env create -f environment.yml
mamba activate langgraph-lab
jupyter lab
```

전제: Ollama 서버 + `gemma4:e4b` 모델 (`ollama pull gemma4:e4b`)

## 확인 포인트

- [ ] 알람 질문에서 에이전트가 `read_skill("alarm-report")` 를 스스로 호출하는가 (메시지 로그의 tool_calls)
- [ ] 스킬 로드 후 답변이 보고서 형식을 정확히 따르는가 / 순수 LLM 답변과 형식 차이가 나는가
- [ ] 코딩 질문에서 karpathy-guidelines 스킬은 결과에 **눈에 보이는 차이**를 만드는가 (절차형 대비)
- [ ] 관련 없는 질문(예: 인사)에는 스킬을 로드하지 않고 그냥 답하는가
- [ ] 스킬 description 문구를 바꾸면 트리거 정확도가 달라지는가 (직접 실험)

## 아이디어 확장

- 스킬 본문에 "이 스킬을 쓸 때는 반드시 ~를 물어보라" 같은 절차를 넣어 다단계 행동 유도
- rag 실습(doc-embedding)의 검색 도구를 함께 등록해 "스킬 + RAG" 에이전트로 확장
- 로컬 모델별(gemma, qwen 등) 스킬 트리거 정확도 비교
