using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// ツールの定義。
    /// </summary>
    public class GeminiChatToolDto
    {
        /// <summary>
        /// 関数呼び出しに使用できる関数宣言のリスト。
        /// </summary>
        [JsonPropertyName("functionDeclarations")]
        public List<GeminiChatFunctionDeclarationDto>? FunctionDeclarations { get; set; }

        /// <summary>
        /// Google 検索による根拠付けの設定。
        /// </summary>
        [JsonPropertyName("googleSearchRetrieval")]
        public GeminiChatGoogleSearchRetrievalDto? GoogleSearchRetrieval { get; set; }

        /// <summary>
        /// コード実行機能の設定。
        /// </summary>
        [JsonPropertyName("codeExecution")]
        public GeminiChatCodeExecutionDto? CodeExecution { get; set; }

        /// <summary>
        /// Google 検索機能の設定。
        /// </summary>
        [JsonPropertyName("googleSearch")]
        public GeminiChatGoogleSearchDto? GoogleSearch { get; set; }

        /// <summary>
        /// コンピュータ操作機能の設定。
        /// </summary>
        [JsonPropertyName("computerUse")]
        public GeminiChatComputerUseDto? ComputerUse { get; set; }

        /// <summary>
        /// URL コンテキスト機能の設定。
        /// </summary>
        [JsonPropertyName("urlContext")]
        public GeminiChatUrlContextDto? UrlContext { get; set; }

        /// <summary>
        /// ファイル検索機能の設定。
        /// </summary>
        [JsonPropertyName("fileSearch")]
        public GeminiChatFileSearchDto? FileSearch { get; set; }

        /// <summary>
        /// Google マップ機能の設定。
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
