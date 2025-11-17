using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Chat API へのリクエストボディ DTO。
    /// </summary>
    internal class OpenAiChatRequestDto
    {
        /// <summary>
        /// 使用するモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// チャットメッセージのリスト。
        /// </summary>
        [JsonPropertyName("messages")]
        public List<OpenAiChatMessageDto>? Messages { get; set; }

        /// <summary>
        /// ストリーミングレスポンスを有効にするかどうか。
        /// </summary>
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
