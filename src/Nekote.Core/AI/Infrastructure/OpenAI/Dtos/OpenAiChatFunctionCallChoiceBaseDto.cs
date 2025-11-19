using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 関数呼び出し選択の基底クラス (非推奨: tool_choice に置き換えられた)。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatFunctionCallChoiceConverter))]
    [Obsolete("This class is deprecated. Use ToolChoice instead.")]
    public abstract class OpenAiChatFunctionCallChoiceBaseDto
    {
    }
}
