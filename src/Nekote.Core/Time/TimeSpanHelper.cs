using System;
using System.Globalization;

namespace Nekote.Core.Time
{
    /// <summary>
    /// <see cref="TimeSpan"/> の操作に関する拡張メソッドを提供します。
    /// 注意: 1日未満 (24時間未満) の <see cref="TimeSpan"/> のみサポートします。
    /// </summary>
    public static class TimeSpanHelper
    {
        /// <summary>
        /// この <see cref="TimeSpan"/> インスタンスの値を、指定された書式を使用して、それと等価な文字列形式に変換します。
        /// </summary>
        /// <param name="value">変換する <see cref="TimeSpan"/>。1日未満 (24時間未満) である必要があります。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>指定された書式の文字列。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> が1日以上の場合、または <paramref name="format"/> が未定義の値の場合にスローされます。</exception>
        public static string ToString(this TimeSpan value, TimeSpanFormatKind format)
        {
            if (value.TotalDays >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "TimeSpan must be less than 1 day (24 hours).");
            }

            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid TimeSpanFormatKind value.");
            }

            var timeSpanFormatString = TimeSpanFormats.GetFormatString(format);
            return value.ToString(timeSpanFormatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeSpan"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="TimeSpan"/>。1日未満 (24時間未満) の値のみサポートします。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="format"/> が未定義の値の場合、または変換後の <see cref="TimeSpan"/> が1日以上の場合にスローされます。</exception>
        /// <exception cref="FormatException">文字列の解析に失敗した場合にスローされます。</exception>
        public static TimeSpan ParseTimeSpan(string value, TimeSpanFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }

            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid TimeSpanFormatKind value.");
            }

            var timeSpanFormatString = TimeSpanFormats.GetFormatString(format);
            var result = TimeSpan.ParseExact(value, timeSpanFormatString, CultureInfo.InvariantCulture, TimeSpanStyles.None);

            if (result.TotalDays >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), result, "TimeSpan must be less than 1 day (24 hours).");
            }

            return result;
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="TimeSpan"/> に変換しようと試みます。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <param name="result">変換に成功した場合、変換された <see cref="TimeSpan"/> が格納されます。1日未満 (24時間未満) の値のみサポートします。</param>
        /// <returns>変換に成功し、結果が1日未満の場合は true、それ以外は false。</returns>
        public static bool TryParseTimeSpan(string value, TimeSpanFormatKind format, out TimeSpan result)
        {
            if (string.IsNullOrWhiteSpace(value) || !Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                result = default;
                return false;
            }

            var timeSpanFormatString = TimeSpanFormats.GetFormatString(format);
            if (TimeSpan.TryParseExact(value, timeSpanFormatString, CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
            {
                // 1日未満の制約を確認
                if (result.TotalDays >= 1)
                {
                    result = default;
                    return false;
                }
                return true;
            }

            result = default;
            return false;
        }
    }
}
