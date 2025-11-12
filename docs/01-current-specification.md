# Nekote.Core AI 統合機能 - 現行仕様書

**作成日:** 2025-11-12
**目的:** AI統合機能の実装可能な詳細仕様を定義する
**前提文書:** PLAYBOOK.md, PLAY_HARDER.md, nekote-ai-integration-strategy-2025-11-08.md, AI_ARCHITECTURE_REVISIONS.md

---

## 1. 設計方針の根本的転換

### 1.1. 旧仕様の問題点

**旧設計の思想:**
- 6つのAIプロバイダーの「最大公約数」として共通ドメインモデルを定義
- `JsonExtensionData`でプロバイダー固有データを吸収
- Mapperを検証層（Anti-Corruption Layer）として機能させる
- 共通データのみドメインモデルに変換、残りはDTOまたは辞書でアクセス

**実装時の問題:**
1. **OpenAI中心の設計に偏る**
   - 先駆者OpenAIの知識に引っ張られ、`Temperature`等がドメインモデルに混入
   - Gemini等の他プロバイダーでは変換・解釈が必要になり、不整合が発生

2. **DTO実装の不完全性**
   - OpenAI専用DTOでさえ、chat responseの基本データを受け止められない
   - トークン数詳細、エラー情報等を無視
   - ドメインモデルを見ながらのDTO設計により、本来必要なフィールドが欠落

3. **教科書的だが実践的でない**
   - 理論的には正しいが、実装品質が低下
   - 共通化の無理強いが逆効果

### 1.2. 新設計の基本思想

**核心原則:**
```
「会話」のみ共通化し、それ以外は完全に独立実装する
```

**設計の二本柱:**

#### 柱1: 「会話」構造のみ汎用定義

**理由:**
- 「ここからはGeminiで」「簡単な検索は安いAPIで」等のプロバイダー切り替えニーズ
- 会話履歴は本質的にプロバイダー非依存

**対象:**
- `ChatMessage` (role + content の構造)
- `ChatRole` (System / User / Assistant)
- 会話履歴の管理構造

**対象外（ベンダー固有実装）:**
- 設定（APIキー、エンドポイント）
- パラメーター（Temperature、MaxTokens等）
- レスポンスデータ（トークン数、メタデータ等）

#### 柱2: ベンダーごとの完全独立実装

**方針:**
- 設定/パラメーター/データは**ベンダーごとに専用ドメインモデル**を定義
- DTOは「テキスト関連」かつ「tool calling無関係」なら**100％完全実装**
- Mapper/Validatorパターンは継承（DTOデータの完全性確認は不可欠）
- 絞り込んだ機能サブセットで100％品質を目指す

**実装範囲の明確化:**
- ✅ **実装する:** テキストベースのチャット、エンベディング、翻訳
- ❌ **実装しない:** 画像/音声/動画、tool calling、ストリーミング（Phase 1では）

---

## 2. アーキテクチャ詳細

### 2.1. レイヤー構造

```
Nekote.Core
├─ AI
│   ├─ Domain                    // 純粋なビジネスロジック
│   │   ├─ Common               // 全プロバイダー共通
│   │   │   ├─ ChatMessage.cs
│   │   │   ├─ ChatRole.cs
│   │   │   └─ ChatHistory.cs
│   │   ├─ OpenAI               // OpenAI専用ドメインモデル
│   │   │   ├─ OpenAiChatRequest.cs
│   │   │   ├─ OpenAiChatResponse.cs
│   │   │   ├─ OpenAiChatOptions.cs
│   │   │   └─ OpenAiConfiguration.cs
│   │   ├─ Gemini               // Gemini専用ドメインモデル
│   │   ├─ Anthropic            // Anthropic専用ドメインモデル
│   │   ├─ XAi                  // xAI専用ドメインモデル
│   │   ├─ Mistral              // Mistral専用ドメインモデル
│   │   └─ DeepSeek             // DeepSeek専用ドメインモデル
│   │
│   ├─ Infrastructure           // 外部システムへのアクセス
│   │   ├─ OpenAI
│   │   │   ├─ Chat
│   │   │   │   ├─ Dtos
│   │   │   │   │   ├─ OpenAiChatRequestDto.cs
│   │   │   │   │   └─ OpenAiChatResponseDto.cs
│   │   │   │   ├─ OpenAiChatMapper.cs
│   │   │   │   └─ OpenAiChatRepository.cs
│   │   │   ├─ Embedding
│   │   │   └─ DependencyInjection
│   │   ├─ Gemini
│   │   ├─ Anthropic
│   │   ├─ xAI
│   │   ├─ Mistral
│   │   └─ DeepSeek
│   │
│   └─ Services                 // 機能の共通インターフェース（抽象化層）
│       ├─ IChatService.cs
│       ├─ IEmbeddingService.cs
│       └─ ITranslationService.cs
```

### 2.2. データフロー

```
ユーザーコード
    ↓ (Common.ChatMessage を使用)
IChatService (共通インターフェース)
    ↓
OpenAiChatService (実装)
    ↓ (OpenAiChatRequest に変換)
OpenAiChatRepository
    ↓ (OpenAiChatRequestDto に変換)
OpenAiChatMapper
    ↓ (HTTP POST)
OpenAI API
    ↓ (JSON Response)
OpenAiChatMapper (DTO → Domain)
    ↓ (OpenAiChatResponse を返す)
OpenAiChatService
    ↓ (必要に応じて Common 型に変換)
ユーザーコード
```

### 2.3. 共通「会話」モデルの詳細

#### ChatMessage (共通)

```csharp
namespace Nekote.Core.AI.Domain.Common;

/// <summary>
/// チャットメッセージの汎用表現。
/// 全てのAIプロバイダーで共通的に使用できる最小限の構造。
/// </summary>
public sealed class ChatMessage
{
    /// <summary>
    /// メッセージの役割を取得します。
    /// </summary>
    public required ChatRole Role { get; init; }

    /// <summary>
    /// メッセージの内容を取得します。
    /// </summary>
    public required string Content { get; init; }
}
```

#### ChatRole (共通)

```csharp
namespace Nekote.Core.AI.Domain.Common;

/// <summary>
/// チャットメッセージの役割を表します。
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// システムメッセージ（プロンプト、指示）。
    /// </summary>
    System,

    /// <summary>
    /// ユーザーメッセージ（質問、入力）。
    /// </summary>
    User,

    /// <summary>
    /// アシスタントメッセージ（AIの応答）。
    /// </summary>
    Assistant
}
```

#### ChatHistory (共通)

```csharp
namespace Nekote.Core.AI.Domain.Common;

/// <summary>
/// 会話履歴を管理します。
/// プロバイダー切り替え時に履歴を保持できるように設計。
/// </summary>
public sealed class ChatHistory
{
    private readonly List<ChatMessage> _messages = new();

    /// <summary>
    /// 現在の会話履歴を取得します。
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages => _messages;

    /// <summary>
    /// メッセージを追加します。
    /// </summary>
    public void AddMessage(ChatRole role, string content)
    {
        _messages.Add(new ChatMessage { Role = role, Content = content });
    }

    /// <summary>
    /// 履歴をクリアします。
    /// </summary>
    public void Clear()
    {
        _messages.Clear();
    }
}
```

### 2.4. ベンダー固有ドメインモデルの設計

#### 基本方針

1. **配置:** `Nekote.Core.AI.Domain.{Vendor}` 名前空間
2. **命名:** `{Vendor}Chat{Purpose}` 形式（例: `OpenAiChatRequest`, `GeminiChatOptions`）
3. **責務:** そのベンダーのAPI仕様を**完全に**反映
4. **属性:** 一切なし（純粋POCO、シリアライズ属性はDTOのみ）

#### OpenAI ドメインモデル例

**OpenAiChatRequest.cs**

```csharp
namespace Nekote.Core.AI.Domain.OpenAI;

/// <summary>
/// OpenAI Chat Completions APIへのリクエストを表します。
/// </summary>
public sealed class OpenAiChatRequest
{
    /// <summary>
    /// 使用するモデル名を取得します（例: "gpt-4", "gpt-3.5-turbo"）。
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// 会話メッセージのリストを取得します。
    /// </summary>
    public required IReadOnlyList<Common.ChatMessage> Messages { get; init; }

    /// <summary>
    /// サンプリング温度（0.0～2.0）を取得します。
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// 生成する最大トークン数を取得します。
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-pサンプリング値を取得します。
    /// </summary>
    public float? TopP { get; init; }

    /// <summary>
    /// 頻度ペナルティ（-2.0～2.0）を取得します。
    /// </summary>
    public float? FrequencyPenalty { get; init; }

    /// <summary>
    /// 存在ペナルティ（-2.0～2.0）を取得します。
    /// </summary>
    public float? PresencePenalty { get; init; }

    /// <summary>
    /// 生成を停止するシーケンスのリストを取得します。
    /// </summary>
    public IReadOnlyList<string>? Stop { get; init; }

    /// <summary>
    /// ユーザー識別子を取得します。
    /// </summary>
    public string? User { get; init; }
}
```

**OpenAiChatResponse.cs**

```csharp
namespace Nekote.Core.AI.Domain.OpenAI;

/// <summary>
/// OpenAI Chat Completions APIからのレスポンスを表します。
/// </summary>
public sealed class OpenAiChatResponse
{
    /// <summary>
    /// レスポンスIDを取得します。
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// オブジェクトタイプを取得します（通常は "chat.completion"）。
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// 作成日時（Unixタイムスタンプ）を取得します。
    /// </summary>
    public required long Created { get; init; }

    /// <summary>
    /// 使用されたモデル名を取得します。
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// 生成されたメッセージの内容を取得します。
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 完了理由を取得します（例: "stop", "length", "content_filter"）。
    /// </summary>
    public required string FinishReason { get; init; }

    /// <summary>
    /// トークン使用量の情報を取得します。
    /// </summary>
    public OpenAiTokenUsage? Usage { get; init; }

    /// <summary>
    /// システムフィンガープリントを取得します。
    /// </summary>
    public string? SystemFingerprint { get; init; }
}
```

**OpenAiTokenUsage.cs**

```csharp
namespace Nekote.Core.AI.Domain.OpenAI;

/// <summary>
/// OpenAI APIのトークン使用量を表します。
/// </summary>
public sealed class OpenAiTokenUsage
{
    /// <summary>
    /// プロンプトで使用されたトークン数を取得します。
    /// </summary>
    public required int PromptTokens { get; init; }

    /// <summary>
    /// 生成で使用されたトークン数を取得します。
    /// </summary>
    public required int CompletionTokens { get; init; }

    /// <summary>
    /// 合計トークン数を取得します。
    /// </summary>
    public required int TotalTokens { get; init; }

    /// <summary>
    /// プロンプトトークンの詳細を取得します。
    /// </summary>
    public OpenAiPromptTokensDetails? PromptTokensDetails { get; init; }

    /// <summary>
    /// 補完トークンの詳細を取得します。
    /// </summary>
    public OpenAiCompletionTokensDetails? CompletionTokensDetails { get; init; }
}
```

### 2.5. DTO設計の原則

#### 絶対原則

1. **100％完全実装**
   - APIレスポンスの全フィールドを正確に定義
   - トークン詳細、エラー情報、メタデータ等を一切省略しない
   - 「テキスト関連」かつ「tool calling無関係」なら全て実装

2. **全プロパティnullable**
   - API側の不整合・変更に防御的に対応
   - `string?`, `int?`, `List<T>?` を徹底

3. **シリアライズ属性のみ**
   - `[JsonPropertyName("...")]` のみ使用
   - ビジネスロジックは一切含まない

4. **DTO専用名前空間**
   - `Nekote.Core.AI.Infrastructure.{Vendor}.{Feature}.Dtos`
   - DTOはInfrastructure層の内部実装詳細

#### OpenAI DTO例

**OpenAiChatRequestDto.cs**

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI Chat Completions APIのリクエストDTO。
/// 全プロパティはnullableとして定義され、API仕様の変更に対応します。
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

    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; init; }

    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; init; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; init; }

    [JsonPropertyName("user")]
    public string? User { get; init; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; init; }
}
```

**OpenAiChatResponseDto.cs**

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI Chat Completions APIのレスポンスDTO。
/// API仕様の全フィールドを正確に実装します。
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

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; init; }
}
```

**OpenAiUsageDto.cs**

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI トークン使用量のDTO。
/// </summary>
internal sealed class OpenAiUsageDto
{
    [JsonPropertyName("prompt_tokens")]
    public int? PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int? CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int? TotalTokens { get; init; }

    [JsonPropertyName("prompt_tokens_details")]
    public OpenAiPromptTokensDetailsDto? PromptTokensDetails { get; init; }

    [JsonPropertyName("completion_tokens_details")]
    public OpenAiCompletionTokensDetailsDto? CompletionTokensDetails { get; init; }
}
```

### 2.6. Mapper/Validator パターン

#### 責務

1. **変換:** ドメインモデル ⇔ DTO
2. **検証:** DTOの完全性確認（nullチェック、必須フィールド検証）
3. **Anti-Corruption Layer:** 外部仕様の変更から内部を保護

#### 設計原則

- **静的クラス:** Mapperはステートレス
- **防御的実装:** DTO側が不完全でも明確なエラーメッセージ
- **例外スロー:** 不正なデータは`InvalidOperationException`で即座に停止

#### OpenAiChatMapper 例

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using Nekote.Core.AI.Domain.Common;
using Nekote.Core.AI.Domain.OpenAI;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI ChatのドメインモデルとDTO間の変換を行います。
/// </summary>
internal static class OpenAiChatMapper
{
    /// <summary>
    /// ドメインモデルをDTOに変換します。
    /// </summary>
    public static OpenAiChatRequestDto ToDto(OpenAiChatRequest request)
    {
        // 必須フィールドの検証
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new ArgumentException("Model name is required.", nameof(request));
        }

        if (request.Messages == null || request.Messages.Count == 0)
        {
            throw new ArgumentException("At least one message is required.", nameof(request));
        }

        return new OpenAiChatRequestDto
        {
            Model = request.Model,
            Messages = request.Messages
                .Select(m => new OpenAiMessageDto
                {
                    Role = MapRole(m.Role),
                    Content = m.Content
                })
                .ToList(),
            Temperature = request.Temperature,
            MaxTokens = request.MaxTokens,
            TopP = request.TopP,
            FrequencyPenalty = request.FrequencyPenalty,
            PresencePenalty = request.PresencePenalty,
            Stop = request.Stop?.ToList(),
            User = request.User,
            Stream = false
        };
    }

    /// <summary>
    /// DTOをドメインモデルに変換します。
    /// </summary>
    public static OpenAiChatResponse ToDomain(OpenAiChatResponseDto dto)
    {
        // 防御的検証
        if (dto.Choices == null || dto.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "OpenAI response is invalid: 'choices' array is null or empty.");
        }

        var firstChoice = dto.Choices[0];

        if (firstChoice.Message == null)
        {
            throw new InvalidOperationException(
                "OpenAI response is invalid: first choice 'message' is null.");
        }

        if (string.IsNullOrWhiteSpace(firstChoice.Message.Content))
        {
            throw new InvalidOperationException(
                "OpenAI response is invalid: message 'content' is null or empty.");
        }

        return new OpenAiChatResponse
        {
            Id = dto.Id ?? "unknown",
            Object = dto.Object ?? "chat.completion",
            Created = dto.Created ?? 0,
            Model = dto.Model ?? "unknown",
            Content = firstChoice.Message.Content,
            FinishReason = firstChoice.FinishReason ?? "unknown",
            Usage = dto.Usage != null ? MapUsage(dto.Usage) : null,
            SystemFingerprint = dto.SystemFingerprint
        };
    }

    private static string MapRole(ChatRole role)
    {
        return role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            _ => throw new ArgumentException($"Unknown role: {role}", nameof(role))
        };
    }

    private static OpenAiTokenUsage MapUsage(OpenAiUsageDto dto)
    {
        return new OpenAiTokenUsage
        {
            PromptTokens = dto.PromptTokens ?? 0,
            CompletionTokens = dto.CompletionTokens ?? 0,
            TotalTokens = dto.TotalTokens ?? 0,
            PromptTokensDetails = dto.PromptTokensDetails != null
                ? MapPromptTokensDetails(dto.PromptTokensDetails)
                : null,
            CompletionTokensDetails = dto.CompletionTokensDetails != null
                ? MapCompletionTokensDetails(dto.CompletionTokensDetails)
                : null
        };
    }
}
```

### 2.7. Repository パターン

#### 責務

- HTTPクライアント経由でのAPI呼び出し
- リクエストのシリアライズ
- レスポンスのデシリアライズ
- エラーハンドリング
- ロギング

#### 設計原則

- `IHttpClientFactory`でHTTPクライアント取得
- `ILogger<T>`でロギング
- `IOptions<TConfig>`で設定取得
- `.ConfigureAwait(false)`を全ての`await`に適用
- `CancellationToken`を受け入れ

#### OpenAiChatRepository 例

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nekote.Core.AI.Domain.OpenAI;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

/// <summary>
/// OpenAI Chat Completions APIとの通信を行います。
/// </summary>
internal sealed class OpenAiChatRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiChatRepository> _logger;
    private readonly string _apiKey;

    public OpenAiChatRepository(
        IHttpClientFactory httpClientFactory,
        ILogger<OpenAiChatRepository> logger,
        IOptions<OpenAiConfiguration> configuration)
    {
        _logger = logger;

        var config = configuration.Value;

        // API キーの解決（必須）
        _apiKey = config.ApiKey
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured. Set 'AI:OpenAI:ApiKey' in configuration.");

        // エンドポイントの解決（デフォルト値あり）
        var endpoint = config.ChatEndpoint
            ?? config.BaseUrl + "/v1/chat/completions"
            ?? "https://api.openai.com/v1/chat/completions";

        _httpClient = httpClientFactory.CreateClient("Nekote.AI.OpenAI.Chat");
        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<OpenAiChatResponse> SendRequestAsync(
        OpenAiChatRequest request,
        CancellationToken cancellationToken = default)
    {
        // ドメインモデル → DTO
        var requestDto = OpenAiChatMapper.ToDto(request);

        // シリアライズ
        var json = JsonSerializer.Serialize(requestDto, JsonDefaults.Options);

        _logger.LogDebug("Sending request to OpenAI Chat API: {Json}", json);

        // HTTP POST
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpResponse = await _httpClient.PostAsync(
            string.Empty,
            content,
            cancellationToken)
            .ConfigureAwait(false);

        // レスポンス読み取り
        var responseJson = await httpResponse.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        _logger.LogDebug("Received response from OpenAI Chat API: {Json}", responseJson);

        // エラーチェック
        if (!httpResponse.IsSuccessStatusCode)
        {
            _logger.LogError(
                "OpenAI Chat API returned error {StatusCode}: {Body}",
                (int)httpResponse.StatusCode,
                responseJson);

            throw new InvalidOperationException(
                $"OpenAI Chat API request failed with status {httpResponse.StatusCode}: {responseJson}");
        }

        // デシリアライズ
        var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(
            responseJson,
            JsonDefaults.Options)
            ?? throw new InvalidOperationException(
                "Failed to deserialize OpenAI Chat API response.");

        // DTO → ドメインモデル
        return OpenAiChatMapper.ToDomain(responseDto);
    }
}
```

### 2.8. サービス層（機能の抽象化）

#### 目的

- ベンダー固有実装をラップし、機能として抽象化
- ユーザーコードがプロバイダー実装の詳細を意識しないようにする
- 共通インターフェースでプロバイダー切り替えを可能にする

#### IChatService インターフェース

```csharp
namespace Nekote.Core.AI.Services;

using Nekote.Core.AI.Domain.Common;

/// <summary>
/// チャット機能の共通インターフェース。
/// プロバイダーに依存しない抽象化を提供します。
/// </summary>
public interface IChatService
{
    /// <summary>
    /// 会話履歴を元にAIの応答を取得します。
    /// </summary>
    /// <param name="history">会話履歴。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>AIの応答メッセージ。</returns>
    Task<ChatMessage> GetResponseAsync(
        ChatHistory history,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 単一メッセージに対するAIの応答を取得します。
    /// </summary>
    /// <param name="userMessage">ユーザーメッセージ。</param>
    /// <param name="systemPrompt">システムプロンプト（オプション）。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>AIの応答メッセージ。</returns>
    Task<ChatMessage> GetResponseAsync(
        string userMessage,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default);
}
```

#### OpenAiChatService 実装例

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using Nekote.Core.AI.Domain.Common;
using Nekote.Core.AI.Domain.OpenAI;
using Nekote.Core.AI.Services;

/// <summary>
/// OpenAIを使用したチャットサービスの実装。
/// </summary>
internal sealed class OpenAiChatService : IChatService
{
    private readonly OpenAiChatRepository _repository;
    private readonly string _defaultModel;

    public OpenAiChatService(
        OpenAiChatRepository repository,
        IOptions<OpenAiConfiguration> configuration)
    {
        _repository = repository;
        _defaultModel = configuration.Value.DefaultModel ?? "gpt-4";
    }

    public async Task<ChatMessage> GetResponseAsync(
        ChatHistory history,
        CancellationToken cancellationToken = default)
    {
        // ChatHistory → OpenAiChatRequest
        var request = new OpenAiChatRequest
        {
            Model = _defaultModel,
            Messages = history.Messages
        };

        // Repository経由でAPI呼び出し
        var response = await _repository.SendRequestAsync(request, cancellationToken)
            .ConfigureAwait(false);

        // OpenAiChatResponse → ChatMessage
        return new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = response.Content
        };
    }

    public async Task<ChatMessage> GetResponseAsync(
        string userMessage,
        string? systemPrompt = null,
        CancellationToken cancellationToken = default)
    {
        var history = new ChatHistory();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
        {
            history.AddMessage(ChatRole.System, systemPrompt);
        }

        history.AddMessage(ChatRole.User, userMessage);

        return await GetResponseAsync(history, cancellationToken)
            .ConfigureAwait(false);
    }
}
```

---

## 3. 生データアクセス機構

### 3.1. 設計課題

**問題:**
- Nekote.Coreが対応していない機能やフィールドへのアクセスが必要
- API仕様変更への即座の対応が困難
- ユーザーが「残りデータ」や「中間JSON」を必要とする場合がある

**要件:**
1. 将来Nekote.Coreが対応しても、古いコード（辞書アクセス）が動作し続ける
2. 型安全性を可能な限り維持
3. パフォーマンスへの影響を最小化

### 3.2. 実装方針（未確定 - 要議論）

**候補1: RawDataプロパティ**

```csharp
public sealed class OpenAiChatResponse
{
    // ... 通常のプロパティ ...

    /// <summary>
    /// 元のJSON文字列を取得します。
    /// Nekote.Coreが対応していないフィールドへのアクセスに使用できます。
    /// </summary>
    public string? RawJson { get; init; }

    /// <summary>
    /// 元のJSONをパースした辞書を取得します。
    /// </summary>
    public IReadOnlyDictionary<string, object?>? RawData { get; init; }
}
```

**候補2: 拡張メソッド**

```csharp
public static class OpenAiChatResponseExtensions
{
    public static T? GetRawValue<T>(this OpenAiChatResponse response, string jsonPath)
    {
        // JSON パスで値を取得
    }
}
```

**候補3: 専用アクセサークラス**

```csharp
public sealed class OpenAiChatResponseAccessor
{
    private readonly JsonDocument _document;

    public OpenAiChatResponseAccessor(string rawJson)
    {
        _document = JsonDocument.Parse(rawJson);
    }

    public T? GetValue<T>(string path) { /* ... */ }
}
```

**決定事項: この機構の詳細設計はPhase 2以降で確定する**

---

## 4. 設定管理

### 4.1. 設定クラスの設計

**基本原則:**
- セクション名の定数は定義しない（柔軟性のため）
- 全プロパティnullable
- 機能別の細分化（Chat / Embedding）

#### OpenAiConfiguration

```csharp
namespace Nekote.Core.AI.Domain.OpenAI;

/// <summary>
/// OpenAI APIの設定。
/// </summary>
public sealed class OpenAiConfiguration
{
    /// <summary>
    /// デフォルトのAPIキーを取得します。
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// ベースURLを取得します（デフォルト: https://api.openai.com）。
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// チャット専用エンドポイントを取得します。
    /// </summary>
    public string? ChatEndpoint { get; init; }

    /// <summary>
    /// エンベディング専用エンドポイントを取得します。
    /// </summary>
    public string? EmbeddingEndpoint { get; init; }

    /// <summary>
    /// デフォルトモデル名を取得します。
    /// </summary>
    public string? DefaultModel { get; init; }

    /// <summary>
    /// チャット専用モデル名を取得します。
    /// </summary>
    public string? ChatModel { get; init; }

    /// <summary>
    /// エンベディング専用モデル名を取得します。
    /// </summary>
    public string? EmbeddingModel { get; init; }
}
```

### 4.2. appsettings.json 例

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-...",
      "BaseUrl": "https://api.openai.com",
      "DefaultModel": "gpt-4",
      "ChatModel": "gpt-4-turbo",
      "EmbeddingModel": "text-embedding-3-small"
    },
    "Gemini": {
      "ApiKey": "AIza...",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "DefaultModel": "gemini-2.5-pro"
    }
  }
}
```

### 4.3. DI登録

```csharp
namespace Nekote.Core.AI.Infrastructure.DependencyInjection;

public static class OpenAiServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAiChat(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection = "AI:OpenAI")
    {
        // 設定をバインド
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(configurationSection));

        // HttpClientを登録
        services.AddHttpClient("Nekote.AI.OpenAI.Chat");

        // Repositoryを登録
        services.AddScoped<OpenAiChatRepository>();

        // Serviceを登録
        services.AddScoped<IChatService, OpenAiChatService>();

        return services;
    }
}
```

---

## 5. エラーハンドリング

### 5.1. 基本方針

- **組み込み例外を優先:** `InvalidOperationException`, `ArgumentException`等
- **カスタム例外は最小限:** 本当に必要な場合のみ
- **境界でキャッチ:** アプリケーション層でのみハンドリング

### 5.2. 例外の種類

| シナリオ | 例外の種類 | 理由 |
|---------|-----------|------|
| APIキー未設定 | `InvalidOperationException` | 設定が無効な状態 |
| モデル名が空 | `ArgumentException` | 引数が不正 |
| API呼び出し失敗（HTTP） | `HttpRequestException` | .NETの標準例外 |
| JSON デシリアライズ失敗 | `JsonException` | System.Text.Jsonの標準例外 |
| DTO検証失敗 | `InvalidOperationException` | データの状態が不正 |

### 5.3. カスタム例外（検討中）

**必要性の判断:**
- レート制限エラーのリトライ処理が必要な場合
- プロバイダー固有のエラー情報を保持したい場合

**候補:**

```csharp
namespace Nekote.Core.AI.Domain.Exceptions;

/// <summary>
/// AI API呼び出しが失敗した場合にスローされます。
/// </summary>
public sealed class AiApiException : InvalidOperationException
{
    public string Provider { get; }
    public int? HttpStatusCode { get; }
    public string? ResponseBody { get; }

    public AiApiException(
        string provider,
        string message,
        int? httpStatusCode = null,
        string? responseBody = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Provider = provider;
        HttpStatusCode = httpStatusCode;
        ResponseBody = responseBody;
    }
}
```

**決定: Phase 1では組み込み例外のみ使用し、必要性が明確になってからカスタム例外を追加**

---

## 6. ロギング

### 6.1. 基本方針

- `ILogger<T>` を全てのRepositoryに注入
- リクエスト/レスポンスのJSON をDebugレベルで記録
- エラーはErrorレベルで記録
- APIキー等の機密情報はマスキング

### 6.2. ログレベル

| 内容 | レベル |
|-----|-------|
| リクエスト送信 | Debug |
| レスポンス受信 | Debug |
| API呼び出し成功 | Information |
| API呼び出し失敗 | Error |
| 検証エラー | Warning |

### 6.3. ログ例

```csharp
_logger.LogDebug("Sending request to OpenAI Chat API: {Json}", requestJson);
_logger.LogDebug("Received response from OpenAI Chat API: {Json}", responseJson);
_logger.LogInformation("OpenAI Chat API call completed successfully. Tokens used: {Tokens}", totalTokens);
_logger.LogError("OpenAI Chat API call failed with status {StatusCode}: {Body}", statusCode, responseBody);
```

---

## 7. JSON シリアライゼーション

### 7.1. JsonDefaults クラス

```csharp
namespace Nekote.Core.AI.Infrastructure;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// JSONシリアライゼーションのデフォルトオプション。
/// </summary>
internal static class JsonDefaults
{
    /// <summary>
    /// 標準オプション（フォーマットなし）。
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// フォーマット付きオプション（ログ用）。
    /// </summary>
    public static readonly JsonSerializerOptions FormattedOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };
}
```

---

## 8. 実装優先順位

### Phase 1: OpenAI Chat（最優先）

**目的:** アーキテクチャの検証と基本パターンの確立

**成果物:**
- `Domain.Common.ChatMessage` / `ChatRole` / `ChatHistory`
- `Domain.OpenAI.*` (Request, Response, Configuration等)
- `Infrastructure.OpenAI.Chat.*` (DTO, Mapper, Repository)
- `Services.IChatService` / `OpenAiChatService`
- DI登録拡張メソッド
- 動作確認用コンソールアプリ

**完了条件:**
- 実際のOpenAI APIを呼び出して応答を取得できる
- エラーハンドリングが動作する
- ロギングが正常に出力される

### Phase 2: Gemini Chat

**目的:** パターンの再現性確認

**成果物:**
- `Domain.Gemini.*`
- `Infrastructure.Gemini.Chat.*`
- `GeminiChatService`

### Phase 3: OpenAI Embedding

**目的:** Chat以外の機能実装

### Phase 4-6: 残りのプロバイダー

- Anthropic
- xAI
- Mistral
- DeepSeek

### Phase 7: 高度な機能

- ストリーミング
- キャッシュ
- リトライポリシー
- 診断システム

---

## 9. テスト戦略

### 9.1. 単体テスト

**対象:**
- Mapper（ドメイン ⇔ DTO変換）
- 設定解決ロジック
- エラー検証ロジック

**ツール:**
- xUnit
- FluentAssertions

### 9.2. 統合テスト

**対象:**
- Repository（モックHTTP使用）
- Service（モックRepository使用）

**ツール:**
- xUnit
- Moq
- WireMock.Net（HTTPモック）

### 9.3. E2Eテスト

**対象:**
- 実API呼び出し（CI/CDでは省略可能）

**注意:**
- APIキーの安全な管理
- レート制限への配慮
- コスト管理

---

## 10. サブセット仕様書

### 10.1. 目的

- 各プロバイダーのAPI仕様書から「実装する機能」のみを抽出
- AIエージェントが参照しやすい形式で管理
- 仕様変更の追跡を容易にする

### 10.2. 構造（例: OpenAI Chat）

```markdown
# OpenAI Chat Completions API - サブセット仕様

**バージョン:** 2024-11-01
**最終更新:** 2025-11-12

## エンドポイント

POST https://api.openai.com/v1/chat/completions

## 認証

Authorization: Bearer {API_KEY}

## リクエストボディ

| フィールド | 型 | 必須 | 説明 |
|-----------|---|------|------|
| model | string | ✅ | 使用するモデル名 |
| messages | array | ✅ | メッセージのリスト |
| temperature | number | ❌ | サンプリング温度（0.0～2.0） |
| max_tokens | integer | ❌ | 生成する最大トークン数 |

## レスポンスボディ

| フィールド | 型 | 説明 |
|-----------|---|------|
| id | string | レスポンスID |
| object | string | オブジェクトタイプ |
| created | integer | Unixタイムスタンプ |
| model | string | 使用されたモデル |
| choices | array | 生成結果のリスト |
| usage | object | トークン使用量 |

（以下、詳細...）
```

### 10.3. 管理方法

- `docs/api-specs/{Provider}/{Feature}.md` に配置
- 実装前に最新仕様と比較
- 変更があればサブセット仕様書を更新→コード更新

---

## 11. 未解決事項

### 11.1. 生データアクセス機構の詳細設計

**決定が必要:**
- 実装方式（RawDataプロパティ / 拡張メソッド / アクセサークラス）
- パフォーマンスへの影響
- API設計の使いやすさ

**決定時期:** Phase 1実装中

### 11.2. ストリーミングの実装方針

**決定が必要:**
- `IAsyncEnumerable<T>` の使用
- SSE（Server-Sent Events）のパース方法
- エラーハンドリング

**決定時期:** Phase 7

### 11.3. カスタム例外の必要性

**決定が必要:**
- レート制限例外が必要か
- プロバイダー固有例外が必要か

**決定時期:** Phase 1でエラーハンドリング実装後

### 11.4. キャッシュ戦略

**決定が必要:**
- キャッシュ対象（Embedding のみ？）
- キャッシュ層の実装（Decorator パターン？）
- キャッシュキーの設計

**決定時期:** Phase 7

---

## 12. 実装スケジュール見積もり

| Phase | 内容 | 見積もり時間 |
|-------|------|------------|
| 1 | OpenAI Chat完全実装 | 2日 |
| 2 | Gemini Chat実装 | 1日 |
| 3 | OpenAI Embedding実装 | 1日 |
| 4 | Anthropic Chat実装 | 1日 |
| 5 | xAI Chat実装 | 1日 |
| 6 | Mistral Chat実装 | 1日 |
| 7 | DeepSeek Chat実装 | 1日 |
| | **合計** | **8日（約1週間）** |

**前提条件:**
- サブセット仕様書が準備済み
- AIエージェントの活用
- 並行作業なし（順次実装）

---

*このドキュメントは実装開始前の最終仕様書です。実装中に判明した問題は `02-issues-and-improvements.md` に記録してください。*
