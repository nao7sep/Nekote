using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "stop" プロパティのポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatStopConverter))]
    internal abstract class OpenAiChatStopBaseDto
    {
    }
}
