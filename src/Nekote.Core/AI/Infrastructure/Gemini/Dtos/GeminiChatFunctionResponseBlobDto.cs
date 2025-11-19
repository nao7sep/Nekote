using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数レスポンスの Blob。
    /// </summary>
    public class GeminiChatFunctionResponseBlobDto
    {
        /// <summary>
        /// MIME タイプ。
        /// </summary>
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        /// <summary>
        /// Base64 でエンコードされたデータ。
        /// </summary>
        [JsonPropertyName("data")]
        public string? Data { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
