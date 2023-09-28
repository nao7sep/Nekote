using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace NekoteConsole
{
    internal static class iTimeTester
    {
        // 日時の invariant なフォーマットを策定するに先立ち、区切り文字を分析
        // CultureInfo.InvariantCulture を指定すると /, : が得られる
        // それでは気になる人がどのくらいいるかや、たとえば日付と時刻の区切り文字が反転しているなどで誤解につながる人がどのくらいいるか、といったことを

        // Windows 11 のパソコンで2022年12月27日に生成したファイルを DateAndTimeSeparators.txt として入れておく
        // 日付の区切り文字の四つ目には Right-to-Left Mark が入っている

        // U+200F Right-to-Left Mark (RLM) Unicode Character
        // https://www.compart.com/en/unicode/U+200F

        // ザッと見たが、/ と : で大丈夫そうだ
        // 日本人も、全ての人が和暦と西暦の両方を理解する
        // 日付の区切り文字が - や . の人たちも、/ に遭遇するにおいて、
        //     それが日付と時刻のいずれの区切り文字なのかすら分からないことは考えにくい

        public static void AnalyzeDateAndTimeSeparators ()
        {
            // 何でもよい
            DateTime xUtcNow = DateTime.UtcNow;

            // Console.WriteLine (xUtcNow.ToString ("/, :", CultureInfo.InvariantCulture)); // → /, :

            // ja などのニュートラルなカルチャーを除外
            // たとえば英語のロケールの多くで日付の区切り文字は / だが、「英語 (カナダ), 0x1009, en-CA」では -
            // 「フランス語 (カナダ), 0x0C0C, fr-CA」もそうなので、フランス語の影響だろうと思ったが、「フランス語 (フランス), 0x040C, fr-FR」は /
            // とりあえず、en や fr といったものを統計に含めるべきでないことは分かる

            // "/" や ":" だけではフォーマットに問題があると言われる
            // 1文字増やすと通るようなので、通してトリミング

            var xSeparators = CultureInfo.GetCultures (CultureTypes.SpecificCultures)
                .Select (x => (Culture: x, DateSeparator: xUtcNow.ToString ("/ ", x).TrimEnd (), TimeSeparator: xUtcNow.ToString (": ", x).TrimEnd ()));

            StringBuilder xBuilder = new StringBuilder ();

            var xHoge = xSeparators.GroupBy (x => x.DateSeparator, StringComparer.Ordinal).OrderByDescending (y => y.Count ());

            foreach (var xGroup in xHoge)
            {
                if (xBuilder.Length > 0)
                    xBuilder.AppendLine ();

                xBuilder.AppendLine (FormattableString.Invariant ($"Date Separator: {xGroup.First ().DateSeparator} ({xGroup.Count ()})"));

                xBuilder.AppendLine (string.Join (Environment.NewLine, xGroup.OrderBy (y => y.Culture.Name)
                    .Select (x => $"\x20\x20\x20\x20{x.Culture.DisplayName}, 0x{x.Culture.LCID:X4}, {x.Culture.Name}")));
            }

            var xMoge = xSeparators.GroupBy (x => x.TimeSeparator, StringComparer.Ordinal).OrderByDescending (y => y.Count ());

            foreach (var xGroup in xMoge)
            {
                if (xBuilder.Length > 0)
                    xBuilder.AppendLine ();

                xBuilder.AppendLine (FormattableString.Invariant ($"Time Separator: {xGroup.First ().TimeSeparator} ({xGroup.Count ()})"));

                xBuilder.AppendLine (string.Join (Environment.NewLine, xGroup.OrderBy (y => y.Culture.Name)
                    .Select (x => $"\x20\x20\x20\x20{x.Culture.DisplayName}, 0x{x.Culture.LCID:X4}, {x.Culture.Name}")));
            }

            Console.WriteLine (xBuilder.ToString ());

            nFile.WriteAllText (nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), "DateAndTimeSeparators.txt"), xBuilder.ToString ());
        }

        public static void TestRoundTrips ()
        {
            DateTime xUtcNow = DateTime.UtcNow,
                xLocalNow = DateTime.Now;

            // System.Reflection によるコードの共通化も考えたが、
            //     それほどの量でないし、コピペの方が（今後もし必要になれば）テストをカスタマイズできる

            // Roundtrip 系のものは、UTC かローカル日時かでフォーマットが異なる

#pragma warning disable CA1806
            string xString = nDateTime.ToRoundtripString (xUtcNow);
            Console.WriteLine (xString); // → 2022-12-27T05:07:40.4867604Z
            DateTime xDateTime = nDateTime.ParseRoundtripString (xString);
            nDateTime.TryParseRoundtripString (xString, out DateTime xResult);
            string xStringAlt = xResult.ToRoundtripString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToRoundtripString (xLocalNow);
            Console.WriteLine (xString); // → 2022-12-27T14:07:40.4867709+09:00
            xDateTime = nDateTime.ParseRoundtripString (xString);
            nDateTime.TryParseRoundtripString (xString, out xResult);
            xStringAlt = xResult.ToRoundtripString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            // GMT (Greenwich Mean Time) が入るので UTC でないといけない

            xString = nDateTime.ToRfc1123String (xUtcNow);
            Console.WriteLine (xString); // → Tue, 27 Dec 2022 05:07:40 GMT
            xDateTime = nDateTime.ParseRfc1123String (xString);
            nDateTime.TryParseRfc1123String (xString, out xResult);
            xStringAlt = xResult.ToRfc1123String ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToFriendlyDateTimeString (xUtcNow);
            Console.WriteLine (xString); // → 2022/12/27 5:07:40
            xDateTime = nDateTime.ParseFriendlyDateTimeString (xString);
            nDateTime.TryParseFriendlyDateTimeString (xString, out xResult);
            xStringAlt = xResult.ToFriendlyDateTimeString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToFriendlyDateTimeShortString (xUtcNow);
            Console.WriteLine (xString); // → 2022/12/27 5:07
            xDateTime = nDateTime.ParseFriendlyDateTimeShortString (xString);
            nDateTime.TryParseFriendlyDateTimeShortString (xString, out xResult);
            xStringAlt = xResult.ToFriendlyDateTimeShortString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToFriendlyDateString (xUtcNow);
            Console.WriteLine (xString); // → 2022/12/27
            xDateTime = nDateTime.ParseFriendlyDateString (xString);
            nDateTime.TryParseFriendlyDateString (xString, out xResult);
            xStringAlt = xResult.ToFriendlyDateString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToFriendlyTimeString (xUtcNow);
            Console.WriteLine (xString); // → 5:07:40
            xDateTime = nDateTime.ParseFriendlyTimeString (xString);
            nDateTime.TryParseFriendlyTimeString (xString, out xResult);
            xStringAlt = xResult.ToFriendlyTimeString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToFriendlyTimeShortString (xUtcNow);
            Console.WriteLine (xString); // → 5:07
            xDateTime = nDateTime.ParseFriendlyTimeShortString (xString);
            nDateTime.TryParseFriendlyTimeShortString (xString, out xResult);
            xStringAlt = xResult.ToFriendlyTimeShortString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToMinimalUniversalDateTimeString (xUtcNow);
            Console.WriteLine (xString); // → 20221227T050740Z
            xDateTime = nDateTime.ParseMinimalUniversalDateTimeString (xString);
            nDateTime.TryParseMinimalUniversalDateTimeString (xString, out xResult);
            xStringAlt = xResult.ToMinimalUniversalDateTimeString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            xString = nDateTime.ToMinimalLocalDateTimeString (xLocalNow);
            Console.WriteLine (xString); // → 20221227-140740
            xDateTime = nDateTime.ParseMinimalLocalDateTimeString (xString);
            nDateTime.TryParseMinimalLocalDateTimeString (xString, out xResult);
            xStringAlt = xResult.ToMinimalLocalDateTimeString ();

            if (xResult != xDateTime || xStringAlt != xString)
                throw new nDataException ();

            TimeSpan xTimeDifference = DateTime.Now - DateTime.UtcNow;

            xString = nTimeSpan.ToRoundtripString (xTimeDifference);
            Console.WriteLine (xString); // → 08:59:59.9999998
            TimeSpan xTimeSpan = nTimeSpan.ParseRoundtripString (xString);
            nTimeSpan.TryParseRoundtripString (xString, out TimeSpan xResultAlt);
            xStringAlt = xResultAlt.ToRoundtripString ();
#pragma warning restore CA1806

            if (xResultAlt != xTimeSpan || xStringAlt != xString)
                throw new nDataException ();

            Console.WriteLine ("OK");
        }

        // 2022年12月28日の実行結果

        // TimeZones-20221228T140339Z.txt (Windows 11)
        // TimeZones-20221228T140515Z.txt (Windows 10)
        // TimeZones-20221228T140856Z.txt (Mac)

        // Windows によるファイルは 175～176 KB で、Mac によるものは 5,813 KB
        // Mac の方が圧倒的にそれっぽいデータ

        // たとえば「オーストラリア東部標準時」には149の adjustment rules があり、
        //     そのうち最後の30エントリーほどは未来のデータ
        // 後述する、全てが 0:00 on January 1 になる問題も Mac では起こらない
        // Mac でも 0:00 on January 1 はあるが、始まりと終わりの両方がそうなっているところは、ザッと見た限りなさそう
        // つまり、DaylightTransitionStart: 0:00 on January 1 のエントリーでも、
        //     終わりは DaylightTransitionEnd: 2:59 on April 2 となっていて、
        //     それらが DateStart: 2023/1/1 および DateEnd: 2023/4/2 と整合しているなど

        // 「どこどこの○年○月○日の何時が本当は何時だったか」といったことを突き詰めるプログラミングの経験はまだないが、
        //     今後そういうニーズが生じれば、そういうものは Mac で作るべきということを覚えておく

        public static void GetTimeZones ()
        {
            StringBuilder xBuilder = new StringBuilder ();

            foreach (var xTimeZone in TimeZoneInfo.GetSystemTimeZones ().OrderBy (x => x.BaseUtcOffset).ThenBy (y => y.Id, StringComparer.Ordinal))
            {
                if (xBuilder.Length > 0)
                    xBuilder.AppendLine ();

                // TimeZoneInfo Class (System) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo

                // TimeZoneInfo.AdjustmentRule Class (System) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.adjustmentrule

                // TimeZoneInfo.TransitionTime Struct (System) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.transitiontime

                xBuilder.AppendLine ($"BaseUtcOffset: {xTimeZone.BaseUtcOffset.ToRoundtripString ()}");
                xBuilder.AppendLine ($"DaylightName: {xTimeZone.DaylightName}");
                xBuilder.AppendLine ($"DisplayName: {xTimeZone.DisplayName}");
                xBuilder.AppendLine ($"HasIanaId: {xTimeZone.HasIanaId}");
                xBuilder.AppendLine ($"Id: {xTimeZone.Id}");
                xBuilder.AppendLine ($"StandardName: {xTimeZone.StandardName}");
                xBuilder.AppendLine ($"SupportsDaylightSavingTime: {xTimeZone.SupportsDaylightSavingTime}");

                // アルファベット順では一番だが、実際に表示すれば、ここの方が見やすい

                var xRules = xTimeZone.GetAdjustmentRules ();

                if (xRules.Length > 0)
                {
                    xBuilder.AppendLine ("AdjustmentRules:");

                    for (int temp = 0; temp < xRules.Length; temp ++)
                    {
                        xBuilder.AppendLine (FormattableString.Invariant ($"\x20\x20\x20\x20[{temp}]"));

                        var xRule = xRules [temp];

                        static string iToString (TimeZoneInfo.TransitionTime time)
                        {
                            // Use fixed-date rules for time transitions that occur on a specific day of a specific month (such as 2:00 A.M. on November 3)
                            //     Use floating-date rules for time transitions that occur on a specific day of a specific week of a specific month (such as 2:00 A.M. on the first Sunday of November)
                            //     といった説明があるので、それにならった

                            // Month は1～12

                            // IsFixedDateRule == true なら Month と Day で日付を得られるはずだが、なぜか全てが 0:00 on January 1 になる
                            // TimeZoneInfo.TransitionTime.Day のページのコードをコピペで実行してもそうなので、どこかにバグがあるか → Mac では大丈夫のようだ

                            // TimeZoneInfo.TransitionTime.Day Property (System) | Microsoft Learn
                            // https://learn.microsoft.com/en-us/dotnet/api/system.timezoneinfo.transitiontime.day

                            if (time.IsFixedDateRule)
                                return FormattableString.Invariant ($"{time.TimeOfDay.ToFriendlyTimeShortString ()} on {nInvariantCulture.MonthNames [time.Month - 1]} {time.Day}");

                            // nInvariantCulture.GetOrdinalNumberString を用意したが、Week は5が「5番目」でなく「最後」を意味するようだ
                            // その曜日が4回しかない月において「最後」として Week が5のところがあるかどうかについては、今回は深入りしない

                            string [] xWeekStrings = { "first", "second", "third", "fourth", "last" };

                            // DayOfWeek Enum (System) | Microsoft Learn
                            // https://learn.microsoft.com/en-us/dotnet/api/system.dayofweek

                            return $"{time.TimeOfDay.ToFriendlyTimeShortString ()} on the {xWeekStrings [time.Week - 1]} {nInvariantCulture.DayOfWeekNames [(int) time.DayOfWeek]} of {nInvariantCulture.MonthNames [time.Month - 1]}";
                        }

                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 BaseUtcOffsetDelta: {xRule.BaseUtcOffsetDelta.ToRoundtripString ()}");
                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 DateEnd: {xRule.DateEnd.ToFriendlyDateString ()}");
                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 DateStart: {xRule.DateStart.ToFriendlyDateString ()}");
                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 DaylightDelta: {xRule.DaylightDelta.ToRoundtripString ()}");
                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 DaylightTransitionEnd: {iToString (xRule.DaylightTransitionEnd)}");
                        xBuilder.AppendLine ($"\x20\x20\x20\x20\x20\x20\x20 DaylightTransitionStart: {iToString (xRule.DaylightTransitionStart)}");
                    }
                }
            }

            string xFilePath = nPath.Map (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), $"TimeZones-{DateTime.UtcNow.ToMinimalUniversalDateTimeString ()}.txt");
            nFile.WriteAllText (xFilePath, xBuilder.ToString ());

            Console.WriteLine (xBuilder.ToString ());
        }
    }
}
