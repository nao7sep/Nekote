# Domain Models Architecture

**Purpose:** Pure business models with zero dependencies
**Layer:** Domain
**Complexity:** ★☆☆☆☆ (Simplest - just POCOs)

---

## Overview

Domain models are **pure POCOs** that represent business concepts. They have:
- **No dependencies** on any external libraries (not even JSON serialization)
- **No attributes** (no `[JsonPropertyName]`, no `[Required]`)
- **Immutable properties** (use `init` accessors)
- **Required semantics** (use C# 11's `required` keyword)

---

## Chat Domain Models

### ChatMessage

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャットメッセージを表します。
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

    /// <summary>
    /// メッセージの名前 (オプション) を取得します。
    /// </summary>
    public string? Name { get; init; }
}
```

### ChatRole (Enum)

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャットメッセージの役割を表します。
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// システムメッセージ。
    /// </summary>
    System,

    /// <summary>
    /// ユーザーメッセージ。
    /// </summary>
    User,

    /// <summary>
    /// アシスタント (AI) メッセージ。
    /// </summary>
    Assistant
}
```

### ChatResponse

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// AI からのチャット応答を表します。
/// </summary>
public sealed class ChatResponse
{
    /// <summary>
    /// 生成されたメッセージの内容を取得します。
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// 応答の完了理由を取得します。
    /// </summary>
    public required string FinishReason { get; init; }

    /// <summary>
    /// 使用されたトークン数の情報を取得します。
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// プロバイダー固有の応答 ID を取得します。
    /// </summary>
    public string? ResponseId { get; init; }
}
```

### TokenUsage

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// トークン使用量の情報を表します。
/// </summary>
public sealed class TokenUsage
{
    /// <summary>
    /// プロンプトで使用されたトークン数を取得します。
    /// </summary>
    public required int PromptTokens { get; init; }

    /// <summary>
    /// 補完 (生成) で使用されたトークン数を取得します。
    /// </summary>
    public required int CompletionTokens { get; init; }

    /// <summary>
    /// 合計トークン数を取得します。
    /// </summary>
    public required int TotalTokens { get; init; }
}
```

### ChatCompletionOptions

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャット補完のオプション設定を表します。
/// </summary>
public sealed class ChatCompletionOptions
{
    /// <summary>
    /// サンプリング温度 (0.0 ～ 2.0) を取得します。
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// 生成する最大トークン数を取得します。
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-p サンプリング値を取得します。
    /// </summary>
    public float? TopP { get; init; }

    /// <summary>
    /// 使用するモデル名を取得します (オプション、未設定なら設定のデフォルトを使用)。
    /// </summary>
    public string? ModelName { get; init; }
}
```

---

## Embedding Domain Models

### EmbeddingResult

```csharp
namespace Nekote.Core.AI.Domain.Embedding;

/// <summary>
/// テキストエンベディングの結果を表します。
/// </summary>
public sealed class EmbeddingResult
{
    /// <summary>
    /// エンベディングベクトルを取得します。
    /// </summary>
    public required IReadOnlyList<float> Vector { get; init; }

    /// <summary>
    /// 元のテキストを取得します。
    /// </summary>
    public required string OriginalText { get; init; }

    /// <summary>
    /// 使用されたトークン数を取得します。
    /// </summary>
    public int? TokenCount { get; init; }
}
```

---

## Translation Domain Models

### TranslationRequest

```csharp
namespace Nekote.Core.AI.Domain.Translation;

/// <summary>
/// 翻訳リクエストを表します。
/// </summary>
public sealed class TranslationRequest
{
    /// <summary>
    /// 翻訳するテキストを取得します。
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// ソース言語コード (例: "en", "ja") を取得します。
    /// </summary>
    public string? SourceLanguage { get; init; }

    /// <summary>
    /// ターゲット言語コード (例: "en", "ja") を取得します。
    /// </summary>
    public required string TargetLanguage { get; init; }
}
```

### TranslationResult

```csharp
namespace Nekote.Core.AI.Domain.Translation;

/// <summary>
/// 翻訳結果を表します。
/// </summary>
public sealed class TranslationResult
{
    /// <summary>
    /// 翻訳されたテキストを取得します。
    /// </summary>
    public required string TranslatedText { get; init; }

    /// <summary>
    /// 検出されたソース言語コードを取得します。
    /// </summary>
    public string? DetectedSourceLanguage { get; init; }
}
```

---

## Implementation Checklist

- [ ] Create `/src/Nekote.Core/AI/Domain/Chat/` directory
- [ ] Create `/src/Nekote.Core/AI/Domain/Embedding/` directory
- [ ] Create `/src/Nekote.Core/AI/Domain/Translation/` directory
- [ ] Implement all models as pure POCOs
- [ ] Verify no external dependencies
- [ ] Verify XML comments are in Japanese
- [ ] Write unit tests for model instantiation

---

**Estimated Time:** 30 minutes
**Dependencies:** None
**Next Step:** Domain Interfaces (02-Domain-Interfaces.md)
