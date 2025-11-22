using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツールロールのメッセージ。
    /// </summary>
    /// <remarks>
    /// ツール実行結果を表す。
    /// </remarks>
    public class OpenAiChatMessageToolDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// ツール呼び出し ID。
        /// </summary>
        [JsonPropertyName("tool_call_id")]
        public string? ToolCallId { get; set; }
    }
}
