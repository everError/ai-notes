# 99 · 용어집 (Glossary)

| 용어 | 의미 |
|---|---|
| **ANN (Approximate Nearest Neighbor)** | 정확한 거리 비교 대신 근사로 빠르게 가까운 벡터를 찾는 인덱스 (HNSW, IVF 등) |
| **Bi-encoder** | 질문과 문서를 각각 따로 임베딩 후 코사인 유사도. bge-m3 같은 임베더가 이 방식 |
| **BGE (BAAI General Embedding)** | 베이징지능원이 만든 임베딩/리랭크 모델 시리즈 (m3, reranker-v2-m3 등) |
| **BM25 (Best Match 25)** | 1990년대 만들어진 키워드 검색 점수 함수. 단어 빈도 + IDF + 길이 정규화 |
| **Chunk** | 긴 문서를 검색·임베딩 단위로 작게 자른 조각 |
| **ColBERT / ColPali** | Late-interaction 검색. 문서를 multi-vector로 표현, 토큰 단위 비교 |
| **Cosine Similarity** | 벡터 사이의 각도로 측정한 유사도. 1=같은 방향, 0=무관 |
| **Cross-encoder** | 질문과 문서를 함께 입력해 점수 매기는 모델. 정확하지만 느림. Reranker가 이 방식 |
| **Dense vector** | 모든 차원이 부동소수점인 임베딩 (예: bge-m3의 1024차원) |
| **Embedding** | 텍스트(또는 이미지)를 의미 벡터로 변환한 결과 |
| **FastAPI** | Python 비동기 web framework. 우리 doc_agent의 server |
| **Faithfulness** | 답변이 검색 컨텍스트가 실제로 뒷받침하는 내용인가. 인용 정확도와 직결 |
| **HNSW (Hierarchical Navigable Small World)** | 그래프 기반 ANN 인덱스. 빠르고 정확. 대부분 벡터 DB의 기본 |
| **Hit / Retrieval Hit** | 검색 결과 한 항목 (보통 chunk + 점수 + 메타) |
| **Hybrid Search** | dense + sparse 검색을 RRF 등으로 융합. 의미 + 키워드 양쪽 |
| **IDF (Inverse Document Frequency)** | "흔한 단어는 가중치 ↓" 패널티. BM25에서 사용 |
| **Kiwi** | 한국어 형태소 분석기. 우리는 kiwipiepy Python 바인딩 사용 |
| **MoE (Mixture of Experts)** | 모델 안에 여러 expert를 두고 입력마다 일부만 활성. Qwen3-30B-A3B = 30B 총/3B 활성 |
| **MCP (Model Context Protocol)** | Anthropic이 제안한 LLM 도구 호출 표준 프로토콜 |
| **Milvus** | 오픈소스 벡터 DB (Apache 2.0). 우리가 사용 |
| **Ollama** | 로컬 LLM 실행 환경. OpenAI 호환 API 제공 |
| **OLE (Object Linking and Embedding)** | Microsoft 표준. PPT 안에 다른 PPT/Excel 등을 삽입할 때 사용 |
| **Parent-Doc Retrieval** | 작은 chunk로 검색 → hit이 잡히면 그 chunk가 속한 큰 단위(슬라이드 전체)를 LLM에 전달 |
| **Plan-Execute-Reflect** | 에이전트 루프 패턴. 시작 시 청사진 → 결정론적 실행 → 검증 |
| **POS (Part-of-Speech)** | 품사 (명사 NNG, 동사 VV 등). 형태소 분석의 결과 |
| **PrimeVue** | Vue 3 UI 라이브러리. chat-vue가 사용 |
| **Pydantic** | Python 데이터 검증 라이브러리. JSON Schema 자동 생성 |
| **Q4_K_M** | 모델 양자화 방식. 4-bit, 더 정밀한 weight를 살린 변형. 메모리 ¼ |
| **Qwen3-VL** | Alibaba의 비전-언어 모델. 우리 메인 LLM |
| **RAG (Retrieval-Augmented Generation)** | 검색 결과를 LLM에 주입해 답변하게 하는 패턴 |
| **ReAct (Reasoning + Acting)** | 에이전트 루프 패턴. LLM이 매 스텝마다 다음 도구 선택 |
| **Reranker** | 1차 검색 top-N을 재정렬하는 cross-encoder. 정밀도 향상 |
| **RRF (Reciprocal Rank Fusion)** | 여러 ranker 결과를 1/(k+rank) 합으로 융합 |
| **Schema** | 데이터 구조 정의. JSON Schema, Pydantic model 등 |
| **SDUI (Server-Driven UI)** | 서버가 보내는 구조화 데이터로 클라이언트가 컴포넌트 렌더 |
| **SSE (Server-Sent Events)** | HTTP를 통한 단방향 스트리밍. ChatGPT 등에서 답변 토큰을 실시간 전송 |
| **Sparse vector** | 차원 대부분이 0이고 일부만 값. BM25 결과나 SPLADE 등 |
| **Structured Output** | LLM이 정해진 형식(JSON 등)으로만 답하게 강제 |
| **Tokenizer** | 텍스트를 token 단위로 분리하는 도구 |
| **Tool Calling** | LLM이 함수를 호출해 외부 작업 수행 |
| **VLM (Vision-Language Model)** | 이미지와 텍스트를 동시에 이해하는 LLM |
| **VRAM** | GPU 메모리. 모델 적재 + 추론에 사용 |
