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

        // 2025-11-18:
        // https://ai.google.dev/api/generate-content の RetrievedContext セクションには、
        // fileSearchStore フィールドが JSON 例には出現するが、ドキュメント本文には記載されていない。
        // これは Google の File Search 機能（https://ai.google.dev/gemini-api/docs/file-search）に関連すると思われる。
        // このフィールドは未記載であり、かつチャット・エンベディング以外の機能に関連するため、現時点では省略されている。

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
