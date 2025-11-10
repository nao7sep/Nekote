using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI メッセージの DTO。
    /// </summary>
    internal sealed class OpenAiMessageDto
    {
        [JsonPropertyName("role")]
        public string? Role { get; init; }

        [JsonPropertyName("content")]
        public string? Content { get; init; }

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        /// <summary>
        /// DTO で明示的に定義されていないすべての JSON プロパティを格納します。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
