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
    }
}
