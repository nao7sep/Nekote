using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// コンテンツのセグメント。
    /// </summary>
    public class GeminiChatSegmentDto
    {
        /// <summary>
        /// 親 Content オブジェクト内の Part オブジェクトのインデックス（出力専用）。
        /// </summary>
        [JsonPropertyName("partIndex")]
        public int? PartIndex { get; set; }

        /// <summary>
        /// 指定された Part の開始インデックス（バイト単位、出力専用）。
        /// </summary>
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        /// <summary>
        /// 指定された Part の終了インデックス（バイト単位、排他的、出力専用）。
        /// </summary>
        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        /// <summary>
        /// レスポンスのセグメントに対応するテキスト（出力専用）。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
