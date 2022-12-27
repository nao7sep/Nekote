using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
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

            if (xResultAlt != xTimeSpan || xStringAlt != xString)
                throw new nDataException ();

            Console.WriteLine ("OK");
        }
    }
}
