using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミングツール呼び出しの増分 DTO (レスポンス解析用)。
    /// </summary>
    internal class OpenAiChatStreamDeltaToolCallDto
    {
        /// <summary>
        /// ツール呼び出しのインデックス。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// ツール呼び出しの一意識別子。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールのタイプ ("function" など)。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 関数呼び出し情報の増分部分。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatStreamDeltaFunctionCallDto? Function { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
