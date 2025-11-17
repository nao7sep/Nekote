using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 思考機能の構成 DTO。
    /// </summary>
    internal class GeminiChatThinkingConfigDto
    {
        /// <summary>
        /// レスポンスに思考を含めるかどうか。
        /// </summary>
        [JsonPropertyName("includeThoughts")]
        public bool? IncludeThoughts { get; set; }

        /// <summary>
        /// 思考トークンの予算。
        /// </summary>
        [JsonPropertyName("thinkingBudget")]
        public int? ThinkingBudget { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
