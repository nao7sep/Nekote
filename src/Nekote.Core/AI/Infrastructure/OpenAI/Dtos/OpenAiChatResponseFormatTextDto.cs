using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// テキストレスポンスフォーマット DTO。
    /// </summary>
    /// <remarks>
    /// デフォルトのレスポンスフォーマット。テキスト応答を生成するために使用される。
    /// </remarks>
    public class OpenAiChatResponseFormatTextDto : OpenAiChatResponseFormatBaseDto
    {
    }
}
