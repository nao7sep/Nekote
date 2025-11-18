using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "prediction" 内の "content" 配列のコンテンツパート DTO。
    /// </summary>
    public class OpenAiChatPredictionContentPartDto
    {
        // type は API ドキュメントでは各フィールドの最後に記載されているが、
        // コンテンツパーツのタイプを判別する際に最も重要な役割を果たすため、
        // 可読性を向上させるためにこのクラスでは最初に配置している。

        /// <summary>
        /// パーツの種類 (現在は "text" のみサポート)。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// type が "text" の場合にのみ使用するテキスト内容。
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
