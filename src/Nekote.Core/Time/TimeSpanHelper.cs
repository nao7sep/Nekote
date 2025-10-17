using System;
using System.Globalization;
using System.Text;

namespace Nekote.Core.Time
{
    /// <summary>
    /// TimeSpan の操作に関する拡張メソッドを提供します。
    /// </summary>
    public static class TimeSpanHelper
    {
        /// <summary>
        /// TimeSpan を指定された書式で文字列に変換します。
        /// </summary>
        public static string ToString(this TimeSpan value, TimeSpanFormatKind format)
        {
            var sb = new StringBuilder();

            switch (format)
            {
                case TimeSpanFormatKind.SortableSeconds:
                case TimeSpanFormatKind.SortableMilliseconds:
                case TimeSpanFormatKind.SortableTicks:
                {
                    sb.Append(((int)value.TotalHours).ToString("D2", CultureInfo.InvariantCulture));
                    sb.Append('-');
                    sb.Append(value.Minutes.ToString("D2", CultureInfo.InvariantCulture));
                    sb.Append('-');
                    sb.Append(value.Seconds.ToString("D2", CultureInfo.InvariantCulture));

                    if (format == TimeSpanFormatKind.SortableMilliseconds)
                    {
                        sb.Append('-');
                        sb.Append(value.Milliseconds.ToString("D3", CultureInfo.InvariantCulture));
                    }
                    else if (format == TimeSpanFormatKind.SortableTicks)
                    {
                        sb.Append('-');
                        sb.Append((value.Ticks % TimeSpan.TicksPerSecond).ToString("D7", CultureInfo.InvariantCulture));
                    }
                    return sb.ToString();
                }

                case TimeSpanFormatKind.UserFriendlyMinutes:
                case TimeSpanFormatKind.UserFriendlySeconds:
                case TimeSpanFormatKind.UserFriendlyMilliseconds:
                case TimeSpanFormatKind.UserFriendlyTicks:
                {
                    sb.Append(((int)value.TotalHours).ToString(CultureInfo.InvariantCulture));
                    sb.Append(':');
                    sb.Append(value.Minutes.ToString("D2", CultureInfo.InvariantCulture));

                    if (format == TimeSpanFormatKind.UserFriendlyMinutes)
                    {
                        return sb.ToString();
                    }

                    sb.Append(':');
                    sb.Append(value.Seconds.ToString("D2", CultureInfo.InvariantCulture));

                    if (format == TimeSpanFormatKind.UserFriendlyMilliseconds)
                    {
                        sb.Append('.');
                        sb.Append(value.Milliseconds.ToString("D3", CultureInfo.InvariantCulture));
                    }
                    else if (format == TimeSpanFormatKind.UserFriendlyTicks)
                    {
                        sb.Append('.');
                        sb.Append((value.Ticks % TimeSpan.TicksPerSecond).ToString("D7", CultureInfo.InvariantCulture));
                    }
                    return sb.ToString();
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
        }

        /// <summary>
        /// 指定された書式の文字列を TimeSpan に変換します。
        /// </summary>
        public static TimeSpan ParseTimeSpan(string value, TimeSpanFormatKind format)
        {
            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = TimeSpanFormats.GetFormatString(format);
            return TimeSpan.ParseExact(value, formatString, CultureInfo.InvariantCulture, TimeSpanStyles.None);
        }
    }
}
