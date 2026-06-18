# langgraph_agent/chain.py
from langgraph.graph import StateGraph, END
from openai import OpenAI
from langgraph_agent.config import OPENAI_API_KEY
from typing import TypedDict

client = OpenAI(api_key=OPENAI_API_KEY)

class GraphState(TypedDict):
    prompt: str
    response: str

def call_openai_node(state):
    prompt = state.get("prompt", "")
    response = client.chat.completions.create(
        model="gpt-3.5-turbo",
        messages=[{"role": "user", "content": prompt}]
    )
    state["response"] = response.choices[0].message.content
    return state

def build_graph():
    builder = StateGraph(GraphState)
    builder.add_node("query_openai", call_openai_node)
    builder.set_entry_point("query_openai")
    builder.add_edge("query_openai", END)
    return builder.compile()
