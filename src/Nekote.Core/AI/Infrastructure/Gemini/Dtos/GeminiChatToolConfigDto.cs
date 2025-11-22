using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ツールの設定。
    /// </summary>
    public class GeminiChatToolConfigDto
    {
        /// <summary>
        /// 関数呼び出しの設定。
        /// </summary>
        [JsonPropertyName("functionCallingConfig")]
        public GeminiChatFunctionCallingConfigDto? FunctionCallingConfig { get; set; }

        /// <summary>
        /// 情報取得の設定。
        /// </summary>
        [JsonPropertyName("retrievalConfig")]
        public GeminiChatRetrievalConfigDto? RetrievalConfig { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
