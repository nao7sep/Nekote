using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// デベロッパーロールのメッセージ。
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

        /// <summary>
        /// 参加者の名前（省略可能）。同じ役割の参加者を区別するために使用される。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
