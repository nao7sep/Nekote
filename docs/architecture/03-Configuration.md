# Configuration System Architecture

**Purpose:** Flexible, provider-agnostic configuration
**Layer:** Infrastructure
**Complexity:** ★★☆☆☆ (Simple - just config classes)

---

## Overview

Configuration classes:
- **No constants** (no hardcoded section names)
- **Flexible fallback chain** (feature-specific → default → hardcoded)
- **Integration-friendly** (works with existing apps)

---

## OpenAI Configuration

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI;

/// <summary>
/// OpenAI サービスの設定を定義します。
/// </summary>
public sealed class OpenAiConfiguration
{
    /// <summary>
    /// デフォルトの API キーを取得します。
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// チャット専用の API キーを取得します。
    /// </summary>
    public string? ChatApiKey { get; init; }

    /// <summary>
    /// エンベディング専用の API キーを取得します。
    /// </summary>
    public string? EmbeddingApiKey { get; init; }

    /// <summary>
    /// デフォルトのベース URL を取得します。
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// チャット専用のエンドポイントを取得します。
    /// </summary>
    public string? ChatEndpoint { get; init; }

    /// <summary>
    /// エンベディング専用のエンドポイントを取得します。
    /// </summary>
    public string? EmbeddingEndpoint { get; init; }

    /// <summary>
    /// デフォルトのモデル名を取得します。
    /// </summary>
    public string? DefaultModelName { get; init; }

    /// <summary>
    /// チャット専用のモデル名を取得します。
    /// </summary>
    public string? ChatModelName { get; init; }

    /// <summary>
    /// エンベディング専用のモデル名を取得します。
    /// </summary>
    public string? EmbeddingModelName { get; init; }
}
```

---

## Gemini Configuration

```csharp
namespace Nekote.Core.AI.Infrastructure.Gemini;

// Naming Convention: We use company names (OpenAI, Anthropic, Mistral) for consistency,
// except for Google where we use "Gemini" because "GoogleConfiguration" would be too
// ambiguous given Google's broad product portfolio. Gemini specifically refers to
// Google's AI model family, providing clear semantic meaning.

/// <summary>
/// Gemini サービスの設定を定義します。
/// </summary>
public sealed class GeminiConfiguration
{
    /// <summary>
    /// デフォルトの API キーを取得します。
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// チャット専用の API キーを取得します。
    /// </summary>
    public string? ChatApiKey { get; init; }

    /// <summary>
    /// エンベディング専用の API キーを取得します。
    /// </summary>
    public string? EmbeddingApiKey { get; init; }

    /// <summary>
    /// デフォルトのベース URL を取得します。
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// チャット専用のエンドポイントを取得します。
    /// </summary>
    public string? ChatEndpoint { get; init; }

    /// <summary>
    /// エンベディング専用のエンドポイントを取得します。
    /// </summary>
    public string? EmbeddingEndpoint { get; init; }

    /// <summary>
    /// デフォルトのモデル名を取得します。
    /// </summary>
    public string? DefaultModelName { get; init; }

    /// <summary>
    /// チャット専用のモデル名を取得します。
    /// </summary>
    public string? ChatModelName { get; init; }

    /// <summary>
    /// エンベディング専用のモデル名を取得します。
    /// </summary>
    public string? EmbeddingModelName { get; init; }
}
```

---

## Configuration Template (Other Providers)

All other providers follow the same pattern:
- `AnthropicConfiguration`
- `XAiConfiguration`
- `MistralConfiguration`
- `DeepSeekConfiguration`
- `DeepLConfiguration` (translation only, no embedding)

---

## appsettings.json Example

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-default-key",
      "ChatApiKey": "sk-chat-only-key",
      "EmbeddingApiKey": "sk-embedding-only-key",
      "BaseUrl": "https://api.openai.com",
      "ChatEndpoint": "https://custom-chat.example.com/chat",
      "DefaultModelName": "gpt-4",
      "ChatModelName": "gpt-4-turbo",
      "EmbeddingModelName": "text-embedding-3-small"
    },
    "Gemini": {
      "ApiKey": "AIza...",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "DefaultModelName": "gemini-2.5-pro"
    }
  }
}
```

---

## Resolution Order

When a repository needs a configuration value:

1. **Feature-specific** (e.g., `ChatApiKey`, `ChatEndpoint`, `ChatModelName`)
2. **Default** (e.g., `ApiKey`, `BaseUrl`, `DefaultModelName`)
3. **Hardcoded fallback** (e.g., `"https://api.openai.com"`)
4. **Throw exception** if critical value (like API key) is still null

---

## Usage Pattern

### In Repository Constructor

```csharp
internal sealed class OpenAiChatRepository : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiConfiguration _config;

    public OpenAiChatRepository(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiChatRepository> logger,
        IOptions<OpenAiConfiguration> configuration)
    {
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
}
```

### In Repository Method

```csharp
public async Task<ChatResponse> GetCompletionAsync(...)
{
    // Resolve model name with fallback
    var modelName = _config.ChatModelName ?? _config.DefaultModelName
        ?? throw new InvalidOperationException(
            "OpenAI model name is not configured. Provide either 'DefaultModelName' or 'ChatModelName'.");

    var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);
    // ... rest of implementation
}
```

---

## Implementation Checklist

- [ ] Create configuration class for each provider
- [ ] Add Gemini naming rationale comment
- [ ] All properties nullable
- [ ] No constants (no `SectionName`)
- [ ] XML comments in Japanese
- [ ] Create example `appsettings.json`

---

**Estimated Time:** 20 minutes
**Dependencies:** None
**Next Step:** JSON Serialization (04-Json-Serialization.md)
