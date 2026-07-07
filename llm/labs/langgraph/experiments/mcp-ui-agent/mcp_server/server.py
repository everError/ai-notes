"""데모용 MCP 서버 (stdio)

두 도구가 서로 다른 형태의 데이터를 반환한다:
- search_books  → list[dict]  (표 형태)
- get_weather   → dict        (단일 레코드 형태)

에이전트 쪽에서 이 형태 차이를 보고 UI 페이로드(type A/B)를 분기한다.
"""

from mcp.server.fastmcp import FastMCP

mcp = FastMCP("demo-data")

BOOKS = [
    {"title": "달빛 도서관", "author": "김하늘", "genre": "판타지", "year": 2021, "rating": 4.6},
    {"title": "시간의 정원사", "author": "이서준", "genre": "판타지", "year": 2019, "rating": 4.3},
    {"title": "붉은 행성의 아이", "author": "박지우", "genre": "SF", "year": 2022, "rating": 4.8},
    {"title": "마지막 알고리즘", "author": "최연우", "genre": "SF", "year": 2020, "rating": 4.1},
    {"title": "고요한 파도", "author": "정민서", "genre": "드라마", "year": 2023, "rating": 4.5},
]

WEATHER = {
    "서울": {"city": "서울", "temp_c": 29, "condition": "맑음", "humidity": 62, "wind_kmh": 8},
    "부산": {"city": "부산", "temp_c": 27, "condition": "구름 조금", "humidity": 71, "wind_kmh": 14},
    "제주": {"city": "제주", "temp_c": 26, "condition": "비", "humidity": 88, "wind_kmh": 22},
}


@mcp.tool()
def search_books(genre: str = "") -> list[dict]:
    """도서 목록을 검색한다. genre 를 주면 해당 장르만, 없으면 전체를 반환한다."""
    if genre:
        return [b for b in BOOKS if b["genre"] == genre]
    return BOOKS


@mcp.tool()
def get_weather(city: str) -> dict:
    """도시의 현재 날씨를 조회한다. 지원 도시: 서울, 부산, 제주"""
    return WEATHER.get(city, {"city": city, "error": "지원하지 않는 도시"})


if __name__ == "__main__":
    import sys

    if "--http" in sys.argv:
        # Windows + Jupyter 에서는 stdio 서브프로세스 생성이 막히므로 HTTP 로 띄운다
        # → http://localhost:8000/mcp
        mcp.run(transport="streamable-http")
    else:
        mcp.run()  # stdio transport (터미널 스크립트에서 사용 시)
