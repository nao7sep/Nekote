using System;
using System.Globalization;

namespace Nekote.Core.Time
{
    /// <summary>
    /// TimeSpan の操作に関する拡張メソッドを提供します。
    /// 注意: 1日未満 (24時間未満) の TimeSpan のみサポートします。
    /// </summary>
    public static class TimeSpanHelper
    {
        /// <summary>
        /// TimeSpan を指定された書式で文字列に変換します。
        /// </summary>
        /// <param name="value">変換する TimeSpan。1日未満 (24時間未満) である必要があります。</param>
        /// <param name="format">使用する書式の種類</param>
        /// <returns>指定された書式の文字列</returns>
        /// <exception cref="ArgumentOutOfRangeException">TimeSpan が1日以上の場合、または format が無効な場合</exception>
        public static string ToString(this TimeSpan value, TimeSpanFormatKind format)
        {
            if (value.TotalDays >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "TimeSpan must be less than 1 day (24 hours).");
            }

            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }

            var formatString = TimeSpanFormats.GetFormatString(format);
            return value.ToString(formatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を TimeSpan に変換します。
        /// </summary>
        /// <param name="value">変換する文字列</param>
        /// <param name="format">使用する書式の種類</param>
        /// <returns>変換された TimeSpan。1日未満 (24時間未満) の値のみサポートします。</returns>
        /// <exception cref="FormatException">文字列の解析に失敗した場合、または結果が1日以上の場合</exception>
        public static TimeSpan ParseTimeSpan(string value, TimeSpanFormatKind format)
        {
            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "Invalid enum value.");
            }

            var formatString = TimeSpanFormats.GetFormatString(format);
            var result = TimeSpan.ParseExact(value, formatString, CultureInfo.InvariantCulture, TimeSpanStyles.None);

            // パースされた TimeSpan が1日未満であることを確認
            if (result.TotalDays >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), result, "TimeSpan must be less than 1 day (24 hours).");
            }

            return result;
        }

        /// <summary>
        /// 指定された書式の文字列を TimeSpan に変換を試行します。
        /// </summary>
        /// <param name="value">変換する文字列</param>
        /// <param name="format">使用する書式の種類</param>
        /// <param name="result">変換された TimeSpan。1日未満 (24時間未満) の値のみサポートします。</param>
        /// <returns>変換に成功し、結果が1日未満の場合は true、それ以外は false</returns>
        public static bool TryParseTimeSpan(string value, TimeSpanFormatKind format, out TimeSpan result)
        {
            if (!Enum.IsDefined<TimeSpanFormatKind>(format))
            {
                result = default;
                return false;
            }

            var formatString = TimeSpanFormats.GetFormatString(format);
            if (TimeSpan.TryParseExact(value, formatString, CultureInfo.InvariantCulture, TimeSpanStyles.None, out result))
            {
                // パースされた TimeSpan が1日未満であることを確認
                if (result.TotalDays >= 1)
                {
                    result = default;
                    return false;
                }
                return true;
            }

            return false;
        }
    }
}
