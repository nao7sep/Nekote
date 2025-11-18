namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "input" がトークン配列の配列の場合の具象 DTO。
    /// </summary>
    internal class OpenAiEmbeddingInputTokenArrayDto : OpenAiEmbeddingInputBaseDto
    {
        /// <summary>
        /// トークン配列のリスト。
        /// </summary>
        public List<int[]>? TokenArrays { get; set; }
    }
}
