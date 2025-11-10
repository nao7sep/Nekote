using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI レスポンスの選択肢を表す DTO。
    /// </summary>
    internal sealed class OpenAiChoiceDto
    {
        [JsonPropertyName("index")]
        public int? Index { get; init; }

        [JsonPropertyName("message")]
        public OpenAiMessageDto? Message { get; init; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; init; }

        /// <summary>
        /// DTO で明示的に定義されていないすべての JSON プロパティを格納します。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
