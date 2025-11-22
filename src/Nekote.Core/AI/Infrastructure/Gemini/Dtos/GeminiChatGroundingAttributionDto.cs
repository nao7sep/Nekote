using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 回答に貢献した情報源の帰属。
    /// </summary>
    public class GeminiChatGroundingAttributionDto
    {
        /// <summary>
        /// このアトリビューションに貢献したソースの識別子（出力専用）。
        /// </summary>
        [JsonPropertyName("sourceId")]
        public GeminiChatAttributionSourceIdDto? SourceId { get; set; }

        /// <summary>
        /// この帰属を構成する引用元のコンテンツ。
        /// </summary>
        [JsonPropertyName("content")]
        public GeminiChatContentDto? Content { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
