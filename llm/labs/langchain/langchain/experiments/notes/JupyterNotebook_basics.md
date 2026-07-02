
# Jupyter Notebook 간단 정리

## Jupyter Notebook이란?
**Jupyter Notebook**은 데이터 과학, 머신러닝, 그리고 연구를 위한 대화형 컴퓨팅 환경을 제공하는 오픈 소스 도구입니다. 코드, 시각화, 설명을 한 곳에서 작성할 수 있는 웹 기반 인터페이스를 제공합니다.

---

## 주요 특징
1. **다목적**: Python뿐만 아니라 R, Julia 등 다양한 언어 지원.
2. **대화형 환경**: 코드 실행 결과를 바로 확인 가능.
3. **통합된 워크플로우**: 코드, 시각화, 텍스트 설명을 한 파일에 통합.
4. **확장 가능**: 다양한 확장 프로그램 및 플러그인을 통해 기능 추가 가능.

---

## 설치 방법
1. **pip로 설치**
   ```bash
   pip install notebook
   ```

2. **conda로 설치**
   ```bash
   conda install -c conda-forge notebook
   ```

3. **설치 확인**
   ```bash
   jupyter --version
   ```

---

## 주요 명령어
1. **Jupyter Notebook 실행**
   ```bash
   jupyter notebook
   ```

2. **Notebook 종료**
   - 실행 중인 터미널에서 `Ctrl+C`.

3. **Jupyter Lab 실행 (옵션)**
   ```bash
   jupyter lab
   ```

---

## 사용 방법
1. **코드 실행**
   - 셀에 코드를 입력하고 `Shift+Enter`로 실행.
2. **Markdown 셀 작성**
   - 텍스트 설명, 수식 등을 작성 가능.
3. **시각화 및 데이터 분석**
   - `matplotlib`, `seaborn` 등을 활용한 데이터 시각화 지원.

---

## 참고 사항
- Jupyter는 데이터 분석과 모델 프로토타이핑에 적합한 환경을 제공합니다.
- 협업을 위해 `.ipynb` 파일을 공유하거나 HTML로 내보낼 수 있습니다.
- 추가 기능을 원한다면 **Jupyter Lab**을 고려해보세요.