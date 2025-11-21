using System.Text.Json;
using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Converters;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数呼び出し選択の基底クラス (非推奨: tool_choice に置き換えられた)。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatFunctionCallChoiceConverter))]
    [Obsolete("This class is deprecated. Use ToolChoice instead.")]
    public abstract class OpenAiChatFunctionCallChoiceBaseDto
    {
        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
