# Nekote.Core AI Architecture - Design Revisions

**Date:** 2025-11-10
**Purpose:** Address critical design decisions before implementation

---

## Summary of Required Changes

This document addresses 11 critical design issues identified in the initial architecture:

1. **xAI Directory Naming** - Fix inconsistency
2. **Naming Rationale Documentation** - Explain Gemini exception
3. **Diagnostic System Redesign** - Build comprehensive interaction tracking
4. **JSON Serialization Options** - Prevent performance waste
5. **Comment Style** - Remove numbered comments
6. **DTO Nullability Strategy** - Handle broken APIs defensively
7. **Exception Strategy** - Clarify custom vs built-in exceptions
8. **Configuration Flexibility** - Remove constants, support integration
9. **Granular Configuration** - Per-feature API keys and endpoints
10. **Provider Naming Consistency** - Company names vs product names
11. **Centralized Prompt System** - Avoid hard-coded prompts

---

## 1. Directory Naming: xAI

### Problem
The company "xAI" is styled with lowercase 'x'. The directory was named `/XAI` but class prefix is correctly `XAi`.

### Solution
- **Directory name:** `/xAI` (matches company branding)
- **Class prefix:** `XAi` (C# convention: first letter of each segment capitalized)
- **Rationale:** "xAI" = "x" + "AI" → `XAiChatRepository`, `XAiConfiguration`

### Updated Directory Structure
```
/Infrastructure
    ├─ /OpenAI       // Company: OpenAI → Classes: OpenAi*
    ├─ /Gemini       // Product: Gemini → Classes: Gemini* (see #2)
    ├─ /Anthropic    // Company: Anthropic → Classes: Anthropic*
    ├─ /xAI          // Company: xAI → Classes: XAi*
    ├─ /Mistral      // Company: Mistral → Classes: Mistral*
    ├─ /DeepSeek     // Company: DeepSeek → Classes: DeepSeek*
    └─ /DeepL        // Company: DeepL → Classes: DeepL*
```

---

## 2. Naming Rationale: Why "Gemini" Not "Google"?

### Decision
Use **company names** for all providers **except Google**, where we use the product name "Gemini".

### Rationale
**Company names are preferred because:**
- They represent the organization providing the service
- They are stable (products get renamed, companies rarely do)
- Example: OpenAI's API isn't called "ChatGPT API" — ChatGPT, Sora, DALL-E are all products under OpenAI

**The Gemini exception:**
- "Google" is too vague (Gmail, Maps, Drive, Cloud, etc.)
- "Google AI" is not the official product branding
- "Gemini" is the specific AI product line (like "Claude" is for Anthropic)
- Counter-argument: Anthropic offers "Claude" but we use "Anthropic"
  - **Resolution:** Use "Gemini" because Google's surface area is too broad, but Anthropic's entire business is Claude

### Code Comment Template
This rationale must appear as a comment in relevant code locations:

```csharp
// Naming Convention: We use company names (OpenAI, Anthropic, Mistral) for consistency,
// except for Google where we use "Gemini" because "GoogleConfiguration" would be too
// ambiguous given Google's broad product portfolio. Gemini specifically refers to
// Google's AI model family, providing clear semantic meaning.
```

**Placement locations:**
1. `Nekote.Core.AI.Infrastructure.Gemini.GeminiConfiguration.cs` (at class level)
2. `Nekote.Core.AI.Infrastructure.DependencyInjection` folder README
3. Main architecture document (Section 3.1)

---

## 3. AI Interaction Tracking System (Redesigned)

### Problem with Original `IDiagnosticDataCollector`
The original design was too simplistic:
- Just a key-value bag
- No structured data
- No scope awareness (can't group by session/window)
- No queryability for building diagnostic UIs

### New Design: `IAiInteractionTracker`

#### 3.1. Core Interface

```csharp
namespace Nekote.Core.AI.Domain.Diagnostics;

/// <summary>
/// AI とのすべてのインタラクションを追跡し、構造化されたデータとして記録します。
/// </summary>
public interface IAiInteractionTracker
{
    /// <summary>
    /// 新しいインタラクションを開始します。
    /// </summary>
    /// <param name="scopeId">スコープ ID (GUI ウィンドウ、Blazor セッション、バッチジョブ ID など)。</param>
    /// <param name="provider">AI プロバイダー名 (例: "OpenAI", "Gemini")。</param>
    /// <param name="feature">機能名 (例: "Chat", "Embedding", "Translation")。</param>
    /// <returns>新しいインタラクションを表す <see cref="AiInteraction"/> オブジェクト。</returns>
    AiInteraction BeginInteraction(string scopeId, string provider, string feature);

    /// <summary>
    /// 特定のスコープに属するすべてのインタラクションを取得します。
    /// </summary>
    IReadOnlyList<AiInteraction> GetInteractionsByScope(string scopeId);

    /// <summary>
    /// 特定のプロバイダーのすべてのインタラクションを取得します。
    /// </summary>
    IReadOnlyList<AiInteraction> GetInteractionsByProvider(string provider);

    /// <summary>
    /// 時間範囲内のインタラクションを取得します。
    /// </summary>
    IReadOnlyList<AiInteraction> GetInteractionsByTimeRange(DateTimeOffset start, DateTimeOffset end);

    /// <summary>
    /// 失敗したインタラクションのみを取得します。
    /// </summary>
    IReadOnlyList<AiInteraction> GetFailedInteractions();

    /// <summary>
    /// すべてのインタラクションをクリアします (テスト用)。
    /// </summary>
    void Clear();
}
```

#### 3.2. Interaction Model

```csharp
namespace Nekote.Core.AI.Domain.Diagnostics;

/// <summary>
/// 単一の AI インタラクションを表します。
/// </summary>
public sealed class AiInteraction
{
    /// <summary>
    /// 一意のインタラクション ID を取得します。
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// スコープ ID (GUI ウィンドウ ID、セッション ID など) を取得します。
    /// </summary>
    public required string ScopeId { get; init; }

    /// <summary>
    /// AI プロバイダー名を取得します。
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// 機能名 (Chat, Embedding, Translation) を取得します。
    /// </summary>
    public required string Feature { get; init; }

    /// <summary>
    /// インタラクション開始時刻を取得します。
    /// </summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// インタラクション完了時刻を取得または設定します。
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// リクエストデータを取得または設定します。
    /// </summary>
    public AiRequestData? Request { get; set; }

    /// <summary>
    /// レスポンスデータを取得または設定します。
    /// </summary>
    public AiResponseData? Response { get; set; }

    /// <summary>
    /// エラー情報を取得または設定します。
    /// </summary>
    public AiErrorData? Error { get; set; }

    /// <summary>
    /// インタラクションが成功したかどうかを取得します。
    /// </summary>
    public bool IsSuccessful => Error == null && Response != null;

    /// <summary>
    /// 実行時間 (ミリ秒) を取得します。
    /// </summary>
    public long? DurationMs => CompletedAt.HasValue
        ? (long)(CompletedAt.Value - StartedAt).TotalMilliseconds
        : null;

    /// <summary>
    /// カスタムメタデータを追加します。
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = new();
}
```

#### 3.3. Request/Response/Error Data Models

```csharp
namespace Nekote.Core.AI.Domain.Diagnostics;

/// <summary>
/// AI リクエストの詳細情報を表します。
/// </summary>
public sealed class AiRequestData
{
    /// <summary>
    /// リクエスト送信先のエンドポイント URL を取得します。
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// HTTP メソッド (POST, GET など) を取得します。
    /// </summary>
    public required string HttpMethod { get; init; }

    /// <summary>
    /// リクエストボディの JSON 文字列を取得します (フォーマット済み)。
    /// </summary>
    public required string JsonBody { get; init; }

    /// <summary>
    /// リクエストヘッダー (機密情報を除く) を取得します。
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>
    /// リクエストサイズ (バイト) を取得します。
    /// </summary>
    public int SizeBytes { get; init; }
}

/// <summary>
/// AI レスポンスの詳細情報を表します。
/// </summary>
public sealed class AiResponseData
{
    /// <summary>
    /// HTTP ステータスコードを取得します。
    /// </summary>
    public required int StatusCode { get; init; }

    /// <summary>
    /// レスポンスボディの JSON 文字列を取得します (フォーマット済み)。
    /// </summary>
    public required string JsonBody { get; init; }

    /// <summary>
    /// レスポンスヘッダーを取得します。
    /// </summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>
    /// レスポンスサイズ (バイト) を取得します。
    /// </summary>
    public int SizeBytes { get; init; }

    /// <summary>
    /// 使用されたトークン数を取得します。
    /// </summary>
    public int? TokensUsed { get; init; }
}

/// <summary>
/// AI エラーの詳細情報を表します。
/// </summary>
public sealed class AiErrorData
{
    /// <summary>
    /// エラーの種類を取得します。
    /// </summary>
    public required string ErrorType { get; init; }

    /// <summary>
    /// エラーメッセージを取得します。
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// HTTP ステータスコード (該当する場合) を取得します。
    /// </summary>
    public int? StatusCode { get; init; }

    /// <summary>
    /// エラーレスポンスボディ (該当する場合) を取得します。
    /// </summary>
    public string? ResponseBody { get; init; }

    /// <summary>
    /// スタックトレースを取得します。
    /// </summary>
    public string? StackTrace { get; init; }
}
```

#### 3.4. Usage Example in Repository

```csharp
public async Task<ChatResponse> GetCompletionAsync(
    IReadOnlyList<ChatMessage> messages,
    ChatCompletionOptions? options = null,
    IAiInteractionTracker? tracker = null,
    CancellationToken cancellationToken = default)
{
    // Begin tracking
    var scopeId = Activity.Current?.Id ?? "unknown";
    var interaction = tracker?.BeginInteraction(scopeId, "OpenAI", "Chat");

    try
    {
        var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, _configuration.ChatModelName);
        var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);

        // Record request
        if (interaction != null)
        {
            interaction.Request = new AiRequestData
            {
                Endpoint = $"{_httpClient.BaseAddress}/v1/chat/completions",
                HttpMethod = "POST",
                JsonBody = json,
                SizeBytes = Encoding.UTF8.GetByteCount(json)
            };
        }

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken)
            .ConfigureAwait(false);

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Record response
        if (interaction != null)
        {
            interaction.Response = new AiResponseData
            {
                StatusCode = (int)response.StatusCode,
                JsonBody = responseJson,
                SizeBytes = Encoding.UTF8.GetByteCount(responseJson)
            };
            interaction.CompletedAt = DateTimeOffset.UtcNow;
        }

        response.EnsureSuccessStatusCode();

        var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(responseJson, JsonDefaults.Options)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAI response.");

        return OpenAiChatMapper.ToDomainModel(responseDto);
    }
    catch (Exception ex)
    {
        // Record error
        if (interaction != null)
        {
            interaction.Error = new AiErrorData
            {
                ErrorType = ex.GetType().Name,
                Message = ex.Message,
                StatusCode = (ex as HttpRequestException)?.StatusCode != null
                    ? (int)(ex as HttpRequestException)!.StatusCode!
                    : null,
                StackTrace = ex.StackTrace
            };
            interaction.CompletedAt = DateTimeOffset.UtcNow;
        }

        throw;
    }
}
```

#### 3.5. Implementation: Thread-Safe In-Memory Tracker

```csharp
namespace Nekote.Core.AI.Infrastructure.Diagnostics;

/// <summary>
/// スレッドセーフなインメモリ AI インタラクショントラッカー。
/// </summary>
internal sealed class InMemoryAiInteractionTracker : IAiInteractionTracker
{
    private readonly ConcurrentBag<AiInteraction> _interactions = new();
    private readonly ILogger<InMemoryAiInteractionTracker> _logger;

    public InMemoryAiInteractionTracker(ILogger<InMemoryAiInteractionTracker> logger)
    {
        _logger = logger;
    }

    public AiInteraction BeginInteraction(string scopeId, string provider, string feature)
    {
        var interaction = new AiInteraction
        {
            Id = Guid.NewGuid(),
            ScopeId = scopeId,
            Provider = provider,
            Feature = feature,
            StartedAt = DateTimeOffset.UtcNow
        };

        _interactions.Add(interaction);
        _logger.LogDebug("Started AI interaction {Id} for {Provider}/{Feature} in scope {ScopeId}",
            interaction.Id, provider, feature, scopeId);

        return interaction;
    }

    public IReadOnlyList<AiInteraction> GetInteractionsByScope(string scopeId)
        => _interactions.Where(i => i.ScopeId == scopeId)
            .OrderByDescending(i => i.StartedAt)
            .ToList();

    public IReadOnlyList<AiInteraction> GetInteractionsByProvider(string provider)
        => _interactions.Where(i => i.Provider == provider)
            .OrderByDescending(i => i.StartedAt)
            .ToList();

    public IReadOnlyList<AiInteraction> GetInteractionsByTimeRange(DateTimeOffset start, DateTimeOffset end)
        => _interactions.Where(i => i.StartedAt >= start && i.StartedAt <= end)
            .OrderByDescending(i => i.StartedAt)
            .ToList();

    public IReadOnlyList<AiInteraction> GetFailedInteractions()
        => _interactions.Where(i => !i.IsSuccessful)
            .OrderByDescending(i => i.StartedAt)
            .ToList();

    public void Clear()
    {
        _interactions.Clear();
        _logger.LogInformation("Cleared all AI interactions.");
    }
}
```

---

## 4. JSON Serialization Options (Static Instances)

### Problem
Creating `JsonSerializerOptions` on every serialization call is wasteful.

### Solution
Define static, reusable `JsonSerializerOptions` instances.

```csharp
namespace Nekote.Core.AI.Infrastructure;

/// <summary>
/// JSON シリアライゼーションのデフォルトオプションを提供します。
/// </summary>
internal static class JsonDefaults
{
    /// <summary>
    /// 標準オプション (フォーマットなし、デシリアライズ用)。
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// フォーマット付きオプション (インデント、診断データ記録用)。
    /// </summary>
    public static readonly JsonSerializerOptions FormattedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
}
```

**Usage:**
```csharp
// Serialize request (formatted for diagnostics)
var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);

// Deserialize response (no formatting needed)
var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(responseJson, JsonDefaults.Options);
```

---

## 5. Comment Style: No Numbered Comments

### Problem
Numbered comments like `// 1. Domain → DTO` are fragile and require renumbering when steps change.

### Solution
Use descriptive comments without numbers.

**Before:**
```csharp
// 1. Domain → DTO (using Mapper)
var requestDto = OpenAiChatMapper.ToRequestDto(...);

// 2. Serialize
var json = JsonSerializer.Serialize(requestDto);

// 3. HTTP call
var response = await _httpClient.PostAsync(...);
```

**After:**
```csharp
// Convert domain model to provider-specific DTO
var requestDto = OpenAiChatMapper.ToRequestDto(...);

// Serialize request for API call
var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);

// Send HTTP request to provider endpoint
var response = await _httpClient.PostAsync(...);
```

**Rationale:** Descriptive comments explain *why* or *what*, not just *order*.

---

## 6. DTO Nullability Strategy: Defensive Design

### Problem
DTOs represent external contracts. APIs can be broken, misconfigured, or return unexpected nulls.

### Solution: Make ALL DTO properties nullable, validate defensively in Mapper.

#### 6.1. DTO Design

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI Chat Completions API のレスポンス DTO。
/// すべてのプロパティは nullable として定義され、API の不整合に対応します。
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

#### 6.2. Defensive Validation in Mapper

```csharp
public static ChatResponse ToDomainModel(OpenAiChatResponseDto dto)
{
    // Defensive validation: API might return broken data
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

    // Map to domain model with required properties
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
```

**Rationale:**
- **DTOs:** Nullable everything (defensive)
- **Domain Models:** Required/nullable based on business logic
- **Mappers:** Explicit validation and error messages

---

## 7. Exception Strategy: Prefer Built-In, Custom When Needed

### Decision
**Use built-in .NET exceptions for most cases.** Define custom exceptions **only** when:
1. Built-in exceptions don't convey sufficient semantic meaning
2. You need to attach provider-specific metadata
3. Callers need to catch and handle specific AI-related failures

### Built-In Exceptions (Preferred)

| Scenario | Built-In Exception | Rationale |
|----------|-------------------|-----------|
| Mapper receives null DTO property | `InvalidOperationException` | Operation failed due to bad state |
| API key is missing from config | `InvalidOperationException` | Configuration is in invalid state |
| User passes invalid input | `ArgumentException` / `ArgumentNullException` | Bad argument |
| JSON deserialization fails | `JsonException` | Let `System.Text.Json` throw naturally |
| Network timeout | `HttpRequestException` / `TaskCanceledException` | Let `HttpClient` throw naturally |

### Custom Exceptions (Selective)

**Only define these 2 custom exceptions:**

```csharp
namespace Nekote.Core.AI.Domain.Exceptions;

/// <summary>
/// AI API 呼び出しが失敗した場合にスローされます。
/// HTTP ステータスコードやレスポンス本文などのメタデータを含みます。
/// </summary>
public sealed class AiApiCallException : InvalidOperationException
{
    public string Provider { get; }
    public string Feature { get; }
    public int? HttpStatusCode { get; }
    public string? ResponseBody { get; }

    public AiApiCallException(
        string provider,
        string feature,
        string message,
        int? httpStatusCode = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Provider = provider;
        Feature = feature;
        HttpStatusCode = httpStatusCode;
        ResponseBody = responseBody;
    }
}

/// <summary>
/// API レート制限に達した場合にスローされます。
/// </summary>
public sealed class AiRateLimitException : AiApiCallException
{
    public TimeSpan? RetryAfter { get; }

    public AiRateLimitException(
        string provider,
        string feature,
        TimeSpan? retryAfter,
        string message,
        int? httpStatusCode = null,
        string? responseBody = null)
        : base(provider, feature, message, httpStatusCode, responseBody)
    {
        RetryAfter = retryAfter;
    }
}
```

**Why these two?**
1. **`AiApiCallException`:** Callers need to know which provider/feature failed and may need the response body for logging
2. **`AiRateLimitException`:** Callers may want to implement retry logic based on `RetryAfter`

**Everything else uses built-in exceptions.**

---

## 8. Configuration: No Constants for Section Names

### Problem
`public const string SectionName = "AI:OpenAI";` prevents integration into existing apps with different config structures.

### Solution
**Remove all `SectionName` constants.** Pass section path as parameter to DI extension methods.

#### 8.1. Updated Configuration Class

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI;

/// <summary>
/// OpenAI サービスの設定を定義します。
/// </summary>
public sealed class OpenAiConfiguration
{
    // NO CONSTANTS - section path is caller's responsibility

    /// <summary>
    /// デフォルトの API キーを取得または設定します。
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// チャット専用の API キー (オプション、未設定なら <see cref="ApiKey"/> を使用)。
    /// </summary>
    public string? ChatApiKey { get; init; }

    /// <summary>
    /// エンベディング専用の API キー (オプション、未設定なら <see cref="ApiKey"/> を使用)。
    /// </summary>
    public string? EmbeddingApiKey { get; init; }

    /// <summary>
    /// デフォルトのベース URL を取得または設定します。
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// チャット専用のエンドポイント (オプション、未設定なら <see cref="BaseUrl"/> + "/v1/chat/completions")。
    /// </summary>
    public string? ChatEndpoint { get; init; }

    /// <summary>
    /// エンベディング専用のエンドポイント (オプション、未設定なら <see cref="BaseUrl"/> + "/v1/embeddings")。
    /// </summary>
    public string? EmbeddingEndpoint { get; init; }

    /// <summary>
    /// デフォルトのモデル名を取得または設定します。
    /// </summary>
    public string? DefaultModelName { get; init; }

    /// <summary>
    /// チャット専用のモデル名 (オプション、未設定なら <see cref="DefaultModelName"/> を使用)。
    /// </summary>
    public string? ChatModelName { get; init; }

    /// <summary>
    /// エンベディング専用のモデル名 (オプション、未設定なら <see cref="DefaultModelName"/> を使用)。
    /// </summary>
    public string? EmbeddingModelName { get; init; }
}
```

#### 8.2. Updated DI Extension Method

```csharp
namespace Nekote.Core.AI.Infrastructure.DependencyInjection;

public static class OpenAiServiceCollectionExtensions
{
    /// <summary>
    /// OpenAI のチャットサービスを登録します。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configuration">アプリケーション設定。</param>
    /// <param name="configurationSectionPath">
    /// OpenAI 設定が格納されているセクションパス (例: "AI:OpenAI", "MyApp:AiProviders:OpenAI")。
    /// </param>
    public static IServiceCollection AddOpenAiChat(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "AI:OpenAI") // Default, but overridable
    {
        // Bind configuration from flexible path
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(configurationSectionPath));

        services.AddHttpClient("OpenAI-Chat", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<OpenAiConfiguration>>().Value;

            // Use feature-specific settings with fallback to defaults
            var apiKey = config.ChatApiKey ?? config.ApiKey
                ?? throw new InvalidOperationException(
                    $"OpenAI API key not found in configuration section '{configurationSectionPath}'. " +
                    "Provide either 'ApiKey' or 'ChatApiKey'.");

            var baseUrl = config.BaseUrl ?? "https://api.openai.com";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        });

        services.AddScoped<IChatCompletionService, OpenAiChatRepository>();
        return services;
    }
}
```

#### 8.3. Usage Examples

**Standard app:**
```csharp
services.AddOpenAiChat(configuration); // Uses default "AI:OpenAI"
```

**Integration with existing app:**
```csharp
services.AddOpenAiChat(configuration, "MyCompany:Integrations:OpenAI");
```

**appsettings.json (feature-specific keys):**
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
    }
  }
}
```

---

## 9. Granular Configuration (Per-Feature Settings)

### Design Rationale
Real-world scenarios require flexibility:
- **Cost management:** Use cheap model for simple tasks, expensive for complex
- **Self-hosting:** Different endpoints for different features
- **Security:** Separate API keys for different capabilities

### Configuration Resolution Order (Priority)
1. **Feature-specific** (e.g., `ChatApiKey`, `ChatEndpoint`, `ChatModelName`)
2. **Default** (e.g., `ApiKey`, `BaseUrl`, `DefaultModelName`)
3. **Hardcoded fallback** (e.g., `"https://api.openai.com"`)

### Implementation in Repository

```csharp
internal sealed class OpenAiChatRepository : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiConfiguration _config;

    public OpenAiChatRepository(IHttpClientFactory factory, IOptions<OpenAiConfiguration> config)
    {
        _config = config.Value;

        // Resolve endpoint with fallback chain
        var endpoint = _config.ChatEndpoint
            ?? (_config.BaseUrl != null ? $"{_config.BaseUrl}/v1/chat/completions" : null)
            ?? "https://api.openai.com/v1/chat/completions";

        _httpClient = factory.CreateClient("OpenAI-Chat");
        _httpClient.BaseAddress = new Uri(endpoint);

        // Resolve API key with fallback
        var apiKey = _config.ChatApiKey ?? _config.ApiKey
            ?? throw new InvalidOperationException("OpenAI API key is not configured.");

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<ChatResponse> GetCompletionAsync(...)
    {
        // Resolve model name with fallback
        var modelName = _config.ChatModelName ?? _config.DefaultModelName
            ?? throw new InvalidOperationException("OpenAI model name is not configured.");

        var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);
        // ... rest of implementation
    }
}
```

---

## 10. Provider Naming: Company Names (Except Gemini)

### Updated Provider List

| Directory | Classes | Rationale |
|-----------|---------|-----------|
| `/OpenAI` | `OpenAi*` | Company name (products: ChatGPT, DALL-E, Sora) |
| `/Gemini` | `Gemini*` | Product name (Google too broad) |
| `/Anthropic` | `Anthropic*` | Company name (product: Claude) |
| `/xAI` | `XAi*` | Company name (product: Grok) |
| `/Mistral` | `Mistral*` | Company name |
| `/DeepSeek` | `DeepSeek*` | Company name |
| `/DeepL` | `DeepL*` | Company name |

### All References to Update
- Change "Claude" → "Anthropic" in:
  - Directory names
  - Class names (`ClaudeChatRepository` → `AnthropicChatRepository`)
  - Configuration classes
  - DI extension methods
  - Documentation
  - Code comments

---

## 11. Centralized Prompt System

### Problem
Hard-coded prompts like:
```csharp
var prompt = $"以下のテキストを、それぞれ約 {targetChunkSize} 単語の意味的なまとまりに分割してください。";
```
are fragile and untestable.

### Solution: Prompt Provider System

#### 11.1. Interface

```csharp
namespace Nekote.Core.AI.Domain.Prompts;

/// <summary>
/// AI プロンプトを提供するインターフェース。
/// </summary>
public interface IPromptProvider
{
    /// <summary>
    /// プロンプトキーに対応するプロンプトテキストを取得します。
    /// </summary>
    /// <param name="key">プロンプトキー (例: "TextChunking.Default")。</param>
    /// <param name="parameters">プロンプトにバインドするパラメータ (オプション)。</param>
    /// <returns>生成されたプロンプト文字列。</returns>
    string GetPrompt(string key, IDictionary<string, object>? parameters = null);
}
```

#### 11.2. Implementation (File-Based)

```csharp
namespace Nekote.Core.AI.Infrastructure.Prompts;

/// <summary>
/// ファイルベースのプロンプトプロバイダー。
/// </summary>
internal sealed class FileBasedPromptProvider : IPromptProvider
{
    private readonly ILogger<FileBasedPromptProvider> _logger;
    private readonly Dictionary<string, string> _prompts = new();

    public FileBasedPromptProvider(
        ILogger<FileBasedPromptProvider> logger,
        string promptsDirectoryPath)
    {
        _logger = logger;
        LoadPrompts(promptsDirectoryPath);
    }

    public string GetPrompt(string key, IDictionary<string, object>? parameters = null)
    {
        if (!_prompts.TryGetValue(key, out var template))
        {
            throw new InvalidOperationException($"Prompt not found: {key}");
        }

        // Simple template substitution (can be replaced with Liquid, Scriban, etc.)
        var prompt = template;
        if (parameters != null)
        {
            foreach (var (paramKey, paramValue) in parameters)
            {
                prompt = prompt.Replace($"{{{paramKey}}}", paramValue?.ToString() ?? string.Empty);
            }
        }

        return prompt;
    }

    private void LoadPrompts(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            _logger.LogWarning("Prompts directory not found: {Path}", directoryPath);
            return;
        }

        foreach (var filePath in Directory.GetFiles(directoryPath, "*.txt", SearchOption.AllDirectories))
        {
            var key = Path.GetFileNameWithoutExtension(filePath);
            var content = File.ReadAllText(filePath);
            _prompts[key] = content;
            _logger.LogDebug("Loaded prompt: {Key} from {Path}", key, filePath);
        }
    }
}
```

#### 11.3. Prompt File Structure

```
/Nekote.Core
    └─ /AI
        └─ /Prompts
            ├─ TextChunking.Default.txt
            ├─ TextChunking.Technical.txt
            ├─ Summarization.Short.txt
            └─ Summarization.Detailed.txt
```

**Example: `TextChunking.Default.txt`**
```
以下のテキストを、それぞれ約 {targetChunkSize} 単語の意味的なまとまりに分割してください。
各チャンクは独立して理解可能であるべきです。

要件:
- チャンクの境界は文の終わりに合わせてください
- 各チャンクは完結した意味を持つようにしてください
- チャンク番号を付けて出力してください

テキスト:
{text}
```

#### 11.4. Usage in Service

```csharp
public sealed class SmartChunkingService
{
    private readonly IChatCompletionService _chatService;
    private readonly IPromptProvider _promptProvider;

    public SmartChunkingService(
        IChatCompletionService chatService,
        IPromptProvider promptProvider)
    {
        _chatService = chatService;
        _promptProvider = promptProvider;
    }

    public async Task<IReadOnlyList<string>> ChunkTextAsync(
        string longText,
        int targetChunkSize = 500,
        CancellationToken cancellationToken = default)
    {
        // Get prompt from centralized provider
        var prompt = _promptProvider.GetPrompt("TextChunking.Default", new Dictionary<string, object>
        {
            ["targetChunkSize"] = targetChunkSize,
            ["text"] = longText
        });

        var messages = new List<ChatMessage>
        {
            new() { Role = ChatRole.System, Content = "You are a text chunking assistant." },
            new() { Role = ChatRole.User, Content = prompt }
        };

        var response = await _chatService.GetCompletionAsync(messages, null, null, cancellationToken)
            .ConfigureAwait(false);

        return ParseChunks(response.Content);
    }
}
```

#### 11.5. DI Registration

```csharp
services.AddSingleton<IPromptProvider>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<FileBasedPromptProvider>>();
    var promptsPath = Path.Combine(AppContext.BaseDirectory, "Prompts");
    return new FileBasedPromptProvider(logger, promptsPath);
});
```

---

## Summary of Key Changes

| # | Issue | Solution |
|---|-------|----------|
| 1 | xAI directory naming | `/xAI` (directory) + `XAi*` (classes) |
| 2 | Naming rationale | Document "Gemini exception" in code comments |
| 3 | Diagnostic system | Replace with `IAiInteractionTracker` (scope-aware, structured) |
| 4 | JSON options | Static `JsonDefaults.Options` and `JsonDefaults.FormattedOptions` |
| 5 | Numbered comments | Use descriptive comments without numbers |
| 6 | DTO nullability | All properties nullable, validate in Mapper |
| 7 | Exception strategy | Prefer built-in; 2 custom exceptions only |
| 8 | Config constants | Remove constants; pass section path to DI methods |
| 9 | Granular config | Feature-specific keys with fallback chain |
| 10 | Provider naming | Company names except "Gemini" (Claude → Anthropic) |
| 11 | Hard-coded prompts | Centralized `IPromptProvider` with file-based storage |

---

**Next Steps:**
1. Update main architecture document with these revisions
2. Begin implementation of Phase 1 (Domain layer)
3. Build `IAiInteractionTracker` infrastructure
4. Create prompt file structure and provider

---

*This is a design revision document. Changes must be reflected in the main architecture document before implementation begins.*
