using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 音声生成の構成 DTO。
    /// </summary>
    public class GeminiChatSpeechConfigDto
    {
        /// <summary>
        /// 音声の構成。
        /// </summary>
        [JsonPropertyName("voiceConfig")]
        public GeminiChatVoiceConfigDto? VoiceConfig { get; set; }

        /// <summary>
        /// 複数話者の音声構成。
        /// </summary>
        [JsonPropertyName("multiSpeakerVoiceConfig")]
        public GeminiChatMultiSpeakerVoiceConfigDto? MultiSpeakerVoiceConfig { get; set; }

        /// <summary>
        /// 言語コード (BCP 47 形式)。
        /// </summary>
        [JsonPropertyName("languageCode")]
        public string? LanguageCode { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
