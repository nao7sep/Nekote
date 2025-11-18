using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ユーザーのおおよその位置情報パラメータ DTO。
    /// </summary>
    internal class OpenAiChatApproximateLocationDto
    {
        /// <summary>
        /// ユーザーの都市 (例: "San Francisco")。
        /// </summary>
        [JsonPropertyName("city")]
        public string? City { get; set; }

        /// <summary>
        /// ユーザーの2文字の ISO 国コード (例: "US")。
        /// </summary>
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        /// <summary>
        /// ユーザーの地域 (例: "California")。
        /// </summary>
        [JsonPropertyName("region")]
        public string? Region { get; set; }

        /// <summary>
        /// ユーザーの IANA タイムゾーン (例: "America/Los_Angeles")。
        /// </summary>
        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
