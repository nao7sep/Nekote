using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ログ確率情報 DTO。
    /// </summary>
    internal class OpenAiChatLogprobsDto
    {
        /// <summary>
        /// トークンごとのログ確率情報のリスト。
        /// </summary>
        [JsonPropertyName("content")]
        public List<OpenAiChatLogprobContentDto>? Content { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
