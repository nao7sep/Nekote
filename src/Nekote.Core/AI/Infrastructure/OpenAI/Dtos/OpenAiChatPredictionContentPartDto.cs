using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "prediction" 内の "content" 配列のコンテンツパート。
    /// </summary>
    public class OpenAiChatPredictionContentPartDto
    {
        /// <summary>
        /// type が "text" の場合にのみ使用するテキスト内容。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// パーツの種類（現在は "text" のみサポート）。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
