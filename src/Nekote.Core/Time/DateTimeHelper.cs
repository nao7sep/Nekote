using System;
using System.Globalization;

namespace Nekote.Core.Time
{
    /// <summary>
    /// 日付と時刻の操作に関する拡張メソッドを提供します。
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateTimeOffset"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateTimeOffset"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this DateTimeOffset value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTimeOffset"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTimeOffset"/>。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTimeOffset"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateTimeOffset"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
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

        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateTime"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateTime"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
        public static string ToString(this DateTime value, DateTimeFormatKind format)
        {
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }
            var formatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTime"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTime"/>。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTime"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateTime"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
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

        /// <summary>
        /// 指定された <see cref="DateTimeFormatKind"/> に応じた <see cref="DateTimeStyles"/> を返します。
        /// </summary>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>対応する <see cref="DateTimeStyles"/>。</returns>
        private static DateTimeStyles GetDateTimeStyles(DateTimeFormatKind format) => format switch
        {
            // --- DateTimeStylesの選択ロジック ---
            //
            // - UTC書式 (`Utc*`):
            //   `AdjustToUniversal` を使用して、パーズ結果を確実にUTCに変換します。
            //   書式自体に 'Z' や "UTC" といったタイムゾーン情報が含まれているため、
            //   タイムゾーン不明な文字列をUTCと見なす `AssumeUniversal` は不要です。
            //   また、ドキュメントでは `AssumeUniversal` と `AdjustToUniversal` の併用は推奨されていません。
            //
            // - ローカル書式 (`Local*`):
            //   `AssumeLocal` を使用します。これらの書式にはタイムゾーン情報が含まれていないため、
            //   パーズ時にシステムのローカルタイムゾーンであると解釈するよう明示的に指定する必要があります。
            //
            // - 日付・時刻のみの書式:
            //   `None` を使用します。これらの書式はタイムゾーンの概念を持たないため、特別なスタイルは不要です。

            // --- 並べ替え可能な書式 ---

            DateTimeFormatKind.LocalSortable or
            DateTimeFormatKind.LocalSortableMilliseconds or
            DateTimeFormatKind.LocalSortableTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcSortable or
            DateTimeFormatKind.UtcSortableMilliseconds or
            DateTimeFormatKind.UtcSortableTicks
                => DateTimeStyles.AdjustToUniversal,

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
                => DateTimeStyles.AdjustToUniversal,

            // --- 日付・時刻の人間が読みやすい書式 ---

            DateTimeFormatKind.DateUserFriendly or
            DateTimeFormatKind.TimeUserFriendlyMinutes or
            DateTimeFormatKind.TimeUserFriendlySeconds or
            DateTimeFormatKind.TimeUserFriendlyMilliseconds or
            DateTimeFormatKind.TimeUserFriendlyTicks
                => DateTimeStyles.None,

            _ => throw new ArgumentException($"Invalid format '{format}' for this operation.", nameof(format))
        };

        /// <summary>
        /// 指定された書式を使用して、この <see cref="DateOnly"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="DateOnly"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateOnly"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateOnly"/>。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateOnly"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="DateOnly"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
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

        /// <summary>
        /// 指定された書式を使用して、この <see cref="TimeOnly"/> のインスタンスの値を、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換対象の <see cref="TimeOnly"/>。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定した書式による文字列形式。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeOnly"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="TimeOnly"/>。</returns>
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

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeOnly"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="TimeOnly"/> が格納されます。</param>
        /// <returns>変換に成功した場合は true、それ以外は false。</returns>
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
