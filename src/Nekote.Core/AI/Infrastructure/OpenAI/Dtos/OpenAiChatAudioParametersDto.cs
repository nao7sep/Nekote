using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// オーディオ出力のパラメータ DTO。
    /// </summary>
    public class OpenAiChatAudioParametersDto
    {
        /// <summary>
        /// オーディオフォーマット。
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// 音声の識別子。
        /// </summary>
        [JsonPropertyName("voice")]
        public string? Voice { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
