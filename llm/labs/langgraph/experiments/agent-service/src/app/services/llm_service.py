from langchain_openai import ChatOpenAI
from langchain_mcp_adapters.client import MultiServerMCPClient
from langgraph.prebuilt import create_react_agent
from langchain_core.messages import HumanMessage
from langchain_core.exceptions import OutputParserException
import asyncio
from dotenv import load_dotenv

load_dotenv()

# 에이전트 인스턴스를 저장할 전역 변수 선언 및 초기화
agent_executor = None

# OpenAI
llm = ChatOpenAI(
    model="gpt-4o-mini"
)

client = MultiServerMCPClient({
    "sqlserver": {
        "transport": 'streamable_http',
        "url": "http://localhost:5298/mcp"
    }
})
# FastAPI 시작 시 호출될 비동기 초기화 함수
async def initialize_agent():
    """
    FastAPI lifespan startup 이벤트에서 호출될 함수입니다.
    MCP 도구를 로드하고 agent_executor를 단 한 번 초기화합니다.
    """
    global agent_executor
    if agent_executor is None:
        print("INFO:     Initializing Agent...")
        mcp_tools = await client.get_tools()
        agent_executor = create_react_agent(llm, mcp_tools)
        print(f"✅ Agent Initialized with tools: {[tool.name for tool in mcp_tools]}")

# API 엔드포인트에서 호출할 비동기 메소드
async def ask_agent(query: str, timeout: float = 60.0) -> str:
    """
    초기화된 agent_executor를 사용하여 사용자 쿼리에 응답합니다.
    """
    if agent_executor is None:
        raise RuntimeError("Agent is not initialized. Check FastAPI lifespan startup event.")

    # .ainvoke()를 사용하여 에이전트를 비동기적으로 호출합니다.
    try:
        # 타임아웃 감싸기
        response = await asyncio.wait_for(
            agent_executor.ainvoke({"messages": [HumanMessage(content=query)]}),
            timeout=timeout
        )

        # 정상 응답 반환
        return response.get("output") or response["messages"][-1].content

    except asyncio.TimeoutError:
        # 1번: 도구 응답 or 모델 응답이 너무 늦을 때
        return "⏱️ 응답이 너무 오래 걸려서 요청을 종료했습니다."

    except OutputParserException as e:
        # 2번: 도구 호출 실패 (응답 파싱 불가 등)
        return f"⚠️ 도구 호출 실패: {str(e)}"

    except Exception as e:
        # 일반적인 예외 처리
        return f"❌ 알 수 없는 오류 발생: {str(e)}"
