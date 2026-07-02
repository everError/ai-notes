## Semantic Kernel: `ChatHistory`와 `OpenAIPromptExecutionSettings` 최신 가이드

지능적인 AI 에이전트를 구축하기 위해서는 대화의 흐름을 기억하고 AI의 행동을 정밀하게 제어하는 능력이 필수적입니다. Semantic Kernel에서는 이 두 가지 핵심 역할을 각각 `ChatHistory`와 `OpenAIPromptExecutionSettings`가 담당합니다.

### \#\# 1. `ChatHistory` - 대화의 기억을 담는 그릇 🧠

`ChatHistory`는 AI 에이전트의 **단기 기억**을 담당하는 가장 중요한 클래스입니다. 사용자와 AI, 그리고 시스템과 도구 사이에 오고 간 모든 메시지를 순서대로 저장하여 대화의 맥락을 유지합니다.

#### **주요 특징**

- **메시지 목록**: 내부적으로는 `List<ChatMessageContent>`와 같이 동작하여, 순차적인 대화 메시지 목록을 관리합니다.
- **역할(Role) 기반**: 각 메시지는 누가 말했는지(`AuthorRole`)를 명확히 구분하여 저장합니다. 이를 통해 AI는 대화의 전체 흐름과 각 발언의 주체를 파악할 수 있습니다.

#### **핵심 구성 요소**

`ChatHistory`는 `ChatMessageContent` 객체들의 목록이며, 각 `ChatMessageContent`는 다음과 같은 주요 `AuthorRole`을 가집니다.

- `System`: AI의 행동 방식이나 정체성을 정의하는 시스템 메시지입니다. 대화의 가장 처음에 추가하여 AI의 전반적인 톤앤매너와 규칙을 설정합니다.
- `User`: 최종 사용자가 입력한 메시지입니다.
- `Assistant`: AI 어시스턴트가 생성한 응답 메시지입니다. 여기에는 일반 텍스트뿐만 아니라, 도구를 호출하라는 요청(`ToolCallContent`)도 포함될 수 있습니다.
- `Tool`: 플러그인(도구)이 실행된 후 그 결과를 담는 메시지입니다. AI는 이 메시지를 보고 도구 실행 결과를 학습하여 다음 행동을 결정합니다.

#### **주요 사용법 및 코드 예시**

```csharp
// 1. ChatHistory 객체 생성
var history = new ChatHistory();

// 2. 역할(Role)에 따라 메시지 추가
// 시스템 메시지: AI의 정체성 설정
history.AddSystemMessage("You are a helpful AI assistant that is an expert in math.");

// 사용자 메시지: 사용자의 질문
history.AddUserMessage("What is 5 + 5?");

// 어시스턴트 메시지: AI의 응답 (이전 대화 기록을 불러올 때 사용)
history.AddAssistantMessage("The answer is 10.");

// 도구 메시지: 도구 실행 결과를 AI에게 알려줄 때 사용
history.AddToolMessage("tool_call_id_123", "The result of the calculation is 10.");
```

---

### \#\# 2. `OpenAIPromptExecutionSettings` - AI의 행동을 제어하는 스위치 🎛️

`OpenAIPromptExecutionSettings`는 OpenAI 모델에 요청을 보낼 때, 모델의 동작을 세밀하게 제어하기 위한 **설정 모음**입니다. 이 설정을 통해 AI가 도구를 어떻게 사용할지, 얼마나 창의적으로 답변할지 등을 결정할 수 있습니다.

#### **가장 중요한 속성: `ToolCallBehavior`**

ReAct 패턴과 같은 에이전트를 구현할 때 가장 핵심이 되는 속성입니다. AI가 플러그인(도구)을 어떻게 처리할지 결정합니다.

| 속성 값                         | 설명                                                                                                                                                                                                              | 사용 사례                                                                                           |
| :------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------- |
| **`AutoInvokeKernelFunctions`** | AI가 도구 호출을 결정하면, Semantic Kernel이 **자동으로 해당 함수를 실행하고 그 결과까지 AI에게 다시 전달**합니다. 개발자는 `while` 루프 없이 단 한 번의 API 호출로 최종 결과를 얻습니다.                         | **(가장 권장됨)** 대부분의 경우, 가장 간단하고 강력한 방법.                                         |
| **`EnableKernelFunctions`**     | AI에게 사용 가능한 도구 목록을 알려주지만, **자동으로 실행하지는 않습니다.** AI가 도구 호출을 요청하면 그 요청(`ToolCallContent`)을 그대로 반환합니다. 개발자가 직접 해당 도구를 실행하고 루프를 제어해야 합니다. | 도구 실행 전후에 검증이나 추가 로직이 필요할 때, Langsmith처럼 명시적인 로그를 남기고 싶을 때 사용. |
| **`None`**                      | AI에게 도구의 존재를 전혀 알리지 않습니다. AI는 도구를 사용할 수 없으며, 순수하게 텍스트만 생성합니다.                                                                                                            | 간단한 텍스트 생성, 요약, 번역 등 도구가 필요 없는 작업에 사용.                                     |

#### **기타 주요 속성**

- **`Temperature`** (0.0 \~ 2.0): 응답의 무작위성(창의성)을 조절합니다. 값이 높을수록 더 다양하고 창의적인 답변이, 낮을수록 더 결정론적이고 일관된 답변이 나옵니다. (기본값: 1.0)
- **`TopP`** (0.0 \~ 1.0): Temperature와 함께 사용되며, 확률이 높은 단어들 중에서 선택하여 응답을 생성합니다. 일반적으로 둘 중 하나만 변경하는 것을 권장합니다.
- **`MaxTokens`**: AI가 생성할 수 있는 최대 토큰(단어/글자 수)을 제한합니다.
- **`StopSequences`**: 특정 문자열이 생성되면 AI가 응답 생성을 즉시 중단하도록 설정합니다.
- **`ResponseFormat`**: AI의 응답을 특정 형식(예: `json_object`를 사용한 JSON 모드)으로 강제할 수 있습니다.

#### **사용법 및 코드 예시**

```csharp
// 1. 실행 설정 객체 생성 및 속성 할당
var executionSettings = new OpenAIPromptExecutionSettings
{
    // ReAct 에이전트를 위해 자동 도구 호출 기능을 켭니다.
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,

    // 응답의 창의성을 약간 낮춥니다.
    Temperature = 0.7,

    // 최대 500 토큰까지만 생성하도록 제한합니다.
    MaxTokens = 500
};

// 2. AI 서비스 호출 시 설정 객체를 전달합니다.
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

var result = await chatCompletionService.GetChatMessageContentAsync(
    chatHistory,
    executionSettings, // 여기에 전달
    kernel);
```
