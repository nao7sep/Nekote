using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API へのリクエストボディ DTO。
    /// </summary>
    public class GeminiEmbeddingRequestDto
    {
        /// <summary>
        /// エンベディングを生成するコンテンツ。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiEmbeddingContentDto? Content { get; set; }

        /// <summary>
        /// エンベディングが使用されるオプションのタスクタイプ。
        /// </summary>
        [JsonPropertyName("taskType")]
        public string? TaskType { get; set; }

        /// <summary>
        /// テキストのタイトル（省略可）。TaskType が RETRIEVAL_DOCUMENT の場合にのみ適用される。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// 出力エンベディングのオプションの削減ディメンション。
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
