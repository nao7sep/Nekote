using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "content" 配列内の "part" オブジェクト DTO。
    /// </summary>
    public class OpenAiChatMessageContentPartDto
    {
        /// <summary>
        /// type が "text" の場合にのみ使用するテキスト内容。
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        /// <summary>
        /// パーツの種類 ("text", "image_url", "input_audio", "file", "refusal")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// type が "image_url" の場合にのみ使用する画像 URL 情報。
        /// </summary>
        [JsonPropertyName("image_url")]
        public OpenAiChatImageUrlDto? ImageUrl { get; set; }

        /// <summary>
        /// type が "input_audio" の場合にのみ使用するオーディオ入力情報。
        /// </summary>
        [JsonPropertyName("input_audio")]
        public OpenAiChatInputAudioDto? InputAudio { get; set; }

        /// <summary>
        /// type が "file" の場合にのみ使用するファイル情報。
        /// </summary>
        [JsonPropertyName("file")]
        public OpenAiChatFileDto? File { get; set; }

        /// <summary>
        /// type が "refusal" の場合にのみ使用する拒否メッセージ。
        /// </summary>
        [JsonPropertyName("refusal")]
        public string? Refusal { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
