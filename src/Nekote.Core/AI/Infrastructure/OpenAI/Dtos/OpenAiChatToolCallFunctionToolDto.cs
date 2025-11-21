using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// モデルによって作成された関数ツールの呼び出し。
    /// </summary>
    public class OpenAiChatToolCallFunctionToolDto : OpenAiChatToolCallBaseDto
    {
        /// <summary>
        /// モデルが呼び出した関数。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolCallFunctionDto? Function { get; set; }
    }
}
