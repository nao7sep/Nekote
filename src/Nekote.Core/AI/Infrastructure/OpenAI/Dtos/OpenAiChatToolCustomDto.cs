using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tools" がカスタムツールの場合の具体的な DTO。
    /// </summary>
    /// <remarks>
    /// 指定された形式を使用して入力を処理するカスタムツール。
    /// </remarks>
    public class OpenAiChatToolCustomDto : OpenAiChatToolBaseDto
    {
        /// <summary>
        /// カスタムツールのプロパティ。
        /// </summary>
        [JsonPropertyName("custom")]
        public OpenAiChatToolCustomDefinitionDto? Custom { get; set; }
    }
}
