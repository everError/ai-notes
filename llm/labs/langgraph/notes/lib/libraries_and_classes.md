# 📚 프로젝트에서 사용된 주요 Python 클래스 및 라이브러리 정리

---

## 📦 FastAPI

- **설명**: Python 기반의 고성능 웹 프레임워크 (Starlette + Pydantic).
- **특징**:
  - 비동기 지원 (`async def`)
  - 자동 Swagger UI 및 Redoc 문서 제공
  - 타입 기반 요청/응답 검증 (Pydantic)

### 주요 클래스/기능:

- `FastAPI`: 앱 인스턴스 생성.
- `@app.get`, `@app.post`, `@app.put`, `@app.delete`: 라우팅 데코레이터.
- `Request`, `Response`, `HTTPException`: 요청, 응답, 예외 처리 객체.
- `status`: HTTP 상태 코드 상수 제공.

---

## 📦 Pydantic

- **설명**: 데이터 유효성 검사 및 설정 관리를 위한 Python 데이터 모델링 라이브러리.
- **FastAPI와 연동되어 API 요청/응답 검증에 사용됨**.

### 주요 클래스:

- `BaseModel`: 데이터 스키마 정의용 베이스 클래스.
  ```python
  class PromptRequest(BaseModel):
      prompt: str
  ```

---

## 📦 OpenAI Python SDK

- **설명**: OpenAI API를 Python에서 사용할 수 있게 해주는 공식 라이브러리.
- **사용법**:
  ```python
  from openai import OpenAI
  client = OpenAI(api_key="...")
  client.chat.completions.create(
      model="gpt-3.5-turbo",
      messages=[{"role": "user", "content": "Hello"}]
  )
  ```

---

## 📦 python-dotenv

- **설명**: `.env` 파일의 환경변수를 `os.environ`으로 로딩해주는 라이브러리.
- **사용법**:
  ```python
  from dotenv import load_dotenv
  load_dotenv()
  ```

---

## 📦 typing (표준 라이브러리)

- **설명**: 정적 타입 힌팅을 위한 Python 내장 모듈.
- **주요 클래스**:

  - `TypedDict`: 딕셔너리 형태의 타입 힌트 정의.
  - `List`, `Dict`, `Optional`, `Union` 등도 함께 사용됨.

  ```python
  class GraphState(TypedDict):
      prompt: str
      response: str
  ```

---

## 📦 Uvicorn

- **설명**: ASGI 서버로, FastAPI 같은 비동기 프레임워크를 실행할 때 사용.
- **사용법**:
  ```
  uvicorn main:app --reload
  ```

---

## 📦 Poetry

- **설명**: Python 프로젝트의 의존성 및 패키징을 관리하는 툴.
- **주요 기능**:
  - `pyproject.toml`로 환경 설정
  - 가상환경 자동 관리
  - 프로젝트 배포 용이

### 주요 명령어:

- `poetry init`: 프로젝트 초기화
- `poetry install`: 의존성 설치
- `poetry shell`: 가상환경 진입
- `poetry add 패키지명`: 의존성 추가
- `poetry run`: 가상환경에서 명령 실행

---

## 📂 .env 파일

- **설명**: 환경변수를 저장하는 파일.
- **예시**:
  ```
  OPENAI_API_KEY=your_openai_api_key
  ```

---
