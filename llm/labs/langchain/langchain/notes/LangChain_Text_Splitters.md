# LangChain Text Splitters

LangChain의 Text Splitters는 텍스트 데이터를 작은 청크로 나누어 LLM에서 효율적으로 처리할 수 있도록 돕는 도구입니다. 텍스트 분할은 RAG(정보 검색 증강 생성)에서 매우 중요한 단계로, 모델이 제한된 토큰 수 내에서 관련성 높은 컨텍스트를 제공받을 수 있도록 보장합니다.

---

## 1. 분할의 필요성
### 1.1 토큰 제한 극복
- LLM은 context window 제한이 있습니다.
- RAG를 수행할 때 사용자 질문 외에도 외부 지식을 전달해야 하는데, 텍스트가 제한을 초과하면 모델이 정상적으로 답변을 생성하지 못할 수 있습니다.

### 1.2 관련성 높은 컨텍스트 제공
- 토큰 제한이 없더라도 불필요한 정보를 포함하면 모델이 의도와 다른 답변을 생성할 가능성이 높습니다.
- 분할된 텍스트 청크는 관련성을 높이고 응답 품질을 개선합니다.

---

## 2. 분할 방법
### 2.1 문자 레벨 분할 (CharacterTextSplitter)
- 텍스트를 고정된 문자 수로 분할.
- 단순하고 빠르지만 문맥을 고려하지 않으므로 자연스러운 청크 생성에는 적합하지 않을 수 있음.

### 2.2 재귀적 분할 (RecursiveCharacterTextSplitter)
- LangChain에서 제공하는 고급 텍스트 분할 도구.
- 텍스트를 재귀적으로 분할하여 더 자연스러운 청크를 생성.
- **특징**:
  1. 구분자를 순차적으로 적용하여 큰 청크에서 시작해 점진적으로 작은 단위로 분할.
  2. 일반적으로 CharacterTextSplitter보다 더 엄격하게 크기를 준수.
- **예제 코드**:
  ```python
  from langchain.text_splitter import RecursiveCharacterTextSplitter

  text = "장문의 텍스트 예제입니다."
  splitter = RecursiveCharacterTextSplitter(chunk_size=500, chunk_overlap=50)
  chunks = splitter.split_text(text)
  print(chunks)  # 분할된 텍스트 청크 출력
  ```

### 2.3 정규표현식 사용 (Regex-based Splitting)
- 정규표현식을 사용하여 특정 패턴을 기반으로 텍스트를 분할.
- 구조화된 텍스트나 특정 형식의 문서 처리에 유용.
- **예시**:
  - 1장, 2장, 3장 등의 챕터 구분.
  - 문장 단위로 구분 (마침표, 느낌표, 물음표).
- **예제 코드**:
  ```python
  from langchain.text_splitter import RegexTextSplitter

  text = "1장. 서론\n2장. 본론\n3장. 결론"
  splitter = RegexTextSplitter(pattern="\\d+장\\.")
  chunks = splitter.split_text(text)
  print(chunks)  # 챕터별 분할된 텍스트
  ```

### 2.4 토큰 기반 분할 (Token-based Splitting)
- LLM의 토큰 제한을 고려하여 텍스트를 분할.
- 각 청크가 특정 토큰 수를 초과하지 않도록 조절 가능.
- **필수 패키지**:
  - `tiktoken`: OpenAI에서 만든 BPE Tokenizer
  - `transformers`: Hugging Face Tokenizer
- **예제 코드**:
  ```python
  import tiktoken

  tokenizer = tiktoken.get_encoding("cl100k_base")
  text = "장문의 텍스트 예제입니다."
  tokens = tokenizer.encode(text)

  max_tokens = 50
  chunks = [tokens[i:i+max_tokens] for i in range(0, len(tokens), max_tokens)]
  print(chunks)  # 토큰 단위로 분할된 텍스트
  ```

### 2.5 맥락을 기반으로 분할 (Semantic Chunking)
- 의미적 유사성에 기반하여 텍스트를 분할.
- 문장 사이의 임베딩 차이를 기반으로 작동.
- **필수 패키지**:
  - `langchain_experimental`: 실험적 기능 포함.
  - OpenAI 임베딩 모델 사용.
- **예제 코드**:
  ```python
  from langchain_experimental.text_splitter import SemanticTextSplitter

  text = "장문의 텍스트 예제입니다."
  splitter = SemanticTextSplitter(embedding_model="text-embedding-ada-002")
  chunks = splitter.split_text(text)
  print(chunks)  # 의미 기반으로 분할된 텍스트 청크
  ```

---

## 요약
LangChain의 Text Splitters는 텍스트 데이터를 효율적으로 처리하고 LLM의 성능을 극대화하기 위한 중요한 도구입니다. 분할 방법은 텍스트 유형과 사용 사례에 따라 선택할 수 있으며, 각 도구는 다양한 요구를 충족할 수 있도록 설계되었습니다.
