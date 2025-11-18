using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// トップログ確率 DTO。
    /// </summary>
    internal class OpenAiChatTopLogprobDto
    {
        /// <summary>
        /// トークン文字列。
        /// </summary>
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        /// <summary>
        /// ログ確率値。
        /// </summary>
        [JsonPropertyName("logprob")]
        public double? Logprob { get; set; }

        /// <summary>
        /// バイト値のリスト。
        /// </summary>
        [JsonPropertyName("bytes")]
        public List<int>? Bytes { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
