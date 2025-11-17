using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Batch Embedding API からのレスポンスボディ DTO。
    /// </summary>
    internal class GeminiEmbeddingBatchResponseDto
    {
        /// <summary>
        /// 各リクエストのエンベディング結果のリスト。
        /// </summary>
        [JsonPropertyName("embeddings")]
        public List<GeminiEmbeddingValuesDto>? Embeddings { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
