using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 時間間隔。
    /// </summary>
    public class GeminiChatIntervalDto
    {
        /// <summary>
        /// 時間間隔の開始日時。
        /// </summary>
        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }

        /// <summary>
        /// 時間間隔の終了日時。
        /// </summary>
        [JsonPropertyName("endTime")]
        public string? EndTime { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
