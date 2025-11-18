using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミングレスポンスのオプション DTO。
    /// </summary>
    internal class OpenAiChatStreamOptionsDto
    {
        /// <summary>
        /// ストリーム終了時に使用量を含めるかどうか。
        /// </summary>
        [JsonPropertyName("include_usage")]
        public bool? IncludeUsage { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
