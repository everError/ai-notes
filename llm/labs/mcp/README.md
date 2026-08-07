# Model Context Protocol (MCP)

> **MCP** 는 LLM 애플리케이션이 외부 도구·데이터와 연결되는 방식을 표준화한 **JSON-RPC 2.0 기반 개방형 프로토콜**입니다.

**기준 스펙: 2026-07-28** — 이 버전에서 프로토콜 코어가 stateless 로 전환되었습니다.

---

## 노트

| 문서                                                       | 내용                                              |
| ---------------------------------------------------------- | ------------------------------------------------- |
| [MCP_Architecture](./notes/MCP_Architecture.md)             | 구성 요소, 무상태 코어, 전송, 라우팅, MRTR        |
| [MCP_Stateless_Migration](./notes/MCP_Stateless_Migration.md) | 기존 구현을 2026-07-28 스펙으로 옮기는 절차       |
| [FastMCP](./notes/FastMCP.md)                               | 파이썬 MCP 서버 프레임워크                        |
| [MCP APPs](./notes/MCP%20APPs.md)                           | 대화창에 인터랙티브 UI 를 렌더링하는 공식 확장    |
| [WebMCP](./notes/WebMCP.md)                                 | 브라우저에서 도구를 노출하는 별도 W3C 표준        |

---

## 해결하려는 문제

도구를 LLM 애플리케이션에 직접 바인딩하면, 애플리케이션 M 개와 도구 N 개마다 개별 연동이 필요합니다(M×N). MCP 는 그 사이에 표준 규약을 두어 **M+N** 으로 줄입니다.

도구를 한 번 MCP 서버로 만들어 두면, MCP 를 지원하는 모든 호스트가 그대로 사용할 수 있습니다.

---

## 구성 요소

| 구성 요소      | 역할                                                             |
| -------------- | ---------------------------------------------------------------- |
| **Host**       | 사용자와 맞닿은 애플리케이션. 어떤 서버에 연결할지, 어떤 호출을 승인할지 결정하는 신뢰 경계 (예: Claude Desktop, LangGraph 에이전트) |
| **Client**     | 호스트 안에서 서버 하나와의 연결을 담당하는 계층                 |
| **Server**     | 요청을 받아 실제 기능을 실행하고 결과를 반환                     |

서버가 노출하는 것은 세 종류입니다.

| 종류         | 설명                                      | 제어 주체    |
| ------------ | ----------------------------------------- | ------------ |
| **Tool**     | 실행 가능한 기능. 부수효과를 가질 수 있음 | 모델         |
| **Resource** | 읽기 전용 데이터·컨텍스트                 | 애플리케이션 |
| **Prompt**   | 재사용 가능한 프롬프트 템플릿             | 사용자       |

---

## 메시지 구조

MCP 는 **JSON-RPC 2.0** 을 그대로 사용합니다.

### 요청

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "echo",
    "arguments": { "text": "hello" },
    "_meta": {
      "io.modelcontextprotocol/clientInfo": { "name": "my-app", "version": "1.0" }
    }
  }
}
```

### 응답

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "content": [{ "type": "text", "text": "Echo: hello" }]
  }
}
```

요청과 응답은 JSON-RPC 의 `id` 로 연결됩니다. 실패 시에는 `result` 대신 `error` 객체가 반환됩니다.

### 주요 메서드

| 메서드           | 용도                     |
| ---------------- | ------------------------ |
| `tools/list`     | 사용 가능한 도구 목록    |
| `tools/call`     | 도구 실행                |
| `resources/list` | 리소스 목록              |
| `resources/read` | 리소스 읽기              |
| `prompts/list`   | 프롬프트 템플릿 목록     |
| `prompts/get`    | 프롬프트 템플릿 가져오기 |

---

## 전송 계층

| 전송                | 용도                     | 상태                        |
| ------------------- | ------------------------ | --------------------------- |
| **stdio**           | 로컬 서브프로세스로 실행 | 유효. 로컬 도구의 기본 선택 |
| **Streamable HTTP** | 원격 서버                | 권장                        |
| HTTP + SSE (legacy) | 구 원격 전송             | 폐기. 최소 12개월 유예      |

---

## 무상태 코어

2026-07-28 스펙에서 세션이 제거되었습니다.

- `initialize` / `initialized` 핸드셰이크와 `Mcp-Session-Id` 가 사라졌습니다
- 모든 요청이 프로토콜 버전과 클라이언트 정보를 스스로 싣습니다
- 서버가 클라이언트에게 먼저 요청을 보낼 수 없습니다. 추가 입력이 필요하면 응답에 담아 반환하고 클라이언트가 재호출합니다(MRTR)
- 라우팅 정보가 `Mcp-Method` / `Mcp-Name` HTTP 헤더로 올라가, 게이트웨이가 본문을 파싱하지 않고 제어할 수 있습니다

결과적으로 MCP 서버 운영이 **평범한 무상태 HTTP 서비스 운영**과 같아졌습니다. 자세한 내용은 [MCP_Architecture](./notes/MCP_Architecture.md), 기존 구현 전환은 [MCP_Stateless_Migration](./notes/MCP_Stateless_Migration.md) 을 참고합니다.

---

## 이 랩의 실습 프로젝트

| 디렉터리                | 내용                                         |
| ----------------------- | -------------------------------------------- |
| `langchain-mcp-test/`   | FastMCP 최소 서버(math, weather) + LangChain 에이전트 연동 |
| `dotnet/Docs-Mcp/`      | .NET MCP 서버                                |
| `rust-sdk/`             | rmcp 기반 서버·클라이언트 양쪽 구현          |
| `mcp-translation/`      | 번역 MCP 서버. Docker + Ollama 연동          |

---

## 특징

| 항목       | 설명                                                         |
| ---------- | ------------------------------------------------------------ |
| 표준화     | 도구를 한 번 만들면 MCP 지원 호스트 전부가 사용 가능          |
| 언어 독립  | JSON-RPC 기반이므로 구현 언어를 가리지 않음                   |
| 확장 가능  | 새 기능은 선택적 확장으로 먼저 출시되어 안정화 기간을 거침    |
| 운영 친화  | 무상태 코어와 헤더 라우팅으로 기존 HTTP 인프라를 그대로 활용  |

LangChain, Semantic Kernel 같은 에이전트 프레임워크가 애플리케이션 **내부** 구조를 다룬다면, MCP 는 애플리케이션과 도구 **사이**의 통신 규약을 다룹니다. 둘은 경쟁 관계가 아니라 층이 다릅니다.

---

## 참고

| 자료                 | URL                                                       |
| -------------------- | --------------------------------------------------------- |
| 공식 문서            | https://modelcontextprotocol.io                            |
| 2026-07-28 스펙 공지 | https://blog.modelcontextprotocol.io/posts/2026-07-28/     |
| 스펙 저장소          | https://github.com/modelcontextprotocol/modelcontextprotocol |
