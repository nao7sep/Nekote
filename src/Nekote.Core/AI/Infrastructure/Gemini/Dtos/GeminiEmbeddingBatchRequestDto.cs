using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Batch Embedding API のリクエストボディ。
    /// </summary>
    public class GeminiEmbeddingBatchRequestDto
    {
        /// <summary>
        /// バッチで処理される埋め込みリクエストのリスト。
        /// </summary>
        [JsonPropertyName("requests")]
        public List<GeminiEmbeddingBatchRequestContentDto>? Requests { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
