# AI 에이전트 개발을 위한 최적의 환경 설정 가이드 (Mamba + Poetry)

## 1\. 개요

이 문서는 \*\*Miniforge(Mamba)\*\*와 **Poetry**를 함께 사용하여 **LangChain**, **LangGraph** 등 AI 에이전트 개발을 위한 **안정적이고 재현 가능한 개발 환경**을 구축하는 방법을 설명합니다.

이 조합은 Mamba의 강력한 **시스템 레벨 관리 능력**과 Poetry의 현대적인 **프로젝트 의존성 관리 능력**을 결합하여, 복잡하고 빠르게 변화하는 AI 생태계에서 발생하는 수많은 문제를 해결하는 최고의 모범 사례(Best Practice)입니다.

---

## 2\. 왜 이 조합을 사용하는가? (역할 분담)

AI 프로젝트는 두 가지 종류의 복잡성을 동시에 가집니다. 이 조합은 각 문제를 가장 잘 해결하는 도구에게 역할을 명확히 분담시킵니다.

- **Miniforge (Mamba)의 역할: 시스템 레벨 관리 🚀**

  - **파이썬 버전 관리**: 프로젝트마다 필요한 `Python 3.11`, `3.12` 등 특정 버전의 파이썬을 시스템과 완벽히 격리하여 설치합니다.
  - **복잡한 라이브러리 설치**: `CUDA`, `PyTorch` 등 컴파일이 필요한 무거운 라이브러리를 **빠르고 안정적으로** 설치합니다.
  - **담당**: 프로젝트의 \*\*'기반 공사'\*\*를 책임집니다.

- **Poetry의 역할: 프로젝트 레벨 관리 📦**

  - **파이썬 패키지 의존성 관리**: `langchain`, `openai` 등 순수 파이썬 라이브러리들의 버전을 `pyproject.toml` 파일로 명확하게 관리합니다.
  - **완벽한 재현성 보장**: `poetry.lock` 파일을 통해 **어떤 환경에서든 100% 동일한 개발 환경을 복제**할 수 있도록 보장합니다. "제 컴퓨터에서는 됐는데..." 문제를 원천 차단합니다.
  - **담당**: 프로젝트의 \*\*'내부 설계 및 자재 관리'\*\*를 책임집니다.

---

## 3\. 단계별 환경 설정 워크플로우

### 1단계: Miniforge 설치

먼저, Mamba가 기본으로 포함된 Miniforge를 설치합니다.

1.  **[Miniforge GitHub 릴리스 페이지](https://www.google.com/search?q=https://github.com/conda-forge/miniforge/releases/latest)에 접속**하여 자신의 운영체제에 맞는 설치 파일을 다운로드합니다.
    - **Windows**: `Miniforge3-Windows-x86_64.exe`
    - **macOS (Apple Silicon, M1/M2/M3)**: `Miniforge3-MacOSX-arm64.sh`
    - **macOS (Intel)**: `Miniforge3-MacOSX-x86_64.sh`
2.  다운로드한 파일을 실행하여 설치를 완료합니다. (대부분 기본 옵션으로 진행)

### 2단계: Mamba로 가상 환경 생성 및 활성화

프로젝트를 위한 격리된 공간(가상 환경)을 만듭니다.

```bash
# 1. 'ai-project-env' 라는 이름으로 Python 3.12 버전의 환경을 생성합니다.
mamba create -n ai-project-env python=3.12

# 2. 생성된 환경을 활성화합니다.
mamba activate ai-project-env
```

이제 터미널 프롬프트 앞에 `(ai-project-env)`가 표시되어, 모든 작업이 이 환경 안에서 이루어짐을 나타냅니다.

### 3단계: 프로젝트 폴더 생성 및 Poetry 설정

이제 실제 작업을 진행할 폴더를 만들고, 그 안에서 Poetry를 설정합니다.

```bash
# 1. 프로젝트 폴더를 만들고 이동합니다.
mkdir my-langchain-agent
cd my-langchain-agent

# 2. 활성화된 환경 안에 Poetry를 설치합니다.
pip install poetry

# 3. Poetry가 새 가상 환경을 만들지 않고 현재 Mamba 환경을 사용하도록 설정합니다.
poetry config virtualenvs.create false --local
```

### 4단계: Poetry 프로젝트 시작 및 라이브러리 관리

모든 준비가 끝났습니다. Poetry를 사용하여 프로젝트를 초기화하고 필요한 라이브러리를 추가합니다.

```bash
# 1. Poetry 프로젝트를 초기화하여 'pyproject.toml' 파일을 생성합니다.
#    (몇 가지 질문이 나오면 Enter를 눌러 기본값으로 진행해도 무방합니다.)
poetry init

# 2. 'poetry add' 명령어로 필요한 라이브러리를 설치합니다.
#    예시: LangChain, LangGraph, OpenAI 라이브러리 설치
poetry add langchain langgraph langchain-openai
```

이 명령을 실행하면 라이브러리들이 Mamba가 만든 `ai-project-env` 환경에 설치되며, 그 정보가 `pyproject.toml`과 `poetry.lock` 파일에 자동으로 기록됩니다.

---

## 4\. 개발 시작하기

1.  **VS Code**와 같은 코드 에디터에서 `my-langchain-agent` 폴더를 엽니다.
2.  에디터의 파이썬 인터프리터를 방금 설정한 `ai-project-env` 환경으로 지정합니다.
3.  이제 코드를 작성하고 실행하면, 완벽하게 격리되고 관리되는 환경에서 개발을 진행할 수 있습니다.
