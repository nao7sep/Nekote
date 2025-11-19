using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数レスポンス。
    /// </summary>
    public class GeminiChatFunctionResponseDto
    {
        /// <summary>
        /// このレスポンスが対象とする関数呼び出しの ID。
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// 呼び出す関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// JSON オブジェクト形式の関数のレスポンス。
        /// </summary>
        [JsonPropertyName("response")]
        public JsonElement? Response { get; set; }

        /// <summary>
        /// 関数レスポンスを構成する順序付き Parts。
        /// </summary>
        [JsonPropertyName("parts")]
        public List<GeminiChatFunctionResponsePartDto>? Parts { get; set; }

        /// <summary>
        /// 関数呼び出しが継続され、さらにレスポンスが返されることを示す。
        /// </summary>
        [JsonPropertyName("willContinue")]
        public bool? WillContinue { get; set; }

        /// <summary>
        /// 会話でレスポンスをスケジュールする方法。
        /// </summary>
        [JsonPropertyName("scheduling")]
        public string? Scheduling { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
