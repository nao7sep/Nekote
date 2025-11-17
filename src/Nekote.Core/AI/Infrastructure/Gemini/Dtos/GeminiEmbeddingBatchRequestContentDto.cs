using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Batch Embedding API の個別リクエストコンテンツ DTO。
    /// </summary>
    internal class GeminiEmbeddingBatchRequestContentDto
    {
        /// <summary>
        /// エンベディングを生成するコンテンツ。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiEmbeddingContentDto? Content { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
