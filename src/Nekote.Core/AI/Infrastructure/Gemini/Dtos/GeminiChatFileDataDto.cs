using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// URI ベースのデータ。
    /// </summary>
    public class GeminiChatFileDataDto
    {
        /// <summary>
        /// MIME タイプ。
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        /// <summary>
        /// URI。
        /// </summary>
        [JsonPropertyName("fileUri")]
        public string? FileUri { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
