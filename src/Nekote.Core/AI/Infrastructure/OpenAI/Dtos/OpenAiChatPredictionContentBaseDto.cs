using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "prediction" 内の "content" プロパティのポリモーフィックな値を表現するための抽象基底 DTO。
    /// </summary>
    [JsonConverter(typeof(OpenAiChatPredictionContentConverter))]
    internal abstract class OpenAiChatPredictionContentBaseDto
    {
    }
}
