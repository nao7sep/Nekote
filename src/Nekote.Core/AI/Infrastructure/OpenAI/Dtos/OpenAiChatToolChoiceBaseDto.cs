using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tool_choice" プロパティのポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatToolChoiceConverter))]
    public abstract class OpenAiChatToolChoiceBaseDto
    {
    }
}
