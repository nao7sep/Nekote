using System;

namespace Nekote.Core.Time
{
    /// <summary>
    /// システムクロックを使用して現在時刻を提供する、IClockのデフォルト実装です。
    /// </summary>
    public class SystemClock : IClock
    {
        /// <inheritdoc />
        public DateTimeOffset GetCurrentDateTimeOffset() => DateTimeOffset.Now;

        /// <inheritdoc />
        public DateTime GetCurrentUtcDateTime() => DateTime.UtcNow;

        /// <inheritdoc />
        public DateTime GetCurrentLocalDateTime() => DateTime.Now;
    }
}
