using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API のコンテンツ (リクエスト/レスポンス共通部)。
    /// </summary>
    public class GeminiChatContentDto
    {
        /// <summary>
        /// コンテンツのロール ("user" または "model")。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        [JsonPropertyName("parts")]
        public List<GeminiChatPartDto>? Parts { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
