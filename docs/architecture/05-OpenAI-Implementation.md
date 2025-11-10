# OpenAI Chat Implementation - Minimal Viable Product

**Purpose:** First working AI integration (Chat only, no streaming, no diagnostics)
**Layer:** Infrastructure
**Complexity:** ★★★☆☆ (Moderate - first real implementation)

---

## Overview

This is the **simplest path to working code**:
- OpenAI Chat Completions API only
- No streaming (just standard completion)
- No diagnostics/tracking (add later)
- No caching (add later)
- Direct `HttpClient` calls

---

## Step 1: DTOs (Data Transfer Objects)

### OpenAiChatRequestDto.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI Chat Completions API のリクエスト DTO。
/// すべてのプロパティは nullable として定義され、API の不整合に対応します。
/// </summary>
internal sealed class OpenAiChatRequestDto
{
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("messages")]
    public List<OpenAiMessageDto>? Messages { get; init; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("top_p")]
    public float? TopP { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}
```

### OpenAiMessageDto.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI メッセージの DTO。
/// </summary>
internal sealed class OpenAiMessageDto
{
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
```

### OpenAiChatResponseDto.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI Chat Completions API のレスポンス DTO。
/// </summary>
internal sealed class OpenAiChatResponseDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("object")]
    public string? Object { get; init; }

    [JsonPropertyName("created")]
    public long? Created { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("choices")]
    public List<OpenAiChoiceDto>? Choices { get; init; }

    [JsonPropertyName("usage")]
    public OpenAiUsageDto? Usage { get; init; }
}
```

### OpenAiChoiceDto.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI レスポンスの選択肢を表す DTO。
/// </summary>
internal sealed class OpenAiChoiceDto
{
    [JsonPropertyName("index")]
    public int? Index { get; init; }

    [JsonPropertyName("message")]
    public OpenAiMessageDto? Message { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}
```

### OpenAiUsageDto.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI のトークン使用量を表す DTO。
/// </summary>
internal sealed class OpenAiUsageDto
{
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; init; }
}
```

---

## Step 2: Mapper (Anti-Corruption Layer)

### OpenAiChatMapper.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI の DTO とドメインモデル間の変換を行います。
/// </summary>
internal static class OpenAiChatMapper
{
    /// <summary>
    /// ドメインモデルを OpenAI API のリクエスト DTO に変換します。
    /// </summary>
    public static OpenAiChatRequestDto ToRequestDto(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options,
        string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName));
        }

        return new OpenAiChatRequestDto
        {
            Model = modelName,
            Messages = messages.Select(m => new OpenAiMessageDto
            {
                Role = MapRole(m.Role),
                Content = m.Content,
                Name = m.Name
            }).ToList(),
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens,
            TopP = options?.TopP,
            Stream = false
        };
    }

    /// <summary>
    /// OpenAI API のレスポンス DTO をドメインモデルに変換します。
    /// </summary>
    public static ChatResponse ToDomainModel(OpenAiChatResponseDto dto)
    {
        // Defensive validation
        if (dto.Choices == null || dto.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "OpenAI response is malformed: 'choices' array is null or empty.");
        }

        var firstChoice = dto.Choices[0];

        if (firstChoice.Message == null)
        {
            throw new InvalidOperationException(
                "OpenAI response is malformed: 'message' object is null.");
        }

        if (string.IsNullOrWhiteSpace(firstChoice.Message.Content))
        {
            throw new InvalidOperationException(
                "OpenAI response is malformed: 'content' is null or empty.");
        }

        return new ChatResponse
        {
            Content = firstChoice.Message.Content,
            FinishReason = firstChoice.FinishReason ?? "unknown",
            Usage = dto.Usage != null ? new TokenUsage
            {
                PromptTokens = dto.Usage.PromptTokens ?? 0,
                CompletionTokens = dto.Usage.CompletionTokens ?? 0,
                TotalTokens = dto.Usage.TotalTokens ?? 0
            } : null,
            ResponseId = dto.Id
        };
    }

    /// <summary>
    /// ドメインの <see cref="ChatRole"/> を OpenAI の文字列表現にマッピングします。
    /// </summary>
    private static string MapRole(ChatRole role)
    {
        return role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            _ => throw new ArgumentException($"Unsupported chat role: {role}", nameof(role))
        };
    }
}
```

---

## Step 3: Repository (Implementation)

### OpenAiChatRepository.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI の Chat Completions API を使用してチャット補完を実行します。
/// </summary>
internal sealed class OpenAiChatRepository : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiChatRepository> _logger;
    private readonly OpenAiConfiguration _config;

    public OpenAiChatRepository(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiChatRepository> logger,
        IOptions<OpenAiConfiguration> configuration)
    {
        _logger = logger;
        _config = configuration.Value;

        // Resolve API key with fallback
        var apiKey = _config.ChatApiKey ?? _config.ApiKey
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured. Provide either 'ApiKey' or 'ChatApiKey'.");

        // Resolve endpoint with fallback chain
        var endpoint = _config.ChatEndpoint
            ?? (_config.BaseUrl != null ? $"{_config.BaseUrl}/v1/chat/completions" : null)
            ?? "https://api.openai.com/v1/chat/completions";

        _httpClient = httpClientFactory.CreateClient("OpenAI-Chat");
        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Resolve model name with fallback
        var modelName = options?.ModelName ?? _config.ChatModelName ?? _config.DefaultModelName
            ?? throw new InvalidOperationException(
                "OpenAI model name is not configured. Provide 'DefaultModelName' or 'ChatModelName'.");

        // Convert domain model to provider-specific DTO
        var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);

        // Serialize request for API call
        var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);
        _logger.LogDebug("Sending chat completion request to OpenAI: {Json}", json);

        // Send HTTP request to provider endpoint
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken)
            .ConfigureAwait(false);

        // Read response body
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        // Check for HTTP errors
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI API returned error: {StatusCode} - {Body}",
                (int)response.StatusCode,
                responseJson);

            throw new InvalidOperationException(
                $"OpenAI API request failed with status {response.StatusCode}: {responseJson}");
        }

        // Deserialize API response
        var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(
            responseJson,
            JsonDefaults.Options)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAI response.");

        // Convert DTO to domain model
        return OpenAiChatMapper.ToDomainModel(responseDto);
    }

    public IAsyncEnumerable<string> GetCompletionStreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement streaming in Phase 2
        throw new NotImplementedException("Streaming is not yet implemented.");
    }
}
```

---

## Step 4: DI Registration

### OpenAiServiceCollectionExtensions.cs

```csharp
namespace Nekote.Core.AI.Infrastructure.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat;

/// <summary>
/// OpenAI サービスの DI 登録を提供します。
/// </summary>
public static class OpenAiServiceCollectionExtensions
{
    /// <summary>
    /// OpenAI のチャットサービスを登録します。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configuration">アプリケーション設定。</param>
    /// <param name="configurationSectionPath">
    /// OpenAI 設定が格納されているセクションパス (デフォルト: "AI:OpenAI")。
    /// </param>
    public static IServiceCollection AddOpenAiChat(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "AI:OpenAI")
    {
        // Bind configuration from flexible path
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(configurationSectionPath));

        // Register HttpClient for OpenAI
        services.AddHttpClient("OpenAI-Chat");

        // Register chat service
        services.AddScoped<IChatCompletionService, OpenAiChatRepository>();

        return services;
    }
}
```

---

## Step 5: Usage Example

### Program.cs (Console Test)

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.DependencyInjection;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // Register OpenAI chat service
    services.AddOpenAiChat(context.Configuration);
});

var host = builder.Build();

// Test the service
var chatService = host.Services.GetRequiredService<IChatCompletionService>();

var messages = new List<ChatMessage>
{
    new() { Role = ChatRole.System, Content = "You are a helpful assistant." },
    new() { Role = ChatRole.User, Content = "Hello! What can you do?" }
};

var response = await chatService.GetCompletionAsync(messages);

Console.WriteLine($"AI Response: {response.Content}");
Console.WriteLine($"Finish Reason: {response.FinishReason}");
Console.WriteLine($"Tokens Used: {response.Usage?.TotalTokens ?? 0}");
```

### appsettings.json

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-your-api-key-here",
      "DefaultModelName": "gpt-4"
    }
  }
}
```

---

## Implementation Checklist

- [ ] Create `/Infrastructure/OpenAI/Chat/Dtos/` folder
- [ ] Implement all 5 DTO classes
- [ ] Implement `OpenAiChatMapper`
- [ ] Implement `OpenAiChatRepository`
- [ ] Implement DI extension method
- [ ] Create test console app
- [ ] Test with real OpenAI API key
- [ ] Verify error handling works

---

## What We're NOT Implementing Yet

- ❌ Streaming (complex SSE parsing)
- ❌ Diagnostics/tracking (add later)
- ❌ Caching (add later)
- ❌ Retry logic (add later)
- ❌ Other providers (one at a time)
- ❌ Embedding or Translation (Chat first)

---

**Estimated Time:** 2-3 hours
**Dependencies:** 01, 02, 03, 04
**Success Criteria:** Console app successfully calls OpenAI and prints response
**Next Step:** Add Unit Tests (06-Testing.md)
