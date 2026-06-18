# Miniconda 간단 정리 및 명령어 모음

## Miniconda란?
**Miniconda**는 Python과 R 언어를 위한 가상 환경 및 패키지 관리 도구인 **Conda**의 경량 배포판입니다. Anaconda에 포함된 여러 추가 패키지 없이 Conda만 설치되므로, 필요에 따라 필요한 패키지만 설치할 수 있습니다.

### 주요 특징
- 가볍고 빠른 설치
- 필요 패키지만 설치 가능
- Python 및 R 환경 관리
- 데이터 과학 및 머신러닝 프로젝트에서 널리 사용됨

---

## 설치 방법
1. [Miniconda 공식 페이지](https://docs.conda.io/en/latest/miniconda.html)에서 운영 체제에 맞는 설치 파일 다운로드
2. 설치 후 Conda 명령어 사용 가능

---

## 주요 명령어

### 환경 관리
1. **Conda 설치 확인**
   ```bash
   conda --version
   ```

2. **새 환경 생성**
   ```bash
   conda create -n <env_name> python=<version>
   ```
   예: Python 3.9 버전으로 `myenv`라는 이름의 환경 생성
   ```bash
   conda create -n myenv python=3.9
   ```

3. **환경 활성화**
   ```bash
   conda activate <env_name>
   ```
   예: `myenv` 활성화
   ```bash
   conda activate myenv
   ```

4. **환경 비활성화**
   ```bash
   conda deactivate
   ```

5. **환경 삭제**
   ```bash
   conda remove -n <env_name> --all
   ```
   예: `myenv` 삭제
   ```bash
   conda remove -n myenv --all
   ```

### 패키지 관리
1. **패키지 설치**
   ```bash
   conda install <package_name>
   ```
   예: `numpy`와 `pandas` 설치
   ```bash
   conda install numpy pandas
   ```

2. **패키지 삭제**
   ```bash
   conda remove <package_name>
   ```

3. **설치된 패키지 목록 확인**
   ```bash
   conda list
   ```

### 환경 백업 및 복원
1. **환경 내 패키지 목록 저장**
   ```bash
   conda env export > environment.yml
   ```

2. **환경 복원**
   ```bash
   conda env create -f environment.yml
   ```

---

## 참고 사항
- Miniconda는 디스크 공간을 절약하고 필요한 패키지만 설치하고자 할 때 유용합니다.
- 프로젝트마다 독립적인 환경을 구성하여 패키지 충돌을 방지할 수 있습니다.
