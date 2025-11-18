using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API のプロンプトフィードバック DTO。
    /// </summary>
    public class GeminiChatPromptFeedbackDto
    {
        /// <summary>
        /// ブロック理由 (プロンプトがブロックされた場合)。
        /// </summary>
        [JsonPropertyName("blockReason")]
        public string? BlockReason { get; set; }

        /// <summary>
        /// 安全性評価のリスト。
        /// </summary>
        [JsonPropertyName("safetyRatings")]
        public List<GeminiChatSafetyRatingDto>? SafetyRatings { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
