using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツールロールのメッセージ DTO。
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

        /// <summary>
        /// 参加者の名前 (省略可能)。同じ役割の参加者を区別するために使用される。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
