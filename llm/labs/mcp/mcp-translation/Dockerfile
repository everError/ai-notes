FROM python:3.13-slim

WORKDIR /app

# 시스템 패키지 업데이트 및 필요한 패키지 설치
RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

# pyproject.toml과 소스 복사
COPY pyproject.toml ./
COPY src/ ./src/

# 패키지 설치
RUN pip install --no-cache-dir -e .

# 환경 변수 기본값
ENV OLLAMA_HOST=http://localhost:11434
ENV MODEL_NAME=hf.co/mradermacher/YanoljaNEXT-Rosetta-12B-2510-i1-GGUF:Q4_K_M

# 서버 실행
CMD ["python", "-m", "mcp_translation"]