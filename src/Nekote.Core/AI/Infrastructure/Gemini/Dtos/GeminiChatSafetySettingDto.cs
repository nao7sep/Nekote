using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 安全性設定 DTO。
    /// </summary>
    public class GeminiChatSafetySettingDto
    {
        /// <summary>
        /// カテゴリ。
        /// </summary>
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        /// <summary>
        /// ブロックのしきい値。
        /// </summary>
        [JsonPropertyName("threshold")]
        public string? Threshold { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
