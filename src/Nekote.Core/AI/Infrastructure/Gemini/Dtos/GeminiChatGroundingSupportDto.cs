using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// グラウンディング サポート DTO。
    /// </summary>
    internal class GeminiChatGroundingSupportDto
    {
        /// <summary>
        /// クレームに関連付けられた引用を指定するインデックスのリスト。
        /// </summary>
        [JsonPropertyName("groundingChunkIndices")]
        public List<int>? GroundingChunkIndices { get; set; }

        /// <summary>
        /// サポート参照の信頼スコア（範囲は 0 ～ 1）。
        /// </summary>
        [JsonPropertyName("confidenceScores")]
        public List<double>? ConfidenceScores { get; set; }

        /// <summary>
        /// このサポートが属するコンテンツのセグメント。
        /// </summary>
        [JsonPropertyName("segment")]
        public GeminiChatSegmentDto? Segment { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
