using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// Web 検索オプション DTO。
    /// </summary>
    public class OpenAiChatWebSearchOptionsDto
    {
        /// <summary>
        /// 検索に使用するコンテキストウィンドウスペースの量に関する高レベルのガイダンス ("low", "medium", "high")。
        /// </summary>
        [JsonPropertyName("search_context_size")]
        public string? SearchContextSize { get; set; }

        /// <summary>
        /// 検索のおおよその位置パラメータ。
        /// </summary>
        [JsonPropertyName("user_location")]
        public OpenAiChatUserLocationDto? UserLocation { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
