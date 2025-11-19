using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 制約のない自由形式テキスト入力フォーマット。
    /// </summary>
    public class OpenAiChatToolCustomFormatTextDto : OpenAiChatToolCustomFormatBaseDto
    {
        /// <summary>
        /// フォーマットのタイプ (常に "text")。
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
