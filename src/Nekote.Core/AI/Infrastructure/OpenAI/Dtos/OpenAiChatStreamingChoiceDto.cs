using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミングレスポンスの "choice" オブジェクト。
    /// </summary>
    public class OpenAiChatStreamingChoiceDto
    {
        /// <summary>
        /// ストリーミング時は "message" ではなく "delta" が使用される。
        /// </summary>
        [JsonPropertyName("delta")]
        public OpenAiChatStreamingDeltaDto? Delta { get; set; }

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
        /// 対数確率情報。
        /// </summary>
        [JsonPropertyName("logprobs")]
        public OpenAiChatLogprobsDto? Logprobs { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
