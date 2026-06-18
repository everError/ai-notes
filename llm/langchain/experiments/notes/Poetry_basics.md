# Poetry 간단 정리 및 명령어 모음

## Poetry란?
**Poetry**는 Python 프로젝트의 의존성 관리와 패키지 관리를 단순화하기 위해 설계된 도구입니다. 의존성 설치, 프로젝트 가상 환경 관리, 배포 준비를 한 곳에서 손쉽게 처리할 수 있습니다.

### 주요 특징
- 의존성 및 패키지 관리 자동화
- 프로젝트별 가상 환경 생성 및 관리
- `pyproject.toml` 파일 기반 설정
- 간편한 배포 지원

---

## 설치 방법
1. Poetry 설치
   ```bash
   curl -sSL https://install.python-poetry.org | python3 -
   ```
   또는, Windows에서:
   ```powershell
   (Invoke-WebRequest -Uri https://install.python-poetry.org -UseBasicParsing).Content | python -
   ```

2. 설치 확인
   ```bash
   poetry --version
   ```

---

## 주요 명령어

### 프로젝트 초기화
1. **새 프로젝트 생성**
   ```bash
   poetry new <project_name>
   ```
   예: `myproject`라는 새 프로젝트 생성
   ```bash
   poetry new myproject
   ```

2. **기존 프로젝트 초기화**
   ```bash
   poetry init
   ```
   - 대화형 프롬프트로 `pyproject.toml` 파일 생성

### 의존성 관리
1. **패키지 설치**
   ```bash
   poetry add <package_name>
   ```
   예: `numpy` 설치
   ```bash
   poetry add numpy
   ```

2. **개발 의존성 추가**
   ```bash
   poetry add <package_name> --dev
   ```
   예: `pytest` 설치
   ```bash
   poetry add pytest --dev
   ```

3. **패키지 제거**
   ```bash
   poetry remove <package_name>
   ```

4. **설치된 패키지 목록 확인**
   ```bash
   poetry show
   ```

### 가상 환경 관리
1. **가상 환경 생성**
   Poetry는 프로젝트별로 가상 환경을 자동 생성합니다. 수동 설정이 필요하지 않습니다.

2. **가상 환경 활성화**
   ```bash
   poetry shell
   ```

3. **가상 환경 비활성화**
   ```bash
   exit
   ```

4. **가상 환경 경로 확인**
   ```bash
   poetry env info
   ```

### 프로젝트 실행
1. **스크립트 실행**
   ```bash
   poetry run python <script_name>.py
   ```

2. **REPL 실행**
   ```bash
   poetry run python
   ```

### 의존성 동기화
1. **의존성 업데이트**
   ```bash
   poetry update
   ```

2. **의존성 설치 (복원)**
   ```bash
   poetry install
   ```

---

## 참고 사항
- Poetry는 Python 프로젝트를 체계적으로 관리하고 배포 과정을 단순화할 수 있는 강력한 도구입니다.
- 기존 프로젝트를 Poetry로 전환하려면 `poetry init`을 사용하여 `pyproject.toml`을 생성하세요.
- [Poetry 공식 문서](https://python-poetry.org/docs/)를 참고하여 추가 기능을 학습하세요.
