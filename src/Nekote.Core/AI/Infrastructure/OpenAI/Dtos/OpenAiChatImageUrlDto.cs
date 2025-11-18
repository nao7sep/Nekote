using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 画像 URL の詳細を表す DTO。
    /// </summary>
    internal class OpenAiChatImageUrlDto
    {
        /// <summary>
        /// 画像の URL または Base64 エンコードされた画像データ。
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// 画像の詳細レベル ("low", "high", "auto")。
        /// </summary>
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
