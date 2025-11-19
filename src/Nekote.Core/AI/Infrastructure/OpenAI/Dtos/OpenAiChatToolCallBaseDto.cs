using System.Text.Json.Serialization;
using Nekote.Core.AI.Infrastructure.OpenAI.Converters;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツール呼び出しのポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatToolCallConverter))]
    public abstract class OpenAiChatToolCallBaseDto
    {
    }
}
