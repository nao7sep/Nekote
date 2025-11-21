using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// テキストタイプの content part DTO。
    /// </summary>
    public class OpenAiChatMessageContentPartTextDto : OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// テキスト内容。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
