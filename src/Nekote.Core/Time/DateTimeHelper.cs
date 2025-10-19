using System;
using System.Globalization;

namespace Nekote.Core.Time
{
    /// <summary>
    /// 日付と時刻の操作に関する拡張メソッドを提供します。
    /// </summary>
    public static class DateTimeHelper
    {
        public static string ToString(this DateTimeOffset value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(formatString, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset ParseDateTimeOffset(string value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            var styles = GetDateTimeStyles(format);
            return DateTimeOffset.ParseExact(value, formatString, CultureInfo.InvariantCulture, styles);
        }

        public static bool TryParseDateTimeOffset(string value, DateTimeFormatKind format, out DateTimeOffset result)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            var styles = GetDateTimeStyles(format);
            return DateTimeOffset.TryParseExact(value, formatString, CultureInfo.InvariantCulture, styles, out result);
        }

        public static string ToString(this DateTime value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(formatString, CultureInfo.InvariantCulture);
        }

        public static DateTime ParseDateTime(string value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            var styles = GetDateTimeStyles(format);
            return DateTime.ParseExact(value, formatString, CultureInfo.InvariantCulture, styles);
        }

        public static bool TryParseDateTime(string value, DateTimeFormatKind format, out DateTime result)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            var styles = GetDateTimeStyles(format);
            return DateTime.TryParseExact(value, formatString, CultureInfo.InvariantCulture, styles, out result);
        }

        private static DateTimeStyles GetDateTimeStyles(DateTimeFormatKind format) => format switch
        {
            // わい: AI は AssumeUniversal のところに AdjustToUniversal を足そうとしたが、必要でない。
            // Assume* は、+09:00 などがなくてタイムゾーンが分からないときに DateTime.Kind に何を設定するかの指定。
            // このクラスでの Parse* は ParseExact なので、完全一致でないとそもそもパーズを通らない。
            // +09:00 などが見つかったからローカルだと判断されたものを AdjustToUniversal で UTC にして返すことは起こりえない。

            // --- 並べ替え可能な書式 ---

            DateTimeFormatKind.LocalSortable or
            DateTimeFormatKind.LocalSortableMilliseconds or
            DateTimeFormatKind.LocalSortableTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcSortable or
            DateTimeFormatKind.UtcSortableMilliseconds or
            DateTimeFormatKind.UtcSortableTicks
                => DateTimeStyles.AssumeUniversal,

            // --- 日付のみ・時刻のみの書式 ---

            DateTimeFormatKind.DateSortable or
            DateTimeFormatKind.TimeSortable or
            DateTimeFormatKind.TimeSortableMilliseconds or
            DateTimeFormatKind.TimeSortableTicks
                => DateTimeStyles.None,

            // --- 人間が読みやすい書式 ---

            DateTimeFormatKind.LocalUserFriendlyMinutes or
            DateTimeFormatKind.LocalUserFriendlySeconds or
            DateTimeFormatKind.LocalUserFriendlyMilliseconds or
            DateTimeFormatKind.LocalUserFriendlyTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcUserFriendlyMinutes or
            DateTimeFormatKind.UtcUserFriendlySeconds or
            DateTimeFormatKind.UtcUserFriendlyMilliseconds or
            DateTimeFormatKind.UtcUserFriendlyTicks
                => DateTimeStyles.AssumeUniversal,

            // --- 日付・時刻の人間が読みやすい書式 ---

            DateTimeFormatKind.DateUserFriendly or
            DateTimeFormatKind.TimeUserFriendlyMinutes or
            DateTimeFormatKind.TimeUserFriendlySeconds or
            DateTimeFormatKind.TimeUserFriendlyMilliseconds or
            DateTimeFormatKind.TimeUserFriendlyTicks
                => DateTimeStyles.None,

            _ => throw new ArgumentException($"Invalid format '{format}' for this operation.", nameof(format))
        };

        public static string ToString(this DateOnly value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(formatString, CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for DateOnly.", nameof(format));
            }
        }

        public static DateOnly ParseDateOnly(string value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    var styles = GetDateTimeStyles(format);
                    return DateOnly.ParseExact(value, formatString, CultureInfo.InvariantCulture, styles);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for DateOnly.", nameof(format));
            }
        }

        public static bool TryParseDateOnly(string value, DateTimeFormatKind format, out DateOnly result)
        {
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    var styles = GetDateTimeStyles(format);
                    return DateOnly.TryParseExact(value, formatString, CultureInfo.InvariantCulture, styles, out result);
                default:
                    result = default;
                    return false;
            }
        }

        public static string ToString(this TimeOnly value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(formatString, CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for TimeOnly.", nameof(format));
            }
        }

        public static TimeOnly ParseTimeOnly(string value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    var styles = GetDateTimeStyles(format);
                    return TimeOnly.ParseExact(value, formatString, CultureInfo.InvariantCulture, styles);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for TimeOnly.", nameof(format));
            }
        }

        public static bool TryParseTimeOnly(string value, DateTimeFormatKind format, out TimeOnly result)
        {
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var formatString = DateTimeFormats.GetFormatString(format);
                    var styles = GetDateTimeStyles(format);
                    return TimeOnly.TryParseExact(value, formatString, CultureInfo.InvariantCulture, styles, out result);
                default:
                    result = default;
                    return false;
            }
        }
    }
}
