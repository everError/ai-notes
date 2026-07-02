from datetime import datetime, timedelta

# 목(Mock) 데이터베이스
# 실제 프로젝트에서는 이 부분이 DB 커넥션 및 쿼리가 됩니다.
_mock_db = {
    "sample": {
      "id": "sample",
      "title": "샘플 AI 채팅",
      "createdAt": datetime(2025, 9, 12, 14, 30, 0),
      "messages": [
        { 
            "id": "msg-1", 
            "role": "user", 
            "parts": [{ "type": "text", "state": "done", "text": "FastAPI에서 스트리밍 응답은 어떻게 구현하나요?" }]
        },
        { 
            "id": "msg-2", 
            "role": "assistant", 
            "parts": [{ "type": "text", "state": "done", "text": "FastAPI에서는 `StreamingResponse`를 사용합니다." }]
        },
        { 
            "id": "msg-3", 
            "role": "user", 
            "parts": [{ "type": "text", "state": "done", "text": "개발할 때, 날짜에 관련된 데이터베이스 규칙이 있나요?" }]
        },
        { 
            "id": "msg-4", 
            "role": "assistant", 
            "parts": [{ 
                "type": "text", 
                "state": "done", 
                "text": """### 1. 데이터베이스 저장

- 모든 날짜 및 시간 데이터는 **UTC (협정 세계시)** 기준으로 저장한다.
- 데이터 저장 시, `DateTime.UtcNow`를 사용하여 현재 시간을 UTC로 설정한다. `DateTime.Now`를 사용할 경우, `.ToUniversalTime()`을 호출하여 UTC로 변환한 후 저장해야 한다.
- **예시**:

```csharp
CreateDate = DateTime.UtcNow,
```""" 
            }]
        },
                { 
            "id": "msg-5", 
            "role": "user", 
            "parts": [{ "type": "text", "state": "done", "text": "표 데이터 예시" }]
        },
        { 
            "id": "msg-6", 
            "role": "assistant", 
            "parts": [{ "type": "data-sheet", "state": "done", "data": [
                    { "id": 1, "product": "노트북", "quantity": 2, "price": 1500000 },
                    { "id": 2, "product": "키보드", "quantity": 5, "price": 80000 },
                    { "id": 3, "product": "마우스", "quantity": 5, "price": 45000 }
                ] }]
        },
        { 
            "id": "msg-7", 
            "role": "user", 
            "parts": [{ "type": "text", "state": "done", "text": "Nuxt 프론트엔드와 연동할 때 주의할 점이 있나요?" }]
        },
        { 
            "id": "msg-8", 
            "role": "assistant", 
            "parts": [{ "type": "reasoning", "state": "streaming", "text": "네, Nuxt의 프록시 설정을 사용하여 CORS 오류를 방지하는 것이 중요합니다." }]
        }
      ]
    }
}

def get_chat_by_id(chat_id: str) -> dict | None:
    """
    ID로 채팅 데이터를 조회합니다.
    데이터가 있으면 dict를, 없으면 None을 반환합니다.
    """
    return _mock_db.get(chat_id)

# 데이터베이스를 대신할 채팅 목록 목(Mock) 데이터
_mock_db_list = [
    {
        "id": "sample",
        "title": "오늘의 AI 토픽",
        "createdAt": datetime.now()
    },
    {
        "id": "chat-456",
        "title": "Nuxt 프로젝트 기획",
        "createdAt": datetime.now() - timedelta(days=1)
    },
    {
        "id": "chat-789",
        "title": "FastAPI 성능 최적화",
        "createdAt": datetime.now() - timedelta(days=3)
    },
    {
        "id": "chat-abc",
        "title": "Untitled", # 프론트에서 처리할 제목 없는 케이스
        "createdAt": datetime.now() - timedelta(weeks=1)
    }
]

def get_all_chats() -> list:
    """
    모든 채팅 목록을 반환합니다.
    (실제로는 DB에서 SELECT * FROM chats ORDER BY createdAt DESC 와 같은 쿼리를 실행합니다)
    """
    return _mock_db_list