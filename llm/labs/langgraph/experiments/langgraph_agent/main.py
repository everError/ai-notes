# poetry run uvicorn main:app --reload
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from langgraph_agent.openai_chain import build_graph

app = FastAPI()
graph = build_graph()

class PromptRequest(BaseModel):
    prompt: str

@app.post("/ask")
async def ask_openai(req: PromptRequest):
    try:
        result = graph.invoke({"prompt": req.prompt})
        return {"response": result.get("response")}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))