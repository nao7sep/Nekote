using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nekote.Core.Time
{
    /// <summary>
    /// TimeSpanFormatKind に対応する書式文字列を管理します。
    /// </summary>
    public static class TimeSpanFormats
    {
        // TimeSpan.ToString() は合計時間 (例: 25時間) を
        // 直接フォーマットする機能がないため、ToString の実装はカスタムロジックで行います。
        // 一方、ParseExact は合計時間に対応しているため、ここでの書式文字列は主にパース処理で使用されます。
        private static readonly ImmutableDictionary<TimeSpanFormatKind, string> Map = new Dictionary<TimeSpanFormatKind, string>
        {
            [TimeSpanFormatKind.SortableSeconds] = @"hh\-mm\-ss",
            [TimeSpanFormatKind.SortableMilliseconds] = @"hh\-mm\-ss\-fff",
            [TimeSpanFormatKind.SortableTicks] = @"hh\-mm\-ss\-fffffff",

            [TimeSpanFormatKind.UserFriendlyMinutes] = @"h\:mm",
            [TimeSpanFormatKind.UserFriendlySeconds] = @"h\:mm\:ss",
            [TimeSpanFormatKind.UserFriendlyMilliseconds] = @"h\:mm\:ss\.fff",
            [TimeSpanFormatKind.UserFriendlyTicks] = @"h\:mm\:ss\.fffffff",
        }.ToImmutableDictionary();

        /// <summary>
        /// 指定された種類に対応する書式パターンを取得します。
        /// </summary>
        public static string GetFormatString(TimeSpanFormatKind kind) => Map[kind];
    }
}
