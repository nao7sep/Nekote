using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 対数確率の情報。
    /// </summary>
    public class OpenAiChatLogprobsDto
    {
        /// <summary>
        /// トークンごとの対数確率のリスト。
        /// </summary>
        [JsonPropertyName("content")]
        public List<OpenAiChatLogprobContentDto>? Content { get; set; }

        /// <summary>
        /// 拒否トークンの対数確率のリスト。
        /// </summary>
        [JsonPropertyName("refusal")]
        public List<OpenAiChatLogprobContentDto>? Refusal { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
