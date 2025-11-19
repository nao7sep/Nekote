using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.Gemini.Dtos
{
    /// <summary>
    /// 関数呼び出しの構成。
    /// </summary>
    public class GeminiChatFunctionCallingConfigDto
    {
        /// <summary>
        /// 関数呼び出しを実行するモード。
        /// </summary>
        [JsonPropertyName("mode")]
        public string? Mode { get; set; }

        /// <summary>
        /// モデルが呼び出す関数を制限する関数名のセット。
        /// </summary>
        [JsonPropertyName("allowedFunctionNames")]
        public List<string>? AllowedFunctionNames { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
