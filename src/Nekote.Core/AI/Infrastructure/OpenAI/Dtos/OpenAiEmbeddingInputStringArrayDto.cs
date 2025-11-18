namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "input" が文字列の配列の場合の具象 DTO。
    /// </summary>
    public class OpenAiEmbeddingInputStringArrayDto : OpenAiEmbeddingInputBaseDto
    {
        /// <summary>
        /// 入力テキストのリスト。
        /// </summary>
        public List<string>? Texts { get; set; }
    }
}
