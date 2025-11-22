using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// アトリビューションに貢献したソースの識別子。
    /// </summary>
    public class GeminiChatAttributionSourceIdDto
    {
        /// <summary>
        /// インライン パッセージの識別子。
        /// </summary>
        [JsonPropertyName("groundingPassage")]
        public GeminiChatGroundingPassageIdDto? GroundingPassage { get; set; }

        /// <summary>
        /// セマンティック リトリーバーで取得された Chunk の識別子。
        /// </summary>
        [JsonPropertyName("semanticRetrieverChunk")]
        public GeminiChatSemanticRetrieverChunkDto? SemanticRetrieverChunk { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
