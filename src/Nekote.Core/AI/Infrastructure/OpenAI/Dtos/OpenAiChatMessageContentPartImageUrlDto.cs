using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// 画像 URL タイプの content part DTO。
    /// </summary>
    public class OpenAiChatMessageContentPartImageUrlDto : OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// 画像 URL 情報。
        /// </summary>
        [JsonPropertyName("image_url")]
        public OpenAiChatImageUrlDto? ImageUrl { get; set; }
    }
}
