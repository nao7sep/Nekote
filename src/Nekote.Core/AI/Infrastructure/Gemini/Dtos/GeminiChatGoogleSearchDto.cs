using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google 検索ツール。
    /// </summary>
    public class GeminiChatGoogleSearchDto
    {
        /// <summary>
        /// 時間範囲フィルタ。
        /// </summary>
        [JsonPropertyName("timeRangeFilter")]
        public GeminiChatIntervalDto? TimeRangeFilter { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
