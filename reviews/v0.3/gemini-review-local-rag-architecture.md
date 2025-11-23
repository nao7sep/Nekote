# Architectural Review: Building a Local RAG System with Nekote.Core

This report outlines the required architectural components to build a complete, local Retrieval-Augmented Generation (RAG) system on top of the existing `Nekote.Core` library.

**Assumption:** The system has access to a local model for computing vector embeddings from text, using the DTOs already defined in `Nekote.Core.AI`.

**Goal:** To design the missing services and abstractions for a RAG pipeline that can be run entirely locally, without dependencies on external packages from providers like OpenAI or Gemini, while adhering to the principles of the `PLAYBOOK.md`.

---

## 1. RAG System Overview

A RAG system consists of two main phases:

1.  **Indexing:** Loading, chunking, and creating vector embeddings from source documents, then storing them in a searchable **Vector Store**.
2.  **Retrieval & Generation:** Taking a user query, creating an embedding for it, searching the Vector Store for relevant document chunks, and then feeding those chunks as "context" to a Large Language Model (LLM) to generate a final answer.

## 2. Core Missing Components

To build this system, the following components are missing from `Nekote.Core`:

1.  **Data Ingestion Pipeline:** Services for loading documents from a source and splitting them into manageable chunks.
2.  **Vector Store:** A database for storing text chunks and their vector embeddings, optimized for similarity search.
3.  **Local AI Service Abstractions:** Interfaces for the local embedding and text generation models.
4.  **RAG Orchestrator:** A high-level pipeline that ties all the services together to process a user query.

---

## 3. Detailed Component Design

Below is a detailed design for each missing component, following the architectural style of `Nekote.Core`.

### 3.1. Data Ingestion Pipeline

This pipeline is responsible for preparing source documents for the RAG system.

#### 3.1.1. Domain Model: `DocumentChunk`

First, we need a pure domain model to represent a piece of text and its associated data.

```csharp
// In: /src/Nekote.Core/Domain/
namespace Nekote.Core.Domain
{
    /// <summary>
    /// テキストのチャンク（断片）と関連メタデータを表すドメインモデル。
    /// </summary>
    public class DocumentChunk
    {
        /// <summary>
        /// チャンクの一意なID。
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// チャンクの元のソース（例：ファイルパス）。
        /// </summary>
        public string Source { get; init; } = string.Empty;

        /// <summary>
        /// チャンクのテキスト内容。
        /// </summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>
        /// このチャンクのベクトル埋め込み。
        /// </summary>
        public float[] Vector { get; set; } = Array.Empty<float>();
    }
}
```

#### 3.1.2. Service: `ITextChunker`

This service is responsible for splitting a large text into `DocumentChunk` objects. This is more advanced than simply splitting by lines. A "Recursive Character" strategy is common and effective.

```csharp
// In: /src/Nekote.Core/Text/Chunking/
namespace Nekote.Core.Text.Chunking
{
    /// <summary>
    /// テキストをドキュメントチャンクに分割する機能を提供します。
    /// </summary>
    public interface ITextChunker
    {
        /// <summary>
        /// 指定されたテキストをチャンクのシーケンスに分割します。
        /// </summary>
        /// <param name="source">チャンクの元のソース識別子。</param>
        /// <param name="text">分割するテキスト。</param>
        /// <returns>DocumentChunkオブジェクトのリスト。</returns>
        List<DocumentChunk> CreateChunks(string source, string text);
    }

    /// <summary>
    /// 再帰的に文字でテキストを分割するチャンカー。
    /// 指定されたセパレーターのリストを使用して、チャンクサイズを超えないように分割を試みます。
    /// </summary>
    public class RecursiveCharacterTextChunker : ITextChunker
    {
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;
        private readonly string[] _separators;

        public RecursiveCharacterTextChunker(int chunkSize = 1000, int chunkOverlap = 100)
        {
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
            // 最も意味のある単位から最も小さい単位へ
            _separators = new[] { "\n\n", "\n", " ", "" };
        }

        public List<DocumentChunk> CreateChunks(string source, string text)
        {
            // 実装ロジック：
            // 1. テキストを指定されたセパレーターで順番に分割しようと試みる。
            // 2. 最初のセパレーター（例："\n\n"）で分割し、チャンクが _chunkSize を下回るか確認。
            // 3. チャンクが大きすぎる場合、次のセパレーター（例："\n"）でそのチャンクをさらに分割する。
            // 4. これを再帰的に繰り返し、最終的に文字単位（""）で分割して、
            //    すべてのチャンクが _chunkSize を下回るようにする。
            // 5. チャンクを結合して返す際、_chunkOverlap を考慮してチャンク間に重複部分を持たせる。
            //    これにより、文脈の損失を防ぐ。
            // (このロジックは複雑であり、ここでは省略します)
            throw new NotImplementedException();
        }
    }
}
```

### 3.2. Vector Storage & Search

This is the heart of the "Retrieval" part of RAG. It needs to store chunks and perform efficient vector similarity searches.

#### 3.2.1. Interface: `IVectorStore`

This interface abstracts the storage mechanism.

```csharp
// In: /src/Nekote.Core/VectorStore/
using Nekote.Core.Domain;

namespace Nekote.Core.VectorStore
{
    /// <summary>
    /// ドキュメントチャンクとそのベクトル埋め込みの保存と検索を抽象化します。
    /// </summary>
    public interface IVectorStore
    {
        /// <summary>
        /// ドキュメントチャンクのコレクションをベクトルストアに追加または更新します。
        /// </summary>
        Task AddOrUpdateAsync(IEnumerable<DocumentChunk> chunks);

        /// <summary>
        /// 指定されたクエリベクトルに最も類似した上位N個のチャンクを検索します。
        /// </summary>
        /// <param name="queryVector">検索に使用するベクトル。</param>
        /// <param name="topN">返す結果の数。</param>
        /// <returns>類似度スコアでソートされたドキュメントチャンクのリスト。</returns>
        Task<List<(DocumentChunk Chunk, float Score)>> SearchAsync(float[] queryVector, int topN = 5);
    }
}
```

#### 3.2.2. Implementation 1: `InMemoryVectorStore` (Simple)

A simple implementation for testing and small-scale use.

```csharp
// In: /src/Nekote.Core/VectorStore/Infrastructure/
public class InMemoryVectorStore : IVectorStore
{
    private readonly List<DocumentChunk> _chunks = new();

    public Task AddOrUpdateAsync(IEnumerable<DocumentChunk> chunks)
    {
        _chunks.AddRange(chunks); // 簡単のため、更新は省略
        return Task.CompletedTask;
    }

    public Task<List<(DocumentChunk, float)>> SearchAsync(float[] queryVector, int topN = 5)
    {
        // 実装ロジック：
        // 1. _chunks内のすべてのチャンクに対してコサイン類似度を計算する。
        //    - CosineSimilarity(A, B) = Dot(A, B) / (Magnitude(A) * Magnitude(B))
        // 2. 結果を類似度スコアで降順にソートする。
        // 3. 上位N個の結果を取得して返す。
        throw new NotImplementedException();
    }
}
```

#### 3.2.3. Implementation 2: `LiteDbVectorStore` (Robust)

For a persistent local store, a file-based database like **LiteDB** is an excellent choice as it's serverless and written in .NET.

```csharp
// In: /src/Nekote.Core/VectorStore/Infrastructure/
// Requires a NuGet package like LiteDB
public class LiteDbVectorStore : IVectorStore
{
    private readonly string _dbPath;

    public LiteDbVectorStore(string dbPath)
    {
        _dbPath = dbPath;
    }

    public Task AddOrUpdateAsync(IEnumerable<DocumentChunk> chunks)
    {
        // 実装ロジック：
        // 1. LiteDatabaseに接続する。
        // 2. "chunks" コレクションを取得する。
        // 3. 各チャンクをBSONドキュメントとして保存する。IDでインデックスを作成する。
        throw new NotImplementedException();
    }

    public Task<List<(DocumentChunk, float)>> SearchAsync(float[] queryVector, int topN = 5)
    {
        // 実装ロジック：
        // 1. LiteDatabaseに接続する。
        // 2. "chunks" コレクション内のすべてのドキュメントをストリーミングで読み取る。
        // 3. 各ドキュメントのベクトルに対してコサイン類似度を計算する。
        // 4. 結果を保持し、ソートして上位N個を返す。
        // 注：これは全スキャンであり、大規模なデータセットでは非効率。
        // 本格的な実装では、Annoy、HNSWlibなどの近似最近傍（ANN）インデックスライブラリと
        // 統合する必要があるが、ローカルシステムとしてはこれで十分な出発点となる。
        throw new NotImplementedException();
    }
}
```

### 3.3. Local AI Service Abstractions

These interfaces abstract the actual local models. The user is responsible for providing the concrete implementations.

```csharp
// In: /src/Nekote.Core/AI/
namespace Nekote.Core.AI
{
    /// <summary>
    /// テキストをベクトル埋め込みに変換するサービスを抽象化します。
    /// </summary>
    public interface IEmbeddingService
    {
        /// <summary>
        /// 指定されたテキストのベクトル埋め込みを生成します。
        /// </summary>
        Task<float[]> CreateEmbeddingAsync(string text);
    }

    /// <summary>
    /// プロンプトに応答してテキストを生成するサービスを抽象化します。
    /// </summary>
    public interface IGenerativeLlmService
    {
        /// <summary>
        /// 指定されたプロンプトに基づいてテキストを生成します。
        /// </summary>
        Task<string> GenerateResponseAsync(string prompt);
    }
}

// 注：これらのインターフェースの実際の実装（例：OnnxEmbeddingService）は、
// LlamaSharp, ONNX Runtime, ML.NET などのライブラリを使用して、
// ローカルにダウンロードされたモデル（例：GTE, LLaMA）をラップすることになります。
```

### 3.4. RAG Orchestrator

This class ties everything together.

```csharp
// In: /src/Nekote.Core/RAG/
using Nekote.Core.AI;
using Nekote.Core.VectorStore;

namespace Nekote.Core.RAG
{
    /// <summary>
    /// RAGパイプラインを調整し、クエリに応答を生成します。
    /// </summary>
    public class RagOrchestrator
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorStore _vectorStore;
        private readonly IGenerativeLlmService _llmService;

        public RagOrchestrator(
            IEmbeddingService embeddingService,
            IVectorStore vectorStore,
            IGenerativeLlmService llmService)
        {
            _embeddingService = embeddingService;
            _vectorStore = vectorStore;
            _llmService = llmService;
        }

        /// <summary>
        /// ユーザーのクエリに対してRAGプロセスを実行し、回答を生成します。
        /// </summary>
        public async Task<string> QueryAsync(string userQuery)
        {
            // 1. クエリを埋め込む
            var queryVector = await _embeddingService.CreateEmbeddingAsync(userQuery);

            // 2. ベクトルストアで関連チャンクを検索する
            var relevantChunks = await _vectorStore.SearchAsync(queryVector, topN: 5);

            // 3. コンテキストを構築する
            var contextBuilder = new System.Text.StringBuilder();
            contextBuilder.AppendLine("Use the following context to answer the question:");
            contextBuilder.AppendLine("---");
            foreach (var (chunk, score) in relevantChunks)
            {
                contextBuilder.AppendLine(chunk.Content);
            }
            contextBuilder.AppendLine("---");

            // 4. LLM用のプロンプトを設計する
            contextBuilder.AppendLine($"Question: {userQuery}");
            contextBuilder.Append("Answer:");
            var finalPrompt = contextBuilder.ToString();

            // 5. LLMから回答を生成する
            var answer = await _llmService.GenerateResponseAsync(finalPrompt);

            return answer;
        }
    }
}
```

---

## 4. Conclusion and Next Steps

To build a local RAG system, `Nekote.Core` requires the addition of several key abstractions and services. This report proposes a robust, testable, and modular architecture that aligns with the project's existing high standards.

**Summary of New Components:**
- **Domain Model:** `DocumentChunk`
- **Interfaces:** `ITextChunker`, `IVectorStore`, `IEmbeddingService`, `IGenerativeLlmService`
- **Implementations:** `RecursiveCharacterTextChunker`, `InMemoryVectorStore`
- **Orchestrator:** `RagOrchestrator`

**Next Steps would be:**
1.  Implement the logic for `RecursiveCharacterTextChunker` and the vector math for `InMemoryVectorStore`.
2.  Choose and implement a persistent vector store, such as the proposed `LiteDbVectorStore`.
3.  Provide concrete implementations for `IEmbeddingService` and `IGenerativeLlmService` by integrating local model runners (e.g., LlamaSharp, ONNX Runtime).
4.  Register all new services in the dependency injection container.
5.  Use the `RagOrchestrator` to answer user queries.

```