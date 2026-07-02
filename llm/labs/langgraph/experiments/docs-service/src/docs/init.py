import os
import shutil
from langchain.docstore.document import Document
from langchain_community.vectorstores import Chroma
from langchain_huggingface import HuggingFaceEmbeddings

# --- Path Settings ---
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
DATA_DIR_PATH = os.path.join(BASE_DIR, "data")
DB_DIR = os.path.join(BASE_DIR, "chroma_db")

def load_documents_from_directory(directory_path: str) -> list[Document]:
    """Loads all .txt files from the specified directory and converts them into a list of Document objects."""
    all_documents = []
    print(f"Searching for .txt files in '{directory_path}'...")

    for filename in os.listdir(directory_path):
        if not filename.endswith(".txt"):
            continue

        file_path = os.path.join(directory_path, filename)
        print(f"-> Loading file: '{filename}'")

        with open(file_path, 'r', encoding='utf-8') as f:
            lines = [line.strip() for line in f.readlines()]
            for i, line in enumerate(lines):
                if line:
                    doc = Document(
                        page_content=line,
                        metadata={"source": filename, "line": i + 1}
                    )
                    all_documents.append(doc)
    return all_documents


def main():
    """
    매번 기존 DB를 삭제하고 data 폴더의 내용으로 새로 생성합니다.
    """
    # ⭐️ 1. 기존 DB 폴더가 있다면 완전히 삭제
    if os.path.exists(DB_DIR):
        print(f"기존 DB 폴더 '{DB_DIR}'를 삭제합니다...")
        shutil.rmtree(DB_DIR)
        print("삭제 완료.")

    # --- 2. 문서 로드 ---
    documents = load_documents_from_directory(DATA_DIR_PATH)
    if not documents:
        print("로드할 문서가 없습니다. data 디렉토리에 .txt 파일이 있는지 확인하세요.")
        return
    print(f"\n총 {len(documents)}개의 청크(chunk)를 로드했습니다.")
    print("-" * 30)

    # --- 3. 임베딩 모델 준비 ---
    print("임베딩 모델을 초기화합니다...")
    model_name = "sentence-transformers/all-mpnet-base-v2"
    embeddings = HuggingFaceEmbeddings(
        model_name=model_name,
        model_kwargs={'device': 'cpu'},
        encode_kwargs={'normalize_embeddings': True}
    )
    print("임베딩 모델이 준비되었습니다.")
    print("-" * 30)

    # --- 4. ChromaDB를 항상 새로 생성 및 저장 ---
    print(f"Chroma DB를 '{DB_DIR}' 경로에 새로 생성하고 문서를 저장합니다...")
    vectordb = Chroma.from_documents(
        documents=documents,
        embedding=embeddings,
        persist_directory=DB_DIR
    )
    print("새로운 DB 생성 및 저장이 완료되었습니다.")
    print("-" * 30)

    # --- 5. 유사도 검색 테스트 ---
    print("저장된 데이터를 기반으로 유사도 검색을 테스트합니다.")
    query = "Is the user's email address a required field?"
    search_results = vectordb.similarity_search(query, k=2)

    print(f"\n[Query]\n{query}\n")
    print("[Search Results]")
    if not search_results:
        print("No results found.")
    for doc in search_results:
        print(f"Content: {doc.page_content}")
        print(f"Source: {doc.metadata['source']} (Line: {doc.metadata['line']})")
        print()

if __name__ == "__main__":
    main()