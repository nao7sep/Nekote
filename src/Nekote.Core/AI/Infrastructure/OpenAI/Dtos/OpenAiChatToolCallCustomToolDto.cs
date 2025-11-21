using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// モデルによって作成されたカスタムツールの呼び出し。
    /// </summary>
    public class OpenAiChatToolCallCustomToolDto : OpenAiChatToolCallBaseDto
    {
        /// <summary>
        /// モデルが呼び出したカスタムツール。
        /// </summary>
        [JsonPropertyName("custom")]
        public OpenAiChatToolCallCustomDto? Custom { get; set; }
    }
}
