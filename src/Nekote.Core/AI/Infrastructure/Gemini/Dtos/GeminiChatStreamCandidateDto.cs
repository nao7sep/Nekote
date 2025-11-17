using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API のストリーミング候補 DTO。
    /// </summary>
    internal class GeminiChatStreamCandidateDto
    {
        /// <summary>
        /// 生成されたコンテンツの増分部分。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiChatContentDto? Content { get; set; }

        /// <summary>
        /// 補完が終了した理由。
        /// </summary>
        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        /// <summary>
        /// この候補のインデックス。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

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
