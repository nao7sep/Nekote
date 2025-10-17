using Nekote.Core.Time;

namespace Nekote.Lab.Console.Testers
{
    /// <summary>
    /// 時間関連のコードをテスト・実行するためのクラス。
    /// </summary>
    public class TimeTester
    {
        private readonly IClock _clock;

        public TimeTester(IClock clock)
        {
            _clock = clock;
        }

        /// <summary>
        /// 現在時刻を取得して表示します。
        /// </summary>
        public void DisplayCurrentTime()
        {
            // IClock を使用して現在時刻を取得します。
            var now = _clock.GetCurrentLocalDateTime();

            // DateTimeHelper を使用して、指定された書式で時刻を文字列に変換します。
            var formattedTime = now.ToString(DateTimeFormatKind.LocalUserFriendlyMinutes);

            // 結果をコンソールに出力します。
            System.Console.WriteLine($"The current time is: {formattedTime}");
        }
    }
}
