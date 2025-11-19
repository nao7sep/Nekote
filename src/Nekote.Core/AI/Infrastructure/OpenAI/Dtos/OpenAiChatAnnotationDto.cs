using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// コンテンツ注釈 DTO (レスポンス解析用)。
    /// </summary>
    public class OpenAiChatAnnotationDto
    {
        /// <summary>
        /// 注釈のタイプ ("url_citation", "file_citation", "file_path", "container_file_citation")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// URL 引用情報 (type が "url_citation" の場合)。
        /// </summary>
        [JsonPropertyName("url_citation")]
        public OpenAiChatUrlCitationDto? UrlCitation { get; set; }

        /// <summary>
        /// ファイル引用情報 (type が "file_citation" の場合)。
        /// </summary>
        [JsonPropertyName("file_citation")]
        public OpenAiChatFileCitationDto? FileCitation { get; set; }

        /// <summary>
        /// ファイルパス情報 (type が "file_path" の場合)。
        /// </summary>
        [JsonPropertyName("file_path")]
        public OpenAiChatFilePathDto? FilePath { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
