# Nekote.Core AI 統合機能 - 潜在的問題と改善提案

**作成日:** 2025-11-12
**目的:** 現行仕様の全ての潜在的問題点と改善案を記録する
**関連文書:** `01-current-specification.md`

---

## 1. 曖昧な仕様の明確化が必要な項目

### 1.1. 「会話」の具体的範囲

**現状の曖昧さ:**
- `ChatMessage` / `ChatRole` が共通であることは明確
- `ChatHistory` も共通
- しかし `ChatCompletionOptions` のような「会話に関するオプション」はどうするのか不明

**問題:**
- Temperatureのような「会話の性質を決めるパラメーター」は共通化すべきか？
- それとも完全にベンダー固有として扱うべきか？

**提案:**

**Option A: 完全分離（推奨）**
```
Common:
  - ChatMessage (role, content)
  - ChatRole (enum)
  - ChatHistory (メッセージリスト管理)

Vendor-Specific:
  - OpenAiChatOptions (temperature, max_tokens, etc.)
  - GeminiChatOptions (temperature, top_k, top_p, etc.)
  - すべてのリクエスト/レスポンスモデル
```

**理由:**
- Optionsは各ベンダーで微妙に異なる（GeminiにはTop-K、OpenAIにはFrequency Penalty等）
- 無理な共通化が旧仕様の失敗原因
- 「会話メッセージの構造」のみ共通化することで、最小限の結合を実現

**Option B: 基本Optionsを共通化**
```
Common:
  - ChatMessage, ChatRole, ChatHistory
  - CommonChatOptions (temperature?, maxTokens?)

Vendor-Specific:
  - OpenAiChatOptions : CommonChatOptions (追加フィールド)
```

**理由:**
- 頻繁に使うパラメーターは共通化したい
- しかし継承による結合が発生する

**決定が必要:** Option A（完全分離）を推奨するが、最終決定が必要

---

### 1.2. 生データアクセス機構の実装方式

**現状:** 3つの候補があり、未決定

#### 候補1: RawDataプロパティ方式

**設計:**
```csharp
public sealed class OpenAiChatResponse
{
    // 通常のプロパティ
    public required string Content { get; init; }
    public OpenAiTokenUsage? Usage { get; init; }

    // 生データアクセス
    public string? RawJson { get; init; }
    public IReadOnlyDictionary<string, JsonElement>? RawData { get; init; }
}

// 使用例
var content = response.Content;  // 型安全
var customField = response.RawData?["custom_field"]?.GetString();  // 生データ
```

**長所:**
- シンプル
- 追加のクラス不要
- 将来プロパティが追加されても辞書から取得可能

**短所:**
- 全レスポンスオブジェクトが肥大化
- 使わないユーザーにもコスト（メモリ、パース）が発生

#### 候補2: 拡張メソッド方式

**設計:**
```csharp
public sealed class OpenAiChatResponse
{
    // 通常のプロパティのみ
    public required string Content { get; init; }

    // 内部的に保持
    internal string? _rawJson;
}

public static class OpenAiChatResponseExtensions
{
    public static JsonDocument GetRawData(this OpenAiChatResponse response)
    {
        if (response._rawJson == null)
            throw new InvalidOperationException("Raw JSON is not available.");

        return JsonDocument.Parse(response._rawJson);
    }

    public static T? GetRawValue<T>(this OpenAiChatResponse response, string jsonPath)
    {
        // JSON パスで値を取得
    }
}

// 使用例
var content = response.Content;  // 型安全
var customField = response.GetRawValue<string>("custom_field");  // 拡張メソッド
```

**長所:**
- ドメインモデルがクリーン
- 使わないユーザーへの影響が最小
- 必要な時だけパース

**短所:**
- `internal` フィールドへのアクセスが必要（同アセンブリ限定）
- 拡張メソッドの発見性が低い

#### 候補3: 専用アクセサークラス方式

**設計:**
```csharp
public sealed class OpenAiChatResponse
{
    // 通常のプロパティのみ
    public required string Content { get; init; }

    // アクセサーを返す
    public OpenAiChatResponseAccessor CreateAccessor()
    {
        return new OpenAiChatResponseAccessor(_rawJson);
    }

    private readonly string? _rawJson;
}

public sealed class OpenAiChatResponseAccessor
{
    private readonly JsonDocument _document;

    internal OpenAiChatResponseAccessor(string? rawJson)
    {
        if (rawJson == null)
            throw new InvalidOperationException("Raw JSON is not available.");
        _document = JsonDocument.Parse(rawJson);
    }

    public T? GetValue<T>(string path) { /* ... */ }
    public bool TryGetValue<T>(string path, out T? value) { /* ... */ }
}

// 使用例
var content = response.Content;  // 型安全
var accessor = response.CreateAccessor();
var customField = accessor.GetValue<string>("custom_field");
```

**長所:**
- 明示的なオプトイン（使いたい人だけアクセサー作成）
- 型安全なAPIを提供しやすい
- パースは1回のみ（アクセサー内でキャッシュ）

**短所:**
- クラスが増える
- やや冗長

#### 推奨案と理由

**推奨: 候補3（専用アクセサークラス）**

**理由:**
1. **明示的なオプトイン:** 生データが必要な場合のみ `CreateAccessor()` を呼ぶ
2. **パフォーマンス:** 使わないユーザーへの影響なし
3. **型安全性:** アクセサークラス内で型安全なAPIを提供できる
4. **拡張性:** 将来的にパス以外のアクセス方法も追加可能
5. **後方互換性:** 将来プロパティが追加されても、アクセサー経由でも取得可能にできる

**実装時の注意:**
- `_rawJson` をRepositoryでセット
- `JsonDocument` のDispose管理
- パスの構文（JSONPath? ドット記法?）

**決定が必要:** Phase 1実装開始前に確定

---

### 1.3. ベンダー固有ドメインモデルの配置層

**現状の曖昧さ:**
- `OpenAiChatRequest` / `OpenAiChatResponse` 等はどの層に属するか？

**候補:**

**Option A: Domain層（現行仕様）**
```
/Domain
  /Common
  /OpenAI
    - OpenAiChatRequest.cs
    - OpenAiChatResponse.cs
    - OpenAiConfiguration.cs
```

**理由:**
- これらは「ドメインロジック」（ビジネス概念）
- DTOとは明確に区別
- PLAYBOOKの「Domain vs Infrastructure」原則に従う

**Option B: Infrastructure層**
```
/Infrastructure
  /OpenAI
    /Domain
      - OpenAiChatRequest.cs
      - OpenAiChatResponse.cs
    /Dtos
      - OpenAiChatRequestDto.cs
    - OpenAiChatRepository.cs
```

**理由:**
- OpenAI特化なので「Infrastructure」として扱う
- 同じベンダー関連を1箇所に集約

**推奨: Option A（Domain層）**

**理由:**
1. PLAYBOOKの原則との整合性
2. `OpenAiChatRequest` は外部API仕様ではなく、「OpenAIとのチャット」というビジネス概念
3. ServiceレイヤーがDomain層のみに依存できる（InfrastructureへのDI登録のみ）

**決定済み:** Option A（現行仕様のまま）

---

### 1.4. サービス層の抽象化レベル

**問題:**
- `IChatService` はどこまで抽象化すべきか？
- プロバイダー切り替えをどう実現するか？

**現行仕様:**
```csharp
public interface IChatService
{
    Task<ChatMessage> GetResponseAsync(ChatHistory history, ...);
    Task<ChatMessage> GetResponseAsync(string userMessage, ...);
}
```

**問題点:**
1. **戻り値が `ChatMessage`（共通型）のみ**
   - OpenAI特有のトークン数情報等が失われる
   - ユーザーがメタデータにアクセスできない

2. **プロバイダー切り替えの仕組みが不明**
   - DI登録時に `IChatService` に対してどのプロバイダーをバインド？
   - 複数プロバイダーを同時利用する場合は？

**改善案:**

#### 案1: ジェネリックサービス（型安全重視）

```csharp
public interface IChatService<TRequest, TResponse>
{
    Task<TResponse> GetResponseAsync(TRequest request, CancellationToken ct);
}

// 実装
public sealed class OpenAiChatService : IChatService<OpenAiChatRequest, OpenAiChatResponse>
{
    // ...
}

// 使用例
var service = serviceProvider.GetRequiredService<IChatService<OpenAiChatRequest, OpenAiChatResponse>>();
var response = await service.GetResponseAsync(request);
var tokens = response.Usage?.TotalTokens;  // 型安全
```

**長所:**
- 完全な型安全性
- プロバイダー固有情報にアクセス可能
- コンパイル時エラー検出

**短所:**
- プロバイダー切り替えが困難（型が異なる）
- 抽象化のメリットが薄い

#### 案2: 名前付きサービス（柔軟性重視）

```csharp
public interface IChatService
{
    Task<ChatMessage> GetResponseAsync(ChatHistory history, CancellationToken ct);
}

// DI登録
services.AddKeyedScoped<IChatService, OpenAiChatService>("openai");
services.AddKeyedScoped<IChatService, GeminiChatService>("gemini");

// 使用例
var openaiChat = serviceProvider.GetRequiredKeyedService<IChatService>("openai");
var geminiChat = serviceProvider.GetRequiredKeyedService<IChatService>("gemini");
```

**長所:**
- 複数プロバイダー同時利用が容易
- インターフェースは共通
- プロバイダー切り替えが文字列ベース

**短所:**
- 型安全性の喪失
- プロバイダー固有情報へのアクセス不可

#### 案3: Factoryパターン

```csharp
public interface IChatServiceFactory
{
    IChatService<TRequest, TResponse> Create<TRequest, TResponse>(string provider);
}

// 使用例
var factory = serviceProvider.GetRequiredService<IChatServiceFactory>();
var openaiService = factory.Create<OpenAiChatRequest, OpenAiChatResponse>("openai");
```

**長所:**
- 型安全性と柔軟性の両立
- プロバイダー名は文字列で切り替え可能

**短所:**
- 実装が複雑
- Factory内部でのサービス解決ロジックが必要

#### 案4: 専用インターフェース + 共通ヘルパー（推奨）

```csharp
// プロバイダー固有インターフェース
public interface IOpenAiChatService
{
    Task<OpenAiChatResponse> GetResponseAsync(OpenAiChatRequest request, CancellationToken ct);
}

public interface IGeminiChatService
{
    Task<GeminiChatResponse> GetResponseAsync(GeminiChatRequest request, CancellationToken ct);
}

// 共通ヘルパー（拡張メソッド）
public static class ChatServiceExtensions
{
    // ChatHistory → プロバイダー固有Request → Response → ChatMessage
    public static async Task<ChatMessage> GetResponseAsync(
        this IOpenAiChatService service,
        ChatHistory history,
        CancellationToken ct = default)
    {
        var request = new OpenAiChatRequest
        {
            Model = "gpt-4",
            Messages = history.Messages
        };

        var response = await service.GetResponseAsync(request, ct);

        return new ChatMessage
        {
            Role = ChatRole.Assistant,
            Content = response.Content
        };
    }

    // Gemini用も同様に実装
}

// 使用例
// 型安全にプロバイダー固有機能を使う
var openaiService = serviceProvider.GetRequiredService<IOpenAiChatService>();
var response = await openaiService.GetResponseAsync(request);
var tokens = response.Usage?.TotalTokens;

// 共通的に使う（簡易版）
var message = await openaiService.GetResponseAsync(history);
```

**長所:**
- **型安全性:** 各プロバイダー専用インターフェース
- **プロバイダー固有情報にアクセス可能:** `OpenAiChatResponse` を直接取得
- **共通性も確保:** 拡張メソッドで `ChatHistory` → `ChatMessage` の変換を提供
- **柔軟性:** 複数プロバイダー同時利用が容易
- **明示的:** ユーザーはどのプロバイダーを使っているか常に認識

**短所:**
- インターフェースが増える（各プロバイダー × 機能数）
- 「プロバイダー非依存」の抽象化は提供しない

**推奨: 案4（専用インターフェース）**

**理由:**
- 新仕様の思想（完全独立実装）と完全に一致
- 無理な抽象化を避ける
- 型安全性を最大化
- 共通的な使い方は拡張メソッドで提供

**決定が必要:** Phase 1実装開始前に確定

---

### 1.5. サブセット仕様書の粒度と更新戦略

**現状:** 概念のみで、具体的なフォーマットや運用方法が不明

**必要な決定事項:**

#### (a) 記述の詳細度

**Option 1: 最小限（フィールドリストのみ）**
```markdown
## Request Fields
- model (string, required)
- messages (array, required)
- temperature (number, optional)
```

**Option 2: 詳細（型、制約、デフォルト値）**
```markdown
## Request Fields
| Field | Type | Required | Range | Default | Description |
|-------|------|----------|-------|---------|-------------|
| model | string | ✅ | - | - | Model name (e.g. "gpt-4") |
| temperature | number | ❌ | 0.0-2.0 | 1.0 | Sampling temperature |
```

**推奨: Option 2（詳細版）**
- AIエージェントが正確なDTOを生成するために必要

#### (b) バージョン管理

**問題:** API仕様が更新された場合、どう追跡するか？

**Option 1: Gitコミット履歴**
- サブセット仕様書の変更をコミットメッセージで記録

**Option 2: 明示的なバージョン番号**
```markdown
# OpenAI Chat Completions API - サブセット仕様

**API Version:** 2024-11-01
**Subset Version:** 1.2.0
**Last Updated:** 2025-11-12
**Changes:**
- v1.2.0: Added `response_format` field
- v1.1.0: Added `system_fingerprint` in response
```

**推奨: Option 2（明示的バージョン）**
- 変更履歴が明確
- どの時点の仕様を実装したか追跡可能

#### (c) 更新頻度とトリガー

**提案:**
1. **定期チェック:** 月に1回、公式仕様書を確認
2. **イベントベース:** プロバイダーがメジャー更新を発表した時
3. **ユーザー報告:** 「このフィールドが足りない」という報告があった時

#### (d) 仕様書の配置

**推奨:**
```
/docs
  /api-specs
    /openai
      - chat-completions.md
      - embeddings.md
    /gemini
      - generate-content.md
    /anthropic
      - messages.md
```

**決定が必要:** Phase 1開始前に詳細フォーマット確定

---

### 1.6. エラーレスポンスの扱い

**問題:** 各プロバイダーのエラーレスポンス形式が異なる

**OpenAI エラーレスポンス例:**
```json
{
  "error": {
    "message": "Invalid API key",
    "type": "invalid_request_error",
    "code": "invalid_api_key"
  }
}
```

**Gemini エラーレスポンス例:**
```json
{
  "error": {
    "code": 400,
    "message": "API key not valid",
    "status": "INVALID_ARGUMENT"
  }
}
```

**現行仕様の問題:**
- エラーDTOの定義がない
- エラーハンドリングが `InvalidOperationException` のみ

**改善提案:**

#### (a) エラーDTO の定義

```csharp
// OpenAI
internal sealed class OpenAiErrorResponseDto
{
    [JsonPropertyName("error")]
    public OpenAiErrorDto? Error { get; init; }
}

internal sealed class OpenAiErrorDto
{
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }
}
```

#### (b) Repositoryでのエラー処理

```csharp
if (!httpResponse.IsSuccessStatusCode)
{
    var errorDto = JsonSerializer.Deserialize<OpenAiErrorResponseDto>(responseJson);

    var errorMessage = errorDto?.Error?.Message ?? "Unknown error";
    var errorType = errorDto?.Error?.Type ?? "unknown";
    var errorCode = errorDto?.Error?.Code;

    _logger.LogError(
        "OpenAI API error: {Message} (Type: {Type}, Code: {Code})",
        errorMessage, errorType, errorCode);

    throw new InvalidOperationException(
        $"OpenAI API request failed: {errorMessage}");
}
```

#### (c) カスタム例外の必要性再検討

**レート制限エラーの特別扱い:**
```csharp
if (errorDto?.Error?.Type == "rate_limit_exceeded")
{
    // Retry-After ヘッダーを確認
    httpResponse.Headers.TryGetValues("Retry-After", out var retryAfterValues);
    var retryAfter = retryAfterValues?.FirstOrDefault();

    throw new AiRateLimitException(
        "OpenAI",
        "Chat",
        retryAfter != null ? TimeSpan.FromSeconds(int.Parse(retryAfter)) : null,
        errorMessage);
}
```

**決定が必要:** Phase 1でエラーハンドリング実装時に詳細確定

---

### 1.7. 設定の優先順位と解決ロジック

**現行仕様:**
```
1. 機能固有（ChatApiKey, ChatEndpoint, ChatModel）
2. デフォルト（ApiKey, BaseUrl, DefaultModel）
3. ハードコードフォールバック
```

**問題:**
- 環境変数はどこに入る？
- 複数設定ソースの優先順位は？

**ASP.NET Core標準の設定優先順位:**
```
1. コマンドライン引数
2. 環境変数
3. appsettings.{Environment}.json
4. appsettings.json
5. User Secrets (開発時)
```

**提案: 設定解決の明確化**

```csharp
public OpenAiChatRepository(
    IHttpClientFactory httpClientFactory,
    ILogger<OpenAiChatRepository> logger,
    IOptions<OpenAiConfiguration> configuration,
    IConfiguration rootConfiguration)  // ← 追加
{
    var config = configuration.Value;

    // API キーの解決
    var apiKey =
        config.ChatApiKey ??  // 1. 機能固有
        config.ApiKey ??      // 2. デフォルト
        rootConfiguration["OPENAI_API_KEY"] ??  // 3. 環境変数
        throw new InvalidOperationException("OpenAI API key is not configured.");

    // エンドポイントの解決
    var endpoint =
        config.ChatEndpoint ??  // 1. 機能固有
        (config.BaseUrl != null ? $"{config.BaseUrl}/v1/chat/completions" : null) ??  // 2. ベースURL + パス
        rootConfiguration["OPENAI_CHAT_ENDPOINT"] ??  // 3. 環境変数
        "https://api.openai.com/v1/chat/completions";  // 4. ハードコード

    // モデル名の解決
    _defaultModel =
        config.ChatModel ??  // 1. 機能固有
        config.DefaultModel ??  // 2. デフォルト
        rootConfiguration["OPENAI_CHAT_MODEL"] ??  // 3. 環境変数
        "gpt-4";  // 4. ハードコード
}
```

**決定が必要:** 環境変数のサポート範囲（APIキーのみ？全て？）

---

## 2. 設計上の潜在的問題

### 2.1. コード重複の増加

**問題:**
- 6プロバイダー × 3機能 = 18個の独立実装
- DTO定義、Mapper、Repository、Serviceがそれぞれ重複

**具体例:**
- `OpenAiChatMapper.MapRole()` と `GeminiChatMapper.MapRole()` はほぼ同じコード
- HTTP呼び出しロジックも類似

**影響:**
- 保守コストの増加
- バグ修正が漏れるリスク
- リファクタリングの困難

**緩和策:**

#### (a) 共通ユーティリティクラス

```csharp
namespace Nekote.Core.AI.Infrastructure.Common;

/// <summary>
/// ChatRoleの文字列変換ユーティリティ。
/// </summary>
internal static class ChatRoleMapper
{
    public static string ToStandardString(ChatRole role)
    {
        return role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            _ => throw new ArgumentException($"Unknown role: {role}")
        };
    }
}
```

各Mapperから呼び出し:
```csharp
Messages = request.Messages
    .Select(m => new OpenAiMessageDto
    {
        Role = ChatRoleMapper.ToStandardString(m.Role),
        Content = m.Content
    })
    .ToList()
```

#### (b) HTTPクライアントヘルパー

```csharp
internal static class HttpClientHelper
{
    public static async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        string endpoint,
        TRequest request,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        // 共通のシリアライズ、POST、ログ、デシリアライズロジック
    }
}
```

#### (c) 抽象基底Repositoryクラス（検討中）

```csharp
internal abstract class HttpApiRepositoryBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ILogger Logger;

    protected async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
        TRequest request,
        string endpoint,
        CancellationToken cancellationToken)
    {
        // 共通ロジック
    }
}

internal sealed class OpenAiChatRepository : HttpApiRepositoryBase
{
    // プロバイダー固有ロジックのみ実装
}
```

**懸念:**
- 抽象基底クラスは結合度を高める
- 各プロバイダーの微妙な違い（認証方式、エラー形式等）への対応が困難

**推奨: (a)と(b)の組み合わせ**
- 完全な独立性を保ちつつ、明らかに共通のロジックのみヘルパー化

**決定が必要:** Phase 2（Gemini実装時）に重複コードが見えてから判断

---

### 2.2. インターフェースの増加による複雑性

**問題:**
- 専用インターフェース推奨案（1.4）を採用すると:
  - 6プロバイダー × 3機能 = 18個のインターフェース
  - `IOpenAiChatService`, `IGeminiChatService`, `IAnthropicChatService` ...
  - `IOpenAiEmbeddingService`, `IGeminiEmbeddingService` ...

**影響:**
- ユーザーが覚えるインターフェースが多い
- IDEの自動補完が煩雑
- 「どれを使えばいいの？」という迷い

**緩和策:**

#### (a) 名前空間による整理

```csharp
namespace Nekote.Core.AI.Services.OpenAI
{
    public interface IChatService { }
    public interface IEmbeddingService { }
}

namespace Nekote.Core.AI.Services.Gemini
{
    public interface IChatService { }
    public interface IEmbeddingService { }
}

// 使用時
using OpenAI = Nekote.Core.AI.Services.OpenAI;
using Gemini = Nekote.Core.AI.Services.Gemini;

var openaiChat = serviceProvider.GetRequiredService<OpenAI.IChatService>();
var geminiChat = serviceProvider.GetRequiredService<Gemini.IChatService>();
```

**長所:**
- インターフェース名がシンプル
- using aliasで明確に区別

**短所:**
- 名前空間の衝突に注意が必要

#### (b) プレフィックス付きインターフェース（現行案）

```csharp
public interface IOpenAiChatService { }
public interface IGeminiChatService { }
```

**長所:**
- 完全に一意
- 名前から用途が明確

**短所:**
- 名前が長い

**推奨: (b) プレフィックス付き**
- C#の慣習に従う
- 明示性を重視

---

### 2.3. テストの複雑性とコスト

**問題:**
- 18個の独立実装 → 18セットのテストが必要
- HTTPモックの設定が煩雑
- 実API呼び出しテストのコスト

**緩和策:**

#### (a) テストヘルパーの共通化

```csharp
public static class TestHelpers
{
    public static ChatHistory CreateSampleHistory()
    {
        var history = new ChatHistory();
        history.AddMessage(ChatRole.System, "You are a helpful assistant.");
        history.AddMessage(ChatRole.User, "Hello!");
        return history;
    }

    public static Mock<HttpMessageHandler> CreateMockHttpHandler(
        string responseJson,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        // HTTPモックの設定
    }
}
```

#### (b) 共通テストケースの定義

```csharp
public abstract class ChatServiceTestBase<TService, TRequest, TResponse>
    where TService : class
{
    [Fact]
    public async Task GetResponseAsync_WithValidRequest_ReturnsResponse()
    {
        // 各プロバイダーで共通のテストロジック
    }

    protected abstract TService CreateService();
    protected abstract TRequest CreateValidRequest();
}

public class OpenAiChatServiceTests : ChatServiceTestBase<IOpenAiChatService, OpenAiChatRequest, OpenAiChatResponse>
{
    protected override IOpenAiChatService CreateService()
    {
        // OpenAI固有のセットアップ
    }
}
```

#### (c) 実APIテストの戦略

**提案:**
1. **単体テスト:** 100％モックHTTP（高速、コストなし）
2. **統合テスト（Smoke Test）:** 週1回、全プロバイダーの基本APIを実際に呼び出し
3. **E2Eテスト:** 手動実行（新機能追加時のみ）

**決定が必要:** CI/CDパイプラインでの実APIテスト頻度

---

### 2.4. 生データアクセスのパフォーマンス懸念

**問題:**
- 全レスポンスでRawJsonを保持する場合、メモリ消費が増加
- JsonDocumentのパースコスト

**影響分析:**

**シナリオ1: 生データアクセス不使用（大多数のユーザー）**
```csharp
var response = await service.GetResponseAsync(request);
var content = response.Content;  // 生データアクセスなし
```
- RawJsonは保持されているがアクセスされない
- メモリの無駄

**シナリオ2: 生データアクセス使用**
```csharp
var response = await service.GetResponseAsync(request);
var content = response.Content;
var accessor = response.CreateAccessor();  // この時点でJsonDocumentパース
var customField = accessor.GetValue<string>("custom_field");
```
- 明示的にオプトインしているのでコスト許容

**最適化案:**

#### Option 1: デフォルトでRawJson無効

```csharp
public async Task<OpenAiChatResponse> GetResponseAsync(
    OpenAiChatRequest request,
    bool includeRawJson = false,  // デフォルトfalse
    CancellationToken ct = default)
{
    // ...
    return new OpenAiChatResponse
    {
        Content = dto.Content,
        RawJson = includeRawJson ? responseJson : null  // 条件付き保持
    };
}
```

**長所:**
- パフォーマンス影響なし（デフォルト）
- 必要な時だけオプトイン

**短所:**
- APIが複雑化

#### Option 2: Lazy初期化

```csharp
public sealed class OpenAiChatResponse
{
    private readonly Lazy<OpenAiChatResponseAccessor?> _accessor;

    internal OpenAiChatResponse(string? rawJson)
    {
        _accessor = new Lazy<OpenAiChatResponseAccessor?>(
            () => rawJson != null ? new OpenAiChatResponseAccessor(rawJson) : null);
    }

    public OpenAiChatResponseAccessor CreateAccessor()
    {
        return _accessor.Value ?? throw new InvalidOperationException("Raw JSON not available.");
    }
}
```

**長所:**
- パースは実際にアクセスされた時のみ
- APIはシンプル

**短所:**
- RawJson文字列は常に保持（メモリ）

#### Option 3: WeakReference（高度）

```csharp
private readonly WeakReference<string> _rawJsonRef;
```

**長所:**
- GCがメモリ回収可能

**短所:**
- 複雑すぎる
- 再取得の仕組みが必要

**推奨: Option 2（Lazy初期化）**
- バランスが良い
- 実装がシンプル
- パフォーマンス影響は限定的（文字列保持のみ）

**決定が必要:** Phase 1実装時に実測して判断

---

### 2.5. 設定ファイルの肥大化

**問題:**
- 6プロバイダー × 複数エンドポイント → appsettings.jsonが巨大化

**例:**
```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-...",
      "BaseUrl": "https://api.openai.com",
      "ChatEndpoint": "...",
      "EmbeddingEndpoint": "...",
      "DefaultModel": "gpt-4",
      "ChatModel": "gpt-4-turbo",
      "EmbeddingModel": "text-embedding-3-small"
    },
    "Gemini": { /* ... */ },
    "Anthropic": { /* ... */ },
    "XAi": { /* ... */ },
    "Mistral": { /* ... */ },
    "DeepSeek": { /* ... */ }
  }
}
```

**影響:**
- 可読性の低下
- 保守の困難

**改善策:**

#### (a) 環境別ファイル分割

```
appsettings.json          // デフォルト
appsettings.Development.json  // 開発環境
appsettings.Production.json   // 本番環境
appsettings.AI.json       // AI設定専用（オプション）
```

#### (b) User Secrets（開発時）

```bash
dotnet user-secrets set "AI:OpenAI:ApiKey" "sk-..."
```

**長所:**
- APIキーをGitにコミットしない
- 開発者ごとに異なるキー

#### (c) 環境変数（本番）

```bash
export AI__OpenAI__ApiKey="sk-..."
export AI__Gemini__ApiKey="AIza..."
```

**推奨: すべて併用**
- 開発: User Secrets
- 本番: 環境変数
- デフォルト: appsettings.json（キーなし）

---

## 3. 実装リスク

### 3.1. APIキーの誤った露出

**リスク:**
- ログにAPIキーが出力される
- エラーメッセージに含まれる
- 診断データに記録される

**対策:**

#### (a) ログ出力時のマスキング

```csharp
_logger.LogDebug(
    "Sending request to OpenAI API with key: {ApiKey}",
    MaskApiKey(_apiKey));

private static string MaskApiKey(string apiKey)
{
    if (apiKey.Length <= 8)
        return "***";

    return $"{apiKey[..4]}...{apiKey[^4..]}";
}
```

#### (b) HTTPヘッダーのログ除外

```csharp
// Authorization ヘッダーはログに含めない
var headers = httpRequest.Headers
    .Where(h => h.Key != "Authorization")
    .ToDictionary(h => h.Key, h => h.Value);

_logger.LogDebug("Request headers: {Headers}", headers);
```

#### (c) 設定クラスのToString()オーバーライド

```csharp
public sealed class OpenAiConfiguration
{
    public string? ApiKey { get; init; }

    public override string ToString()
    {
        return $"OpenAiConfiguration {{ ApiKey: {(ApiKey != null ? "***" : "null")} }}";
    }
}
```

**決定済み:** 全Repository実装時にマスキング徹底

---

### 3.2. レート制限への対処不足

**問題:**
- 各プロバイダーにレート制限がある
- 現行仕様では対処メカニズムがない

**影響:**
- 頻繁なAPI呼び出しで制限に達する
- リトライなしでエラー

**段階的対応:**

#### Phase 1: エラーとして報告（最小限）

```csharp
if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
{
    var retryAfter = httpResponse.Headers.RetryAfter?.Delta;

    throw new InvalidOperationException(
        $"Rate limit exceeded. Retry after {retryAfter?.TotalSeconds ?? 0} seconds.");
}
```

#### Phase 7: Pollyによる自動リトライ（高度）

```csharp
services.AddHttpClient("Nekote.AI.OpenAI.Chat")
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
        .WaitAndRetryAsync(
            3,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                // ログ出力
            }));
```

**決定:** Phase 1では単純なエラースロー、Phase 7で自動リトライ実装

---

### 3.3. 非同期処理のデッドロック

**問題:**
- `.ConfigureAwait(false)` の漏れ
- UIスレッドでの同期的待機

**対策:**

#### (a) 全awaitに`.ConfigureAwait(false)`

```csharp
var response = await _httpClient.PostAsync(endpoint, content, ct)
    .ConfigureAwait(false);

var json = await response.Content.ReadAsStringAsync(ct)
    .ConfigureAwait(false);
```

#### (b) Analyzerによる強制

**.editorconfig:**
```ini
[*.cs]
dotnet_diagnostic.CA2007.severity = error  # ConfigureAwait must be used
```

#### (c) レビューチェックリスト

- [ ] 全ての`await`に`.ConfigureAwait(false)`があるか
- [ ] 非同期メソッドが`CancellationToken`を受け入れているか

**決定済み:** 全非同期メソッドで徹底

---

### 3.4. DTOバージョン不整合

**問題:**
- プロバイダーがAPI仕様を変更
- Nekote.CoreのDTOが古いまま
- 新しいフィールドが無視される

**検出メカニズム:**

#### (a) JsonExtensionData（警告用）

```csharp
internal sealed class OpenAiChatResponseDto
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    // ... 他のプロパティ ...

    // 未知のフィールドをキャッチ
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }
}

// Mapperで警告
if (dto.ExtensionData?.Count > 0)
{
    _logger.LogWarning(
        "OpenAI response contains unknown fields: {Fields}. Nekote.Core may be outdated.",
        string.Join(", ", dto.ExtensionData.Keys));
}
```

**長所:**
- 新フィールドの存在を検出
- ログで警告

**短所:**
- ユーザーが気づかない可能性

#### (b) サブセット仕様書との定期比較

**プロセス:**
1. 月1回、公式仕様書をチェック
2. サブセット仕様書を更新
3. AIエージェントにDTOとの差分を確認させる
4. 差分があればIssue作成

**決定:** Phase 1でJsonExtensionData実装、定期チェックは運用フェーズ

---

## 4. 実装優先度の再検討

### 4.1. 現行の優先順位

```
Phase 1: OpenAI Chat
Phase 2: Gemini Chat
Phase 3: OpenAI Embedding
Phase 4-6: 残りプロバイダー
Phase 7: 高度機能
```

**問題点:**
- Embedding（Phase 3）の優先度が低い
- しかしRAG実装には必須
- Phase 3を待つとRAG検証が遅れる

**代替案:**

#### Option A: 機能優先（推奨）

```
Phase 1: OpenAI Chat
Phase 2: OpenAI Embedding  ← 変更
Phase 3: RAG実装検証
Phase 4: Gemini Chat
Phase 5: Gemini Embedding
Phase 6-8: 残りプロバイダー
```

**理由:**
- RAGが主要ユースケース
- OpenAIで完全な機能セットを先に完成させる
- Geminiは後から追加でも影響なし

#### Option B: プロバイダー優先（現行）

```
Phase 1: OpenAI Chat
Phase 2: Gemini Chat
Phase 3: OpenAI Embedding
...
```

**理由:**
- プロバイダー切り替えのアーキテクチャ検証が早い
- パターン確立が優先

**推奨: Option A（機能優先）**
- 実用性を重視
- OpenAI単体で完結した機能提供

**決定が必要:** Phase 1完了前に確定

---

### 4.2. ストリーミングの優先度

**現行:** Phase 7（最後）

**再検討の理由:**
- ストリーミングは現代的なAI UIの標準
- ユーザー体験への影響が大きい
- OpenAI、Gemini両方でサポート必須

**代替案:**

#### Option 1: Phase 3に前倒し

```
Phase 1: OpenAI Chat (非ストリーミング)
Phase 2: OpenAI Embedding
Phase 3: OpenAI Chat (ストリーミング)  ← 追加
Phase 4: Gemini Chat (両方)
```

**理由:**
- 早期に完全なChat機能を提供
- Gemini実装時にパターン確立済み

#### Option 2: 現行維持（Phase 7）

**理由:**
- SSEパースが複雑
- 非ストリーミングでも機能的に十分
- 後で追加しても互換性は保てる

**推奨: Option 2（現行維持）**
- MVP（最小viable product）の原則
- 複雑性を後回し

---

## 5. ドキュメント・仕様書の改善提案

### 5.1. サブセット仕様書のテンプレート

**提案: 標準化されたフォーマット**

```markdown
# {Provider} {Feature} API - サブセット仕様

**API Version:** YYYY-MM-DD
**Subset Version:** X.Y.Z
**Last Updated:** YYYY-MM-DD
**Official Docs:** https://...

---

## 変更履歴

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-12 | Initial version |

---

## エンドポイント

**Method:** POST
**URL:** https://...
**Content-Type:** application/json

---

## 認証

**Type:** Bearer Token
**Header:** `Authorization: Bearer {API_KEY}`

---

## リクエスト仕様

### 必須フィールド

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| model | string | non-empty | Model identifier |

### オプションフィールド

| Field | Type | Default | Range | Description |
|-------|------|---------|-------|-------------|
| temperature | number | 1.0 | 0.0-2.0 | Sampling temperature |

### 例

```json
{
  "model": "gpt-4",
  "messages": [...]
}
```

---

## レスポンス仕様

### 成功レスポンス（200 OK）

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| id | string | No | Unique response ID |

### 例

```json
{
  "id": "chatcmpl-123",
  ...
}
```

---

## エラーレスポンス

### エラー形式

| Status Code | Error Type | Description |
|-------------|------------|-------------|
| 400 | invalid_request_error | Malformed request |
| 401 | authentication_error | Invalid API key |
| 429 | rate_limit_error | Too many requests |

### 例

```json
{
  "error": {
    "message": "Invalid API key",
    "type": "authentication_error"
  }
}
```

---

## 実装メモ

- [ ] DTO実装完了
- [ ] Mapper実装完了
- [ ] Repository実装完了
- [ ] テスト作成完了

---

## 参考リンク

- [Official API Reference](https://...)
- [Rate Limits](https://...)
- [Error Handling](https://...)
```

---

### 5.2. コードコメントの標準化

**問題:**
- Mapperでの検証ロジックに一貫性がない
- なぜそのチェックが必要かが不明

**提案: コメントテンプレート**

```csharp
/// <summary>
/// DTOをドメインモデルに変換します。
/// </summary>
/// <param name="dto">変換元のDTO。</param>
/// <returns>変換されたドメインモデル。</returns>
/// <exception cref="InvalidOperationException">
/// DTOが不正な場合（必須フィールドがnull、空配列等）。
/// </exception>
/// <remarks>
/// この検証は、API仕様の変更や不整合からドメインモデルを保護するために必須です。
/// 各チェックの理由:
/// - Choices配列: API仕様では必ず1つ以上存在することが保証されているため
/// - Message.Content: 空のレスポンスは無効なため
/// </remarks>
public static OpenAiChatResponse ToDomain(OpenAiChatResponseDto dto)
{
    // API仕様では choices は必須配列（要素数 >= 1）
    if (dto.Choices == null || dto.Choices.Count == 0)
    {
        throw new InvalidOperationException(
            "OpenAI response is invalid: 'choices' array is null or empty.");
    }

    // 以下同様...
}
```

---

### 5.3. 実装チェックリスト

**Phase完了時のチェックリスト:**

```markdown
## Phase 1: OpenAI Chat 完了チェックリスト

### ドメインモデル
- [ ] OpenAiChatRequest 実装
- [ ] OpenAiChatResponse 実装
- [ ] OpenAiTokenUsage 実装
- [ ] XMLコメント（日本語）完備

### DTO
- [ ] OpenAiChatRequestDto 実装
- [ ] OpenAiChatResponseDto 実装
- [ ] OpenAiChoiceDto 実装
- [ ] OpenAiMessageDto 実装
- [ ] OpenAiUsageDto 実装
- [ ] OpenAiErrorResponseDto 実装
- [ ] 全フィールドnullable
- [ ] JsonPropertyName属性完備

### Mapper
- [ ] ToDto 実装
- [ ] ToDomain 実装
- [ ] 防御的検証実装
- [ ] エラーメッセージ明確

### Repository
- [ ] コンストラクタ実装
- [ ] API呼び出し実装
- [ ] エラーハンドリング実装
- [ ] ロギング実装
- [ ] ConfigureAwait(false) 完備

### Service
- [ ] IOpenAiChatService インターフェース
- [ ] OpenAiChatService 実装
- [ ] 拡張メソッド（ChatHistory対応）

### DI
- [ ] AddOpenAiChat 拡張メソッド
- [ ] 設定バインド実装
- [ ] HttpClient登録

### 設定
- [ ] OpenAiConfiguration 実装
- [ ] appsettings.json サンプル
- [ ] 環境変数サポート

### テスト
- [ ] Mapper単体テスト
- [ ] Repository統合テスト（モックHTTP）
- [ ] Service単体テスト
- [ ] E2Eテスト（実API）

### ドキュメント
- [ ] サブセット仕様書作成
- [ ] README更新
- [ ] サンプルコード作成
```

---

## 6. 長期的な懸念事項

### 6.1. API仕様の大幅変更への対応

**シナリオ:**
- OpenAIがChat Completions APIを廃止し、新APIに移行
- 後方互換性なし

**影響:**
- Nekote.CoreのOpenAI実装が全面的に obsolete
- ユーザーコードが動作しなくなる

**対策:**

#### (a) バージョニング戦略

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.V1;  // 現行API
namespace Nekote.Core.AI.Infrastructure.OpenAI.V2;  // 新API

// 両方をサポート
services.AddOpenAiChatV1(...);
services.AddOpenAiChatV2(...);
```

#### (b) Obsolete属性による移行期間

```csharp
[Obsolete("Use IOpenAiChatServiceV2 instead. This version will be removed in Nekote.Core 3.0.")]
public interface IOpenAiChatService { }
```

#### (c) 移行ガイドの提供

```markdown
# Migration Guide: OpenAI Chat V1 → V2

## Breaking Changes
- `OpenAiChatRequest.Temperature` is now `Options.Temperature`
- Response format changed

## Migration Steps
1. Update service registration
2. Update request construction
3. Update response handling

## Code Examples
...
```

**決定:** 発生時に対応（事前準備不要）

---

### 6.2. Nekote.Coreのメジャーバージョンアップ

**問題:**
- 大規模なリファクタリングが必要になった場合
- 既存ユーザーへの影響

**提案: セマンティックバージョニング厳守**

```
1.0.0 - 初回リリース（Phase 1-6完了）
1.1.0 - 新機能追加（後方互換）
1.2.0 - 新プロバイダー追加（後方互換）
2.0.0 - 破壊的変更（アーキテクチャ刷新）
```

**破壊的変更の基準:**
- パブリックインターフェースの削除・変更
- ドメインモデルの大幅変更
- DI登録方法の変更

**非破壊的変更:**
- 新インターフェースの追加
- 新プロバイダーの追加
- 内部実装の改善

---

### 6.3. パフォーマンスのボトルネック

**潜在的問題:**
- 大量のAPI呼び出し
- メモリリーク
- スレッドプール枯渇

**監視項目:**
1. **レスポンス時間**
   - Repository単位
   - Service単位
   - E2E

2. **メモリ使用量**
   - RawJson保持による増加
   - HttpClient使用後のGC

3. **スレッド使用**
   - 非同期処理の適切性
   - デッドロック発生

**対策:**

#### (a) ベンチマークテスト

```csharp
[MemoryDiagnoser]
public class OpenAiChatBenchmark
{
    [Benchmark]
    public async Task SingleRequest()
    {
        // 1リクエストの性能測定
    }

    [Benchmark]
    public async Task ParallelRequests()
    {
        // 並行10リクエストの性能測定
    }
}
```

#### (b) プロファイリング

- Visual Studio Profiler
- dotMemory（JetBrains）
- PerfView

**決定:** Phase 3完了後にベンチマーク実施

---

## 7. 未解決の設計判断（要即時決定）

### 優先度: 高（Phase 1開始前に決定必須）

1. **「会話」の範囲**
   - ChatMessageのみ？ ChatOptions含む？
   - 推奨: ChatMessageのみ

2. **生データアクセス方式**
   - RawDataプロパティ？ 専用アクセサー？
   - 推奨: 専用アクセサークラス

3. **サービス層の設計**
   - 共通インターフェース？ 専用インターフェース？
   - 推奨: 専用インターフェース + 拡張メソッド

4. **サブセット仕様書フォーマット**
   - テンプレート確定
   - 推奨: 上記5.1のテンプレート使用

### 優先度: 中（Phase 1実装中に決定）

5. **エラーDTO実装**
   - 各プロバイダーで必須
   - Phase 1実装中に追加

6. **生データアクセスのパフォーマンス**
   - Lazy初期化で対応
   - 実測して判断

7. **カスタム例外の必要性**
   - Phase 1でエラーハンドリング実装後に判断

### 優先度: 低（Phase 2以降で決定）

8. **コード重複の緩和策**
   - Phase 2（Gemini実装時）に重複が見えてから判断

9. **実装優先順位**
   - 機能優先 vs プロバイダー優先
   - Phase 1完了後に判断

10. **ストリーミング優先度**
    - Phase 7のまま維持（推奨）

---

## 8. 次のアクション

### 即座に実施すべきこと

1. **未解決事項の決定**
   - 上記「優先度: 高」の4項目を確定
   - 決定内容を `01-current-specification.md` に反映

2. **OpenAI サブセット仕様書作成**
   - `/docs/api-specs/openai/chat-completions.md`
   - 5.1のテンプレート使用

3. **Phase 1実装開始**
   - Common.ChatMessage 実装
   - OpenAI.Domain実装
   - OpenAI.Infrastructure.Chat実装

### Phase 1完了後に実施すること

4. **アーキテクチャレビュー**
   - 設計の妥当性確認
   - パターンの確立確認

5. **Phase 2計画の最終化**
   - 実装優先順位の確定
   - スケジュール調整

---

*このドキュメントは実装過程で継続的に更新されます。新たな問題や改善案が発見された場合、随時追記してください。*
