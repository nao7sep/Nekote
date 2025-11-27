using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// コンテンツ注釈（レスポンス解析用）。
    /// </summary>
    public class OpenAiChatAnnotationDto
    {
        /// <summary>
        /// 注釈のタイプ（現在は "url_citation" のみ）。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// URL 引用情報（type が "url_citation" の場合）。
        /// </summary>
        [JsonPropertyName("url_citation")]
        public OpenAiChatUrlCitationDto? UrlCitation { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
