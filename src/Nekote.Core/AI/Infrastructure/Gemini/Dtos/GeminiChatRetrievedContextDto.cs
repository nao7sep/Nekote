using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ファイル検索ツールで取得されたコンテキストのグラウンディング チャンク DTO。
    /// </summary>
    internal class GeminiChatRetrievedContextDto
    {
        /// <summary>
        /// セマンティック検索ドキュメントの URI 参照。
        /// </summary>
        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        /// <summary>
        /// ドキュメントのタイトル。
        /// </summary>
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        /// <summary>
        /// チャンクのテキスト。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
