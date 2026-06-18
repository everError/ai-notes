# MCP Translation - Quick Start Guide

번역 MCP 서버

## 📋 전제조건

- Python 3.13

### 프로젝트 구조 생성
```
mcp-translation/
├── src/
│   └── mcp_translation/
│       ├── __init__.py
│       └── server.py          # MCP 서버 메인 코드
├── tests/
│   └── test_translation.py    # 테스트 코드
├── notebooks/                  # Jupyter 노트북용
├── pyproject.toml             # 프로젝트 설정
├── README.md
└── .gitignore
```

### 의존성 설치

```bash
# 기본 설치
pip install -e .

# 개발 도구 포함 (Jupyter 등)
pip install -e ".[dev]"
```

### 모델 테스트 (Jupyter Notebook)

노트북에서 테스트할 내용:
- ✅ NLLB-200, hunyuan, rosetta 등 모델 로드 및 번역
- ✅ 한국어 ↔ 영어 양방향 번역
- ✅ 번역 품질 평가 (왕복 번역)
- ✅ 속도 벤치마크

### MCP 서버 실행

```bash
# 서버 실행
python -m mcp_translation
```

## 🛠️ MCP 서버 기능

서버에서 제공하는 도구들:

### 1. json 구조에서 소스 번역
### 2. 텍스트 번역
### 3. 서버 설정 조회