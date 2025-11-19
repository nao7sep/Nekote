using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミングチャンクの差分 (delta) DTO。
    /// </summary>
    public class OpenAiChatStreamDeltaDto
    {
        /// <summary>
        /// メッセージ内容の増分部分。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// 関数呼び出し情報の増分部分 (非推奨: tool_calls に置き換えられた)。
        /// </summary>
        [JsonPropertyName("function_call")]
        public OpenAiChatFunctionCallDto? FunctionCall { get; set; }

        /// <summary>
        /// ツール呼び出し情報の増分部分のリスト。
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<OpenAiChatToolCallDto>? ToolCalls { get; set; }

        /// <summary>
        /// モデルが生成を拒否した場合の理由。
        /// </summary>
        [JsonPropertyName("refusal")]
        public string? Refusal { get; set; }

        /// <summary>
        /// メッセージのロール (最初のチャンクでのみ返される)。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
