using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数ロールのメッセージ DTO (非推奨: tool に置き換えられた)。
    /// </summary>
    /// <remarks>
    /// 関数実行結果を表す。
    /// </remarks>
    [Obsolete("This message type is deprecated. Use OpenAiChatMessageToolDto instead.")]
    public class OpenAiChatMessageFunctionDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}
