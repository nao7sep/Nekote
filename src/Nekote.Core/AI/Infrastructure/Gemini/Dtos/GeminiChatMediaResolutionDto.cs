using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 入力メディアのメディア解像度。
    /// </summary>
    public class GeminiChatMediaResolutionDto
    {
        /// <summary>
        /// メディア解像度レベル。
        /// </summary>
        [JsonPropertyName("level")]
        public string? Level { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
