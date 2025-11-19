using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール選択の基底クラス。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatToolChoiceConverter))]
    public abstract class OpenAiChatToolChoiceBaseDto
    {
    }
}
