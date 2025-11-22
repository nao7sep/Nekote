namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "input" が単一の文字列の場合の具体的な DTO。
    /// </summary>
    public class OpenAiEmbeddingInputStringDto : OpenAiEmbeddingInputBaseDto
    {
        /// <summary>
        /// 入力テキスト。
        /// </summary>
        public string? Text { get; set; }
    }
}
