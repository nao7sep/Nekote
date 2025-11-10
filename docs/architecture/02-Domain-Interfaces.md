# Domain Interfaces Architecture

**Purpose:** Service contracts with zero implementation
**Layer:** Domain
**Complexity:** ★★☆☆☆ (Simple - just interfaces)

---

## Overview

Domain interfaces define **contracts** for AI services. They:
- **Declare operations** the domain needs
- **Accept and return** domain models only (never DTOs)
- **Have no implementation** (that's Infrastructure's job)
- **Use async patterns** with `CancellationToken`

---

## IChatCompletionService

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
    /// <param name="options">オプション設定。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>AI からの応答を含む <see cref="ChatResponse"/>。</returns>
    Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ストリーミングモードでチャット補完を実行し、トークンを逐次的に取得します。
    /// </summary>
    /// <param name="messages">チャットメッセージのリスト。</param>
    /// <param name="options">オプション設定。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>トークンのストリーム。</returns>
    IAsyncEnumerable<string> GetCompletionStreamAsync(
        IReadOnlyList<ChatMessage> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}
```

---

## ITextEmbeddingService

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
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>エンベディングベクトルを含む <see cref="EmbeddingResult"/>。</returns>
    Task<EmbeddingResult> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のテキストを一括でベクトル表現に変換します。
    /// </summary>
    /// <param name="texts">エンベディング化するテキストのリスト。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>各テキストのエンベディングベクトルを含むリスト。</returns>
    Task<IReadOnlyList<EmbeddingResult>> GetEmbeddingsAsync(
        IReadOnlyList<string> texts,
        CancellationToken cancellationToken = default);
}
```

---

## ITranslationService

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
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>翻訳結果を含む <see cref="TranslationResult"/>。</returns>
    Task<TranslationResult> TranslateAsync(
        TranslationRequest request,
        CancellationToken cancellationToken = default);
}
```

---

## Design Notes

### Why No Diagnostics Parameter?

The original design had `IDiagnosticDataCollector? diagnosticCollector = null` as a parameter.

**Decision:** Remove it from interface signatures. Instead:
1. Diagnostics/tracking is **Infrastructure concern**
2. Use `ILogger<T>` (already injected into repositories)
3. If needed, use `Activity` from `System.Diagnostics` for distributed tracing
4. Application can optionally inject a tracker into repositories via constructor

**Rationale:**
- Domain interfaces should be **minimal** and **focused**
- Diagnostics is cross-cutting, not part of business contract
- Keeps method signatures clean for 99% of callers

---

## Implementation Checklist

- [ ] Create interface files in respective `/Domain/Chat`, `/Domain/Embedding`, `/Domain/Translation` folders
- [ ] Verify all methods return `Task` or `IAsyncEnumerable`
- [ ] Verify all methods accept `CancellationToken`
- [ ] Verify XML comments are in Japanese
- [ ] No implementation code (interfaces only)

---

**Estimated Time:** 15 minutes
**Dependencies:** Domain Models (01-Domain-Models.md)
**Next Step:** Configuration System (03-Configuration.md)
