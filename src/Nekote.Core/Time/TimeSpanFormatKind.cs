namespace Nekote.Core.Time
{
    /// <summary>
    /// TimeSpan の文字列表現の種類を定義します。
    /// 注意: 1日未満 (24時間未満) の TimeSpan のみサポートします。
    /// </summary>
    public enum TimeSpanFormatKind
    {
        /// <summary>
        /// 並べ替え可能でパスセーフな書式 (秒まで)。例: 23-59-59
        /// </summary>
        SortableSeconds,

        /// <summary>
        /// 並べ替え可能でパスセーフな書式 (ミリ秒まで)。例: 23-59-59-123
        /// </summary>
        SortableMilliseconds,

        /// <summary>
        /// 並べ替え可能でパスセーフな書式 (ティックまで)。例: 23-59-59-1234567
        /// </summary>
        SortableTicks,

        /// <summary>
        /// 人間が読みやすい書式 (分まで)。例: 23:59
        /// </summary>
        UserFriendlyMinutes,

        /// <summary>
        /// 人間が読みやすい書式 (秒まで)。例: 23:59:59
        /// </summary>
        UserFriendlySeconds,

        /// <summary>
        /// 人間が読みやすい書式 (ミリ秒まで)。例: 23:59:59.123
        /// </summary>
        UserFriendlyMilliseconds,

        /// <summary>
        /// 人間が読みやすい書式 (ティックまで)。例: 23:59:59.1234567
        /// </summary>
        UserFriendlyTicks,
    }
}
