using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Converters;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" プロパティのポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatMessageContentConverter))]
    public abstract class OpenAiChatMessageContentBaseDto
    {
    }
}
