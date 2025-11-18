using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 予測出力の構成 DTO。
    /// </summary>
    public class OpenAiChatPredictionDto
    {
        /// <summary>
        /// 予測コンテンツ。
        /// リクエスト送信時は単純な文字列またはコンテンツパーツの配列を使用する。
        /// </summary>
        [JsonPropertyName("content")]
        [JsonConverter(typeof(OpenAiChatPredictionContentConverter))]
        public OpenAiChatPredictionContentBaseDto? Content { get; set; }

        /// <summary>
        /// 予測のタイプ (現在は常に "content")。
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
