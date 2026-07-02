## 개요

Retrieval-Augmented Generation (RAG)은 LLM의 성능을 향상시키기 위해 외부 지식을 활용하는 기술입니다. 이 레포지토리는 RAG의 기본 개념, 실습, 주요 도구인 LangChain을 활용한 응용 사례를 중심으로 학습한 내용을 체계적으로 정리합니다.

## 폴더 구조

```
langchain/
├── experiments/
│   ├── notes/
│   │   ├── Miniconda_basics.md                           # Miniconda 간단 정리 및 명령어
│   │   ├── Poetry_basics.md                              # Poetry 간단 정리 및 명령어
│   │   ├── JupyterNotebook_basics.md                     # Jupyter Notebook 간단 정리
│   │   ├── Gradio_basics.md                              # Gradio 간단 정리 및 명령어
│   │   ├── Langsmith.md                                  # Langsmith 간단 정리
│   ├── conda_rag/                                        # Conda로 생성한 테스트 실습 프로젝트
│   ├── langchain_env/                                    # Poetry로 생성한 LangChain 샘플 프로젝트
│   │   ├── DocumentLoader_RAG.ipynb                      # 문서읽기 및 간단한 RAG 구현 실습
│   │   ├── LCEL.ipynb                                    # LCEL 실습
│   │   ├── Data_Processing.ipynb                         # 데이터 처리 및 임베딩 실습
│   │   ├── Vectorstore_Retrieval                         # 여러 벡터저장소 검색기 사용
├── notes/
│   ├── Advanced_Search_Methods.mcd                       # 고급 검색 기법
│   ├── Advanced_Search_Techniques_In_RAG.md              # RAG 검색 기법
│   ├── Chroma_FAISS.md                                   # Chroma, FAISS 비교
│   ├── Embedding_Models.md                               # EmbeddingModel 기본 개념 정리
│   ├── Evaluation.md                                     # Heuristic, LLM as Judge
│   ├── LangChain_basics.md                               # LangChain 기본 개념 정리
│   ├── LangChain_Document_Loaders.md                     # LangChain의 Document Loaders 정리
│   ├── Langchain_Rag_Components.md                       # LangChain의 주요 RAG 컴포넌트
│   ├── LangChain_Text_Splitters.md                       # LangChain의 Text Splitters 정리
│   ├── LCEL_basics.md                                    # LCEL 간단 정리
│   ├── RAG_basics.md                                     # RAG 기본 개념 정리
│   ├── RAG_Evaluation.md                                 # RAG 검색 시스템 평가 및 데이터셋 준비
│   ├── RAG_Search_Engines.md                             # RAG 검색 방식
│   ├── Understanding_Similarity_And_Relevance_Score.md   # Vector 유사도, 관련성

```

- **experiments/**: 실습 프로젝트와 관련된 폴더.
  - **notes/**: Miniconda, Poetry, Jupyter Notebook 등 기초 도구에 대한 간단한 정리 파일.
  - **conda_rag/**: Conda 환경에서 실습한 RAG 프로젝트.
  - **langchain_env/**: LangChain을 활용한 RAG 실습 프로젝트 파일.
- **notes/**: 개념 정리 및 이론 관련 문서 모음.

## 프로젝트 목표

- LLM과 RAG 기술에 대한 이해를 심화.
- LangChain 및 기타 도구를 활용한 실습과 연구.
- 관련된 기술과 개념의 체계적 정리.

## 기술 스택

- **언어**: Python
- **도구**: LangChain, Gradio, Jupyter Notebook
- **환경 관리**: Conda, Poetry

## 참고

- 인프런 강의 - RAG 마스터: 기초부터 고급기법까지 (feat. LangChain)
- [LangChain 공식 문서](https://langchain.readthedocs.io/)
