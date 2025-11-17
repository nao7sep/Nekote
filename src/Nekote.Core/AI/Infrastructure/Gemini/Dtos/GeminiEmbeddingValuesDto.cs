using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API のエンベディング値 DTO。
    /// </summary>
    internal class GeminiEmbeddingValuesDto
    {
        /// <summary>
        /// エンベディングベクトル。
        /// </summary>
        [JsonPropertyName("values")]
        public float[]? Values { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
