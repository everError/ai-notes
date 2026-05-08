# 42 · SDUI / Generative UI 패턴

## SDUI = Server-Driven UI

서버가 단순 텍스트만 보내는 게 아니라 **"이런 모양으로 그려줘"** 라는 구조화 데이터를 함께 보내고, 클라이언트는 그 데이터를 보고 적절한 컴포넌트(표/차트/카드)를 렌더하는 패턴.

```
서버 응답:
{
  "type": "data-table",
  "data": {
    "columns": [{"field":"name","header":"이름"}, {"field":"date","header":"날짜"}],
    "rows": [{"name":"Doc A","date":"2025-08-15"}]
  }
}
   ↓
클라이언트:
  "type=data-table 이니까 DataTable 컴포넌트로 그리자"
  → 정렬·필터·CSV export 가능한 표 렌더
```

장점:
- 클라이언트는 **렌더링만** 책임
- 서버가 데이터 형태 결정 (LLM이 답변 데이터에 어울리는 컴포넌트 선택)
- 새 컴포넌트 추가가 양쪽에 분산되어도 결합 적음

## Generative UI

LLM이 자기 답변에 어울리는 UI를 자동 선택·생성하는 더 적극적인 형태:

```
사용자: "최근 매출 추이 보여줘"
LLM:    답변 텍스트 + render_linechart(...) 도구 호출
   ↓
프론트:  텍스트 메시지 + 라인차트 컴포넌트
```

핵심 변화: UI 컴포넌트가 **LLM의 도구**가 됩니다. 백엔드 시스템 프롬프트에 컴포넌트 카탈로그 노출 → LLM이 자유롭게 선택.

## SDUI/Generative UI 페이로드 패턴

흔한 컴포넌트 타입과 필드:

| type | 필드 | 용도 |
|---|---|---|
| `data-table` | `{title, columns, rows}` | 표 (정렬·필터 가능) |
| `data-barchart` | `{title, labels, datasets}` | 카테고리 비교 |
| `data-linechart` | `{title, labels, datasets}` | 시간/순서 추세 |
| `data-piechart` | `{title, labels, datasets}` | 비율 분포 |
| `data-document` | `{title, items: [...]}` | 출처 카드 그리드 |
| `interrupt` | `{message, choices}` | HITL 확인 prompt |

각 type별 페이로드 스키마는 프로젝트마다 정의. 클라이언트는 type을 보고 분기.

## 발행 채널

### A. OpenAI metadata 채널 (비표준 자체 약속)

OpenAI 호환 SSE wire에 추가로 `delta.metadata.sdui` 필드:

```
data: {
  "id": "chatcmpl-abc...",
  "model": "...",
  "choices": [{
    "delta": {
      "role": "assistant",
      "content": "",
      "metadata": {
        "sdui": { "type": "data-table", "data": {...} }
      }
    }
  }]
}
```

- 순수 OpenAI 클라이언트는 metadata를 무시
- 자체 클라이언트는 metadata.sdui를 읽어 컴포넌트 렌더

장점: 단순. 단점: 비표준이라 다른 클라이언트와 호환 X.

### B. tool_calls 채널 (OpenAI 표준 안에서)

UI 컴포넌트를 LLM 도구로 정의:

```
서버가 LLM에게 노출:
  tools = [
    render_table(columns, rows),
    render_barchart(labels, datasets),
    ...
  ]
LLM 응답:
  message.tool_calls = [{id:..., name:"render_table", arguments:{...}}]
프론트:
  tool_call_id로 매핑해 렌더
```

장점: OpenAI 표준 안에서 끝남. 단점: LLM이 도구 호출을 정확히 해야 함.

### C. AG-UI Protocol (2026 신표준)

CopilotKit 주도의 agent ↔ UI 표준 프로토콜. 16개 표준 이벤트:

- `RUN_STARTED` / `RUN_FINISHED`
- `TEXT_MESSAGE_*`
- `TOOL_CALL_START` / `TOOL_CALL_ARGS` / `TOOL_CALL_END`
- `STATE_SNAPSHOT` / `STATE_DELTA` (JSON Patch)
- `MESSAGES_SNAPSHOT`
- `RAW`, `CUSTOM`

장점: 멀티에이전트·승인 플로우·상태 동기화가 표준화됨. Microsoft / Oracle / LangChain / Mastra / PydanticAI 채택.

단점: 1st-party SDK가 React 위주. Vue/Angular 등은 직접 구현.

### D. A2UI (Google, 2026)

선언적 JSON UI 스펙. AG-UI 위에 UI 페이로드 표준 제공. 클라이언트가 JSON 트리를 native widget으로 렌더.

### E. Vercel AI SDK + streamUI

Next.js의 RSC(React Server Components) 기반. `streamUI`로 React 컴포넌트를 직접 스트리밍.

장점: 코드와 UI가 한 함수 안. 단점: Next.js / React 종속.

### F. Anthropic Artifacts

Claude.ai의 사이드 패널 패턴. 코드/문서를 별도 영역에 렌더. self-host 환경은 OSS 클론으로 흉내.

### G. MCP Apps (sandboxed iframe)

Model Context Protocol의 SEP-1865 (2026-01). 도구가 sandboxed iframe HTML/JS를 반환:

```
tool result: { _meta: { ui: { resourceUri: "ui://..." } } }
   → 호스트가 iframe 띄움
```

ChatGPT / Claude / VSCode 호스트용. 보안 격리에 강력.

## 패턴별 트레이드오프

| 패턴 | 표준성 | 복잡도 | 라이브러리 지원 |
|---|---|---|---|
| metadata.sdui (자체) | 비표준 | 낮음 | 직접 구현 |
| tool_calls | OpenAI 표준 | 중간 | OpenAI/Anthropic SDK |
| AG-UI | 신표준 | 중상 | React 1st-party, Vue 미흡 |
| A2UI | 신표준 | 중간 | React/Flutter/Angular |
| Vercel AI SDK | Next.js 표준 | 낮음 (Next.js 안) | Next.js 종속 |
| MCP Apps | MCP 표준 | 중상 | 호스트 의존 |

## 그린필드 권장 (2026)

- **단순 시작**: 자체 metadata.sdui (자체 프로토콜이지만 작은 변경으로 시작 가능)
- **표준 시작**: tool_calls (OpenAI 표준 안)
- **장기**: AG-UI + A2UI (생태계 모멘텀, multi-agent 지원)

## hasUsableData 가드

LLM이 실수로 빈 컴포넌트를 발행하는 케이스 방지:

```typescript
function hasUsableData(metadata) {
  switch (metadata.type) {
    case 'data-table':
      return metadata.data?.rows?.length > 0
    case 'data-barchart':
    case 'data-linechart':
    case 'data-piechart':
      return metadata.data?.datasets?.length > 0
    case 'data-document':
      return metadata.data?.items?.length > 0
    default:
      return Object.keys(metadata.data ?? {}).length > 0
  }
}
```

빈 페이로드는 렌더 스킵 → "빈 흰 카드" 같은 UI 결함 방지.

## 외부 학습 자원

- [Server-Driven UI 개념 (Airbnb 2017)](https://medium.com/airbnb-engineering/a-deep-dive-into-airbnbs-server-driven-ui-system-842244c5f5)
- [AG-UI Protocol](https://docs.ag-ui.com/) — 2026 generative UI 표준
- [A2UI](https://a2ui.org/) — Google 선언적 JSON UI
- [Vercel AI SDK Generative UI](https://ai-sdk.dev/docs/ai-sdk-ui/generative-user-interfaces)
- [CopilotKit AG-UI 가이드](https://www.copilotkit.ai/ag-ui)
- [MCP Apps blog](https://blog.modelcontextprotocol.io/posts/2026-01-26-mcp-apps/)
- [Anthropic Artifacts (UX 설명)](https://www.anthropic.com/news/artifacts)
