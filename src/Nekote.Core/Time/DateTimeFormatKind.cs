namespace Nekote.Core.Time
{
    /// <summary>
    /// 日付と時刻の文字列表現の種類を定義します。
    /// </summary>
    public enum DateTimeFormatKind
    {
        // --- Sortable Formats ---

        /// <summary>
        /// 並べ替え可能なローカル時刻: yyyyMMdd-HHmmss
        /// </summary>
        LocalSortable,

        /// <summary>
        /// 並べ替え可能なUTC時刻: yyyyMMddTHHmmssZ
        /// </summary>
        UtcSortable,

        /// <summary>
        /// ミリ秒付きの並べ替え可能なローカル時刻: yyyyMMdd-HHmmss-fff
        /// </summary>
        LocalSortableMilliseconds,

        /// <summary>
        /// ミリ秒付きの並べ替え可能なUTC時刻: yyyyMMddTHHmmss-fffZ
        /// </summary>
        UtcSortableMilliseconds,

        /// <summary>
        /// ティック付きの並べ替え可能なローカル時刻: yyyyMMdd-HHmmss-fffffff
        /// </summary>
        LocalSortableTicks,

        /// <summary>
        /// ティック付きの並べ替え可能なUTC時刻: yyyyMMddTHHmmss-fffffffZ
        /// </summary>
        UtcSortableTicks,

        /// <summary>
        /// タイムゾーン情報を含むラウンドトリップ書式。
        /// </summary>
        Roundtrip,

        // --- DateOnly & TimeOnly Formats ---

        /// <summary>
        /// 並べ替え可能な日付: yyyyMMdd
        /// </summary>
        DateSortable,

        /// <summary>
        /// 並べ替え可能な時刻: HHmmss
        /// </summary>
        TimeSortable,

        /// <summary>
        /// ミリ秒付きの並べ替え可能な時刻: HHmmss-fff
        /// </summary>
        TimeSortableMilliseconds,

        /// <summary>
        /// ティック付きの並べ替え可能な時刻: HHmmss-fffffff
        /// </summary>
        TimeSortableTicks,

        // --- User-Friendly Formats ---

        /// <summary>
        /// 人間が読みやすい、分までのローカル時刻書式: yyyy/M/d H:mm
        /// </summary>
        LocalUserFriendlyMinutes,

        /// <summary>
        /// 人間が読みやすい、秒までのローカル時刻書式: yyyy/M/d H:mm:ss
        /// </summary>
        LocalUserFriendlySeconds,

        /// <summary>
        /// 人間が読みやすい、ミリ秒までのローカル時刻書式: yyyy/M/d H:mm:ss.fff
        /// </summary>
        LocalUserFriendlyMilliseconds,

        /// <summary>
        /// 人間が読みやすい、ティックまでのローカル時刻書式: yyyy/M/d H:mm:ss.fffffff
        /// </summary>
        LocalUserFriendlyTicks,

        /// <summary>
        /// 人間が読みやすい、分までのUTC時刻書式: yyyy/M/d H:mm UTC
        /// </summary>
        UtcUserFriendlyMinutes,

        /// <summary>
        /// 人間が読みやすい、秒までのUTC時刻書式: yyyy/M/d H:mm:ss UTC
        /// </summary>
        UtcUserFriendlySeconds,

        /// <summary>
        /// 人間が読みやすい、ミリ秒までのUTC時刻書式: yyyy/M/d H:mm:ss.fff UTC
        /// </summary>
        UtcUserFriendlyMilliseconds,

        /// <summary>
        /// 人間が読みやすい、ティックまでのUTC時刻書式: yyyy/M/d H:mm:ss.fffffff UTC
        /// </summary>
        UtcUserFriendlyTicks,
    }
}
