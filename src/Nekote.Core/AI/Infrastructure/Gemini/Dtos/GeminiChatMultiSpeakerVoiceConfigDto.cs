using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 複数話者の音声設定。
    /// </summary>
    public class GeminiChatMultiSpeakerVoiceConfigDto
    {
        /// <summary>
        /// 話者ごとの音声設定のリスト。
        /// </summary>
        [JsonPropertyName("speakerVoiceConfigs")]
        public List<GeminiChatSpeakerVoiceConfigDto>? SpeakerVoiceConfigs { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
