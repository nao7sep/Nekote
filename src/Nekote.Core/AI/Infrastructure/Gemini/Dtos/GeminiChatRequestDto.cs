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

        /// <summary>
        /// モデルが次のレスポンスの生成に使用できる Tools のリスト。
        /// </summary>
        [JsonPropertyName("tools")]
        public List<GeminiChatToolDto>? Tools { get; set; }

        /// <summary>
        /// リクエストで指定された Tool のツール構成。
        /// </summary>
        [JsonPropertyName("toolConfig")]
        public GeminiChatToolConfigDto? ToolConfig { get; set; }

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
