using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Embedding API の埋め込み値。
    /// </summary>
    public class GeminiEmbeddingValuesDto
    {
        /// <summary>
        /// 埋め込みベクトル。
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
