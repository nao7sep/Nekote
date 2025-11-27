using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミング時のツール呼び出し情報の増分。
    /// </summary>
    public class OpenAiChatStreamingToolCallDto
    {
        /// <summary>
        /// ツール呼び出し配列内のインデックス。
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// 関数呼び出し情報の増分。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatStreamingToolCallFunctionDto? Function { get; set; }

        /// <summary>
        /// ツール呼び出しの ID。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// ツールのタイプ（現在は "function" のみサポート）。
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
