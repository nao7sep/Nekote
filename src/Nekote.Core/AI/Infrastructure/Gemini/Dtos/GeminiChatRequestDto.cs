using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// Gemini Chat API へのリクエストボディ DTO。
    /// </summary>
    public class GeminiChatRequestDto
    {
        /// <summary>
        /// チャットコンテンツのリスト。
        /// </summary>
        [JsonPropertyName("contents")]
        public List<GeminiChatContentDto>? Contents { get; set; }

        // tools と toolConfig は、このライブラリが現在ツール関連の操作をサポートしていないため省略されている。
        // API からのレスポンスを解析する DTO ではデータの損失を防ぐためにこれらのフィールドをサポートするが、
        // リクエスト DTO ではツール関連のデータを送信しないため含まれていない。

        /// <summary>
        /// 安全性設定のリスト。
        /// </summary>
        [JsonPropertyName("safetySettings")]
        public List<GeminiChatSafetySettingDto>? SafetySettings { get; set; }

        /// <summary>
        /// システム指示。
        /// </summary>
        [JsonPropertyName("systemInstruction")]
        public GeminiChatContentDto? SystemInstruction { get; set; }

        /// <summary>
        /// 生成の構成。
        /// </summary>
        [JsonPropertyName("generationConfig")]
        public GeminiChatGenerationConfigDto? GenerationConfig { get; set; }

        /// <summary>
        /// キャッシュされたコンテンツの名前。
        /// </summary>
        [JsonPropertyName("cachedContent")]
        public string? CachedContent { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
