using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// レスポンスの "choice" オブジェクト DTO。
    /// </summary>
    internal class OpenAiChatChoiceDto
    {
        /// <summary>
        /// 補完が終了した理由 ("stop", "length", "content_filter" など)。
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        /// <summary>
        /// この候補のインデックス。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// ログ確率情報。
        /// </summary>
        [JsonPropertyName("logprobs")]
        public OpenAiChatLogprobsDto? Logprobs { get; set; }

        /// <summary>
        /// 生成されたメッセージ。
        /// </summary>
        [JsonPropertyName("message")]
        public OpenAiChatMessageDto? Message { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
