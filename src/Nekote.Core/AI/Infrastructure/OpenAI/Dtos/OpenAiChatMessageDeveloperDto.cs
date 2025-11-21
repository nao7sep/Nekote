using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// デベロッパーロールのメッセージ DTO。
    /// </summary>
    /// <remarks>
    /// o1 モデル以降で system の代わりに使用される。
    /// </remarks>
    public class OpenAiChatMessageDeveloperDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
