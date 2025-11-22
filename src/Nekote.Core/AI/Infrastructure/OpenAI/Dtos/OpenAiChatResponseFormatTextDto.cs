using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// テキストレスポンスフォーマット。
    /// </summary>
    /// <remarks>
    /// デフォルトのレスポンスフォーマット。テキスト応答を生成するために使用される。
    /// このクラスは空だが、ポリモーフィック型階層における型識別子として機能する。
    /// JSON コンバーターが "type" フィールドに基づいて異なる型を逆シリアル化する際に使用される。
    /// </remarks>
    public class OpenAiChatResponseFormatTextDto : OpenAiChatResponseFormatBaseDto
    {
    }
}
