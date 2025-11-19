using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ツールの詳細。
    /// </summary>
    public class GeminiChatToolDto
    {
        /// <summary>
        /// 関数呼び出しに使用できる関数宣言のリスト。
        /// </summary>
        [JsonPropertyName("functionDeclarations")]
        public List<GeminiChatFunctionDeclarationDto>? FunctionDeclarations { get; set; }

        /// <summary>
        /// Google 検索取得。
        /// </summary>
        [JsonPropertyName("googleSearchRetrieval")]
        public GeminiChatGoogleSearchRetrievalDto? GoogleSearchRetrieval { get; set; }

        /// <summary>
        /// コード実行。
        /// </summary>
        [JsonPropertyName("codeExecution")]
        public GeminiChatCodeExecutionDto? CodeExecution { get; set; }

        /// <summary>
        /// Google 検索。
        /// </summary>
        [JsonPropertyName("googleSearch")]
        public GeminiChatGoogleSearchDto? GoogleSearch { get; set; }

        /// <summary>
        /// コンピュータ使用。
        /// </summary>
        [JsonPropertyName("computerUse")]
        public GeminiChatComputerUseDto? ComputerUse { get; set; }

        /// <summary>
        /// URL コンテキスト。
        /// </summary>
        [JsonPropertyName("urlContext")]
        public GeminiChatUrlContextDto? UrlContext { get; set; }

        /// <summary>
        /// ファイル検索。
        /// </summary>
        [JsonPropertyName("fileSearch")]
        public GeminiChatFileSearchDto? FileSearch { get; set; }

        /// <summary>
        /// Google マップ。
        /// </summary>
        [JsonPropertyName("googleMaps")]
        public GeminiChatGoogleMapsDto? GoogleMaps { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
