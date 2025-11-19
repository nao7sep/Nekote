using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ツールの定義 (レガシー実装)。
    /// </summary>
    /// <remarks>
    /// このクラスは後方互換性のために残されている。
    /// 新しいコードでは <see cref="OpenAiChatToolBaseDto"/> とその派生クラスを使用すること。
    /// </remarks>
    [Obsolete("This class is legacy. Use OpenAiChatToolBaseDto and its derived classes instead.")]
    public class OpenAiChatToolDto
    {
        /// <summary>
        /// ツールの種類 (通常は "function")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// 関数の定義。
        /// </summary>
        [JsonPropertyName("function")]
        public OpenAiChatFunctionDto? Function { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
