from ollama import AsyncClient
import json
import re
from .config import Config
from .prompts import build_system_prompt


class OllamaTranslator:
    def __init__(self, host: str = None, model: str = None):
        self.host = host or Config.OLLAMA_HOST
        self.model = model or Config.MODEL_NAME
        self.client = AsyncClient(host=self.host)
    
    async def translate(
        self,
        source: dict,
        target_language: str,
        content: dict = None
    ) -> dict:
        """
        JSON 번역
        
        Args:
            source: {"question": "안녕하세요"}
            target_language: "English"
            content: {"tone": "Natural", "glossary": {...}}
        
        Returns:
            {"question": "Hello"} or raises Exception
        """
        system_message = build_system_prompt(target_language, content)
        user_message = json.dumps(source, ensure_ascii=False)
        
        try:
            # 비동기 스트리밍
            response_stream = await self.client.chat(
                model=self.model,
                messages=[
                    {'role': 'system', 'content': system_message},
                    {'role': 'user', 'content': user_message}
                ],
                stream=True
            )
            
            full_response = ""
            
            async for chunk in response_stream:
                if chunk.get('done'):
                    break
                full_response += chunk.get('message', {}).get('content', '')
            
            # JSON 파싱
            return self._parse_json(full_response)
            
        except Exception as e:
            raise Exception(f"Translation failed: {str(e)}")
    
    def _parse_json(self, response: str) -> dict:
        """
        3단계 JSON 파싱
        """
        # 1단계: 직접 파싱
        try:
            return json.loads(response.strip())
        except:
            pass
        
        # 2단계: 코드 블록 제거
        try:
            cleaned = response.strip()
            if cleaned.startswith("```"):
                cleaned = cleaned.split("```", 2)[1]
                if cleaned.startswith("json"):
                    cleaned = cleaned[4:]
            return json.loads(cleaned.strip())
        except:
            pass
        
        # 3단계: 정규식
        json_pattern = r'\{.*\}'
        match = re.search(json_pattern, response, re.DOTALL)
        if match:
            try:
                return json.loads(match.group())
            except:
                pass
        
        raise ValueError(f"Failed to parse JSON from response: {response[:200]}")