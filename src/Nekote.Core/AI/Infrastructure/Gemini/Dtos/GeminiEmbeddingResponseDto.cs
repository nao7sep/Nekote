using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API からのレスポンスボディ DTO。
    /// </summary>
    internal class GeminiEmbeddingResponseDto
    {
        /// <summary>
        /// 生成されたエンベディングのリスト。
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
