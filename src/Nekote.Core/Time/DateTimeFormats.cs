using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Nekote.Core.Time
{
    /// <summary>
    /// DateTimeFormatKindに対応する書式文字列を管理します。
    /// </summary>
    public static class DateTimeFormats
    {
        private static readonly ImmutableDictionary<DateTimeFormatKind, string> Map = new Dictionary<DateTimeFormatKind, string>
        {
            [DateTimeFormatKind.LocalSortable] = "yyyyMMdd'-'HHmmss",
            [DateTimeFormatKind.LocalSortableMilliseconds] = "yyyyMMdd'-'HHmmss'-'fff",
            [DateTimeFormatKind.LocalSortableTicks] = "yyyyMMdd'-'HHmmss'-'fffffff",
            [DateTimeFormatKind.UtcSortable] = "yyyyMMdd'T'HHmmss'Z'",
            [DateTimeFormatKind.UtcSortableMilliseconds] = "yyyyMMdd'T'HHmmss'-'fff'Z'",
            [DateTimeFormatKind.UtcSortableTicks] = "yyyyMMdd'T'HHmmss'-'fffffff'Z'",

            [DateTimeFormatKind.DateSortable] = "yyyyMMdd",
            [DateTimeFormatKind.TimeSortable] = "HHmmss",
            [DateTimeFormatKind.TimeSortableMilliseconds] = "HHmmss'-'fff",
            [DateTimeFormatKind.TimeSortableTicks] = "HHmmss'-'fffffff",

            [DateTimeFormatKind.LocalUserFriendlyMinutes] = "yyyy'/'M'/'d' H':'mm",
            [DateTimeFormatKind.LocalUserFriendlySeconds] = "yyyy'/'M'/'d' H':'mm':'ss",
            [DateTimeFormatKind.LocalUserFriendlyMilliseconds] = "yyyy'/'M'/'d' H':'mm':'ss'.'fff",
            [DateTimeFormatKind.LocalUserFriendlyTicks] = "yyyy'/'M'/'d' H':'mm':'ss'.'fffffff",
            [DateTimeFormatKind.UtcUserFriendlyMinutes] = "yyyy'/'M'/'d' H':'mm 'UTC'",
            [DateTimeFormatKind.UtcUserFriendlySeconds] = "yyyy'/'M'/'d' H':'mm':'ss 'UTC'",
            [DateTimeFormatKind.UtcUserFriendlyMilliseconds] = "yyyy'/'M'/'d' H':'mm':'ss'.'fff 'UTC'",
            [DateTimeFormatKind.UtcUserFriendlyTicks] = "yyyy'/'M'/'d' H':'mm':'ss'.'fffffff 'UTC'",

            [DateTimeFormatKind.DateUserFriendly] = "yyyy'/'M'/'d'",
            [DateTimeFormatKind.TimeUserFriendlyMinutes] = "H':'mm",
            [DateTimeFormatKind.TimeUserFriendlySeconds] = "H':'mm':'ss",
            [DateTimeFormatKind.TimeUserFriendlyMilliseconds] = "H':'mm':'ss'.'fff",
            [DateTimeFormatKind.TimeUserFriendlyTicks] = "H':'mm':'ss'.'fffffff",
        }.ToImmutableDictionary();

        /// <summary>
        /// 指定された種類に対応する書式文字列を取得します。
        /// </summary>
        public static string GetFormatString(DateTimeFormatKind kind) => Map[kind];
    }
}
