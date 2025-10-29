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
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTimeOffset"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTimeOffset"/>。</returns>
        public static DateTimeOffset ParseDateTimeOffset(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTimeOffset.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
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
            if (string.IsNullOrWhiteSpace(value) || !Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTimeOffset.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
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
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 指定された書式の文字列を、それと等価な <see cref="DateTime"/> に変換します。
        /// </summary>
        /// <param name="value">変換する文字列。</param>
        /// <param name="format">使用する書式の種類。</param>
        /// <returns>変換された <see cref="DateTime"/>。</returns>
        public static DateTime ParseDateTime(string value, DateTimeFormatKind format)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }
            if (!Enum.IsDefined<DateTimeFormatKind>(format))
            {
                throw new ArgumentOutOfRangeException(nameof(format), format, "The specified format is not a valid DateTimeFormatKind value.");
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTime.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
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
            if (string.IsNullOrWhiteSpace(value) || !Enum.IsDefined<DateTimeFormatKind>(format))
            {
                result = default;
                return false;
            }
            var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
            var dateTimeStyles = GetDateTimeStyles(format);
            return DateTime.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
        }

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
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"The format '{format}' is not valid for DateOnly. Only DateSortable and DateUserFriendly formats are supported.", nameof(format));
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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return DateOnly.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
                default:
                    throw new ArgumentException($"The format '{format}' is not valid for DateOnly. Only DateSortable and DateUserFriendly formats are supported.", nameof(format));
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
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }
            switch (format)
            {
                case DateTimeFormatKind.DateSortable:
                case DateTimeFormatKind.DateUserFriendly:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return DateOnly.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
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
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    return value.ToString(dateTimeFormatString, CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException($"The format '{format}' is not valid for TimeOnly. Only time-related formats are supported.", nameof(format));
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
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null, empty, or whitespace.", nameof(value));
            }
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return TimeOnly.ParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles);
                default:
                    throw new ArgumentException($"The format '{format}' is not valid for TimeOnly. Only time-related formats are supported.", nameof(format));
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
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }
            switch (format)
            {
                case DateTimeFormatKind.TimeSortable:
                case DateTimeFormatKind.TimeSortableMilliseconds:
                case DateTimeFormatKind.TimeSortableTicks:
                case DateTimeFormatKind.TimeUserFriendlyMinutes:
                case DateTimeFormatKind.TimeUserFriendlySeconds:
                case DateTimeFormatKind.TimeUserFriendlyMilliseconds:
                case DateTimeFormatKind.TimeUserFriendlyTicks:
                    var dateTimeFormatString = DateTimeFormats.GetFormatString(format);
                    var dateTimeStyles = GetDateTimeStyles(format);
                    return TimeOnly.TryParseExact(value, dateTimeFormatString, CultureInfo.InvariantCulture, dateTimeStyles, out result);
                default:
                    result = default;
                    return false;
            }
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
            //   `AssumeUniversal` と `AdjustToUniversal` の両方を指定しています。
            //   一部の書式（特に 'Z' を含むもの）では、`AssumeUniversal` を付けないと
            //   'Z' がUTC指標として正しく認識されず、テストが失敗する場合があります。
            //   本来、ドキュメント上は `AssumeUniversal` と `AdjustToUniversal` の併用は非推奨ですが、
            //   現状この組み合わせでテストが正しく通るため、実装しています。
            //
            // - ローカル書式 (`Local*`):
            //   `AssumeLocal` を使用します。これらの書式にはタイムゾーン情報が含まれていないため、
            //   パーズ時にシステムのローカルタイムゾーンであると解釈するよう明示的に指定する必要があります。
            //
            // - 日付・時刻のみの書式:
            //   `None` を使用します。これらの書式はタイムゾーンの概念を持たないため、特別なスタイルは不要です。

            // わい: パッと見た限り、併用を非推奨とする公式ドキュメントは見つからず。
            //
            // タイムゾーンが関係する値を返してもらいたい format の指定のときに、どちらが返ってほしいかを Assume* で指定するのは理にかなう。
            //
            // そこに AdjustToUniversal が不可欠か再テストしたく、なくしてみたところ、七つの Utc* の format で確実に失敗した。
            // "yyyyMMdd'T'HHmmss'Z'" や "yyyy'/'M'/'d H':'mm 'UTC'" などだ。
            // 返ってきた値は、必ず9時間、先へ進んでいた。
            // AdjustToUniversal の追加によりテストに通ることを考えると、返ってきているものは「9時間先のデータが入ったローカル日時」だ。
            // 公式ドキュメントの Remarks には、
            // If the input string does not contain any indication of the time zone,
            // the date and time parsing methods interpret the value of the date and time string based on the time zone setting for the operating system. とある。
            // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.datetimestyles?view=net-9.0
            // おそらく、DateTime はローカル日時が基本で、Parse* 時に UTC っぽいなと思えば、
            // DateTimeStyles には存在しない AdjustToLocal 的な変換を勝手に行う仕様になっているのだろう。
            //
            // 入力がローカルか UTC か、それが検出されるかどうかにより、四つのパターンになる。
            // ローカル日時 → ローカル文字列 → ローカルだと検出されれば Kind のみそう設定され、そうでなければ AssumeLocal により同じことが起こる。
            // UTC 日時 → UTC 文字列 → UTC だと検出されれば「勝手にローカルに変換され」、そうでなければ AssumeUniversal により Kind のみそう設定される。
            // ここで勝手に変換されるのを、AdjustToUniversal により、同じ the time zone setting for the operating system に基づいて元に戻す。
            // 無駄な処理だが、20年前につくられた DateTime が基本ローカルなのは、C 言語の文字列がデフォルトでは Unicode でなかったのと同じようなことだろう。
            // そういうものだと諦めて、無駄な処理を組み込んでおくしかない。

            // --- 並べ替え可能な書式 ---

            DateTimeFormatKind.LocalSortable or
            DateTimeFormatKind.LocalSortableMilliseconds or
            DateTimeFormatKind.LocalSortableTicks
                => DateTimeStyles.AssumeLocal,

            DateTimeFormatKind.UtcSortable or
            DateTimeFormatKind.UtcSortableMilliseconds or
            DateTimeFormatKind.UtcSortableTicks
                => DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,

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
                => DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,

            // --- 日付・時刻の人間が読みやすい書式 ---

            DateTimeFormatKind.DateUserFriendly or
            DateTimeFormatKind.TimeUserFriendlyMinutes or
            DateTimeFormatKind.TimeUserFriendlySeconds or
            DateTimeFormatKind.TimeUserFriendlyMilliseconds or
            DateTimeFormatKind.TimeUserFriendlyTicks
                => DateTimeStyles.None,

            _ => throw new ArgumentException($"The specified format is not valid for this operation.", nameof(format))
        };
    }
}
