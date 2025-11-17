using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Embedding API へのリクエストボディ DTO。
    /// </summary>
    internal class OpenAiEmbeddingRequestDto
    {
        /// <summary>
        /// 使用するモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// エンベディングを生成する入力テキスト (文字列または文字列の配列)。
        /// </summary>
        [JsonPropertyName("input")]
        public object? Input { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
