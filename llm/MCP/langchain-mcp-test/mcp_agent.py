import asyncio
import os
from dotenv import load_dotenv
from langchain_core.messages import AIMessage, ToolMessage
from langchain_mcp_adapters.client import MultiServerMCPClient
from langgraph.prebuilt import create_react_agent

load_dotenv()

# 공통 추론 출력 함수
def print_reasoning(title: str, result):
    messages = result.get("messages", []) if isinstance(result, dict) else result

    print(f"\n🧠 질문: {title}")
    print("\n🧩 추론 상세 과정:")
    step_count = 1
    for msg in messages:
        if isinstance(msg, AIMessage):
            tool_calls = getattr(msg, "tool_calls", [])
            for call in tool_calls:
                print(f"  Step {step_count}:")
                print(f"    🔧 도구 호출: {call['name']}")
                print(f"    📥 입력값: {call['args']}")
                step_count += 1
        elif isinstance(msg, ToolMessage):
            print(f"    📤 도구 응답: {msg.content}")

    final_ai_messages = [msg for msg in messages if isinstance(msg, AIMessage) and msg.content]
    if final_ai_messages:
        print("\n✅ 최종 응답:")
        print(final_ai_messages[-1].content)
    else:
        print("\n⚠️ 최종 응답 메시지를 찾을 수 없습니다.")

async def main():
    async with MultiServerMCPClient(
        {
            "math": {
                "command": "python",
                "args": ["./math_server.py"],
                "transport": "stdio",
            },
            "weather": {
                "url": "http://localhost:8000/sse",
                "transport": "sse",
            }
        }
    ) as client:
        agent = create_react_agent("openai:gpt-4o-mini", client.get_tools())

        # 🧮 수학 질문
        math_question = "what's (3 + 5) * 12?"
        math_result = await agent.ainvoke({
            "messages": [{"role": "user", "content": math_question}]
        })
        print_reasoning(math_question, math_result)

        # 🌤️ 날씨 질문
        weather_question = "what is the weather in nyc?"
        weather_result = await agent.ainvoke({
            "messages": [{"role": "user", "content": weather_question}]
        })
        print_reasoning(weather_question, weather_result)

if __name__ == "__main__":
    asyncio.run(main())
