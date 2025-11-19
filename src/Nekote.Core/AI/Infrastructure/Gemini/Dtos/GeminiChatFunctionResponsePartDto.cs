using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数レスポンスパート。
    /// </summary>
    public class GeminiChatFunctionResponsePartDto
    {
        /// <summary>
        /// インライン メディア バイト。
        /// </summary>
        [JsonPropertyName("inlineData")]
        public GeminiChatFunctionResponseBlobDto? InlineData { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
