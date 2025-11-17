using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 音声構成 DTO。
    /// </summary>
    internal class GeminiChatVoiceConfigDto
    {
        /// <summary>
        /// 事前構築済み音壷の構成。
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
