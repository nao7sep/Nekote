using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 画像生成の構成 DTO。
    /// </summary>
    internal class GeminiChatImageConfigDto
    {
        /// <summary>
        /// アスペクト比。
        /// </summary>
        [JsonPropertyName("aspectRatio")]
        public string? AspectRatio { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
