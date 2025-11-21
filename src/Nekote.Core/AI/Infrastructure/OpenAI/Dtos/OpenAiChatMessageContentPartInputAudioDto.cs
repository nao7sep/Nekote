using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 入力オーディオタイプの content part DTO。
    /// </summary>
    public class OpenAiChatMessageContentPartInputAudioDto : OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// オーディオ入力情報。
        /// </summary>
        [JsonPropertyName("input_audio")]
        public OpenAiChatInputAudioDto? InputAudio { get; set; }
    }
}
