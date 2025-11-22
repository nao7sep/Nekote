using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 音声の設定。
    /// </summary>
    public class GeminiChatVoiceConfigDto
    {
        /// <summary>
        /// 事前定義済み音声の設定。
        /// </summary>
        [JsonPropertyName("prebuiltVoiceConfig")]
        public GeminiChatPrebuiltVoiceConfigDto? PrebuiltVoiceConfig { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
