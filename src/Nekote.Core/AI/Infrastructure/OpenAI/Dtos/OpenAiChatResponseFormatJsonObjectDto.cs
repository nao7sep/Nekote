using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// JSON オブジェクトレスポンスフォーマット。
    /// </summary>
    /// <remarks>
    /// JSON 応答を生成する古い方法。json_schema をサポートするモデルには json_schema の使用が推奨される。
    /// モデルに JSON を生成するよう指示するシステムまたはユーザーメッセージがないと、モデルは JSON を生成しないことに注意。
    /// このクラスは空だが、ポリモーフィック型階層における型識別子として機能する。
    /// JSON コンバーターが "type" フィールドに基づいて異なる型を逆シリアル化する際に使用される。
    /// </remarks>
    public class OpenAiChatResponseFormatJsonObjectDto : OpenAiChatResponseFormatBaseDto
    {
    }
}
