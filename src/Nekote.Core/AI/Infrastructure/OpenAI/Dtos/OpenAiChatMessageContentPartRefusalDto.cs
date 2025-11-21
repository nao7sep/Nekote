using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 拒否タイプの content part DTO。
    /// </summary>
    public class OpenAiChatMessageContentPartRefusalDto : OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// 拒否メッセージ。
        /// </summary>
        [JsonPropertyName("refusal")]
        public string? Refusal { get; set; }
    }
}
