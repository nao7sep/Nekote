using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// システムロールのメッセージ DTO。
    /// </summary>
    /// <remarks>
    /// モデルの動作を指示するために使用される。
    /// </remarks>
    public class OpenAiChatMessageSystemDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
