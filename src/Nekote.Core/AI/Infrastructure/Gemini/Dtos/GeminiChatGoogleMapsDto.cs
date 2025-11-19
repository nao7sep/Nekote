using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google マップツール。
    /// </summary>
    public class GeminiChatGoogleMapsDto
    {
        /// <summary>
        /// ウィジェット コンテキスト トークンを返すかどうか。
        /// </summary>
        [JsonPropertyName("enableWidget")]
        public bool? EnableWidget { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
