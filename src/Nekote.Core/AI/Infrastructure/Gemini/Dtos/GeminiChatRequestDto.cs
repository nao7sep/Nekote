using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API へのリクエストボディ DTO。
    /// </summary>
    internal class GeminiChatRequestDto
    {
        /// <summary>
        /// チャットコンテンツのリスト。
        /// </summary>
        [JsonPropertyName("contents")]
        public List<GeminiChatContentDto>? Contents { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
