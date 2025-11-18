using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ファイル入力を表す DTO。
    /// </summary>
    internal class OpenAiChatFileDto
    {
        /// <summary>
        /// Base64 エンコードされたファイルデータ。
        /// </summary>
        [JsonPropertyName("file_data")]
        public string? FileData { get; set; }

        /// <summary>
        /// アップロードされたファイルの ID。
        /// </summary>
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }

        /// <summary>
        /// ファイル名。
        /// </summary>
        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
