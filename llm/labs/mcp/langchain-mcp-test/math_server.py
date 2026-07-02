from mcp.server.fastmcp import FastMCP

print("[math_server] starting...")  # ✅ 이 줄 추가

mcp = FastMCP("Math")

@mcp.tool()
def add(a: int, b: int) -> int:
    return a + b

@mcp.tool()
def multiply(a: int, b: int) -> int:
    return a * b

if __name__ == "__main__":
    print("[math_server] running stdio...")
    mcp.run(transport="stdio")
