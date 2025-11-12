# Nekote.Core AI 機能実装戦略書（完全版）

**日付:** 2025-11-12
**バージョン:** 2.0
**目的:** 現実の AI API 環境における堅牢性、保守性、拡張性を実現する実装戦略の完全な定義

---

## 目次

1. [設計哲学：教科書的アプローチの失敗と現実的対案](#1-設計哲学教科書的アプローチの失敗と現実的対案)
2. [核心原則：二層抽象化モデル](#2-核心原則二層抽象化モデル)
3. [ディレクトリ構造とファイル配置](#3-ディレクトリ構造とファイル配置)
4. [第一層：普遍的契約（Universal Contract）](#4-第一層普遍的契約universal-contract)
5. [第二層：プロバイダー固有実装](#5-第二層プロバイダー固有実装)
6. [生データアクセス戦略](#6-生データアクセス戦略)
7. [仕様書管理とバージョン追跡](#7-仕様書管理とバージョン追跡)
8. [実装ロードマップ](#8-実装ロードマップ)
9. [サポート対象プロバイダー詳細](#9-サポート対象プロバイダー詳細)
10. [テスト戦略](#10-テスト戦略)
11. [AI アシスタント向けコード生成ガイドライン](#11-ai-アシスタント向けコード生成ガイドライン)

---

## 1. 設計哲学：教科書的アプローチの失敗と現実的対案

### 1.1. 「最大公約数モデル」の失敗

当初のアプローチは、全ての AI プロバイダーに共通する機能を抽出し、統一的なドメインモデル（例: `ChatCompletionOptions` に `Temperature`, `MaxTokens` などを含める）を作成するものだった。これは DDD や Clean Architecture の教科書的な設計である。

**しかし、実装過程で以下の致命的な欠陥が明らかになった：**

#### 問題 1: OpenAI バイアス

最も有名で先行している OpenAI の仕様に引きずられ、共通ドメインモデルが事実上「OpenAI のモデル」になる。例えば：

- `Temperature` は OpenAI では `0.0 ～ 2.0` だが、Gemini では異なる範囲を持つ可能性がある
- OpenAI の `finish_reason` の値（`stop`, `length`, `tool_calls` など）は他のプロバイダーでは異なる語彙を使用する
- Gemini の 200 万トークンのコンテキストウィンドウに関連する設定は、共通モデルに存在しない

#### 問題 2: 情報損失

共通モデルに収まらないデータが黙って捨てられる。AI アシスタントが DTO を生成する際、「共通モデルに対応するフィールドだけを実装すればよい」という誤った最適化を行い、以下のような重要データが失われる：

- トークン使用量の詳細（プロンプトキャッシュヒット数、推論トークン数など）
- プロバイダー固有のメタデータ（レスポンス ID、モデルバージョン、フィンガープリントなど）
- エラー情報の詳細（エラーコード、パラメーター名、サジェスションなど）

#### 問題 3: 保守性の悪化

一つのプロバイダーの API 変更（例: OpenAI が新しいパラメーターを追加）が、無関係な他のプロバイダー（例: Gemini）の実装にも影響を与える可能性がある。共通ドメインモデルを変更すると、全てのマッパーとリポジトリに波及する。

#### 問題 4: 不完全な DTO

AI アシスタントがドメインモデルを参照しながら DTO を設計した結果、**OpenAI 専用の DTO なのに、OpenAI のレスポンスに含まれる最も基本的なフィールドさえ受け止められない DTO** が生成された。これは抽象化の失敗の典型例である。

### 1.2. 現実的対案：専門化と高次抽象化

**新しい戦略は、以下の二つの核心原則に基づく：**

#### 原則 A: 普遍的なものは「会話」のみ

唯一、全ての AI プロバイダーに共通し、かつアプリケーションがプロバイダー間でやり取りしたいデータ構造は **「会話履歴」** である。

- ユーザーが「ここからは Gemini で続けて」と言ったとき、それまでの `ChatMessage[]` を新しいプロバイダーに渡せる必要がある
- この用途のために、`ChatMessage` と `ChatRole` のみを普遍的契約として定義する

#### 原則 B: それ以外は完全に独立

**設定、パラメーター、DTO、マッパー、リポジトリ** は、各プロバイダーごとに **完全に独立して実装** する。

- OpenAI の `Temperature` と Gemini の `Temperature` は、**名前が同じでも別物** として扱う
- コードの重複を恐れない。各プロバイダーの能力を 100% 引き出すことを優先する
- 共通化は、**サービスインターフェース（`IChatCompletionService` 等）** のレベルで行う

**この対案の利点：**

1. **情報損失ゼロ:** 各プロバイダーの API 仕様に 100% 忠実な DTO を作成できる
2. **保守性の向上:** OpenAI の変更は OpenAI のコードのみに影響する
3. **明確な責務:** マッパーは「プロバイダー固有の世界」と「アプリケーションの世界」の境界として機能する
4. **将来性:** 新しいプロバイダーの追加が既存コードに一切影響しない

---

## 2. 核心原則：二層抽象化モデル

この戦略は、**二つの異なるレベル** で抽象化を行う。

### 2.1. 第一層：普遍的契約（Universal Contract Layer）

**役割:** アプリケーションコードが依存する、最小限かつ普遍的な「契約」を定義する。

**含まれるもの：**

1. **`ChatMessage` 構造体**
   - プロパティ: `Role`, `Content`, `Name` （最小限）
   - 役割: プロバイダー間で会話履歴を転送するための標準フォーマット

2. **`ChatRole` 列挙型**
   - 値: `System`, `User`, `Assistant`（最小限の共通セット）
   - 役割: メッセージの送信者を識別する

3. **サービスインターフェース**
   - `IChatCompletionService`
   - `ITextEmbeddingService`
   - `ITranslationService`
   - 役割: アプリケーションが使用する「機能」の契約

**重要な設計決定:**

- このレイヤーには `Temperature`, `MaxTokens`, `ModelName` などのパラメーターは **含まない**
- これらは第二層（プロバイダー固有）で定義される
- このレイヤーのモデルは **純粋な POCO** であり、JSON シリアライゼーション属性を持たない

### 2.2. 第二層：プロバイダー固有実装（Specialized Implementation Layer）

**役割:** 各 AI プロバイダーの仕様に忠実かつ完全に従った実装を提供する。

**含まれるもの（各プロバイダーごとに独立して存在）：**

1. **Configuration（設定）**
   - 例: `OpenAiConfiguration`, `GeminiConfiguration`
   - 内容: API キー、ベース URL、デフォルトモデル名など
   - 注入: DI から `IOptions<T>` として

2. **Options（パラメーター）**
   - 例: `OpenAiChatOptions`, `GeminiChatOptions`
   - 内容: `Temperature`, `MaxTokens`, `TopP`, `ModelName` など
   - 渡し方: サービスメソッドの引数として

3. **DTOs（データ転送オブジェクト）**
   - 例: `OpenAiChatRequestDto`, `OpenAiChatResponseDto`
   - 役割: API の JSON と 1:1 対応
   - 特徴: `[JsonPropertyName]` 属性を持ち、全プロパティが nullable

4. **Mapper（変換・検証）**
   - 例: `OpenAiChatMapper`
   - 役割: DTO ↔ 普遍的契約（`ChatMessage` 等）の変換
   - 責務: 受信データの防御的検証（null チェック、必須フィールド確認）

5. **Repository（実装）**
   - 例: `OpenAiChatRepository`
   - 役割: `IChatCompletionService` の具象実装
   - 責務: HTTP 通信、エラーハンドリング、ログ出力

**設計原則:**

- プロバイダー間でのコード共有は **意図的に行わない**
- 各プロバイダーの実装は、他のプロバイダーの存在を知らない
- 「DRY 原則よりも明確さを優先する」（Don't Repeat Yourself より Don't Abstract Prematurely）

### 2.3. 抽象化のレベル比較

| レベル | 対象 | 抽象化の種類 | 例 |
|--------|------|--------------|-----|
| **アプリケーション** | ユーザーコード | サービスインターフェースに依存 | `IChatCompletionService chat = ...;` |
| **第一層** | 普遍的契約 | データ構造の統一 | `ChatMessage`, `ChatRole` |
| **第二層** | プロバイダー実装 | プロバイダー固有の詳細 | `OpenAiChatRepository`, `OpenAiChatOptions` |
| **外部 API** | HTTP エンドポイント | 各ベンダーの仕様 | `https://api.openai.com/v1/chat/completions` |

---

## 3. ディレクトリ構造とファイル配置

### 3.1. 全体構造

```
/src/Nekote.Core/AI
├─ /Domain                          ← 【第一層】普遍的契約
│  ├─ /Chat
│  │  ├─ IChatCompletionService.cs
│  │  ├─ ChatMessage.cs
│  │  ├─ ChatRole.cs
│  │  └─ ChatResponse.cs
│  ├─ /Embedding
│  │  ├─ ITextEmbeddingService.cs
│  │  └─ EmbeddingResult.cs
│  ├─ /Translation
│  │  ├─ ITranslationService.cs
│  │  └─ TranslationResult.cs
│  └─ /Exceptions
│     ├─ AiApiCallException.cs
│     └─ AiRateLimitException.cs
│
└─ /Infrastructure                  ← 【第二層】プロバイダー固有実装
   ├─ JsonDefaults.cs               ← 共通ユーティリティ（JSON シリアライゼーション設定）
   │
   ├─ /OpenAI                       ← OpenAI 専用ディレクトリ
   │  ├─ OpenAiConfiguration.cs
   │  ├─ /Chat
   │  │  ├─ OpenAiChatOptions.cs
   │  │  ├─ OpenAiChatRepository.cs
   │  │  ├─ OpenAiChatMapper.cs
   │  │  └─ /Dtos
   │  │     ├─ OpenAiChatRequestDto.cs
   │  │     ├─ OpenAiChatResponseDto.cs
   │  │     ├─ OpenAiMessageDto.cs
   │  │     ├─ OpenAiChoiceDto.cs
   │  │     └─ OpenAiUsageDto.cs
   │  ├─ /Embedding
   │  │  ├─ OpenAiEmbeddingOptions.cs
   │  │  ├─ OpenAiEmbeddingRepository.cs
   │  │  ├─ OpenAiEmbeddingMapper.cs
   │  │  └─ /Dtos
   │  │     ├─ OpenAiEmbeddingRequestDto.cs
   │  │     └─ OpenAiEmbeddingResponseDto.cs
   │  └─ /Specs                     ← OpenAI のサブセット仕様書
   │     ├─ chat-completions-api.md
   │     └─ embeddings-api.md
   │
   ├─ /Gemini                       ← Gemini 専用ディレクトリ
   │  ├─ GeminiConfiguration.cs
   │  ├─ /Chat
   │  │  ├─ GeminiChatOptions.cs
   │  │  ├─ GeminiChatRepository.cs
   │  │  ├─ GeminiChatMapper.cs
   │  │  └─ /Dtos
   │  │     └─ ... (Gemini 固有の DTO)
   │  ├─ /Embedding
   │  │  └─ ... (Gemini 固有の実装)
   │  └─ /Specs
   │     └─ ... (Gemini のサブセット仕様書)
   │
   ├─ /Anthropic                    ← Anthropic 専用ディレクトリ
   │  └─ ... (同様の構造)
   │
   ├─ /xAI                          ← xAI 専用ディレクトリ
   │  └─ ... (同様の構造)
   │
   ├─ /Mistral                      ← Mistral 専用ディレクトリ
   │  └─ ... (同様の構造)
   │
   ├─ /DeepSeek                     ← DeepSeek 専用ディレクトリ
   │  └─ ... (同様の構造)
   │
   ├─ /DeepL                        ← DeepL 専用ディレクトリ（翻訳のみ）
   │  ├─ DeepLConfiguration.cs
   │  ├─ /Translation
   │  │  └─ ... (翻訳機能のみ実装)
   │  └─ /Specs
   │     └─ translation-api.md
   │
   └─ /DependencyInjection          ← DI 登録用拡張メソッド
      ├─ OpenAiServiceCollectionExtensions.cs
      ├─ GeminiServiceCollectionExtensions.cs
      ├─ AnthropicServiceCollectionExtensions.cs
      ├─ XAiServiceCollectionExtensions.cs
      ├─ MistralServiceCollectionExtensions.cs
      ├─ DeepSeekServiceCollectionExtensions.cs
      └─ DeepLServiceCollectionExtensions.cs
```

### 3.2. 命名規則

#### プロバイダー名の規則

| プロバイダー | ディレクトリ名 | クラスプレフィックス | 理由 |
|--------------|----------------|----------------------|------|
| OpenAI | `/OpenAI` | `OpenAi*` | 会社名（製品: ChatGPT, DALL-E など） |
| Google | `/Gemini` | `Gemini*` | 製品名（"Google" は範囲が広すぎる） |
| Anthropic | `/Anthropic` | `Anthropic*` | 会社名（製品: Claude） |
| xAI | `/xAI` | `XAi*` | 会社名（"x" + "AI" → `XAi`） |
| Mistral | `/Mistral` | `Mistral*` | 会社名 |
| DeepSeek | `/DeepSeek` | `DeepSeek*` | 会社名 |
| DeepL | `/DeepL` | `DeepL*` | 会社名（翻訳専門） |

**重要な例外:**

- **Gemini のみ製品名を使用する理由:**
  - `GoogleConfiguration` は曖昧すぎる（Gmail? Maps? Cloud?）
  - "Google AI" は公式ブランドではない
  - "Gemini" は AI モデルファミリーの明確な名称

この理由を、関連ファイルにコメントとして記載すること：

```csharp
// 命名規則: 会社名（OpenAI, Anthropic, Mistral）を使用するのが原則だが、
// Google は製品範囲が広すぎるため例外的に「Gemini」（AI モデルファミリー名）を使用する。
```

#### ファイル名の規則

1. **Configuration:** `[Provider]Configuration.cs` （例: `OpenAiConfiguration.cs`）
2. **Options:** `[Provider][Feature]Options.cs` （例: `OpenAiChatOptions.cs`）
3. **Repository:** `[Provider][Feature]Repository.cs` （例: `OpenAiChatRepository.cs`）
4. **Mapper:** `[Provider][Feature]Mapper.cs` （例: `OpenAiChatMapper.cs`）
5. **DTOs:** `[Provider][Type]Dto.cs` （例: `OpenAiChatRequestDto.cs`）

### 3.3. 垂直スライス原則

**各プロバイダーのディレクトリは「垂直スライス」として機能する:**

- 一つのプロバイダーに関連する全てのファイルが、そのプロバイダーのディレクトリ配下に集約される
- 他のプロバイダーのコードを参照することは **禁止**
- プロバイダー間で共有されるコードは、`/Domain` か `/Infrastructure` の直下（例: `JsonDefaults.cs`）にのみ配置される

**利点:**

- プロバイダーの追加・削除が容易
- 一つのプロバイダーの変更が他に波及しない
- コードレビュー時に関連ファイルが物理的に近い

### 3.4. テストプロジェクトの構造

テストプロジェクトは、ソースプロジェクトの構造を **完全にミラーリング** する：

```
/tests/Nekote.Core.Tests/AI
├─ /Domain
│  └─ /Chat
│     ├─ ChatMessageTests.cs
│     └─ ChatRoleTests.cs
│
└─ /Infrastructure
   ├─ /OpenAI
   │  └─ /Chat
   │     ├─ OpenAiChatMapperTests.cs
   │     └─ OpenAiChatRepositoryTests.cs
   │
   └─ /Gemini
      └─ /Chat
         └─ ... (同様)
```

---

## 4. 第一層：普遍的契約（Universal Contract）

### 4.1. ChatMessage（会話メッセージ）

**ファイル:** `/src/Nekote.Core/AI/Domain/Chat/ChatMessage.cs`

**役割:** プロバイダー間で会話履歴を転送するための、最小限かつ普遍的なデータ構造。

**完全な実装:**

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャットメッセージを表します。
/// これはプロバイダー間で共有される唯一の普遍的なデータ構造です。
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
    /// メッセージ送信者の名前（オプション）を取得します。
    /// </summary>
    /// <remarks>
    /// 主に multi-user チャットや、特定のシステムプロンプトに名前を付ける場合に使用されます。
    /// </remarks>
    public string? Name { get; init; }
}
```

**設計上の重要な決定:**

1. **`sealed` クラス:** 継承を禁止し、意図しない拡張を防ぐ
2. **`required` プロパティ:** `Role` と `Content` は必須。`Name` はオプション
3. **`init` アクセサー:** イミュータブル（不変）なオブジェクトとして扱う
4. **属性なし:** `[JsonPropertyName]` などの属性は一切含まない（純粋な POCO）

**なぜこれだけなのか:**

- `Temperature`, `ModelName` などのパラメーターは会話履歴に含まれない
- プロバイダー固有のメタデータ（例: OpenAI の `function_call`）は、マッパーで処理される
- この構造体は「最小限の共通部分」のみを表現する

### 4.2. ChatRole（メッセージの役割）

**ファイル:** `/src/Nekote.Core/AI/Domain/Chat/ChatRole.cs`

**完全な実装:**

```csharp
namespace Nekote.Core.AI.Domain.Chat;

/// <summary>
/// チャットメッセージの役割を表します。
/// </summary>
public enum ChatRole
{
    /// <summary>
    /// システムメッセージ（AI の振る舞いを指示）。
    /// </summary>
    System,

    /// <summary>
    /// ユーザーメッセージ（人間からの入力）。
    /// </summary>
    User,

    /// <summary>
    /// アシスタントメッセージ（AI からの応答）。
    /// </summary>
    Assistant
}
```

**なぜこの三つだけなのか:**

- OpenAI, Gemini, Anthropic の全てが、最低限この三つの役割をサポートしている
- プロバイダー固有の追加役割（例: OpenAI の `tool`, `function`）は、マッパーでハンドリングされる
- マッパーは、普遍的な `ChatRole` をプロバイダー固有の文字列（例: `"user"`, `"assistant"`）にマッピングする責務を持つ

### 4.3. ChatResponse（AI の応答）

**ファイル:** `/src/Nekote.Core/AI/Domain/Chat/ChatResponse.cs`

**役割:** AI からの応答を、アプリケーションに返すためのドメインモデル。

**完全な実装:**

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
    /// 応答の完了理由を取得します（例: "stop", "length", "tool_calls"）。
    /// </summary>
    public required string FinishReason { get; init; }

    /// <summary>
    /// 使用されたトークン数の情報を取得します（利用可能な場合）。
    /// </summary>
    public TokenUsage? Usage { get; init; }

    /// <summary>
    /// プロバイダー固有の応答 ID を取得します（利用可能な場合）。
    /// </summary>
    public string? ResponseId { get; init; }

    /// <summary>
    /// モデルの第一級プロパティにマップされなかった、プロバイダー固有の生データを格納します。
    /// </summary>
    /// <remarks>
    /// このディクショナリには、以下が含まれます：
    /// - DTO の <c>JsonExtensionData</c> に含まれていた未知のフィールド
    /// - DTO には存在するが、このモデルの第一級プロパティにマップされなかったフィールド
    /// 例: <c>RawData["model"]</c>, <c>RawData["created"]</c>, <c>RawData["system_fingerprint"]</c>
    /// </remarks>
    public required IReadOnlyDictionary<string, object> RawData { get; init; }
}
```

**`RawData` の重要性:**

1. **情報損失の防止:** API が返す全てのデータにアクセス可能
2. **将来の拡張性:** 新しいフィールドが API に追加されても、`RawData` 経由でアクセスできる
3. **後方互換性:** 将来、頻出するキーが第一級プロパティに昇格しても、古いコードは `RawData` を使い続けられる

### 4.4. TokenUsage（トークン使用量）

**ファイル:** `/src/Nekote.Core/AI/Domain/Chat/TokenUsage.cs`

**完全な実装:**

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
    /// 補完（生成）で使用されたトークン数を取得します。
    /// </summary>
    public required int CompletionTokens { get; init; }

    /// <summary>
    /// 合計トークン数を取得します。
    /// </summary>
    public required int TotalTokens { get; init; }

    /// <summary>
    /// プロバイダー固有の詳細情報（キャッシュヒット数、推論トークン数など）を格納します。
    /// </summary>
    public IReadOnlyDictionary<string, object>? Details { get; init; }
}
```

**`Details` プロパティの用途:**

- OpenAI: `prompt_tokens_details` (キャッシュヒット数など)
- Anthropic: `cache_read_input_tokens`, `cache_creation_input_tokens`
- これらの詳細情報は、コスト計算やデバッグに有用

### 4.5. IChatCompletionService（チャット補完サービス）

**ファイル:** `/src/Nekote.Core/AI/Domain/Chat/IChatCompletionService.cs`

**役割:** アプリケーションが依存する「チャット機能」の契約。

**完全な実装:**

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
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>AI からの応答を含む <see cref="ChatResponse"/>。</returns>
    /// <remarks>
    /// プロバイダー固有のオプション（Temperature など）は、実装クラスの
    /// コンストラクターまたは専用のメソッドオーバーロードで渡されます。
    /// </remarks>
    Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default);
}
```

**重要な設計決定:**

1. **オプションパラメーターなし:** `Temperature`, `MaxTokens` などは、このインターフェースには含まれない。これらはプロバイダー固有のメソッドオーバーロードで処理される。
2. **最小限のシグネチャ:** 全てのプロバイダーが実装可能な、最小限の契約のみを定義する。
3. **ストリーミングは別インターフェース:** `IStreamingChatCompletionService` として分離する（実装フェーズ 2 以降）。

### 4.6. ITextEmbeddingService（エンベディングサービス）

**ファイル:** `/src/Nekote.Core/AI/Domain/Embedding/ITextEmbeddingService.cs`

**完全な実装:**

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

### 4.7. EmbeddingResult（エンベディング結果）

**ファイル:** `/src/Nekote.Core/AI/Domain/Embedding/EmbeddingResult.cs`

**完全な実装:**

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
    /// 使用されたトークン数を取得します（利用可能な場合）。
    /// </summary>
    public int? TokenCount { get; init; }

    /// <summary>
    /// プロバイダー固有の生データを格納します。
    /// </summary>
    public IReadOnlyDictionary<string, object>? RawData { get; init; }
}
```

### 4.8. ITranslationService（翻訳サービス）

**ファイル:** `/src/Nekote.Core/AI/Domain/Translation/ITranslationService.cs`

**完全な実装:**

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
    /// <param name="text">翻訳するテキスト。</param>
    /// <param name="targetLanguage">ターゲット言語コード（例: "ja", "en"）。</param>
    /// <param name="sourceLanguage">ソース言語コード（省略時は自動検出）。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>翻訳結果を含む <see cref="TranslationResult"/>。</returns>
    Task<TranslationResult> TranslateAsync(
        string text,
        string targetLanguage,
        string? sourceLanguage = null,
        CancellationToken cancellationToken = default);
}
```

### 4.9. TranslationResult（翻訳結果）

**ファイル:** `/src/Nekote.Core/AI/Domain/Translation/TranslationResult.cs`

**完全な実装:**

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
    /// 検出されたソース言語コードを取得します（自動検出の場合）。
    /// </summary>
    public string? DetectedSourceLanguage { get; init; }

    /// <summary>
    /// プロバイダー固有の生データを格納します。
    /// </summary>
    public IReadOnlyDictionary<string, object>? RawData { get; init; }
}
```

### 4.10. 例外クラス

**ファイル:** `/src/Nekote.Core/AI/Domain/Exceptions/AiApiCallException.cs`

```csharp
namespace Nekote.Core.AI.Domain.Exceptions;

/// <summary>
/// AI API 呼び出しが失敗した場合にスローされます。
/// </summary>
public class AiApiCallException : InvalidOperationException
{
    /// <summary>
    /// プロバイダー名を取得します。
    /// </summary>
    public string Provider { get; }

    /// <summary>
    /// HTTP ステータスコードを取得します（該当する場合）。
    /// </summary>
    public int? HttpStatusCode { get; }

    /// <summary>
    /// レスポンスボディを取得します（該当する場合）。
    /// </summary>
    public string? ResponseBody { get; }

    public AiApiCallException(
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

**ファイル:** `/src/Nekote.Core/AI/Domain/Exceptions/AiRateLimitException.cs`

```csharp
namespace Nekote.Core.AI.Domain.Exceptions;

/// <summary>
/// API レート制限に達した場合にスローされます。
/// </summary>
public sealed class AiRateLimitException : AiApiCallException
{
    /// <summary>
    /// リトライ可能になるまでの待機時間を取得します。
    /// </summary>
    public TimeSpan? RetryAfter { get; }

    public AiRateLimitException(
        string provider,
        string message,
        TimeSpan? retryAfter = null,
        string? responseBody = null)
        : base(provider, message, 429, responseBody)
    {
        RetryAfter = retryAfter;
    }
}
```

---

## 5. 第二層：プロバイダー固有実装

この層は、各 AI プロバイダーの API 仕様に **100% 忠実** な実装を提供する。

### 5.1. Configuration（設定クラス）

**役割:** アプリケーション起動時に設定され、リポジトリに DI 経由で注入される静的な設定値。

#### 5.1.1. 実装例：OpenAiConfiguration.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/OpenAiConfiguration.cs`

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
    /// チャット専用の API キーを取得します（オプション、未設定なら <see cref="ApiKey"/> を使用）。
    /// </summary>
    public string? ChatApiKey { get; init; }

    /// <summary>
    /// エンベディング専用の API キーを取得します（オプション、未設定なら <see cref="ApiKey"/> を使用）。
    /// </summary>
    public string? EmbeddingApiKey { get; init; }

    /// <summary>
    /// デフォルトのベース URL を取得します。
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// チャット専用のエンドポイントを取得します（オプション、未設定なら <see cref="BaseUrl"/> + "/v1/chat/completions"）。
    /// </summary>
    public string? ChatEndpoint { get; init; }

    /// <summary>
    /// エンベディング専用のエンドポイントを取得します（オプション、未設定なら <see cref="BaseUrl"/> + "/v1/embeddings"）。
    /// </summary>
    public string? EmbeddingEndpoint { get; init; }

    /// <summary>
    /// デフォルトのモデル名を取得します。
    /// </summary>
    public string? DefaultModelName { get; init; }

    /// <summary>
    /// チャット専用のモデル名を取得します（オプション、未設定なら <see cref="DefaultModelName"/> を使用）。
    /// </summary>
    public string? ChatModelName { get; init; }

    /// <summary>
    /// エンベディング専用のモデル名を取得します（オプション、未設定なら <see cref="DefaultModelName"/> を使用）。
    /// </summary>
    public string? EmbeddingModelName { get; init; }
}
```

#### 5.1.2. 設計ルール

1. **全プロパティ Nullable:** 柔軟なフォールバック戦略を可能にする
2. **機能ごとの分離:** `ChatApiKey`, `EmbeddingApiKey` のように、機能ごとに設定を分離
3. **フォールバックチェーン:** `ChatApiKey` → `ApiKey` → 例外スロー
4. **定数なし:** `public const string SectionName = "AI:OpenAI"` のような定数は含めない（DI 登録時にパスを指定）
5. **`sealed` クラス:** 継承を禁止

#### 5.1.3. appsettings.json の例

```json
{
  "AI": {
    "OpenAI": {
      "ApiKey": "sk-proj-...",
      "ChatModelName": "gpt-4-turbo",
      "EmbeddingModelName": "text-embedding-3-small"
    },
    "Gemini": {
      "ApiKey": "AIza...",
      "BaseUrl": "https://generativelanguage.googleapis.com",
      "ChatModelName": "gemini-2.5-pro"
    }
  }
}
```

### 5.2. Options（パラメータークラス）

**役割:** API 呼び出しごとに変わりうる動的な設定値（`Temperature`, `MaxTokens` など）。

#### 5.2.1. 実装例：OpenAiChatOptions.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Chat/OpenAiChatOptions.cs`

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

/// <summary>
/// OpenAI チャット補完のオプション設定を表します。
/// </summary>
public sealed class OpenAiChatOptions
{
    /// <summary>
    /// サンプリング温度（0.0 ～ 2.0）を取得します。
    /// </summary>
    public float? Temperature { get; init; }

    /// <summary>
    /// 生成する最大トークン数を取得します。
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Top-p サンプリング値（0.0 ～ 1.0）を取得します。
    /// </summary>
    public float? TopP { get; init; }

    /// <summary>
    /// 使用するモデル名を取得します（オプション、未設定なら Configuration の値を使用）。
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    /// 頻度ペナルティ（-2.0 ～ 2.0）を取得します。
    /// </summary>
    public float? FrequencyPenalty { get; init; }

    /// <summary>
    /// 存在ペナルティ（-2.0 ～ 2.0）を取得します。
    /// </summary>
    public float? PresencePenalty { get; init; }

    /// <summary>
    /// ストップシーケンスのリストを取得します。
    /// </summary>
    public IReadOnlyList<string>? StopSequences { get; init; }

    /// <summary>
    /// ユーザー ID を取得します（オプション、トラッキング用）。
    /// </summary>
    public string? UserId { get; init; }
}
```

#### 5.2.2. 設計ルール

1. **プロバイダー固有:** OpenAI の API 仕様に完全に従う（Gemini とは別クラス）
2. **全プロパティ Nullable:** 未設定の場合は API のデフォルト値を使用
3. **検証なし:** Options クラスは「ただのデータ保持者」であり、検証はマッパーまたはリポジトリで行う
4. **XML コメントに範囲を記載:** `Temperature (0.0 ～ 2.0)` のように、有効な範囲を明記

### 5.3. DTOs（Data Transfer Objects）

**役割:** API の JSON 構造と 1:1 で対応する、純粋なデータクラス。

#### 5.3.1. 実装例：OpenAiChatRequestDto.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Chat/Dtos/OpenAiChatRequestDto.cs`

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json.Serialization;

/// <summary>
/// OpenAI Chat Completions API のリクエスト DTO。
/// 全てのプロパティは nullable として定義され、API の不整合に対応します。
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

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("user")]
    public string? User { get; init; }

    /// <summary>
    /// 未知のフィールドをキャプチャするための拡張データ。
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
```

#### 5.3.2. 実装例：OpenAiChatResponseDto.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Chat/Dtos/OpenAiChatResponseDto.cs`

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

using System.Text.Json;
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

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; init; }

    /// <summary>
    /// 未知のフィールドをキャプチャするための拡張データ。
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
```

#### 5.3.3. DTO 設計の厳格なルール

1. **全プロパティ Nullable:** API の不整合（予期せぬ `null`）に備える
2. **`[JsonPropertyName]` 必須:** JSON の `snake_case` と C# の `PascalCase` を正確にマッピング
3. **`[JsonExtensionData]` 必須:** 全ての DTO に含める（未知のフィールドをキャプチャ）
4. **`internal` クラス:** DTO は外部に公開しない（マッパー経由でのみアクセス）
5. **`sealed` クラス:** 継承を禁止
6. **API 仕様に忠実:** OpenAI の公式ドキュメントに記載されている **全てのフィールド** を実装する（省略しない）
7. **属性のみ:** ビジネスロジックは一切含めない

#### 5.3.4. JsonExtensionData の型について

```csharp
[JsonExtensionData]
public Dictionary<string, JsonElement>? ExtensionData { get; set; }
```

- **型:** `Dictionary<string, JsonElement>` （`object` ではない）
- **理由:** `JsonElement` は型情報を保持し、後で適切な型に変換できる
- **Nullable:** API によっては `ExtensionData` が不要な場合もあるため、nullable にする

### 5.4. Mapper（変換・検証クラス）

**役割:** DTO とドメインモデルの変換、および受信データの防御的検証。

#### 5.4.1. 実装例：OpenAiChatMapper.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Chat/OpenAiChatMapper.cs`

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using System.Text.Json;
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
        OpenAiChatOptions? options,
        string modelName)
    {
        // Validate required parameters
        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                "Model name is required for OpenAI chat completion.",
                nameof(modelName));
        }

        if (messages == null || messages.Count == 0)
        {
            throw new ArgumentException(
                "Messages list cannot be null or empty.",
                nameof(messages));
        }

        // Convert domain messages to DTO messages
        var dtoMessages = messages.Select(msg => new OpenAiMessageDto
        {
            Role = MapRoleToDtoString(msg.Role),
            Content = msg.Content,
            Name = msg.Name
        }).ToList();

        return new OpenAiChatRequestDto
        {
            Model = modelName,
            Messages = dtoMessages,
            Temperature = options?.Temperature,
            MaxTokens = options?.MaxTokens,
            TopP = options?.TopP,
            FrequencyPenalty = options?.FrequencyPenalty,
            PresencePenalty = options?.PresencePenalty,
            Stop = options?.StopSequences?.ToList(),
            Stream = false,
            User = options?.UserId
        };
    }

    /// <summary>
    /// OpenAI API のレスポンス DTO をドメインモデルに変換します。
    /// </summary>
    public static ChatResponse ToDomainModel(OpenAiChatResponseDto dto)
    {
        // Defensive validation: API might return broken data
        if (dto.Choices == null || dto.Choices.Count == 0)
        {
            throw new InvalidOperationException(
                "OpenAI API returned a response with no choices. This indicates an API error or unexpected response format.");
        }

        var firstChoice = dto.Choices[0];

        if (firstChoice.Message == null)
        {
            throw new InvalidOperationException(
                "OpenAI API returned a choice with no message. This indicates an API error or unexpected response format.");
        }

        if (string.IsNullOrWhiteSpace(firstChoice.Message.Content))
        {
            throw new InvalidOperationException(
                "OpenAI API returned a message with no content. This indicates an API error or unexpected response format.");
        }

        // Build RawData dictionary
        var rawData = new Dictionary<string, object>();

        // Add DTO fields that are not mapped to first-class properties
        if (dto.Id != null) rawData["id"] = dto.Id;
        if (dto.Object != null) rawData["object"] = dto.Object;
        if (dto.Created.HasValue) rawData["created"] = dto.Created.Value;
        if (dto.Model != null) rawData["model"] = dto.Model;
        if (dto.SystemFingerprint != null) rawData["system_fingerprint"] = dto.SystemFingerprint;

        // Add ExtensionData (unknown fields)
        if (dto.ExtensionData != null)
        {
            foreach (var kvp in dto.ExtensionData)
            {
                rawData[kvp.Key] = kvp.Value;
            }
        }

        // Map Usage to domain model
        TokenUsage? usage = null;
        if (dto.Usage != null)
        {
            usage = new TokenUsage
            {
                PromptTokens = dto.Usage.PromptTokens ?? 0,
                CompletionTokens = dto.Usage.CompletionTokens ?? 0,
                TotalTokens = dto.Usage.TotalTokens ?? 0,
                Details = BuildUsageDetails(dto.Usage)
            };
        }

        return new ChatResponse
        {
            Content = firstChoice.Message.Content,
            FinishReason = firstChoice.FinishReason ?? "unknown",
            Usage = usage,
            ResponseId = dto.Id,
            RawData = rawData
        };
    }

    /// <summary>
    /// ドメインの <see cref="ChatRole"/> を OpenAI の文字列表現にマッピングします。
    /// </summary>
    private static string MapRoleToDtoString(ChatRole role)
    {
        return role switch
        {
            ChatRole.System => "system",
            ChatRole.User => "user",
            ChatRole.Assistant => "assistant",
            _ => throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                $"Unsupported ChatRole value: {role}")
        };
    }

    /// <summary>
    /// OpenAI の Usage DTO から詳細情報を抽出します。
    /// </summary>
    private static IReadOnlyDictionary<string, object>? BuildUsageDetails(OpenAiUsageDto usage)
    {
        var details = new Dictionary<string, object>();

        // Add prompt_tokens_details if available
        if (usage.PromptTokensDetails != null)
        {
            if (usage.PromptTokensDetails.CachedTokens.HasValue)
            {
                details["prompt_cached_tokens"] = usage.PromptTokensDetails.CachedTokens.Value;
            }
        }

        // Add completion_tokens_details if available
        if (usage.CompletionTokensDetails != null)
        {
            if (usage.CompletionTokensDetails.ReasoningTokens.HasValue)
            {
                details["reasoning_tokens"] = usage.CompletionTokensDetails.ReasoningTokens.Value;
            }
        }

        return details.Count > 0 ? details : null;
    }
}
```

#### 5.4.2. Mapper 設計の厳格なルール

1. **`static` クラス:** マッパーは状態を持たない
2. **防御的検証:** DTO のプロパティが `null` である可能性を常に考慮し、明確なエラーメッセージで例外をスローする
3. **`RawData` の構築:** DTO の全てのフィールド（第一級プロパティにマップされないもの）と `ExtensionData` を `RawData` ディクショナリに転送する
4. **Enum の検証:** `switch` 式で `_` (default case) を必ず含め、未定義値に対して例外をスローする
5. **コメントは日本語:** XML コメントは日本語、例外メッセージは英語

### 5.5. Repository（実装クラス）

**役割:** `IChatCompletionService` の具象実装。HTTP 通信、エラーハンドリング、ログ出力を担当。

#### 5.5.1. 実装例：OpenAiChatRepository.cs（簡略版）

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Chat/OpenAiChatRepository.cs`

```csharp
namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Domain.Exceptions;

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

        // Resolve API key with fallback chain
        var apiKey = _config.ChatApiKey ?? _config.ApiKey
            ?? throw new InvalidOperationException(
                "OpenAI API key is not configured. Provide either 'ApiKey' or 'ChatApiKey' in configuration.");

        // Resolve endpoint with fallback chain
        var endpoint = _config.ChatEndpoint
            ?? (_config.BaseUrl != null ? $"{_config.BaseUrl}/v1/chat/completions" : null)
            ?? "https://api.openai.com/v1/chat/completions";

        // Create named HttpClient
        _httpClient = httpClientFactory.CreateClient("OpenAI-Chat");
        _httpClient.BaseAddress = new Uri(endpoint);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Nekote.Core/1.0");
    }

    public async Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        CancellationToken cancellationToken = default)
    {
        return await GetCompletionAsync(messages, options: null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// OpenAI 固有のオプションを受け入れるメソッドオーバーロード。
    /// </summary>
    public async Task<ChatResponse> GetCompletionAsync(
        IReadOnlyList<ChatMessage> messages,
        OpenAiChatOptions? options,
        CancellationToken cancellationToken = default)
    {
        // Resolve model name with fallback chain
        var modelName = options?.ModelName ?? _config.ChatModelName ?? _config.DefaultModelName
            ?? throw new InvalidOperationException(
                "OpenAI model name is not configured. Provide either 'DefaultModelName', 'ChatModelName', or pass 'ModelName' in options.");

        // Convert domain model to DTO
        var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);

        // Serialize request
        var requestJson = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);
        _logger.LogDebug("Sending request to OpenAI: {Json}", requestJson);

        try
        {
            // Send HTTP request
            var response = await _httpClient.PostAsJsonAsync(
                "",
                requestDto,
                JsonDefaults.Options,
                cancellationToken)
                .ConfigureAwait(false);

            // Read response body
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            _logger.LogDebug("Received response from OpenAI: {Json}", responseJson);

            // Handle non-success status codes
            if (!response.IsSuccessStatusCode)
            {
                throw new AiApiCallException(
                    provider: "OpenAI",
                    message: $"OpenAI API returned status code {(int)response.StatusCode}: {response.ReasonPhrase}",
                    httpStatusCode: (int)response.StatusCode,
                    responseBody: responseJson);
            }

            // Deserialize response
            var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(
                responseJson,
                JsonDefaults.Options)
                ?? throw new InvalidOperationException("Failed to deserialize OpenAI response. Response body was null.");

            // Convert DTO to domain model
            return OpenAiChatMapper.ToDomainModel(responseDto);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request to OpenAI failed.");
            throw new AiApiCallException(
                provider: "OpenAI",
                message: "HTTP request to OpenAI failed. See inner exception for details.",
                innerException: ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize OpenAI response.");
            throw new InvalidOperationException(
                "Failed to deserialize OpenAI response. The API may have returned an unexpected format.",
                ex);
        }
    }
}
```

#### 5.5.2. Repository 設計の厳格なルール

1. **`sealed` クラス:** 継承を禁止
2. **`internal` アクセス修飾子:** 外部に公開しない（DI 経由でのみ使用）
3. **コンストラクター注入:** `IHttpClientFactory`, `ILogger<T>`, `IOptions<TConfig>` を注入
4. **フォールバックチェーン:** API キー、エンドポイント、モデル名の解決に優先順位を適用
5. **`ConfigureAwait(false)`:** 全ての `await` に必須
6. **例外の再スロー:** `HttpRequestException`, `JsonException` は適切にラップして再スロー
7. **ログ出力:** リクエストとレスポンスの JSON を `LogDebug` で出力（本番環境では無効化可能）
8. **プロバイダー固有オーバーロード:** インターフェースメソッドとは別に、`OpenAiChatOptions` を受け入れるオーバーロードを提供

### 5.6. DI 登録用拡張メソッド

**役割:** アプリケーションの `Startup.cs` または `Program.cs` から、簡単にサービスを登録できるようにする。

#### 5.6.1. 実装例：OpenAiServiceCollectionExtensions.cs

**ファイル:** `/src/Nekote.Core/AI/Infrastructure/DependencyInjection/OpenAiServiceCollectionExtensions.cs`

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
    /// OpenAI 設定が格納されているセクションパス（例: "AI:OpenAI"）。
    /// </param>
    public static IServiceCollection AddOpenAiChat(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "AI:OpenAI")
    {
        // Bind configuration
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(configurationSectionPath));

        // Register HttpClient for OpenAI Chat
        services.AddHttpClient("OpenAI-Chat");

        // Register repository as scoped service
        services.AddScoped<IChatCompletionService, OpenAiChatRepository>();

        return services;
    }

    /// <summary>
    /// OpenAI のエンベディングサービスを登録します。
    /// </summary>
    public static IServiceCollection AddOpenAiEmbedding(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "AI:OpenAI")
    {
        // Bind configuration (reuse same section as Chat)
        services.Configure<OpenAiConfiguration>(
            configuration.GetSection(configurationSectionPath));

        // Register HttpClient for OpenAI Embedding
        services.AddHttpClient("OpenAI-Embedding");

        // Register repository as scoped service
        services.AddScoped<ITextEmbeddingService, OpenAiEmbeddingRepository>();

        return services;
    }
}
```

#### 5.6.2. DI 拡張メソッドの設計ルール

1. **`public static` メソッド:** 外部から呼び出し可能
2. **`this IServiceCollection services`:** 拡張メソッドとして定義
3. **設定パスは引数:** ハードコードしない（デフォルト値を提供）
4. **`Scoped` ライフタイム:** リポジトリは `Scoped` で登録（HTTP リクエストごとに一つのインスタンス）
5. **名前付き HttpClient:** `AddHttpClient("OpenAI-Chat")` のように、機能ごとに名前を付ける

---

## 6. 生データアクセス戦略

### 6.1. 問題の定義

AI API は頻繁に変更される。新しいフィールドが追加されたり、既存のフィールドの意味が変わったりする。`Nekote.Core` が全ての変更に即座に対応することは不可能であり、ユーザーは「公式にサポートされていないフィールド」にアクセスする必要がある。

**要件:**

1. ドメインモデルに第一級プロパティとして存在しないデータにアクセスできること
2. 将来、そのデータが第一級プロパティに昇格しても、古いコードが壊れないこと（後方互換性）
3. API が返す「生の JSON」を必要に応じて取得できること

### 6.2. RawData ディクショナリ

**実装方法:**

全てのドメインモデル（`ChatResponse`, `EmbeddingResult`, `TranslationResult` など）に、`RawData` プロパティを含める。

```csharp
/// <summary>
/// モデルの第一級プロパティにマップされなかった、プロバイダー固有の生データを格納します。
/// </summary>
public required IReadOnlyDictionary<string, object> RawData { get; init; }
```

**マッパーでの実装:**

マッパーは、以下のデータを `RawData` に格納する責務を持つ：

1. DTO の `JsonExtensionData`（未知のフィールド）
2. DTO の既知のプロパティのうち、ドメインモデルの第一級プロパティにマップされないもの

**例（OpenAiChatMapper の一部）:**

```csharp
var rawData = new Dictionary<string, object>();

// Add DTO fields that are not mapped to first-class properties
if (dto.Id != null) rawData["id"] = dto.Id;
if (dto.Object != null) rawData["object"] = dto.Object;
if (dto.Created.HasValue) rawData["created"] = dto.Created.Value;
if (dto.Model != null) rawData["model"] = dto.Model;
if (dto.SystemFingerprint != null) rawData["system_fingerprint"] = dto.SystemFingerprint;

// Add ExtensionData (unknown fields)
if (dto.ExtensionData != null)
{
    foreach (var kvp in dto.ExtensionData)
    {
        rawData[kvp.Key] = kvp.Value;
    }
}

return new ChatResponse
{
    Content = firstChoice.Message.Content,
    FinishReason = firstChoice.FinishReason ?? "unknown",
    Usage = usage,
    ResponseId = dto.Id,
    RawData = rawData  // ← ここ
};
```

### 6.3. 使用例

**ユーザーコード（プロバイダー固有のフィールドにアクセス）:**

```csharp
var response = await chatService.GetCompletionAsync(messages);

// 第一級プロパティにアクセス
Console.WriteLine($"Content: {response.Content}");

// RawData からプロバイダー固有のフィールドにアクセス
if (response.RawData.TryGetValue("model", out var model))
{
    Console.WriteLine($"Model used: {model}");
}

if (response.RawData.TryGetValue("system_fingerprint", out var fingerprint))
{
    Console.WriteLine($"System fingerprint: {fingerprint}");
}
```

### 6.4. 後方互換性の保証

**シナリオ:**

1. **現在:** `system_fingerprint` は `RawData["system_fingerprint"]` 経由でアクセスされる
2. **将来:** `Nekote.Core` が更新され、`ChatResponse` に `public string? SystemFingerprint { get; init; }` プロパティが追加される
3. **要求:** 古いコード（`RawData` を使用）が壊れないこと

**実装:**

マッパーは、第一級プロパティに昇格したフィールドも、引き続き `RawData` に含める：

```csharp
// 将来のコード（SystemFingerprint が第一級プロパティになった後）
return new ChatResponse
{
    Content = firstChoice.Message.Content,
    FinishReason = firstChoice.FinishReason ?? "unknown",
    Usage = usage,
    ResponseId = dto.Id,
    SystemFingerprint = dto.SystemFingerprint,  // ← 新しい第一級プロパティ
    RawData = rawData  // ← system_fingerprint はここにも含まれる
};
```

これにより、古いコードが `RawData["system_fingerprint"]` を参照し続けても動作する。

### 6.5. JSON 文字列への直接アクセス

**要件:** デバッグやログ記録のため、API が返した「生の JSON」にアクセスしたい。

**実装方法:**

リポジトリは、レスポンス JSON を一度 `string` として読み取り、ログに出力する。ユーザーが必要な場合は、この JSON を返すメソッドを提供する。

**例（オプション機能）:**

```csharp
public sealed class OpenAiChatRepository : IChatCompletionService
{
    // 標準メソッド
    public async Task<ChatResponse> GetCompletionAsync(...) { ... }

    // デバッグ用メソッド（生 JSON を返す）
    public async Task<(ChatResponse Response, string RawJson)> GetCompletionWithRawJsonAsync(
        IReadOnlyList<ChatMessage> messages,
        OpenAiChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // ... (HTTP 呼び出し)

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(responseJson, JsonDefaults.Options);
        var domainModel = OpenAiChatMapper.ToDomainModel(responseDto);

        return (domainModel, responseJson);  // ← 生 JSON も返す
    }
}
```

**注意:** この機能はオプションであり、通常のユーザーには不要。高度なデバッグやトラブルシューティングにのみ使用される。

---

## 7. 仕様書管理とバージョン追跡

### 7.1. 問題の定義

AI API の公式ドキュメントは、以下の課題を持つ：

1. **巨大すぎる:** OpenAI のドキュメントは数百ページに及び、AI のコンテキストウィンドウを圧迫する
2. **ノイズが多い:** 無関係な API（音声認識、画像生成など）が混在している
3. **構造化されていない:** HTML や JavaScript で動的に生成され、機械可読性が低い

**対策:** 実装対象の機能に限定した「サブセット仕様書」を手動で作成し、管理する。

### 7.2. サブセット仕様書の構造

**ファイル配置:** `/src/Nekote.Core/AI/Infrastructure/[Provider]/Specs/[api-name].md`

**例:** `/src/Nekote.Core/AI/Infrastructure/OpenAI/Specs/chat-completions-api.md`

**内容:**

```markdown
# OpenAI Chat Completions API - Subset Specification

**Version:** 2025-11-12 (based on OpenAI API v1)
**Endpoint:** `POST https://api.openai.com/v1/chat/completions`
**Official Docs:** https://platform.openai.com/docs/api-reference/chat/create

---

## Request Body (JSON)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `model` | string | Yes | Model ID (e.g., "gpt-4-turbo") |
| `messages` | array | Yes | Array of message objects |
| `temperature` | number | No | Sampling temperature (0.0 ~ 2.0, default: 1.0) |
| `max_tokens` | integer | No | Maximum tokens to generate |
| `top_p` | number | No | Top-p sampling (0.0 ~ 1.0, default: 1.0) |
| `frequency_penalty` | number | No | Frequency penalty (-2.0 ~ 2.0, default: 0.0) |
| `presence_penalty` | number | No | Presence penalty (-2.0 ~ 2.0, default: 0.0) |
| `stop` | string or array | No | Stop sequences |
| `stream` | boolean | No | Enable streaming (default: false) |
| `user` | string | No | User ID for tracking |

### Message Object

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `role` | string | Yes | One of: "system", "user", "assistant" |
| `content` | string | Yes | Message content |
| `name` | string | No | Name of the sender |

---

## Response Body (JSON)

| Field | Type | Description |
|-------|------|-------------|
| `id` | string | Unique response ID (e.g., "chatcmpl-123") |
| `object` | string | Object type (always "chat.completion") |
| `created` | integer | Unix timestamp |
| `model` | string | Model used |
| `choices` | array | Array of choice objects |
| `usage` | object | Token usage information |
| `system_fingerprint` | string | System fingerprint (for reproducibility) |

### Choice Object

| Field | Type | Description |
|-------|------|-------------|
| `index` | integer | Choice index |
| `message` | object | Generated message |
| `finish_reason` | string | One of: "stop", "length", "tool_calls", "content_filter" |

### Usage Object

| Field | Type | Description |
|-------|------|-------------|
| `prompt_tokens` | integer | Tokens in prompt |
| `completion_tokens` | integer | Tokens in completion |
| `total_tokens` | integer | Total tokens used |
| `prompt_tokens_details` | object | Detailed prompt token info (optional) |
| `completion_tokens_details` | object | Detailed completion token info (optional) |

---

## Error Response

| Field | Type | Description |
|-------|------|-------------|
| `error` | object | Error object |
| `error.message` | string | Error message |
| `error.type` | string | Error type (e.g., "invalid_request_error") |
| `error.param` | string | Problematic parameter (if applicable) |
| `error.code` | string | Error code (if applicable) |
```

### 7.3. サブセット仕様書の利点

1. **コンテキスト効率:** 必要最小限の情報のみを AI に渡せる
2. **正確性の向上:** ノイズが減り、DTO とマッパーの生成精度が向上する
3. **バージョン管理:** ファイルの先頭に日付とバージョンを記載し、変更履歴を Git で追跡できる
4. **自己完結性:** ネットワーク接続なしでも、仕様を参照できる

### 7.4. 仕様書の更新戦略

**定期レビュー:**

1. プロバイダーの公式ドキュメントを 3 ヶ月ごとにチェック
2. 変更があれば、サブセット仕様書を更新
3. DTO とマッパーを AI に再生成させる

**AI を使った比較:**

1. 古いサブセット仕様書と、新しい公式ドキュメントから抜粋した部分を AI に渡す
2. AI に差分を列挙させる
3. 重要な変更のみをサブセット仕様書に反映

**Python SDK からの抽出:**

プロバイダーが公式 Python SDK を提供している場合、そこから DTO の構造を逆算することも有効：

```python
# 例: OpenAI Python SDK から型情報を抽出
from openai.types.chat import ChatCompletion

# この型を C# の DTO に変換
```

---

## 8. 実装ロードマップ

### 8.1. 全体戦略

**原則:** 一度に一つのプロバイダー、一つの機能に集中する。完全に動作するまで次に進まない。

**優先順位:**

1. **OpenAI Chat** （最も重要、かつドキュメントが充実）
2. **OpenAI Embedding** （RAG の基盤）
3. **Gemini Chat** （アーキテクチャの検証）
4. **他のプロバイダー** （需要に応じて追加）

### 8.2. フェーズ 1: 普遍的契約の実装

**目標:** 第一層（Domain レイヤー）を完成させる。

**タスク:**

1. `/src/Nekote.Core/AI/Domain/Chat/` ディレクトリを作成
2. `ChatMessage.cs`, `ChatRole.cs`, `ChatResponse.cs`, `TokenUsage.cs` を実装
3. `IChatCompletionService.cs` を実装
4. `/Domain/Embedding/` と `/Domain/Translation/` も同様に実装
5. `/Domain/Exceptions/` に `AiApiCallException.cs` と `AiRateLimitException.cs` を実装
6. コンパイルが通ることを確認

**成功基準:**

- [ ] 全てのドメインモデルとインターフェースがコンパイルエラーなしで存在する
- [ ] XML コメントが日本語で記述されている
- [ ] `RawData` プロパティが全てのドメインモデルに存在する

**所要時間:** 約 1 時間

### 8.3. フェーズ 2: OpenAI Chat の完全実装

**目標:** 最初の動作する AI 統合を完成させる。

**タスク:**

1. サブセット仕様書を作成: `/Infrastructure/OpenAI/Specs/chat-completions-api.md`
2. `OpenAiConfiguration.cs` を実装
3. `OpenAiChatOptions.cs` を実装
4. DTO を実装（`OpenAiChatRequestDto.cs`, `OpenAiChatResponseDto.cs`, `OpenAiMessageDto.cs`, `OpenAiChoiceDto.cs`, `OpenAiUsageDto.cs`）
   - **重要:** `JsonExtensionData` を全ての DTO に含める
5. `OpenAiChatMapper.cs` を実装
   - ドメインモデル ↔ DTO の変換
   - 防御的検証（null チェック）
   - `RawData` の構築
6. `OpenAiChatRepository.cs` を実装
   - HTTP 通信
   - エラーハンドリング
   - ログ出力
7. `OpenAiServiceCollectionExtensions.cs` を実装
8. `JsonDefaults.cs` を実装（`/Infrastructure` 直下）
9. `Nekote.Lab.Console` でエンドツーエンドテストを実行

**成功基準:**

- [ ] コンソールアプリから OpenAI API を呼び出し、応答が表示される
- [ ] エラーハンドリングが機能する（無効な API キーでテスト）
- [ ] ログに リクエスト/レスポンス JSON が出力される
- [ ] `RawData` に未知のフィールドが含まれている（手動で確認）

**所要時間:** 約 3～4 時間

### 8.4. フェーズ 3: 単体テストの追加

**目標:** マッパーとリポジトリのテストを作成し、品質を保証する。

**タスク:**

1. `/tests/Nekote.Core.Tests/AI/Infrastructure/OpenAI/Chat/` ディレクトリを作成
2. `OpenAiChatMapperTests.cs` を実装
   - 正常系: 有効な DTO → ドメインモデル変換
   - 異常系: null フィールドに対する例外スロー
   - `RawData` の正しい構築
3. `OpenAiChatRepositoryTests.cs` を実装
   - モック `HttpClient` を使用した統合テスト
   - エラーレスポンスのハンドリング

**成功基準:**

- [ ] 全てのテストがパスする
- [ ] コードカバレッジ 80% 以上

**所要時間:** 約 2 時間

### 8.5. フェーズ 4: OpenAI Embedding の実装

**目標:** 二つ目の機能を追加し、アーキテクチャの拡張性を証明する。

**タスク:**

1. サブセット仕様書を作成: `/Infrastructure/OpenAI/Specs/embeddings-api.md`
2. `OpenAiEmbeddingOptions.cs` を実装
3. DTO を実装（`OpenAiEmbeddingRequestDto.cs`, `OpenAiEmbeddingResponseDto.cs`）
4. `OpenAiEmbeddingMapper.cs` を実装
5. `OpenAiEmbeddingRepository.cs` を実装
6. `OpenAiServiceCollectionExtensions.cs` に `AddOpenAiEmbedding` メソッドを追加

**成功基準:**

- [ ] エンベディング API が正常に動作する
- [ ] ベクトルが正しく返される

**所要時間:** 約 2～3 時間

### 8.6. フェーズ 5: Gemini Chat の実装

**目標:** 二つ目のプロバイダーを追加し、アーキテクチャのプロバイダー非依存性を証明する。

**タスク:**

1. Gemini のサブセット仕様書を作成
2. OpenAI の実装をコピー＆ペーストし、Gemini の仕様に合わせて修正
3. DTO、マッパー、リポジトリを Gemini 用に調整
4. `GeminiServiceCollectionExtensions.cs` を実装

**成功基準:**

- [ ] Gemini API が正常に動作する
- [ ] OpenAI と Gemini の両方のサービスを同時に DI 登録できる
- [ ] 会話履歴（`ChatMessage[]`）を OpenAI から Gemini に引き継げる

**所要時間:** 約 2～3 時間

### 8.7. フェーズ 6 以降: 追加機能とプロバイダー

**優先順位（低 → 高）:**

1. **ストリーミング:** `IAsyncEnumerable<string>` を使用したストリーミングレスポンス
2. **キャッシング:** デコレーターパターンによるエンベディングのキャッシュ
3. **リトライポリシー:** Polly ライブラリを使用した自動リトライ
4. **残りのプロバイダー:** Anthropic, xAI, Mistral, DeepSeek, DeepL

---

## 9. サポート対象プロバイダー詳細

### 9.1. OpenAI

**特徴:**
- 業界標準、最も広く使用されている
- Chat, Embedding, Image Generation, Audio, Fine-tuning など幅広い API
- `gpt-4-turbo`, `gpt-4`, `gpt-3.5-turbo` などのモデル

**Nekote.Core でのサポート範囲:**
- ✅ Chat Completions API
- ✅ Embeddings API
- ❌ Image Generation（範囲外）
- ❌ Audio（範囲外）

**実装優先度:** 最高（フェーズ 2）

### 9.2. Gemini (Google)

**特徴:**
- 超大規模コンテキストウィンドウ（100 万 ～ 200 万トークン）
- ビッグドキュメント分析に最適
- `gemini-2.5-pro`, `gemini-2.0-flash` などのモデル

**Nekote.Core でのサポート範囲:**
- ✅ Chat (generateContent API)
- ✅ Embeddings
- ❌ Image/Video（範囲外）

**実装優先度:** 高（フェーズ 5）

### 9.3. Anthropic

**特徴:**
- Claude シリーズ（`claude-sonnet-4.5`, `claude-opus-4` など）
- 最新モデルはコーディングとエージェントタスクで GPT-5 を上回る
- 文章の質と安全性に定評

**Nekote.Core でのサポート範囲:**
- ✅ Messages API
- ✅ Text Embeddings（Voyage AI 経由の可能性）
- ❌ Vision（範囲外）

**実装優先度:** 中（フェーズ 6 以降）

### 9.4. xAI

**特徴:**
- Grok シリーズ（`grok-3` など）
- X (Twitter) へのリアルタイムアクセス
- 制限が少なく、ウィットに富んだ回答

**Nekote.Core でのサポート範囲:**
- ✅ Chat Completions API（OpenAI 互換）
- ❌ Embeddings（未提供の可能性）

**実装優先度:** 低（需要に応じて）

### 9.5. Mistral

**特徴:**
- ヨーロッパ（フランス）製、GDPR コンプライアンス
- オンプレミスデプロイ対応
- コストパフォーマンスに優れる

**Nekote.Core でのサポート範囲:**
- ✅ Chat Completions API
- ✅ Embeddings API
- ❌ Fine-tuning（範囲外）

**実装優先度:** 低（需要に応じて）

### 9.6. DeepSeek

**特徴:**
- コーディングと数学に特化（`deepseek-coder`, `deepseek-r1`）
- SWE-bench などのベンチマークでトップクラス
- 非常に高いコストパフォーマンス

**Nekote.Core でのサポート範囲:**
- ✅ Chat Completions API
- ✅ Embeddings API（提供されている場合）

**実装優先度:** 中（技術的なタスクに有用）

### 9.7. DeepL

**特徴:**
- 翻訳専門（Chat や Embedding は提供していない）
- 高品質な翻訳、特に日英・英日で定評
- API はシンプル

**Nekote.Core でのサポート範囲:**
- ✅ Translation API のみ
- ❌ Chat, Embedding（提供されていない）

**実装優先度:** 低（翻訳が必要な場合のみ）

---

## 10. テスト戦略

### 10.1. テストの種類

#### 10.1.1. 単体テスト（Unit Tests）

**対象:** マッパー、ドメインモデル

**ツール:** xUnit, FluentAssertions

**例:**

```csharp
[Fact]
public void ToRequestDto_ValidMessages_ReturnsCorrectDto()
{
    // Arrange
    var messages = new List<ChatMessage>
    {
        new() { Role = ChatRole.System, Content = "You are a helpful assistant." },
        new() { Role = ChatRole.User, Content = "Hello!" }
    };
    var options = new OpenAiChatOptions { Temperature = 0.7f };
    var modelName = "gpt-4-turbo";

    // Act
    var dto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);

    // Assert
    dto.Model.Should().Be("gpt-4-turbo");
    dto.Messages.Should().HaveCount(2);
    dto.Messages[0].Role.Should().Be("system");
    dto.Messages[1].Role.Should().Be("user");
    dto.Temperature.Should().Be(0.7f);
}

[Fact]
public void ToRequestDto_NullModelName_ThrowsArgumentException()
{
    // Arrange
    var messages = new List<ChatMessage> { new() { Role = ChatRole.User, Content = "Test" } };

    // Act & Assert
    Action act = () => OpenAiChatMapper.ToRequestDto(messages, null, null!);
    act.Should().Throw<ArgumentException>()
        .WithMessage("*Model name is required*");
}
```

#### 10.1.2. 統合テスト（Integration Tests）

**対象:** リポジトリ（モック HTTP クライアントを使用）

**ツール:** xUnit, Moq, WireMock.Net

**例:**

```csharp
[Fact]
public async Task GetCompletionAsync_ValidRequest_ReturnsResponse()
{
    // Arrange
    var mockHandler = new Mock<HttpMessageHandler>();
    mockHandler.Protected()
        .Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(@"{
                ""id"": ""chatcmpl-123"",
                ""choices"": [{
                    ""message"": { ""role"": ""assistant"", ""content"": ""Hello!"" },
                    ""finish_reason"": ""stop""
                }]
            }")
        });

    var httpClient = new HttpClient(mockHandler.Object);
    var repository = new OpenAiChatRepository(/* inject mocked dependencies */);

    var messages = new List<ChatMessage>
    {
        new() { Role = ChatRole.User, Content = "Hi" }
    };

    // Act
    var response = await repository.GetCompletionAsync(messages);

    // Assert
    response.Content.Should().Be("Hello!");
    response.FinishReason.Should().Be("stop");
}
```

#### 10.1.3. エンドツーエンドテスト（E2E Tests）

**対象:** 実際の API 呼び出し

**注意:** API キーが必要。CI/CD では環境変数から読み込む。

**例:**

```csharp
[Fact]
[Trait("Category", "E2E")]
public async Task OpenAI_RealApiCall_ReturnsValidResponse()
{
    // Arrange
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    if (string.IsNullOrEmpty(apiKey))
    {
        // Skip test if API key is not available
        return;
    }

    var services = new ServiceCollection();
    services.AddOpenAiChat(configuration);
    var provider = services.BuildServiceProvider();
    var chatService = provider.GetRequiredService<IChatCompletionService>();

    var messages = new List<ChatMessage>
    {
        new() { Role = ChatRole.User, Content = "Say 'test passed'" }
    };

    // Act
    var response = await chatService.GetCompletionAsync(messages);

    // Assert
    response.Content.Should().Contain("test passed");
}
```

### 10.2. テストカバレッジ目標

- **マッパー:** 100%（全てのパスをテスト）
- **リポジトリ:** 80% 以上（エラーハンドリングを含む）
- **ドメインモデル:** 基本的なインスタンス化のみ（setter/getter のテストは不要）

### 10.3. CI/CD での実行

**戦略:**

1. **単体テスト:** 全ての PR で自動実行
2. **統合テスト:** 全ての PR で自動実行（モック使用）
3. **E2E テスト:** 毎晩または週次で実行（実際の API 使用）

---

## 11. AI アシスタント向けコード生成ガイドライン

このセクションは、AI アシスタント（GitHub Copilot, ChatGPT, Claude など）が `Nekote.Core` のコードを生成する際に従うべきルールを定義する。

### 11.1. コンテキストの準備

AI にコード生成を依頼する際、以下の 4 つのドキュメントを必ず提供すること：

1. **PLAYBOOK.md** （基本的な C# コーディング規約）
2. **PLAY_HARDER.md** （アプリケーションアーキテクチャ）
3. **ai-implementation-strategy.md** （本ドキュメント）
4. **プロバイダー固有のサブセット仕様書** （例: `chat-completions-api.md`）

### 11.2. 生成ルール

#### DTO 生成時

1. サブセット仕様書に記載されている **全てのフィールド** を実装する（省略しない）
2. 全てのプロパティを `nullable` にする
3. `[JsonPropertyName]` 属性を正確に記述する（`snake_case` ↔ `PascalCase`）
4. `[JsonExtensionData]` プロパティを必ず含める
5. `internal sealed class` として定義する
6. XML コメントは日本語で記述する

#### Mapper 生成時

1. 防御的検証を実装する（null チェック、必須フィールドの確認）
2. 例外メッセージは英語で、具体的な内容を含める
3. `RawData` ディクショナリを必ず構築する
   - DTO の `ExtensionData` を含める
   - 第一級プロパティにマップされないフィールドを含める
4. `static` クラスとして定義する
5. `switch` 式で `_` (default case) を必ず含め、例外をスローする

#### Repository 生成時

1. コンストラクターで `IHttpClientFactory`, `ILogger<T>`, `IOptions<TConfig>` を注入する
2. フォールバックチェーンを実装する（API キー、エンドポイント、モデル名）
3. 全ての `await` に `.ConfigureAwait(false)` を付ける
4. リクエスト/レスポンス JSON を `LogDebug` で出力する
5. HTTP ステータスコードが成功 (2xx) でない場合は `AiApiCallException` をスローする
6. `internal sealed class` として定義する

#### DI 拡張メソッド生成時

1. 設定パスは引数として受け取る（ハードコードしない）
2. デフォルト値を提供する（例: `"AI:OpenAI"`）
3. リポジトリは `Scoped` ライフタイムで登録する
4. 名前付き HttpClient を使用する（例: `"OpenAI-Chat"`）
5. `public static` メソッドとして定義する

### 11.3. 禁止事項

1. ❌ DTO にビジネスロジックを含めない
2. ❌ マッパーに HTTP 通信を含めない
3. ❌ リポジトリにドメインロジックを含めない
4. ❌ 共通基底クラスを作成しない（プロバイダー間でのコード共有は禁止）
5. ❌ `Console.WriteLine()` を使用しない（`ILogger<T>` を使用）
6. ❌ ハードコードされた文字列（API キー、エンドポイント）を含めない

### 11.4. コード生成の手順

**ステップ 1:** サブセット仕様書を読み込む

```
AI に渡すプロンプト:
「以下の OpenAI Chat Completions API のサブセット仕様書を読み込んでください。」
[chat-completions-api.md の内容を貼り付け]
```

**ステップ 2:** DTO を生成

```
AI に渡すプロンプト:
「上記の仕様書に基づき、OpenAiChatRequestDto.cs と OpenAiChatResponseDto.cs を生成してください。
全てのフィールドを実装し、JsonExtensionData を含めてください。」
```

**ステップ 3:** Mapper を生成

```
AI に渡すプロンプト:
「OpenAiChatMapper.cs を生成してください。
ToRequestDto と ToDomainModel の二つのメソッドを実装し、
防御的検証と RawData の構築を含めてください。」
```

**ステップ 4:** Repository を生成

```
AI に渡すプロンプト:
「OpenAiChatRepository.cs を生成してください。
IChatCompletionService を実装し、HTTP 通信とエラーハンドリングを含めてください。」
```

**ステップ 5:** DI 拡張メソッドを生成

```
AI に渡すプロンプト:
「OpenAiServiceCollectionExtensions.cs に AddOpenAiChat メソッドを追加してください。」
```

### 11.5. レビューチェックリスト

AI が生成したコードをレビューする際、以下を確認：

- [ ] 全てのクラスに XML コメント（日本語）が存在する
- [ ] DTO の全てのプロパティが nullable
- [ ] DTO に `JsonExtensionData` が存在する
- [ ] Mapper で `RawData` を構築している
- [ ] Mapper で防御的検証を実装している
- [ ] Repository でフォールバックチェーンを実装している
- [ ] Repository で `.ConfigureAwait(false)` を使用している
- [ ] DI 拡張メソッドで設定パスが引数として受け取られている

---

## 12. まとめ

本戦略書は、`Nekote.Core` に AI 機能を統合するための完全な設計指針を提供する。

**核心原則:**

1. **普遍的なものは「会話」のみ:** `ChatMessage` 構造体のみをプロバイダー間で共有する
2. **完全な独立性:** 各プロバイダーの実装は完全に独立し、互いに依存しない
3. **情報損失ゼロ:** `RawData` ディクショナリと `JsonExtensionData` により、全てのデータにアクセス可能
4. **仕様書駆動:** サブセット仕様書を管理し、AI の精度を向上させる
5. **段階的実装:** 一度に一つのプロバイダー、一つの機能に集中する

**次のステップ:**

1. フェーズ 1（普遍的契約）の実装を開始
2. OpenAI のサブセット仕様書を作成
3. フェーズ 2（OpenAI Chat）の実装を完了
4. テストを追加し、品質を保証
5. 他のプロバイダーを段階的に追加

この戦略により、`Nekote.Core` は、変化の速い AI 環境において、堅牢で保守可能なライブラリとなる。

---

**文書バージョン:** 2.0
**最終更新:** 2025-11-12
**作成者:** nao7sep
```

