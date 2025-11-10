using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos
{
    /// <summary>
    /// OpenAI Chat Completions API のリクエスト DTO。
    /// すべてのプロパティは nullable として定義され、API の不整合に対応します。
    /// </summary>
    internal sealed class OpenAiChatRequestDto
    {
        [JsonPropertyName("model")]
        public string? Model { get; init; }

        [JsonPropertyName("messages")]
        public List<OpenAiMessageDto>? Messages { get; init; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        [JsonPropertyName("top_p")]
        public float? TopP { get; init; }

        [JsonPropertyName("stream")]
        public bool? Stream { get; init; }

        /// <summary>
        /// DTO で明示的に定義されていないすべての JSON プロパティを格納します。
        /// プロバイダー固有のパラメータをここに含めることができます。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
