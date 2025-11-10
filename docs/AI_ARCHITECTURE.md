# Nekote.Core AI Architecture Design

**Version:** 1.0
**Date:** 2025-11-10
**Author:** nao7sep
**Status:** Design Specification

---

## 1. Executive Summary

This document defines the complete architecture for AI integration into `Nekote.Core`, following the principles established in:
- **The AI Coding Playbook** (fundamental C# and architectural rules)
- **The Play Harder Playbook** (application-level patterns)
- **Nekote AI Integration Strategy** (AI-specific philosophy and scope)

**Core Philosophy:** AI is not a feature added *on top* of Nekote—it is built *into the foundation* as a first-class infrastructure component.

---

## 2. Architectural Principles

### 2.1. Strict Layering

```
┌─────────────────────────────────────────┐
│         Application Layer               │  (Consumer code - not in Nekote.Core)
│    (Web API, Console, Avalonia UI)      │
└─────────────────────────────────────────┘
                    ↓ depends on
┌─────────────────────────────────────────┐
│         Domain Layer                    │  (Pure interfaces, POCOs)
│  - IChatCompletionService               │  (Contracts only - no implementation)
│  - ITextEmbeddingService                │
│  - Domain Models (ChatMessage, etc.)    │
└─────────────────────────────────────────┘
                    ↑ implements
┌─────────────────────────────────────────┐
│      Infrastructure Layer               │  (Concrete implementations)
│  - OpenAiChatRepository                 │
│  - GeminiEmbeddingRepository            │
│  - Internal DTOs + Mappers              │
│  - HttpClient-based API calls           │
└─────────────────────────────────────────┘
```

**Key Rules:**
- Domain layer has **zero dependencies** (pure contracts)
- Infrastructure layer depends **only** on Domain
- Application layer depends **only** on Domain (via DI)
- Infrastructure is **never** exposed to Application

### 2.2. Anti-Corruption Layer (ACL)

Each AI provider is treated as an external system. The ACL protects Nekote's domain from provider-specific contracts.

```
External API (OpenAI JSON)
        ↓
Internal DTO (OpenAiChatRequestDto) [JsonPropertyName attributes here]
        ↓
Mapper (OpenAiChatMapper) [Translation logic here]
        ↓
Domain Model (ChatMessage) [Pure POCO]
```

**Critical Rule:** Domain models must **never** contain serialization attributes.

---

## 3. Namespace Organization

### 3.1. Directory Structure (Vertical Slices)

```
/Nekote.Core
│
├─ /AI                                    // Root namespace: Nekote.Core.AI
│   │
│   ├─ /Domain                            // Domain Layer (Contracts)
│   │   │
│   │   ├─ /Chat                          // Namespace: Nekote.Core.AI.Domain.Chat
│   │   │   ├─ IChatCompletionService.cs
│   │   │   ├─ ChatMessage.cs            // Domain model (POCO)
│   │   │   ├─ ChatRole.cs               // Enum
│   │   │   └─ ChatResponse.cs           // Domain model
│   │   │
│   │   ├─ /Embedding                     // Namespace: Nekote.Core.AI.Domain.Embedding
│   │   │   ├─ ITextEmbeddingService.cs
│   │   │   ├─ EmbeddingRequest.cs       // Domain model
│   │   │   └─ EmbeddingResult.cs        // Domain model
│   │   │
│   │   ├─ /Translation                   // Namespace: Nekote.Core.AI.Domain.Translation
│   │   │   ├─ ITranslationService.cs
│   │   │   ├─ TranslationRequest.cs
│   │   │   └─ TranslationResult.cs
│   │   │
│   │   └─ /Diagnostics                   // Namespace: Nekote.Core.AI.Domain.Diagnostics
│   │       ├─ IDiagnosticDataCollector.cs
│   │       └─ DiagnosticEntry.cs
│   │
│   └─ /Infrastructure                    // Infrastructure Layer (Implementations)
│       │
│       ├─ /OpenAI                        // Vertical Slice: OpenAI
│       │   │
│       │   ├─ /Chat
│       │   │   ├─ OpenAiChatRepository.cs       // implements IChatCompletionService
│       │   │   ├─ /Dtos
│       │   │   │   ├─ OpenAiChatRequestDto.cs   // [JsonPropertyName] here
│       │   │   │   └─ OpenAiChatResponseDto.cs
│       │   │   └─ OpenAiChatMapper.cs           // DTO ↔ Domain translation
│       │   │
│       │   ├─ /Embedding
│       │   │   ├─ OpenAiEmbeddingRepository.cs
│       │   │   ├─ /Dtos
│       │   │   │   ├─ OpenAiEmbeddingRequestDto.cs
│       │   │   │   └─ OpenAiEmbeddingResponseDto.cs
│       │   │   └─ OpenAiEmbeddingMapper.cs
│       │   │
│       │   └─ OpenAiConfiguration.cs     // Configuration model
│       │
│       ├─ /Gemini                        // Vertical Slice: Gemini
│       │   ├─ /Chat
│       │   │   ├─ GeminiChatRepository.cs
│       │   │   ├─ /Dtos
│       │   │   └─ GeminiChatMapper.cs
│       │   ├─ /Embedding
│       │   └─ GeminiConfiguration.cs
│       │
│       ├─ /Anthropic                     // Vertical Slice: Anthropic (Claude)
│       │   ├─ /Chat
│       │   └─ AnthropicConfiguration.cs
│       │
│       ├─ /XAI                           // Vertical Slice: xAI (Grok)
│       │   ├─ /Chat
│       │   └─ XAiConfiguration.cs
│       │
│       ├─ /Mistral                       // Vertical Slice: Mistral
│       │   ├─ /Chat
│       │   └─ MistralConfiguration.cs
│       │
│       ├─ /DeepSeek                      // Vertical Slice: DeepSeek
│       │   ├─ /Chat
│       │   └─ DeepSeekConfiguration.cs
│       │
│       ├─ /DeepL                         // Vertical Slice: DeepL (Translation)
│       │   ├─ DeepLTranslationRepository.cs
│       │   ├─ /Dtos
│       │   └─ DeepLConfiguration.cs
│       │
│       ├─ /Caching                       // Cross-cutting: Caching decorators
│       │   ├─ CachedEmbeddingRepository.cs      // Decorator pattern
│       │   └─ CacheConfiguration.cs
│       │
│       ├─ /Diagnostics                   // Cross-cutting: Diagnostics
│       │   └─ DiagnosticDataCollector.cs        // implements IDiagnosticDataCollector
│       │
│       └─ /DependencyInjection           // DI registration extensions
│           ├─ AiServiceCollectionExtensions.cs
│           ├─ OpenAiServiceCollectionExtensions.cs
│           ├─ GeminiServiceCollectionExtensions.cs
│           └─ [... one per provider ...]
```

**Design Rationale:**
1. **Domain/Infrastructure split** enforces strict separation of concerns
2. **Vertical slices** (OpenAI, Gemini, etc.) maximize cohesion
3. **One DTO per external contract** ensures fidelity to each API spec
4. **One Mapper per provider** encapsulates translation logic
5. **Separate /DependencyInjection folder** keeps registration logic organized

---

## 4. Domain Layer Design

### 4.1. Core Service Interfaces

#### IChatCompletionService

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャット補完サービスのインターフェースを定義します。
/// </summary>
public interface IChatCompletionService
{
    /// <summary>
    /// チャットメッセージのリストを受け取り、AI からの応答を非同期で取得します。
    /// </summary>
    /// <param name="messages">チャットメッセージのリスト。</param>
    /// <param name="options">オプション設定 (temperature, max_tokens など)。</param>
    /// <param name="diagnosticCollector">診断データを収集するためのオプションのコレクター。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>AI からの応答を含む <see cref="ChatResponse"/>。</returns>
    Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ストリーミングモードでチャット補完を実行し、トークンを逐次的に取得します。
    /// </summary>
    /// <param name="messages">チャットメッセージのリスト。</param>
    /// <param name="options">オプション設定。</param>
    /// <param name="diagnosticCollector">診断データコレクター。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>トークンのストリーム。</returns>
    IAsyncEnumerable<string> GetCompletionStreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default);
}
```

#### ITextEmbeddingService

```csharp
namespace Nekote.Core.AI.Domain.Embedding;

/// <summary>
/// テキストエンベディングサービスのインターフェースを定義します。
/// </summary>
public interface ITextEmbeddingService
{
    /// <summary>
    /// 単一のテキストをベクトル表現に変換します。
    /// </summary>
    /// <param name="text">エンベディング化するテキスト。</param>
    /// <param name="diagnosticCollector">診断データコレクター。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>エンベディングベクトルを含む <see cref="EmbeddingResult"/>。</returns>
    Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のテキストを一括でベクトル表現に変換します。
    /// </summary>
    /// <param name="texts">エンベディング化するテキストのリスト。</param>
    /// <param name="diagnosticCollector">診断データコレクター。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>各テキストのエンベディングベクトルを含むリスト。</returns>
    Task<IReadOnlyList<EmbeddingResult>> GetEmbeddingsAsync(
        IReadOnlyList<string> texts,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default);
}
```

#### ITranslationService

```csharp
namespace Nekote.Core.AI.Domain.Translation;

/// <summary>
/// テキスト翻訳サービスのインターフェースを定義します。
/// </summary>
public interface ITranslationService
{
    /// <summary>
    /// テキストを指定された言語に翻訳します。
    /// </summary>
    /// <param name="request">翻訳リクエスト。</param>
    /// <param name="diagnosticCollector">診断データコレクター。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>翻訳結果を含む <see cref="TranslationResult"/>。</returns>
    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default);
}
```

### 4.2. Core Domain Models (Pure POCOs)

#### ChatMessage

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャットメッセージを表します。
/// </summary>
public sealed class ChatMessage
{
    /// <summary>
    /// メッセージの役割 (user, assistant, system など) を取得または設定します。
    /// </summary>
    public required ChatRole Role { get; init; }

    /// <summary>
    /// メッセージの内容を取得または設定します。
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// メッセージの名前 (オプション) を取得または設定します。
    /// </summary>
    public string? Name { get; init; }
}
```

#### ChatResponse

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// AI からのチャット応答を表します。
/// </summary>
public sealed class ChatResponse
{
    /// <summary>
    /// 生成されたメッセージを取得または設定します。
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 応答の完了理由 (stop, length, content_filter など) を取得または設定します。
    /// </summary>
    public required string FinishReason { get; init; }

    /// <summary>
    /// 使用されたトークン数の情報を取得または設定します。
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// プロバイダー固有の応答 ID を取得または設定します。
    /// </summary>
    public string? ResponseId { get; init; }
}
```

#### EmbeddingResult

```csharp
namespace Nekote.Core.AI.Domain.Embedding;

/// <summary>
/// テキストエンベディングの結果を表します。
/// </summary>
public sealed class EmbeddingResult
{
    /// <summary>
    /// エンベディングベクトルを取得または設定します。
    /// </summary>
    public required IReadOnlyList<float> Vector { get; init; }

    /// <summary>
    /// 元のテキストを取得または設定します。
    /// </summary>
    public required string OriginalText { get; init; }

    /// <summary>
    /// 使用されたトークン数を取得または設定します。
    /// </summary>
    public int? TokenCount { get; init; }
}
```

**Critical Design Notes:**
1. **No attributes:** These are pure POCOs. No `[JsonPropertyName]`, no `[Required]`.
2. **Immutable:** Use `init` accessors, not `set`.
3. **Required properties:** Use C# 11's `required` keyword for mandatory properties.
4. **Provider-agnostic:** These models represent the **business concept**, not any specific API's JSON format.

### 4.3. Diagnostics Interface

```csharp
namespace Nekote.Core.AI.Domain.Diagnostics;

/// <summary>
/// 診断データを収集するためのインターフェースを定義します。
/// </summary>
public interface IDiagnosticDataCollector
{
    /// <summary>
    /// キーと値のペアを診断データとして記録します。
    /// </summary>
    /// <param name="key">診断データのキー。</param>
    /// <param name="value">診断データの値。</param>
    void Collect(string key, object? value);

    /// <summary>
    /// 特定のキーに関連付けられたすべての値を取得します。
    /// </summary>
    /// <param name="key">検索するキー。</param>
    /// <returns>指定されたキーに関連付けられた値のコレクション。</returns>
    IReadOnlyList<object?> GetValues(string key);

    /// <summary>
    /// すべての診断エントリを時系列順で取得します。
    /// </summary>
    /// <returns>すべての診断エントリ。</returns>
    IReadOnlyList<DiagnosticEntry> GetAllEntries();
}
```

---

## 5. Infrastructure Layer Design

### 5.1. Repository Implementation Pattern

Each provider implements the domain interface using the **Repository Pattern**.

**Example: OpenAI Chat Repository**

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

/// <summary>
/// OpenAI の Chat Completions API を使用してチャット補完を実行します。
/// </summary>
internal sealed class OpenAiChatRepository : IChatCompletionService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiChatRepository> _logger;
    private readonly OpenAiConfiguration _configuration;

    public OpenAiChatRepository(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiChatRepository> logger,
        IOptions<OpenAiConfiguration> configuration)
    {
        _httpClient = httpClientFactory.CreateClient("OpenAI");
        _logger = logger;
        _configuration = configuration.Value;
    }

    public async Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Domain → DTO (using Mapper)
        var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, _configuration.ModelName);

        // 2. Serialize
        var json = JsonSerializer.Serialize(requestDto);
        diagnosticCollector?.Collect("OpenAI.Request.JSON", json);

        // 3. HTTP call
        _logger.LogDebug("Sending chat completion request to OpenAI API.");
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/v1/chat/completions", content, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        // 4. Deserialize
        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        diagnosticCollector?.Collect("OpenAI.Response.JSON", responseJson);

        var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(responseJson)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAI response.");

        // 5. DTO → Domain (using Mapper)
        return OpenAiChatMapper.ToDomainModel(responseDto);
    }

    public async IAsyncEnumerable<string> GetCompletionStreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        IDiagnosticDataCollector? diagnosticCollector = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Streaming implementation (SSE parsing)
        // ... (similar pattern)
        yield break;
    }
}
```

**Key Responsibilities:**
1. **Inject dependencies:** `IHttpClientFactory`, `ILogger<T>`, `IOptions<TConfig>`
2. **Map Domain → DTO** before API call
3. **Make HTTP call** using `HttpClient` (no SDK dependency)
4. **Collect diagnostics** (raw JSON) if collector provided
5. **Map DTO → Domain** after API call

### 5.2. DTO Design (Anti-Corruption Layer)

DTOs must **perfectly match** the external API's JSON contract.

**Example: OpenAI Chat Request DTO**

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI Chat Completions API のリクエスト DTO。
/// </summary>
internal sealed class OpenAiChatRequestDto
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required List<OpenAiMessageDto> Messages { get; init; }

    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }
}

internal sealed class OpenAiMessageDto
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }
}
```

**Critical Rules:**
1. **`internal sealed`:** DTOs are implementation details, never exposed publicly
2. **`[JsonPropertyName]`:** This is the **only** place serialization attributes exist
3. **Exact match:** Property names must match the API spec (e.g., `max_tokens`, not `MaxTokens`)

### 5.3. Mapper Pattern

The Mapper is the **Anti-Corruption Layer**.

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

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
            Stream = false
        };
    }

    /// <summary>
    /// OpenAI API のレスポンス DTO をドメインモデルに変換します。
    /// </summary>
    public static ChatResponse ToDomainModel(OpenAiChatResponseDto dto)
    {
        var firstChoice = dto.Choices.FirstOrDefault()
            ?? throw new InvalidOperationException("OpenAI response contains no choices.");

        return new ChatResponse
        {
            Content = firstChoice.Message.Content,
            FinishReason = firstChoice.FinishReason,
            Usage = dto.Usage != null ? new TokenUsage
            {
                PromptTokens = dto.Usage.PromptTokens,
                CompletionTokens = dto.Usage.CompletionTokens,
                TotalTokens = dto.Usage.TotalTokens
            } : null,
            ResponseId = dto.Id
        };
    }

    private static string MapRole(ChatRole role)
    {
        return role switch
        {
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            ChatRole.System => "system",
            _ => throw new ArgumentException($"Unsupported chat role: {role}", nameof(role))
        };
    }
}
```

**Design Rationale:**
- **Static class:** Mappers are stateless utilities
- **Clear responsibility:** Only converts between DTO and Domain
- **Validation:** Throws exceptions for invalid data
- **Provider-specific logic:** Handles provider quirks (e.g., role name mapping)

---

## 6. Caching Strategy (Decorator Pattern)

For deterministic operations (e.g., text embedding), implement caching as a **decorator**.

```csharp
namespace Nekote.Core.AI.Infrastructure.Caching;

/// <summary>
/// <see cref="ITextEmbeddingService"/> の実装をラップし、結果をキャッシュします。
/// </summary>
internal sealed class CachedEmbeddingRepository : ITextEmbeddingService
{
    private readonly ITextEmbeddingService _innerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedEmbeddingRepository> _logger;

    public CachedEmbeddingRepository(
        ITextEmbeddingService innerService,
        IMemoryCache cache,
        ILogger<CachedEmbeddingRepository> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"embedding:{text.GetHashCode()}";

        if (_cache.TryGetValue(cacheKey, out EmbeddingResult? cached))
        {
            _logger.LogDebug("Cache hit for embedding: {CacheKey}", cacheKey);
            diagnosticCollector?.Collect("Cache.Hit", cacheKey);
            return cached!;
        }

        _logger.LogDebug("Cache miss for embedding: {CacheKey}", cacheKey);
        diagnosticCollector?.Collect("Cache.Miss", cacheKey);

        var result = await _innerService.GetEmbeddingAsync(text, diagnosticCollector, cancellationToken)
            .ConfigureAwait(false);

        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));
        return result;
    }

    public Task<IReadOnlyList<EmbeddingResult>> GetEmbeddingsAsync(
        IReadOnlyList<string> texts,
        IDiagnosticDataCollector? diagnosticCollector = null,
        CancellationToken cancellationToken = default)
    {
        // Similar caching logic for batch operations
        // ...
    }
}
```

**DI Registration (with decorator):**

```csharp
services.AddScoped<ITextEmbeddingService, OpenAiEmbeddingRepository>(); // Real implementation
services.Decorate<ITextEmbeddingService, CachedEmbeddingRepository>(); // Wrap with caching
```

---

## 7. Configuration and DI Registration

### 7.1. Configuration Model

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI;

/// <summary>
/// OpenAI サービスの設定を定義します。
/// </summary>
public sealed class OpenAiConfiguration
{
    /// <summary>
    /// 設定セクション名。
    /// </summary>
    public const string SectionName = "AI:OpenAI";

    /// <summary>
    /// API キーを取得または設定します。
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// API エンドポイント URL を取得または設定します。
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.openai.com";

    /// <summary>
    /// 使用するモデル名を取得または設定します。
    /// </summary>
    public required string ModelName { get; init; }
}
```

**appsettings.json example:**

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-...",
      "BaseUrl": "https://api.openai.com",
      "ModelName": "gpt-4"
    },
    "Gemini": {
      "ApiKey": "...",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "ModelName": "gemini-2.5-pro"
    }
  }
}
```

### 7.2. DI Extension Methods

```csharp
namespace Nekote.Core.AI.Infrastructure.DependencyInjection;

/// <summary>
/// OpenAI サービスの DI 登録を提供します。
/// </summary>
public static class OpenAiServiceCollectionExtensions
{
    /// <summary>
    /// OpenAI のチャットサービスを登録します。
    /// </summary>
    public static IServiceCollection AddOpenAiChat(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register configuration
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(OpenAiConfiguration.SectionName));

        // 2. Register named HttpClient
        services.AddHttpClient("OpenAI", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<OpenAiConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        });

        // 3. Register service
        services.AddScoped<IChatCompletionService, OpenAiChatRepository>();

        return services;
    }

    /// <summary>
    /// OpenAI のエンベディングサービスを登録します (キャッシュ付き)。
    /// </summary>
    public static IServiceCollection AddOpenAiEmbedding(
        this IServiceCollection services,
        IConfiguration configuration,
        bool enableCaching = true)
    {
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(OpenAiConfiguration.SectionName));

        services.AddHttpClient("OpenAI", (sp, client) =>
        {
            var config = sp.GetRequiredService<IOptions<OpenAiConfiguration>>().Value;
            client.BaseAddress = new Uri(config.BaseUrl);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {config.ApiKey}");
        });

        services.AddScoped<ITextEmbeddingService, OpenAiEmbeddingRepository>();

        if (enableCaching)
        {
            services.Decorate<ITextEmbeddingService, CachedEmbeddingRepository>();
        }

        return services;
    }
}
```

**Usage in application:**

```csharp
var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // Register AI services
    services.AddOpenAiChat(context.Configuration);
    services.AddOpenAiEmbedding(context.Configuration, enableCaching: true);
    services.AddGeminiChat(context.Configuration);

    // Register diagnostics
    services.AddSingleton<IDiagnosticDataCollector, DiagnosticDataCollector>();
});
```

---

## 8. Multi-Provider Support Strategy

### 8.1. Provider Selection via Named Services

When multiple providers implement the same interface, use **named services**.

```csharp
public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register all providers
        services.AddKeyedScoped<IChatCompletionService, OpenAiChatRepository>("OpenAI");
        services.AddKeyedScoped<IChatCompletionService, GeminiChatRepository>("Gemini");
        services.AddKeyedScoped<IChatCompletionService, ClaudeChatRepository>("Claude");

        // Register default (from configuration)
        var defaultProvider = configuration["AI:DefaultProvider"] ?? "OpenAI";
        services.AddScoped<IChatCompletionService>(sp =>
            sp.GetRequiredKeyedService<IChatCompletionService>(defaultProvider));

        return services;
    }
}
```

**Consumer code:**

```csharp
// Use default provider
public class MyService
{
    private readonly IChatCompletionService _chatService;

    public MyService(IChatCompletionService chatService)
    {
        _chatService = chatService; // Gets default provider
    }
}

// Use specific provider
public class AdvancedService
{
    private readonly IChatCompletionService _gemini;
    private readonly IChatCompletionService _claude;

    public AdvancedService(
        [FromKeyedServices("Gemini")] IChatCompletionService gemini,
        [FromKeyedServices("Claude")] IChatCompletionService claude)
    {
        _gemini = gemini;
        _claude = claude;
    }
}
```

### 8.2. Provider Capabilities Matrix

| Provider  | Chat | Embedding | Translation | Max Context | Specialty |
|-----------|------|-----------|-------------|-------------|-----------|
| OpenAI    | ✓    | ✓         | ✗           | ~128K       | General-purpose baseline |
| Gemini    | ✓    | ✓         | ✗           | 2M          | Large document analysis |
| Claude    | ✓    | ✗         | ✗           | 200K        | Coding, agent tasks |
| xAI       | ✓    | ✗         | ✗           | ~128K       | Real-time trends (X access) |
| Mistral   | ✓    | ✓         | ✗           | ~128K       | Privacy-first (EU) |
| DeepSeek  | ✓    | ✗         | ✗           | ~128K       | Math, coding (cost-efficient) |
| DeepL     | ✗    | ✗         | ✓           | N/A         | Translation specialist |

---

## 9. RAG (Retrieval-Augmented Generation) Implementation

### 9.1. Smart Chunking Service

```csharp
namespace Nekote.Core.AI.Services;

/// <summary>
/// 長文を意味的な概念に基づいて分割します。
/// </summary>
public sealed class SmartChunkingService
{
    private readonly IChatCompletionService _chatService;
    private readonly ILogger<SmartChunkingService> _logger;

    public SmartChunkingService(
        IChatCompletionService chatService,
        ILogger<SmartChunkingService> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    /// <summary>
    /// 長文を AI を使用して意味的なチャンクに分割します。
    /// </summary>
    public async Task<IReadOnlyList<string>> ChunkTextAsync(
        string longText,
        int targetChunkSize = 500,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"""
            以下のテキストを、それぞれ約 {targetChunkSize} 単語の意味的なまとまりに分割してください。
            各チャンクは独立して理解可能であるべきです。

            テキスト:
            {longText}
            """;

        var messages = new List<ChatMessage>
        {
            new() { Role = ChatRole.System, Content = "You are a text chunking assistant." },
            new() { Role = ChatRole.User, Content = prompt }
        };

        var response = await _chatService.GetCompletionAsync(messages, null, null, cancellationToken)
            .ConfigureAwait(false);

        // Parse AI response into chunks
        return ParseChunks(response.Content);
    }

    private static IReadOnlyList<string> ParseChunks(string aiResponse)
    {
        // Implementation: Parse AI's structured response
        // ...
    }
}
```

### 9.2. Semantic Search Service

```csharp
namespace Nekote.Core.AI.Services;

/// <summary>
/// エンベディングを使用したセマンティック検索を提供します。
/// </summary>
public sealed class SemanticSearchService
{
    private readonly ITextEmbeddingService _embeddingService;

    public SemanticSearchService(ITextEmbeddingService embeddingService)
    {
        _embeddingService = embeddingService;
    }

    /// <summary>
    /// クエリに最も類似したチャンクを検索します。
    /// </summary>
    public async Task<IReadOnlyList<SearchResult>> SearchAsync(
        string query,
        IReadOnlyList<string> chunks,
        int topK = 5,
        CancellationToken cancellationToken = default)
    {
        // 1. Get query embedding
        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(query, null, cancellationToken)
            .ConfigureAwait(false);

        // 2. Get all chunk embeddings (cached if using CachedEmbeddingRepository)
        var chunkEmbeddings = await _embeddingService.GetEmbeddingsAsync(chunks, null, cancellationToken)
            .ConfigureAwait(false);

        // 3. Calculate cosine similarity
        var results = chunkEmbeddings
            .Select((embedding, index) => new SearchResult
            {
                ChunkIndex = index,
                ChunkText = chunks[index],
                Similarity = CalculateCosineSimilarity(queryEmbedding.Vector, embedding.Vector)
            })
            .OrderByDescending(r => r.Similarity)
            .Take(topK)
            .ToList();

        return results;
    }

    private static float CalculateCosineSimilarity(
        IReadOnlyList<float> vectorA,
        IReadOnlyList<float> vectorB)
    {
        var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
        var magnitudeA = Math.Sqrt(vectorA.Sum(x => x * x));
        var magnitudeB = Math.Sqrt(vectorB.Sum(x => x * x));
        return (float)(dotProduct / (magnitudeA * magnitudeB));
    }
}
```

---

## 10. Responses API Support (OpenAI)

For stateful conversations using OpenAI's new Responses API:

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// ステートフルな会話コンテキストを管理します。
/// </summary>
public sealed class ConversationContext
{
    /// <summary>
    /// 前回の応答 ID (Responses API 用)。
    /// </summary>
    public string? PreviousResponseId { get; set; }

    /// <summary>
    /// 会話履歴 (ステートレスモード用)。
    /// </summary>
    public List<ChatMessage> Messages { get; } = new();
}
```

**Repository method:**

```csharp
public async Task<ChatResponse> ContinueConversationAsync(
    ConversationContext context,
    string userMessage,
    CancellationToken cancellationToken = default)
{
    var requestDto = new OpenAiResponsesRequestDto
    {
        Model = _configuration.ModelName,
        Input = userMessage,
        PreviousResponseId = context.PreviousResponseId // Key: stateful context
    };

    // ... HTTP call ...

    var response = OpenAiChatMapper.ToDomainModel(responseDto);
    context.PreviousResponseId = response.ResponseId; // Update for next call
    return response;
}
```

---

## 11. Error Handling Strategy

### 11.1. Exception Types

```csharp
namespace Nekote.Core.AI.Domain.Exceptions;

/// <summary>
/// AI サービスの基底例外クラス。
/// </summary>
public abstract class AiServiceException : Exception
{
    protected AiServiceException(string message) : base(message) { }
    protected AiServiceException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// API 呼び出しが失敗した場合にスローされます。
/// </summary>
public sealed class AiApiException : AiServiceException
{
    public int? StatusCode { get; }
    public string? ResponseBody { get; }

    public AiApiException(string message, int? statusCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}

/// <summary>
/// レート制限に達した場合にスローされます。
/// </summary>
public sealed class RateLimitException : AiServiceException
{
    public TimeSpan? RetryAfter { get; }

    public RateLimitException(string message, TimeSpan? retryAfter)
        : base(message)
    {
        RetryAfter = retryAfter;
    }
}
```

### 11.2. Repository Error Handling

```csharp
public async Task<ChatResponse> GetCompletionAsync(...)
{
    try
    {
        var response = await _httpClient.PostAsync(...).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogError("OpenAI API returned error: {StatusCode} - {Body}",
                (int)response.StatusCode, errorBody);

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta;
                throw new RateLimitException(
                    $"OpenAI rate limit exceeded. Retry after: {retryAfter}",
                    retryAfter);
            }

            throw new AiApiException(
                $"OpenAI API request failed with status {response.StatusCode}",
                (int)response.StatusCode,
                errorBody);
        }

        // ... success path ...
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Network error occurred while calling OpenAI API.");
        throw new AiApiException("Network error occurred while calling OpenAI API.", null, null, ex);
    }
}
```

---

## 12. Testing Strategy

### 12.1. Unit Tests (Domain Logic)

```csharp
namespace Nekote.Core.Tests.AI.Infrastructure.OpenAI;

public sealed class OpenAiChatMapperTests
{
    [Fact]
    public void ToRequestDto_ValidMessages_MapsCorrectly()
    {
        // Arrange
        var messages = new List<ChatMessage>
        {
            new() { Role = ChatRole.User, Content = "Hello" }
        };

        // Act
        var dto = OpenAiChatMapper.ToRequestDto(messages, null, "gpt-4");

        // Assert
        Assert.Equal("gpt-4", dto.Model);
        Assert.Single(dto.Messages);
        Assert.Equal("user", dto.Messages[0].Role);
        Assert.Equal("Hello", dto.Messages[0].Content);
    }
}
```

### 12.2. Integration Tests (HTTP Mocking)

```csharp
public sealed class OpenAiChatRepositoryTests
{
    [Fact]
    public async Task GetCompletionAsync_ValidRequest_ReturnsResponse()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://api.openai.com/v1/chat/completions")
            .Respond("application/json", TestData.OpenAiSuccessResponse);

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.openai.com");

        var repository = new OpenAiChatRepository(
            CreateMockHttpClientFactory(httpClient),
            Mock.Of<ILogger<OpenAiChatRepository>>(),
            Options.Create(new OpenAiConfiguration
            {
                ApiKey = "test-key",
                ModelName = "gpt-4"
            }));

        var messages = new List<ChatMessage>
        {
            new() { Role = ChatRole.User, Content = "Test" }
        };

        // Act
        var response = await repository.GetCompletionAsync(messages);

        // Assert
        Assert.NotNull(response);
        Assert.NotEmpty(response.Content);
    }
}
```

---

## 13. Migration Path (Implementation Order)

### Phase 1: Foundation (Weeks 1-2)
1. Create Domain interfaces (`IChatCompletionService`, `ITextEmbeddingService`)
2. Create Domain models (`ChatMessage`, `ChatResponse`, etc.)
3. Implement `IDiagnosticDataCollector`

### Phase 2: First Provider (Weeks 3-4)
1. Implement OpenAI Chat (with DTOs and Mapper)
2. Create DI extension methods
3. Write unit tests for Mapper
4. Write integration tests for Repository

### Phase 3: Caching & Second Provider (Weeks 5-6)
1. Implement `CachedEmbeddingRepository` decorator
2. Implement OpenAI Embedding
3. Implement Gemini Chat
4. Test multi-provider DI registration

### Phase 4: RAG Services (Weeks 7-8)
1. Implement `SmartChunkingService`
2. Implement `SemanticSearchService`
3. Create end-to-end RAG example in `Nekote.Lab.Console`

### Phase 5: Remaining Providers (Weeks 9-12)
1. Anthropic (Claude)
2. xAI (Grok)
3. Mistral
4. DeepSeek
5. DeepL (Translation)

---

## 14. Code Quality Checklist

Before committing any AI-related code, verify:

- [ ] Domain models are pure POCOs (no attributes)
- [ ] DTOs perfectly match external API specs
- [ ] Mappers are the only code that knows both DTO and Domain
- [ ] All `await` calls use `.ConfigureAwait(false)`
- [ ] All async methods accept `CancellationToken`
- [ ] XML comments are in Japanese
- [ ] Exception messages and logs are in English
- [ ] No hard-coded strings (use `IConfiguration`)
- [ ] No `Console.WriteLine()` (use `ILogger<T>`)
- [ ] Repository classes are `internal sealed`
- [ ] One public type per file
- [ ] File names match type names
- [ ] Tests follow "Arrange, Act, Assert" pattern

---

## 15. Open Questions / Future Decisions

1. **Streaming Response Handling:** Should we use `IAsyncEnumerable<string>` (token-by-token) or `IAsyncEnumerable<ChatResponse>` (chunk-by-chunk)?
2. **Retry Policy:** Should we implement automatic retry (e.g., Polly library) in repositories, or leave that to the application layer?
3. **Token Counting:** Should we implement client-side token counting (e.g., using `tiktoken`), or always rely on API responses?
4. **Multi-Modal Support:** If we add image/audio in the future, should they be separate interfaces (e.g., `IImageGenerationService`) or extend existing interfaces?

---

**End of Architecture Document**

*This is a living document. As implementation progresses, update this file with actual decisions and code patterns.*
