using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// オブジェクト形式の関数呼び出し選択 (特定の関数を強制)。
    /// </summary>
    [Obsolete("This class is deprecated. Use ToolChoice instead.")]
    public class OpenAiChatFunctionCallChoiceObjectDto : OpenAiChatFunctionCallChoiceBaseDto
    {
        /// <summary>
        /// 関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
