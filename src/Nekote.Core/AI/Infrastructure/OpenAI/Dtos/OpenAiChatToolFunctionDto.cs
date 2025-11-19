using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "tools" が関数ツールの場合の具象 DTO。
    /// </summary>
    /// <remarks>
    /// レスポンスの生成に使用できる関数ツール。
    /// </remarks>
    public class OpenAiChatToolFunctionDto : OpenAiChatToolBaseDto
    {
        /// <summary>
        /// 関数の定義。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatToolFunctionDefinitionDto? Function { get; set; }

        /// <summary>
        /// ツールの種類 (関数ツールの場合は常に "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
