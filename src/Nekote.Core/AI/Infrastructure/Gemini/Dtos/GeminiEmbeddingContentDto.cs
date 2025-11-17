using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API のコンテンツ DTO。
    /// </summary>
    internal class GeminiEmbeddingContentDto
    {
        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        [JsonPropertyName("parts")]
        public List<GeminiEmbeddingPartDto>? Parts { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
