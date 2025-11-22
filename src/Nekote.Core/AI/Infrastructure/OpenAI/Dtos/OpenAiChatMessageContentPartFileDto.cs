using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ファイルタイプのコンテンツパーツ。
    /// </summary>
    public class OpenAiChatMessageContentPartFileDto : OpenAiChatMessageContentPartBaseDto
    {
        /// <summary>
        /// ファイル情報。
        /// </summary>
        [JsonPropertyName("file")]
        public OpenAiChatFileDto? File { get; set; }
    }
}
