using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API のリクエストボディ。
    /// </summary>
    public class GeminiEmbeddingRequestDto
    {
        /// <summary>
        /// 埋め込みを生成するコンテンツ。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiEmbeddingContentDto? Content { get; set; }

        /// <summary>
        /// 埋め込みの使用目的を指定するタスクタイプ（省略可）。
        /// </summary>
        [JsonPropertyName("taskType")]
        public string? TaskType { get; set; }

        /// <summary>
        /// テキストのタイトル（省略可）。TaskType が RETRIEVAL_DOCUMENT の場合にのみ適用される。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// 出力する埋め込みの次元数（省略可）。
        /// </summary>
        [JsonPropertyName("outputDimensionality")]
        public int? OutputDimensionality { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
