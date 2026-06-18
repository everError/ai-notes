# WebMCP

## 개요

WebMCP는 웹사이트가 클라이언트 사이드(브라우저)에서 AI 에이전트에게 구조화된 도구(tool)를 노출할 수 있도록 하는 JavaScript API 표준이다. Google과 Microsoft가 공동으로 제안하고 W3C Web Machine Learning Community Group에서 표준화 중이다.

Anthropic의 MCP(백엔드 서버 기반)와는 별개의 표준으로, 브라우저 탭 안에서 클라이언트 사이드로 동작한다.

---

## Anthropic MCP vs WebMCP

|             | Anthropic MCP      | WebMCP                         |
| ----------- | ------------------ | ------------------------------ |
| 동작 위치   | 백엔드 서버        | 브라우저 클라이언트            |
| 구현 언어   | Python, Node.js 등 | JavaScript (프론트엔드)        |
| 사용자 존재 | 불필요 (자동화)    | 사용자가 브라우저에 있어야 함  |
| 통신 방식   | JSON-RPC           | postMessage (브라우저 내부)    |
| 상태        | 성숙               | 표준화 진행 중 (Early Preview) |

둘은 상호 보완 관계다. 예를 들어 항공사 예약 사이트가 백엔드 MCP 서버(Claude, ChatGPT와 직접 연동)와 WebMCP(브라우저에서 사용자와 함께 예약 플로우 처리)를 동시에 운영할 수 있다.

---

## 핵심 개념

### AI 에이전트의 기존 웹 접근 방식 (문제)

현재 AI 에이전트가 웹사이트를 사용할 때:

1. 화면 스크린샷 캡처
2. 비전 모델로 UI 분석
3. 클릭할 위치 추측
4. DOM 파싱 반복

이 방식은 느리고, 비용이 많이 들며, 페이지 디자인이 조금만 바뀌어도 깨진다.

### WebMCP의 접근 방식 (해결)

웹사이트가 AI에게 "여기 사용 가능한 함수 목록이에요"라고 명시적으로 알려준다. AI는 스크린샷 없이 구조화된 함수를 직접 호출한다.

```
기존: AI가 스크린샷 → 분석 → 버튼 클릭 추측 → 반복 (수십 단계)
WebMCP: AI가 searchProducts(query, filters) 한 번 호출 → 결과 수신
```

---

## API 구조

`navigator.modelContext`라는 브라우저 네이티브 API를 통해 동작한다.

### 1. Declarative API (선언형)

기존 HTML 폼에 속성 몇 개만 추가해서 AI 도구로 등록한다.

```html
<form
  toolname="book_flight"
  tooldescription="항공편을 예약합니다"
  toolautosubmit="true"
>
  <input name="departure" type="date" />
  <input name="destination" type="text" />
  <input name="seat_class" type="text" />
  <button type="submit">예약</button>
</form>
```

- `toolname`: 필수. 도구 이름
- `tooldescription`: 필수. 도구 설명 (이 속성 없으면 WebMCP 도구로 인식 안 됨)
- `toolautosubmit`: 선택. AI가 폼을 채우면 자동 제출

기존 폼이 잘 구조화되어 있다면 속성 몇 개 추가만으로 AI에게 노출 가능하다.

### 2. Imperative API (명령형)

JavaScript로 복잡한 도구를 직접 등록한다.

```javascript
navigator.modelContext.registerTool({
  name: "searchProducts",
  description: "상품을 검색합니다",
  inputSchema: {
    type: "object",
    properties: {
      query: {
        type: "string",
        description: "검색어",
      },
      maxPrice: {
        type: "number",
        description: "최대 가격 (원)",
      },
      category: {
        type: "string",
        enum: ["electronics", "clothing", "food"],
      },
    },
    required: ["query"],
  },
  execute: async (input, client) => {
    const results = await fetch(
      `/api/products?q=${input.query}&maxPrice=${input.maxPrice}`,
    );
    return await results.json();
  },
});
```

- `inputSchema`: JSON Schema 형식으로 파라미터 정의
- `execute`: 실제 실행될 콜백 함수 (`input`, `client` 두 인자를 받음)
- `annotations.readOnlyHint`: true로 설정하면 데이터 조회 전용 도구임을 명시

---

## 동작 구조

```
웹사이트 (브라우저 탭)
  └── navigator.modelContext.registerTool() 로 도구 등록
          ↓
      브라우저가 도구 목록 관리
          ↓
      AI 에이전트(브라우저 확장 등)가 도구 발견
          ↓
      AI가 도구 직접 호출
          ↓
      JS 함수 실행 → 결과 반환
```

브라우저가 중간에서 중재자 역할을 한다. 보안 정책(Same-Origin Policy, CSP, HTTPS 필수)도 브라우저가 자동으로 적용한다.

---

## 설계 원칙

WebMCP는 사용자가 없는 완전 자동화가 아니라, **사용자가 브라우저 앞에 있는 협업 시나리오**를 위해 설계됐다.

- 사람의 웹 인터페이스가 1순위, AI 도구는 보조
- 민감한 작업은 사용자 확인 필요
- Headless 브라우징, 완전 자율 에이전트는 WebMCP 범위 밖

---

## 사용 시나리오

**이커머스**
사용자: "100달러 이하이고 M 사이즈 재고 있는 친환경 드레스 찾아줘"
→ AI가 `searchProducts` 호출 → 구조화된 JSON 결과 수신 → 조건에 맞는 상품만 표시

**그래픽 디자인 툴**
사용자: "봄 테마에 흰 배경인 전단지 템플릿 보여줘"
→ AI가 `filterTemplates(theme="spring", background="white")` 호출
→ 사용자가 마음에 드는 것 선택
→ AI가 `editDesign("제목을 'Yard Sale Extravaganza!'로 변경")` 호출

**고객 지원**
사용자가 AI에게 문의하면 AI가 지원 폼을 구조화된 데이터로 자동 제출. 브라우저가 알고 있는 기술 정보(OS, 브라우저 버전 등)도 자동 포함.

---

## 현재 상태 및 지원 환경

- Chrome 146 Canary에서 `chrome://flags` → "WebMCP for testing" 플래그로 테스트 가능
- Google Chrome Early Preview Program 가입 시 공식 문서 및 데모 접근 가능
- Microsoft Edge 지원 예정 (Microsoft가 표준 공동 작성 중)
- W3C 공식 표준 전환 진행 중
- 2026년 중후반 정식 발표 예상

---

## 현재 사용 가능한 구현체

### MCP-B

WebMCP의 레퍼런스 구현체로, `navigator.modelContext` API를 폴리필(polyfill)하여 현재 브라우저에서도 사용 가능하게 한다.

```bash
npm install @mcp-b/transports @modelcontextprotocol/sdk zod
```

```javascript
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { TabServerTransport } from "@mcp-b/transports/tab";

const server = new McpServer({ name: "my-site", version: "1.0.0" });

server.tool(
  "addToCart",
  { productId: z.string(), quantity: z.number() },
  async (args) => {
    await cart.add(args.productId, args.quantity);
    return { content: [{ type: "text", text: "장바구니에 추가됨" }] };
  },
);

const transport = new TabServerTransport();
await server.connect(transport);
```

MCP-B 브라우저 확장을 설치하면 해당 사이트의 도구를 발견하고 Claude Desktop, Cursor 등 MCP 클라이언트와 연결할 수 있다.

---

## 참고 링크

| 자료                     | URL                                                                                                             |
| ------------------------ | --------------------------------------------------------------------------------------------------------------- |
| W3C 공식 스펙            | https://webmachinelearning.github.io/webmcp/                                                                    |
| W3C GitHub               | https://github.com/webmachinelearning/webmcp                                                                    |
| 스펙 Proposal            | https://github.com/webmachinelearning/webmcp/blob/main/docs/proposal.md                                         |
| Chrome 개발자 블로그     | https://developer.chrome.com/blog/webmcp-epp                                                                    |
| MCP-B 공식 문서          | https://docs.mcp-b.ai/                                                                                          |
| Chrome DevTools 퀵스타트 | https://github.com/WebMCP-org/chrome-devtools-quickstart                                                        |
| VentureBeat 분석         | https://venturebeat.com/infrastructure/google-chrome-ships-webmcp-in-early-preview-turning-every-website-into-a |
