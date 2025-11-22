using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API のレスポンスボディ。
    /// </summary>
    public class GeminiEmbeddingResponseDto
    {
        /// <summary>
        /// 生成された埋め込み。
        /// </summary>
        [JsonPropertyName("embedding")]
        public GeminiEmbeddingValuesDto? Embedding { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
