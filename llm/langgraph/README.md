# AI Agent API Server

## 📌 프로젝트 개요

1. 강의 실습 파일 실행
2. **FastAPI**와 **LangGraph**를 활용하여 **AI 기반의 Agent API 서버**를 구축

---

## 🚀 기술 스택

### ✅ **백엔드**

- **FastAPI**: 비동기 기반의 Python 웹 프레임워크
- **LangGraph**: LLM(대형 언어 모델) 기반의 워크플로우 그래프 처리

### ✅ **AI & LLM 연동**

- **LangChain**: AI 모델과의 통합 및 프롬프트 관리
- **OpenAI / Ollama**: LLM 모델 활용 가능
- **ChromaDB**: 벡터 DB 저장 및 검색

---

## 폴더 구조

```
langgraph/
├── experiments/
├── notes/
│   ├── ToolCalling.md                              # ToolCalling 개념 정리
│   ├── LangChainBuiltinTools                       # LangChain 내장 도구
│   ├── LangChainTool                               # Tool 의 개념 및 활용
│   ├── LangChainToolDef                            # 랭체인에서 Tool 정의
│   ├── lib/                                        # 개발에 사용한 클래스 등 정리
...
```

## 참고

- 인프런 강의 - AI 에이전트로 구현하는 RAG 시스템(w. LangGraph)
- [LangChain 공식 문서](https://langchain.readthedocs.io/)
