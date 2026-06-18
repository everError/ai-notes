from pydantic import BaseModel
from typing import List, Literal, Any, Optional
from datetime import datetime

# 'parts' 배열 안의 객체 구조를 정의하는 모델을 새로 만듭니다.
class MessagePart(BaseModel):
    type: Literal[
        "text", 
        "reasoning", 
        "source-url", 
        "source-document",
        "file",
        'data-sheet', # 'data-{type}'
        "step-start",
        "dynamic-tool",
        #'tool-{type}'
        ]
    text: Optional[str] = None 
    data: Optional[Any] = None
    state: Literal["done", "streaming"] # done, ...

# Message 데이터 구조 정의
class Message(BaseModel):
    id: str
    role: Literal["user", "assistant"]
    parts: List[MessagePart]

# Chat 데이터 구조 정의
class Chat(BaseModel):
    id: str
    title: str
    createdAt: datetime
    messages: List[Message]

# Chat 목록
class ChatListItem(BaseModel):
    id: str
    title: str
    createdAt: datetime