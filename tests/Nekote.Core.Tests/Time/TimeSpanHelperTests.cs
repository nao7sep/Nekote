using System;
using Nekote.Core.Time;
using Xunit;

namespace Nekote.Core.Tests.Time
{
    /// <summary>
    /// <see cref="TimeSpanHelper"/> のテストクラスです。
    /// </summary>
    public class TimeSpanHelperTests
    {
        private static readonly Random _random = new();

        /// <summary>
        /// ランダムな <see cref="TimeSpan"/> を作成します。
        /// </summary>
        private static TimeSpan CreateRandomTimeSpan()
        {
            return new TimeSpan(_random.Next(0, 24), _random.Next(0, 60), _random.Next(0, 60));
        }

        /// <summary>
        /// <see cref="TimeSpan"/> のラウンドトリップテストです。
        /// </summary>
        [Theory]
        [InlineData(TimeSpanFormatKind.SortableSeconds)]
        [InlineData(TimeSpanFormatKind.SortableMilliseconds)]
        [InlineData(TimeSpanFormatKind.SortableTicks)]
        [InlineData(TimeSpanFormatKind.UserFriendlyMinutes)]
        [InlineData(TimeSpanFormatKind.UserFriendlySeconds)]
        [InlineData(TimeSpanFormatKind.UserFriendlyMilliseconds)]
        [InlineData(TimeSpanFormatKind.UserFriendlyTicks)]
        public void TimeSpan_Roundtrip(TimeSpanFormatKind format)
        {
            var originalValue = CreateRandomTimeSpan();
            var formattedString = originalValue.ToString(format);
            var formatString = TimeSpanFormats.GetFormatString(format);
            var expectedString = originalValue.ToString(formatString);

            var parsedValue = TimeSpanHelper.ParseTimeSpan(formattedString, format);
            var actualParsedString = parsedValue.ToString(formatString);
            Assert.Equal(expectedString, actualParsedString);

            Assert.True(TimeSpanHelper.TryParseTimeSpan(formattedString, format, out var tryParsedValue));
            var actualTryParsedString = tryParsedValue.ToString(formatString);
            Assert.Equal(expectedString, actualTryParsedString);
        }
    }
}
