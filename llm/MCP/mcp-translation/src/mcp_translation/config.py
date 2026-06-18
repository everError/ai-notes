import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    # Ollama 설정
    OLLAMA_HOST = os.getenv("OLLAMA_HOST", "http://localhost:11434")
    MODEL_NAME = os.getenv("MODEL_NAME", 
        "hf.co/mradermacher/YanoljaNEXT-Rosetta-12B-2510-i1-GGUF:Q4_K_M")
    
    # 번역 기본값
    DEFAULT_CONTEXT = os.getenv("DEFAULT_CONTEXT", "General")
    DEFAULT_TONE = os.getenv("DEFAULT_TONE", "Informative and helpful")
    DEFAULT_GLOSSARY = {
        "작업지시": "Production Order",
        "공정": "Process",
        "양품": "Good Products",
        "불량": "Defective Products",
        "지시수량": "Order Quantity",
        "목표수량": "Order Quantity",
        "재고": "Stock",
    }