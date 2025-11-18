using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Embedding API からのレスポンスボディ DTO。
    /// </summary>
    public class OpenAiEmbeddingResponseDto
    {
        /// <summary>
        /// オブジェクトの種類 (通常は "list")。
        /// </summary>
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        /// <summary>
        /// 生成されたエンベディングのリスト。
        /// </summary>
        [JsonPropertyName("data")]
        public List<OpenAiEmbeddingDataDto>? Data { get; set; }

        /// <summary>
        /// 使用されたモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// トークン使用量の詳細。
        /// </summary>
        [JsonPropertyName("usage")]
        public OpenAiEmbeddingUsageDto? Usage { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
