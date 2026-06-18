# LangChain Document Loaders

LangChain의 Document Loaders는 다양한 데이터 형식의 문서를 처리하여 LangChain 애플리케이션에서 활용할 수 있도록 돕는 도구입니다. 각 데이터 형식에 특화된 로더를 사용해 텍스트 데이터를 효율적으로 추출하고, 문서 객체로 변환할 수 있습니다.

---

## 1. PDF 문서
PDF 문서의 텍스트를 처리하기 위해 LangChain은 여러 도구를 제공합니다. 

### 1.1 PyPDFLoader
- **설명**: langchain_community 패키지에서 제공하는 PyPDFLoader는 PDF 파일에서 텍스트를 추출하며, 페이지별로 문서 객체(`Document`)로 변환합니다.
- **특징**:
  - 페이지 단위로 텍스트를 처리.
  - 각 문서는 `page_content`와 `metadata`를 포함.
- **필수 패키지**:
  - `pypdf` 설치: `pip install pypdf`
- **예제 코드**:
  ```python
  from langchain.document_loaders import PyPDFLoader

  loader = PyPDFLoader("example.pdf")
  documents = loader.load()
  for doc in documents:
      print(doc.page_content)  # 페이지 텍스트
      print(doc.metadata)      # 메타데이터 정보
  ```
- **대안 도구**:
  - **PyMuPDFLoader**: 더 정교한 PDF 처리 가능.
  - **UnstructuredPDFLoader**: PDF를 다양한 형식으로 처리 가능.

---

## 2. 웹 문서
웹 페이지의 콘텐츠를 로드하여 문서 객체로 변환할 수 있습니다.

### 2.1 WebBaseLoader
- **설명**: 특정 URL의 웹 페이지 내용을 로드하여 문서 객체로 변환.
- **특징**:
  - HTML 파싱 및 텍스트 추출.
  - 메타데이터에 URL 정보 포함.
- **필수 패키지**:
  - `beautifulsoup4` 설치: `pip install beautifulsoup4`
- **예제 코드**:
  ```python
  from langchain.document_loaders import WebBaseLoader

  loader = WebBaseLoader("https://example.com")
  documents = loader.load()
  for doc in documents:
      print(doc.page_content)  # 웹 페이지 텍스트
      print(doc.metadata)      # URL 정보
  ```
- **대안 도구**:
  - **RecursiveUrlLoader**: 링크를 따라가는 크롤링 지원.
  - **SitemapLoader**: 사이트맵 기반으로 데이터를 로드.

---

## 3. JSON 파일
JSON 및 JSONL 파일의 내용을 로드하여 문서 객체로 변환합니다.

### 3.1 JSONLoader
- **설명**: langchain_community 패키지에서 제공하며, JSON 파일을 문서 객체로 변환합니다.
- **특징**:
  - 각 JSON 레코드를 문서로 처리.
  - JSONL 파일 지원.
- **필수 패키지**:
  - `jq` 설치: JSON 데이터를 필터링 및 가공할 때 필요.
  - 설치: `sudo apt-get install jq` (Linux), `brew install jq` (macOS)
- **예제 코드**:
  ```python
  from langchain.document_loaders import JSONLoader

  loader = JSONLoader("example.json")
  documents = loader.load()
  for doc in documents:
      print(doc.page_content)  # JSON 데이터
      print(doc.metadata)      # 메타데이터 정보
  ```

---

## 4. CSV 문서
CSV 파일의 데이터를 로드하여 각 행을 개별 문서 객체로 변환합니다.

### 4.1 CSVLoader
- **설명**: CSV 파일을 로드하며, 각 행을 개별 문서 객체로 처리.
- **특징**:
  - 행 단위로 처리.
  - 문서 객체에 `page_content`와 열 메타데이터 포함.
- **필수 패키지**:
  - 추가 설치 필요 없음.
- **예제 코드**:
  ```python
  from langchain.document_loaders import CSVLoader

  loader = CSVLoader("example.csv")
  documents = loader.load()
  for doc in documents:
      print(doc.page_content)  # 행 데이터
      print(doc.metadata)      # 열 정보
  ```

---

## 요약
LangChain의 Document Loaders는 다양한 데이터 소스에서 텍스트를 추출하고 문서 객체로 변환하는 강력한 도구를 제공합니다. 각 로더는 데이터 형식에 특화되어 있으며, 필수 패키지를 설치하고 적절히 활용하면 LLM 애플리케이션에서 유용하게 사용할 수 있습니다.
