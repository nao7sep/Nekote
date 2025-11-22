using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 対数確率とトークンの候補情報。
    /// </summary>
    public class GeminiChatLogprobsCandidateDto
    {
        /// <summary>
        /// 候補のトークン文字列値。
        /// </summary>
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        /// <summary>
        /// 候補のトークン ID 値。
        /// </summary>
        [JsonPropertyName("tokenId")]
        public int? TokenId { get; set; }

        /// <summary>
        /// 候補の対数確率。
        /// </summary>
        [JsonPropertyName("logProbability")]
        public double? LogProbability { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
