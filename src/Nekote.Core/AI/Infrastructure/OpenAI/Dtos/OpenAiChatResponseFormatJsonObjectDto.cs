using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// JSON オブジェクトレスポンスフォーマット DTO。
    /// </summary>
    /// <remarks>
    /// JSON 応答を生成する古い方法。json_schema をサポートするモデルには json_schema の使用が推奨される。
    /// モデルに JSON を生成するよう指示するシステムまたはユーザーメッセージがないと、モデルは JSON を生成しないことに注意。
    /// </remarks>
    public class OpenAiChatResponseFormatJsonObjectDto : OpenAiChatResponseFormatBaseDto
    {
    }
}
