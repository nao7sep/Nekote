namespace Nekote.Core.AI.Infrastructure.OpenAI.Dtos
{
    /// <summary>
    /// "stop" が文字列の配列の場合の具体的な DTO（最大4個）。
    /// </summary>
    public class OpenAiChatStopArrayDto : OpenAiChatStopBaseDto
    {
        /// <summary>
        /// 停止シーケンスのリスト。
        /// </summary>
        public List<string>? Sequences { get; set; }
    }
}
