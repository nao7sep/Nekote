using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 事前構築済み音声の構成 DTO。
    /// </summary>
    public class GeminiChatPrebuiltVoiceConfigDto
    {
        /// <summary>
        /// 音壷名。
        /// </summary>
        [JsonPropertyName("voiceName")]
        public string? VoiceName { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
