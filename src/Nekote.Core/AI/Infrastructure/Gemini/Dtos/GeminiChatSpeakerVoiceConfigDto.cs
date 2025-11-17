using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 話者の音声構成 DTO。
    /// </summary>
    internal class GeminiChatSpeakerVoiceConfigDto
    {
        /// <summary>
        /// 話者名。
        /// </summary>
        [JsonPropertyName("speaker")]
        public string? Speaker { get; set; }

        /// <summary>
        /// 音声の構成。
        /// </summary>
        [JsonPropertyName("voiceConfig")]
        public GeminiChatVoiceConfigDto? VoiceConfig { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
