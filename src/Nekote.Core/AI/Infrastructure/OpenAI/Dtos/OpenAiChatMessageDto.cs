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
        /// メッセージのロール ("system", "user", "assistant")。
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
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
