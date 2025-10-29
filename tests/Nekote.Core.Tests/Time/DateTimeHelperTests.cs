using System;
using Nekote.Core.Randomization;
using Nekote.Core.Time;
using Xunit;

namespace Nekote.Core.Tests.Time
{
    /// <summary>
    /// <see cref="DateTimeHelper"/> のテストクラスです。
    /// </summary>
    public class DateTimeHelperTests
    {
        private static readonly IRandomProvider _random = new SystemRandomProvider();

        /// <summary>
        /// 指定された種類のランダムな <see cref="DateTimeOffset"/> を作成します。
        /// </summary>
        private static DateTimeOffset CreateRandomDateTimeOffset(DateTimeKind kind)
        {
            var ticks = (long)(_random.NextDouble() * DateTime.MaxValue.Ticks);
            var dateTime = new DateTime(ticks, kind == DateTimeKind.Unspecified ? DateTimeKind.Local : kind);
            if (kind == DateTimeKind.Utc)
            {
                return new DateTimeOffset(dateTime.ToUniversalTime());
            }
            return new DateTimeOffset(dateTime.ToLocalTime());
        }

        /// <summary>
        /// 指定された種類のランダムな <see cref="DateTime"/> を作成します。
        /// </summary>
        private static DateTime CreateRandomDateTime(DateTimeKind kind)
        {
            var ticks = (long)(_random.NextDouble() * DateTime.MaxValue.Ticks);
            return new DateTime(ticks, kind);
        }

        /// <summary>
        /// ランダムな <see cref="DateOnly"/> を作成します。
        /// </summary>
        private static DateOnly CreateRandomDateOnly()
        {
            var year = _random.Next(1, 10000);
            var month = _random.Next(1, 13);
            var day = _random.Next(1, DateTime.DaysInMonth(year, month) + 1);
            return new DateOnly(year, month, day);
        }

        /// <summary>
        /// ランダムな <see cref="TimeOnly"/> を作成します。
        /// </summary>
        private static TimeOnly CreateRandomTimeOnly()
        {
            var ticks = (long)(_random.NextDouble() * TimeOnly.MaxValue.Ticks);
            return new TimeOnly(ticks);
        }

        /// <summary>
        /// 書式指定子から <see cref="DateTimeKind"/> を取得します。
        /// </summary>
        private static DateTimeKind GetDateTimeKindFromFormat(DateTimeFormatKind format)
        {
            var name = format.ToString();
            if (name.StartsWith("Utc")) return DateTimeKind.Utc;
            if (name.StartsWith("Local")) return DateTimeKind.Local;
            return DateTimeKind.Unspecified;
        }

        /// <summary>
        /// <see cref="DateTimeOffset"/> のラウンドトリップテストです。
        /// 注意: DateTimeFormatKind のうち、日付のみ・時刻のみの書式（DateSortable, DateUserFriendly, TimeSortable など）は
        /// DateTimeOffset には適用できないため、ここではテストしていません。
        /// これらの書式は DateTime_Roundtrip, DateOnly_Roundtrip, TimeOnly_Roundtrip でカバーされています。
        /// </summary>
        [Theory]
        [InlineData(DateTimeFormatKind.LocalSortable)]
        [InlineData(DateTimeFormatKind.LocalSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.LocalSortableTicks)]
        [InlineData(DateTimeFormatKind.UtcSortable)]
        [InlineData(DateTimeFormatKind.UtcSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.UtcSortableTicks)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyTicks)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyTicks)]
        public void DateTimeOffset_Roundtrip(DateTimeFormatKind format)
        {
            // Arrange
            var dateTimeKind = GetDateTimeKindFromFormat(format);
            var originalValue = CreateRandomDateTimeOffset(dateTimeKind);
            var sourceString = originalValue.ToString(format);
            var formatString = DateTimeFormats.GetFormatString(format);
            var expectedString = originalValue.ToString(formatString);

            // Act
            var parsedValue = DateTimeHelper.ParseDateTimeOffset(sourceString, format);
            var actualParsedString = parsedValue.ToString(formatString);
            Assert.True(DateTimeHelper.TryParseDateTimeOffset(sourceString, format, out var tryParsedValue));
            var actualTryParsedString = tryParsedValue.ToString(formatString);

            // Assert
            Assert.Equal(expectedString, actualParsedString);
            Assert.Equal(expectedString, actualTryParsedString);
        }

        /// <summary>
        /// <see cref="DateTime"/> のラウンドトリップテストです。
        /// </summary>
        [Theory]
        [InlineData(DateTimeFormatKind.LocalSortable)]
        [InlineData(DateTimeFormatKind.LocalSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.LocalSortableTicks)]
        [InlineData(DateTimeFormatKind.UtcSortable)]
        [InlineData(DateTimeFormatKind.UtcSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.UtcSortableTicks)]
        [InlineData(DateTimeFormatKind.DateSortable)]
        [InlineData(DateTimeFormatKind.TimeSortable)]
        [InlineData(DateTimeFormatKind.TimeSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.TimeSortableTicks)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.LocalUserFriendlyTicks)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.UtcUserFriendlyTicks)]
        [InlineData(DateTimeFormatKind.DateUserFriendly)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyTicks)]
        public void DateTime_Roundtrip(DateTimeFormatKind format)
        {
            // Arrange
            var dateTimeKind = GetDateTimeKindFromFormat(format);
            var originalValue = CreateRandomDateTime(dateTimeKind);
            var sourceString = originalValue.ToString(format);
            var formatString = DateTimeFormats.GetFormatString(format);
            var expectedString = originalValue.ToString(formatString);

            // Act
            var parsedValue = DateTimeHelper.ParseDateTime(sourceString, format);
            var actualParsedString = parsedValue.ToString(formatString);
            Assert.True(DateTimeHelper.TryParseDateTime(sourceString, format, out var tryParsedValue));
            var actualTryParsedString = tryParsedValue.ToString(formatString);

            // Assert
            Assert.Equal(expectedString, actualParsedString);
            Assert.Equal(expectedString, actualTryParsedString);
        }

        /// <summary>
        /// <see cref="DateOnly"/> のラウンドトリップテストです。
        /// </summary>
        [Theory]
        [InlineData(DateTimeFormatKind.DateSortable)]
        [InlineData(DateTimeFormatKind.DateUserFriendly)]
        public void DateOnly_Roundtrip(DateTimeFormatKind format)
        {
            // Arrange
            var expectedValue = CreateRandomDateOnly();
            var sourceString = expectedValue.ToString(format);

            // Act
            var parsedValue = DateTimeHelper.ParseDateOnly(sourceString, format);
            Assert.True(DateTimeHelper.TryParseDateOnly(sourceString, format, out var tryParsedValue));

            // Assert
            Assert.Equal(expectedValue, parsedValue);
            Assert.Equal(expectedValue, tryParsedValue);
        }

        /// <summary>
        /// <see cref="TimeOnly"/> のラウンドトリップテストです。
        /// </summary>
        [Theory]
        [InlineData(DateTimeFormatKind.TimeSortable)]
        [InlineData(DateTimeFormatKind.TimeSortableMilliseconds)]
        [InlineData(DateTimeFormatKind.TimeSortableTicks)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyMinutes)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlySeconds)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyMilliseconds)]
        [InlineData(DateTimeFormatKind.TimeUserFriendlyTicks)]
        public void TimeOnly_Roundtrip(DateTimeFormatKind format)
        {
            // Arrange
            var originalValue = CreateRandomTimeOnly();
            var sourceString = originalValue.ToString(format);
            var formatString = DateTimeFormats.GetFormatString(format);
            var expectedString = originalValue.ToString(formatString);

            // Act
            var parsedValue = DateTimeHelper.ParseTimeOnly(sourceString, format);
            var actualParsedString = parsedValue.ToString(formatString);
            Assert.True(DateTimeHelper.TryParseTimeOnly(sourceString, format, out var tryParsedValue));
            var actualTryParsedString = tryParsedValue.ToString(formatString);

            // Assert
            Assert.Equal(expectedString, actualParsedString);
            Assert.Equal(expectedString, actualTryParsedString);
        }
    }
}
