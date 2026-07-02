from fastapi import APIRouter
from app.schemas.query import QueryRequest, QueryResponse
from app.services.llm_service import ask_agent

# API 문서의 가독성을 위해 prefix와 tags를 추가
router = APIRouter(
    prefix="/query",
    tags=["LLM Query"]
)

# 이제 경로는 자동으로 "/query/" 가 됩니다.
@router.post("/", response_model=QueryResponse)
async def handle_query(request: QueryRequest) -> QueryResponse:
    """
    사용자 쿼리를 받아 LLM 서비스에 전달하고 답변을 반환합니다.
    """
    answer = await ask_agent(request.query)
    return QueryResponse(answer=answer)