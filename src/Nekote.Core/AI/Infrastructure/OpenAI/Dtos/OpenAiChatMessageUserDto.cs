using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ユーザーロールのメッセージ DTO。
    /// </summary>
    /// <remarks>
    /// ユーザーからの入力を表す。
    /// </remarks>
    public class OpenAiChatMessageUserDto : OpenAiChatMessageBaseDto
    {
        /// <summary>
        /// メッセージの内容。
        /// リクエスト送信時は常に単純な文字列 content を使用する。
        /// レスポンス受信時は JsonConverter がこれを処理する。
        /// </summary>
        [JsonPropertyName("content")]
        public OpenAiChatMessageContentBaseDto? Content { get; set; }

        /// <summary>
        /// 参加者の名前 (省略可能)。同じ役割の参加者を区別するために使用される。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
