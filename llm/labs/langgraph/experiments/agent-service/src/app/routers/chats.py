from fastapi import APIRouter, HTTPException
from typing import List
from app.services.chat_service import get_chat_by_id, get_all_chats
from app.schemas.chat import Chat, ChatListItem

# APIRouter 인스턴스를 생성합니다.
# prefix: 이 라우터의 모든 경로 앞에 공통적으로 붙일 경로
# tags: API 문서에서 그룹화할 태그 이름
router = APIRouter(
    prefix="/api/chats",
    tags=["chats"]
)

# 채팅 내용
@router.get("/{chat_id}", response_model=Chat)
async def get_chat_details(chat_id: str):
    """
    ID로 채팅 상세 정보를 조회하는 API 엔드포인트
    """
    chat_data = get_chat_by_id(chat_id)
    
    if not chat_data:
        raise HTTPException(status_code=404, detail="Chat not found")
        
    return chat_data

# 채팅 목록
@router.get("-list", response_model=List[ChatListItem])
async def list_all_chats():
    """
    모든 채팅 목록을 조회합니다.
    """
    chats_list = get_all_chats()
    return chats_list