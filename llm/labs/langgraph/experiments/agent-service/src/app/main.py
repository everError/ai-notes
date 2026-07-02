from fastapi import FastAPI
from app.routers import query, chats
from contextlib import asynccontextmanager
from app.services.llm_service import initialize_agent

@asynccontextmanager
async def lifespan(app: FastAPI):
    # --- 애플리케이션 시작 시 ---
    # agent 모델 초기화
    await initialize_agent()
    yield
    # --- 애플리케이션 종료 시 ---
    print("INFO:     Shutting down application.")
# FastAPI 앱 초기화
app = FastAPI(
    title="LLM Query API",
    description="LangChain을 사용하여 LLM에 쿼리하는 구조화된 테스트용 API입니다.",
    version="1.0.0",
    lifespan=lifespan
)

# query 라우터를 앱에 포함시킵니다.
app.include_router(query.router)
app.include_router(chats.router)

@app.get("/")
def read_root():
    """서버 상태 확인을 위한 기본 GET 엔드포인트"""
    return {"status": "LLM Query API is running"}