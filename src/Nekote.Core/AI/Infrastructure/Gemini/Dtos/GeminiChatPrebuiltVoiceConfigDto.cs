using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 事前定義済み音声の設定。
    /// </summary>
    public class GeminiChatPrebuiltVoiceConfigDto
    {
        /// <summary>
        /// 音声名。
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
