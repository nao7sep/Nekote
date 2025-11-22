using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 単一のモダリティのトークン カウント情報。
    /// </summary>
    public class GeminiChatModalityTokenCountDto
    {
        /// <summary>
        /// このトークン数に関連付けられたモダリティ。
        /// </summary>
        [JsonPropertyName("modality")]
        public string? Modality { get; set; }

        /// <summary>
        /// トークンの数。
        /// </summary>
        [JsonPropertyName("tokenCount")]
        public int? TokenCount { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
