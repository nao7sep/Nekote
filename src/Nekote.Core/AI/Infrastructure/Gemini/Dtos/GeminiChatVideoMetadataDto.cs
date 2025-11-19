using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 動画メタデータ。
    /// </summary>
    public class GeminiChatVideoMetadataDto
    {
        /// <summary>
        /// 動画の開始オフセット。
        /// </summary>
        [JsonPropertyName("startOffset")]
        public string? StartOffset { get; set; }

        /// <summary>
        /// 動画の終了オフセット。
        /// </summary>
        [JsonPropertyName("endOffset")]
        public string? EndOffset { get; set; }

        /// <summary>
        /// モデルに送信された動画のフレームレート。
        /// </summary>
        [JsonPropertyName("fps")]
        public double? Fps { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
