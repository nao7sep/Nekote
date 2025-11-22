using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// コンテンツのソース帰属情報のコレクション。
    /// </summary>
    public class GeminiChatCitationMetadataDto
    {
        /// <summary>
        /// 特定の回答のソースへの引用。
        /// </summary>
        [JsonPropertyName("citationSources")]
        public List<GeminiChatCitationSourceDto>? CitationSources { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
