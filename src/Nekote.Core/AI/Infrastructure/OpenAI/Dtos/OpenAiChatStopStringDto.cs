namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "stop" が単純な文字列の場合の具象 DTO。
    /// </summary>
    internal class OpenAiChatStopStringDto : OpenAiChatStopBaseDto
    {
        /// <summary>
        /// 停止シーケンス。
        /// </summary>
        public string? Sequence { get; set; }
    }
}
