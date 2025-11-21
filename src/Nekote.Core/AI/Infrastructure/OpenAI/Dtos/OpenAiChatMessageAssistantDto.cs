using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// アシスタントロールのメッセージ DTO。
    /// </summary>
    /// <remarks>
    /// モデルからの応答を表す。
    /// </remarks>
    public class OpenAiChatMessageAssistantDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public OpenAiChatMessageContentBaseDto? Content { get; set; }

        /// <summary>
        /// オーディオ情報。
        /// レスポンス受信時はすべてのフィールドが含まれる。
        /// リクエスト送信時は id のみを使用して以前のオーディオを参照する。
        /// </summary>
        [JsonPropertyName("audio")]
        public OpenAiChatAudioDto? Audio { get; set; }

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
        /// 関数呼び出し情報 (非推奨: tool_calls に置き換えられた)。
        /// </summary>
        [JsonPropertyName("function_call")]
        [Obsolete("This field is deprecated. Use ToolCalls instead.")]
        public OpenAiChatFunctionCallDto? FunctionCall { get; set; }

        /// <summary>
        /// ツール呼び出し情報のリスト。
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<OpenAiChatToolCallBaseDto>? ToolCalls { get; set; }
    }
}
