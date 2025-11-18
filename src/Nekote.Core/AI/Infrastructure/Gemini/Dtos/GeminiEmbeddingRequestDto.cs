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
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
