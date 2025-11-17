using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API からのレスポンスボディ DTO。
    /// </summary>
    internal class GeminiChatResponseDto
    {
        /// <summary>
        /// 生成された候補のリスト。
        /// </summary>
        [JsonPropertyName("candidates")]
        public List<GeminiChatCandidateDto>? Candidates { get; set; }

        /// <summary>
        /// プロンプトのフィードバック情報。
        /// </summary>
        [JsonPropertyName("promptFeedback")]
        public GeminiChatPromptFeedbackDto? PromptFeedback { get; set; }

        /// <summary>
        /// 使用量メタデータ。
        /// </summary>
        [JsonPropertyName("usageMetadata")]
        public GeminiChatUsageMetadataDto? UsageMetadata { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
