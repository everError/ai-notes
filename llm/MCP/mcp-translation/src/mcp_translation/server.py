from fastmcp import FastMCP
from .translator import OllamaTranslator
from .config import Config
import asyncio
import logging

# logging.basicConfig(level=logging.INFO)
# logger = logging.getLogger(__name__)

mcp = FastMCP(
    "translation-server",
    instructions="MCP server for translation."
)
translator = OllamaTranslator()

@mcp.tool(
    description="""Translate JSON data.
    
    - Maintains JSON structure (keys and nesting)
    - Applies MES-specific glossary automatically
    
    Examples:
    - source: {"question": "작업지시의 생산실적을 알려주세요"}
      target_language: "English"
      → {"question": "Please provide the production results for the Production Order"}
    
    - source: {"error": "Equipment failure detected"}
      target_language: "Korean"
      → {"error": "설비 고장이 감지되었습니다"}
    """
)
async def translate_json(
    source: dict,
    target_language: str,
) -> dict:
    """
    JSON 데이터 번역
    
    Args:
        source: {"question": "안녕하세요"}
        target_language: "English" or "Korean"
    
    Returns:
        {"question": "Hello"}
    """
    content = {}
    content["tone"] = Config.DEFAULT_TONE
    content["context"] = Config.DEFAULT_CONTEXT
    content["glossary"] = Config.DEFAULT_GLOSSARY
    
    # logger.info(f"Translating to {target_language}")
    
    result = await translator.translate(
        source=source,
        target_language=target_language,
        content=content
    )
    
    # logger.info("Translation completed")
    return result


@mcp.tool(
    description="""Translate multiple JSON objects at once.
    
    - Processes array of JSON objects in batch
    - More efficient than calling translate_json multiple times
    - Maintains order of input array
    
    Examples:
    - sources: [
        {"question": "작업지시를 확인하세요"},
        {"error": "설비 고장 발생"}
      ]
      target_language: "English"
      → [
        {"question": "Please check the Production Order"},
        {"error": "Equipment failure occurred"}
      ]
    """
)
async def translate_json_batch(
    sources: list[dict],
    target_language: str,
) -> list[dict]:
    """
    여러 JSON 데이터를 한번에 번역
    
    Args:
        sources: [{"question": "..."}, ...]
        target_language: "English" or "Korean"
    
    Returns:
        [{"question": "..."}, ...]
    """
    content = {
        "tone": Config.DEFAULT_TONE,
        "context": Config.DEFAULT_CONTEXT,
        "glossary": Config.DEFAULT_GLOSSARY
    }
    
    # 비동기로 모든 번역 동시 실행
    tasks = [
        translator.translate(
            source=source,
            target_language=target_language,
            content=content
        )
        for source in sources
    ]
    
    results = await asyncio.gather(*tasks)
    return results


@mcp.tool(
    description="""Translate plain text quickly.
    
    - Simple text input/output
    
    Examples:
    - text: "작업지시를 확인하세요"
      target_language: "English"
      → "Please check the Production Order"
    
    - text: "Process completed successfully"
      target_language: "Korean"
      → "공정이 성공적으로 완료되었습니다"
    """
)
async def translate_text(
    text: str,
    target_language: str,
) -> str:
    """
    단순 텍스트 번역
    
    Args:
        text: "안녕하세요"
        target_language: "English"
    
    Returns:
        "Hello"
    """
    source = {"text": text}
    content = {}
    content["tone"] = Config.DEFAULT_TONE
    content["context"] = Config.DEFAULT_CONTEXT
    content["glossary"] = Config.DEFAULT_GLOSSARY
    
    result = await translator.translate(source, target_language, content)
    return result.get("text", "")


@mcp.tool(
    description="""Translate multiple plain texts at once.
    
    - Processes array of text strings in batch
    - More efficient than calling translate_text multiple times
    - Maintains order of input array
    
    Examples:
    - texts: [
        "작업지시를 확인하세요",
        "설비 고장 발생",
        "생산 완료"
      ]
      target_language: "English"
      → [
        "Please check the Production Order",
        "Equipment failure occurred",
        "Production completed"
      ]
    """
)
async def translate_text_batch(
    texts: list[str],
    target_language: str,
) -> list[str]:
    """
    여러 텍스트를 한번에 번역
    
    Args:
        texts: ["안녕하세요", "감사합니다"]
        target_language: "English" or "Korean"
    
    Returns:
        ["Hello", "Thank you"]
    """
    content = {
        "tone": Config.DEFAULT_TONE,
        "context": Config.DEFAULT_CONTEXT,
        "glossary": Config.DEFAULT_GLOSSARY
    }
    
    # 비동기로 모든 번역 동시 실행
    tasks = [
        translator.translate(
            source={"text": text},
            target_language=target_language,
            content=content
        )
        for text in texts
    ]
    
    results = await asyncio.gather(*tasks)
    return [result.get("text", "") for result in results]


@mcp.tool(
    description="""Get current server configuration.
    
    Returns:
    - Ollama host URL
    - Model name (Rosetta-12B)
    - Default context (MES)
    - Default tone (Natural)
    - Default glossary terms
    
    Use this to verify server settings before translation.
    """
)
def get_config() -> dict:
    """서버 설정 확인"""
    return {
        "ollama_host": Config.OLLAMA_HOST,
        "model": Config.MODEL_NAME,
        "context": Config.DEFAULT_CONTEXT,
        "tone": Config.DEFAULT_TONE,
        "glossary": Config.DEFAULT_GLOSSARY
    }