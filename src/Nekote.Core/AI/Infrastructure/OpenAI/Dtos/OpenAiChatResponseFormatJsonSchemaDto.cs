using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// JSON スキーマレスポンスフォーマット。
    /// </summary>
    /// <remarks>
    /// 構造化された JSON 応答を生成するために使用される。Structured Outputs について詳しく学ぶ。
    /// </remarks>
    public class OpenAiChatResponseFormatJsonSchemaDto : OpenAiChatResponseFormatBaseDto
    {
        /// <summary>
        /// JSON スキーマを含む Structured Outputs 構成オプション。
        /// </summary>
        [JsonPropertyName("json_schema")]
        public OpenAiChatJsonSchemaDto? JsonSchema { get; set; }
    }
}
