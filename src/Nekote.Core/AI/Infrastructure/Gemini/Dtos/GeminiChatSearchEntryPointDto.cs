using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Google 検索のエントリ ポイント DTO。
    /// </summary>
    internal class GeminiChatSearchEntryPointDto
    {
        /// <summary>
        /// ウェブページまたはアプリの WebView に埋め込むことができるウェブ コンテンツ スニペット。
        /// </summary>
        [JsonPropertyName("renderedContent")]
        public string? RenderedContent { get; set; }

        /// <summary>
        /// 検索語句と検索 URL タプルの配列を表す Base64 エンコードされた JSON。
        /// </summary>
        [JsonPropertyName("sdkBlob")]
        public string? SdkBlob { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
