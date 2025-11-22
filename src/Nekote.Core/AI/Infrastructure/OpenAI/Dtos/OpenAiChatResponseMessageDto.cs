using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// レスポンスのチャット補完メッセージ。
    /// </summary>
    /// <remarks>
    /// モデルによって生成されたチャット補完メッセージ。
    /// リクエストメッセージ (OpenAiChatMessage*Dto) とは構造が異なる。
    /// </remarks>
    public class OpenAiChatResponseMessageDto
    {
        /// <summary>
        /// メッセージの内容。
        /// </summary>
        [JsonPropertyName("content")]
        public string? Content { get; set; }

        /// <summary>
        /// モデルが生成した拒否メッセージ。
        /// </summary>
        [JsonPropertyName("refusal")]
        public string? Refusal { get; set; }

        /// <summary>
        /// このメッセージの作者の役割。
        /// </summary>
        [JsonPropertyName("role")]
        public string? Role { get; set; }

        /// <summary>
        /// メッセージの注釈 (Web 検索ツール使用時など)。
        /// </summary>
        [JsonPropertyName("annotations")]
        public List<OpenAiChatAnnotationDto>? Annotations { get; set; }

        /// <summary>
        /// オーディオ出力モダリティがリクエストされた場合のオーディオ応答データ。
        /// </summary>
        [JsonPropertyName("audio")]
        public OpenAiChatAudioDto? Audio { get; set; }

        /// <summary>
        /// 関数呼び出し情報 (非推奨: tool_calls に置き換えられた)。
        /// </summary>
        [JsonPropertyName("function_call")]
        [Obsolete("This field is deprecated. Use ToolCalls instead.")]
        public OpenAiChatFunctionCallDto? FunctionCall { get; set; }

        /// <summary>
        /// モデルが生成したツール呼び出し (関数呼び出しなど)。
        /// </summary>
        [JsonPropertyName("tool_calls")]
        public List<OpenAiChatToolCallBaseDto>? ToolCalls { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
