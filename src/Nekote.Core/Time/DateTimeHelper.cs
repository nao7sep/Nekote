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
            return value.ToString(DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture);
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

        public static string ToString(this DateTime value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            return value.ToString(DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture);
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

        private static DateTimeStyles GetDateTimeStyles(DateTimeFormatKind format) => format switch
        {
            DateTimeFormatKind.LocalSortable or
            DateTimeFormatKind.LocalSortableMilliseconds or
            DateTimeFormatKind.LocalSortableTicks or
            DateTimeFormatKind.LocalUserFriendlyMinutes or
            DateTimeFormatKind.LocalUserFriendlySeconds or
            DateTimeFormatKind.LocalUserFriendlyMilliseconds or
            DateTimeFormatKind.LocalUserFriendlyTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcSortable or
            DateTimeFormatKind.UtcSortableMilliseconds or
            DateTimeFormatKind.UtcSortableTicks or
            DateTimeFormatKind.UtcUserFriendlyMinutes or
            DateTimeFormatKind.UtcUserFriendlySeconds or
            DateTimeFormatKind.UtcUserFriendlyMilliseconds or
            DateTimeFormatKind.UtcUserFriendlyTicks
                => DateTimeStyles.AssumeUniversal,

            _ => throw new ArgumentException($"Invalid format '{format}' for this operation.", nameof(format))
        };

        public static string ToString(this DateOnly value, DateTimeFormatKind format)
        {
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    return value.ToString(DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture);
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
                    return DateOnly.ParseExact(value, DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture, DateTimeStyles.None);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for DateOnly.", nameof(format));
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
                    return value.ToString(DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture);
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
                    return TimeOnly.ParseExact(value, DateTimeFormats.GetFormatString(format), CultureInfo.InvariantCulture, DateTimeStyles.None);
                default:
                    throw new ArgumentException($"Invalid format '{format}' for TimeOnly.", nameof(format));
            }
        }
    }
}
