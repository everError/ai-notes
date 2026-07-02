# Semantic Kernel + ASP.NET Core 사용 메서드 요약

## Semantic Kernel 관련 기능

### ✅ Kernel 초기화 및 모델 등록

- `Kernel.CreateBuilder()`

  - Semantic Kernel 빌더 인스턴스 생성

- `builder.AddOpenAIChatCompletion(modelId, apiKey)`

  - OpenAI 모델 등록 (예: gpt-4)

- `builder.Build()`

  - Kernel 인스턴스 생성 완료

### ✅ 프롬프트 실행

- `kernel.InvokePromptAsync(prompt)`

  - 전체 응답을 한 번에 가져옴

- `kernel.InvokePromptStreamingAsync(prompt)`

  - `IAsyncEnumerable<StreamingKernelContent>` 형태로 토큰 단위 스트리밍

- `StreamingKernelContent.ToString()`

  - 각 토큰을 문자열로 반환

### ✅ 스킬 등록 및 호출

- `kernel.ImportPluginFromObject(object, pluginName)`

  - C# 객체의 메서드를 Skill로 등록

- `kernel.InvokeAsync("pluginName", "functionName", arguments)`

  - 등록된 Skill 호출 및 실행

### ✅ Memory 기능 (임베딩 기반 기억 저장)

- `kernel.Memory.SaveInformationAsync(collection, text, id)`

  - 정보를 컬렉션에 저장

- `kernel.Memory.SearchAsync(collection, query)`

  - 유사한 정보 검색

### ✅ Planner 기능 (목표 기반 자동 계획 생성)

- `SequentialPlanner.Create(kernel)`

  - 단계별 작업 계획 생성

- `planner.CreatePlanAsync(goal)`

  - 목표(goal)를 기반으로 실행 가능한 Plan 생성

- `plan.InvokeAsync(kernel)`

  - Plan 실행

### ✅ Agent/Message 관련

- `MCPMessage` / `role`, `content`, `metadata`

  - 에이전트 간 메시지를 표준화된 구조로 주고받음

- `AgentGroup`, `IAgent`, `AgentState`

  - 여러 에이전트의 역할 분리 및 상태 관리 구성 가능

## SSE (Server-Sent Events) 스트리밍 응답

- `Response.Headers["Content-Type"] = "text/event-stream"`

  - SSE 전송 형식 지정

- `await Response.WriteAsync("data: <내용>\n\n")`

  - SSE 메시지 형식에 맞춰 전송

- `await Response.Body.FlushAsync()`

  - 버퍼링 없이 즉시 전송

## 참고

- [https://github.com/microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel)
- [https://learn.microsoft.com/en-us/semantic-kernel/](https://learn.microsoft.com/en-us/semantic-kernel/)
