using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// エンベディングレスポンスの "data" 配列内の単一エンベディング DTO。
    /// </summary>
    internal class OpenAiEmbeddingDataDto
    {
        /// <summary>
        /// エンベディングベクトル。
        /// </summary>
        [JsonPropertyName("embedding")]
        public float[]? Embedding { get; set; }

        /// <summary>
        /// この埋め込みのインデックス。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// オブジェクトの種類 (通常は "embedding")。
        /// </summary>
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
