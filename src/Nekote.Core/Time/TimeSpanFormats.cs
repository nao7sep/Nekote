using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nekote.Core.Time
{
    /// <summary>
    /// TimeSpanFormatKind に対応する書式文字列を管理します。
    /// 注意: 1日未満（24時間未満）の TimeSpan のみサポートします。
    /// </summary>
    public static class TimeSpanFormats
    {
        // 1日未満の TimeSpan に対して、標準的な書式文字列を使用します。
        // ToString と ParseExact の両方でこれらの書式文字列を使用できます。
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
