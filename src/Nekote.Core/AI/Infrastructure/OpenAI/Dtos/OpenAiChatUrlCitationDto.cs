using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// URL 引用情報。
    /// </summary>
    public class OpenAiChatUrlCitationDto
    {
        /// <summary>
        /// メッセージ内の URL 引用の最後の文字のインデックス。
        /// </summary>
        [JsonPropertyName("end_index")]
        public int? EndIndex { get; set; }

        /// <summary>
        /// メッセージ内の URL 引用の最初の文字のインデックス。
        /// </summary>
        [JsonPropertyName("start_index")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// Web リソースのタイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// Web リソースの URL。
        /// </summary>
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
