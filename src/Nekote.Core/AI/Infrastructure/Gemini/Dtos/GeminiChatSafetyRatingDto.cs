using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API の安全性評価。
    /// </summary>
    public class GeminiChatSafetyRatingDto
    {
        /// <summary>
        /// 安全性カテゴリ。
        /// </summary>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        /// <summary>
        /// 安全性の確率。
        /// </summary>
        [JsonPropertyName("probability")]
        public string? Probability { get; set; }

        /// <summary>
        /// このレーティングが原因でコンテンツがブロックされたかどうか。
        /// </summary>
        [JsonPropertyName("blocked")]
        public bool? Blocked { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
