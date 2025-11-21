using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Converters;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// カスタムツールの入力フォーマットの基底クラス。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatToolCustomFormatConverter))]
    public abstract class OpenAiChatToolCustomFormatBaseDto
    {
        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
