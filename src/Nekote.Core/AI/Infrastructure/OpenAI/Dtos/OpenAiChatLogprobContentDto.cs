using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// トークンごとの対数確率情報。
    /// </summary>
    public class OpenAiChatLogprobContentDto
    {
        /// <summary>
        /// バイト値のリスト。
        /// </summary>
        [JsonPropertyName("bytes")]
        public List<int>? Bytes { get; set; }

        /// <summary>
        /// 対数確率値。
        /// </summary>
        [JsonPropertyName("logprob")]
        public double? Logprob { get; set; }

        /// <summary>
        /// トークン文字列。
        /// </summary>
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        /// <summary>
        /// トップ対数確率のリスト。
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public List<OpenAiChatTopLogprobDto>? TopLogprobs { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
