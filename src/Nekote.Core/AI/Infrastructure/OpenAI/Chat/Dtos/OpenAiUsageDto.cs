using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI のトークン使用量を表す DTO。
    /// </summary>
    internal sealed class OpenAiUsageDto
    {
        [JsonPropertyName("prompt_tokens")]
        public int? PromptTokens { get; init; }

        [JsonPropertyName("completion_tokens")]
        public int? CompletionTokens { get; init; }

        [JsonPropertyName("total_tokens")]
        public int? TotalTokens { get; init; }

        /// <summary>
        /// DTO で明示的に定義されていないすべての JSON プロパティを格納します。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
