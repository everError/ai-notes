def build_system_prompt(target_language: str, content: dict = None) -> str:
    """
    시스템 프롬프트 생성
    
    Args:
        target_language: "English", "Korean" 등
        content: {
            "tone": "Natural",
            "glossary": {"작업지시": "Production Order"},
            "context": "MES"
        }
    """
    parts = [f"Translate the user's text to {target_language}."]
    
    if content:
        if "tone" in content:
            parts.append(f"tone: {content['tone']}")
        
        if "glossary" in content:
            parts.append("glossary:")
            for src, tgt in content["glossary"].items():
                parts.append(f"- {src} -> {tgt}")
        
        if "context" in content:
            parts.append(f"context: {content['context']}")
    
    parts.append("Provide the final translation immediately without any other text.")
    
    return "\n".join(parts)