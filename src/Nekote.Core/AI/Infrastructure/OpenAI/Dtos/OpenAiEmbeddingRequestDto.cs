using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// OpenAI Embedding API へのリクエストボディ DTO。
    /// </summary>
    public class OpenAiEmbeddingRequestDto
    {
        /// <summary>
        /// エンベディングを生成する入力テキスト（文字列、文字列の配列、またはトークン配列の配列）。
        /// </summary>
        [JsonPropertyName("input")]
        public OpenAiEmbeddingInputBaseDto? Input { get; set; }

        /// <summary>
        /// 使用するモデルの識別子。
        /// </summary>
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        /// <summary>
        /// 出力エンベディングの次元数（text-embedding-3 以降のモデルでサポート）。
        /// </summary>
        [JsonPropertyName("dimensions")]
        public int? Dimensions { get; set; }

        /// <summary>
        /// エンベディングを返す形式（"float" または "base64"）。
        /// </summary>
        [JsonPropertyName("encoding_format")]
        public string? EncodingFormat { get; set; }

        /// <summary>
        /// エンドユーザーを表す一意の識別子。悪用の監視と検出に役立つ。
        /// </summary>
        [JsonPropertyName("user")]
        public string? User { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
