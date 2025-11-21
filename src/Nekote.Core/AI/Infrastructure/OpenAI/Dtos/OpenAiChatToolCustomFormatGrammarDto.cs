using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// ユーザー定義の文法による入力フォーマット。
    /// </summary>
    public class OpenAiChatToolCustomFormatGrammarDto : OpenAiChatToolCustomFormatBaseDto
    {
        /// <summary>
        /// 選択した文法の定義。
        /// </summary>
        [JsonPropertyName("grammar")]
        public OpenAiChatToolCustomGrammarDto? Grammar { get; set; }

        /// <summary>
        /// フォーマットのタイプ (常に "grammar")。
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }
}
