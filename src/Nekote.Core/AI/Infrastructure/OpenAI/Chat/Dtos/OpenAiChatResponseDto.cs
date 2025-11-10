using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI Chat Completions API のレスポンス DTO。
    /// すべてのプロパティは nullable として定義され、API の不整合に対応します。
    /// </summary>
    internal sealed class OpenAiChatResponseDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("object")]
        public string? Object { get; init; }

        [JsonPropertyName("created")]
        public long? Created { get; init; }

        [JsonPropertyName("model")]
        public string? Model { get; init; }

        [JsonPropertyName("choices")]
        public List<OpenAiChoiceDto>? Choices { get; init; }

        [JsonPropertyName("usage")]
        public OpenAiUsageDto? Usage { get; init; }

        /// <summary>
        /// DTO で明示的に定義されていないすべての JSON プロパティを格納します。
        /// API が新しいフィールドを追加した場合、ここに保存されます。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
