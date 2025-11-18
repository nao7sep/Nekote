using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API のメッセージ DTO (リクエスト/レスポンス共通部)。
    /// </summary>
    internal class OpenAiChatMessageDto
    {
        /// <summary>
        /// メッセージのロール ("system", "user", "assistant", "developer", "tool")。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// メッセージの内容。
        /// リクエスト送信時は常に単純な文字列 content を使用する。
        /// レスポンス受信時は JsonConverter がこれを処理する。
        /// </summary>
        [JsonPropertyName("content")]
        [JsonConverter(typeof(OpenAiChatMessageContentConverter))]
        public OpenAiChatMessageContentBaseDto? Content { get; set; }

        /// <summary>
        /// モデルが生成を拒否した場合の理由。
        /// </summary>
        [JsonPropertyName("refusal")]
        public string? Refusal { get; set; }

        /// <summary>
        /// コンテンツに対する注釈のリスト。
        /// </summary>
        [JsonPropertyName("annotations")]
        public List<OpenAiChatAnnotationDto>? Annotations { get; set; }

        /// <summary>
        /// モデルが呼び出すツール呼び出しのリスト。
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<OpenAiChatToolCallDto>? ToolCalls { get; set; }

        /// <summary>
        /// 関数呼び出し情報 (非推奨、tool_calls を使用)。
        /// </summary>
        [JsonPropertyName("function_call")]
        public OpenAiChatFunctionCallDto? FunctionCall { get; set; }

        /// <summary>
        /// このメッセージの送信者名。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// オーディオ情報。
        /// </summary>
        [JsonPropertyName("audio")]
        public object? Audio { get; set; }

        /// <summary>
        /// コンテンツパーツのリスト。
        /// </summary>
        [JsonPropertyName("content_parts")]
        public List<object>? ContentParts { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
