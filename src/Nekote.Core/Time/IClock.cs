using System;

namespace Nekote.Core.Time
{
    /// <summary>
    /// 時間の取得を抽象化し、テスト容易性を向上させるためのインターフェース。
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// 現在の日付と時刻をタイムゾーンオフセット付きで取得します。
        /// </summary>
        DateTimeOffset GetCurrentDateTimeOffset();

        /// <summary>
        /// 現在のUTC日付と時刻を取得します。
        /// </summary>
        DateTime GetCurrentUtcDateTime();

        /// <summary>
        /// 現在のローカル日付と時刻を取得します。
        /// </summary>
        DateTime GetCurrentLocalDateTime();
    }
}
