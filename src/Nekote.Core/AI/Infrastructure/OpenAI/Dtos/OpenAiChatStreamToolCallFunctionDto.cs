using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ストリーミング時のツール呼び出しにおける関数情報の増分部分。
    /// </summary>
    public class OpenAiChatStreamToolCallFunctionDto
    {
        /// <summary>
        /// モデルによって JSON 形式で生成された関数を呼び出すための引数。
        /// モデルは常に有効な JSON を生成するとは限らず、関数スキーマで定義されていないパラメータを幻視する可能性がある。
        /// </summary>
        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }

        /// <summary>
        /// 呼び出す関数の名前。
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// API から返される未知のフィールドを保持する。
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
