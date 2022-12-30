using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iStringTester
    {
        // 文字列を行に分割する処理をテスト
        // 引数によっては .NET と同じ結果が得られることも確認
        // 処理の結果は、長くなるのでコードの近くに

        // 半角空白が分かりやすいよう、入出力の両方において _ に置き換えている

        // Mac での動作を確認した

        public static void EnumerateLines ()
        {
            void iEnumerateLinesAlt (string value, bool trimsTrailingWhiteSpaces, bool reducesEmptyLines)
            {
                IEnumerable <string> xEnumerated = nString.EnumerateLines (value, trimsTrailingWhiteSpaces, reducesEmptyLines);
                int xCount = xEnumerated.Count ();

                Console.WriteLine ($"nStringLineReader (trimsTrailingWhiteSpaces: {trimsTrailingWhiteSpaces}, reducesEmptyLines: {reducesEmptyLines}) → {xCount}");

                if (xCount > 0)
                    Console.WriteLine (string.Join (Environment.NewLine, xEnumerated.Select (x => $"\x20\x20\x20\x20|{x.Replace ('\x20', '_')}|")));
            }

            void iEnumerateLines (string value)
            {
                string xValue = value.Replace ('_', '\x20');

                List <string> xLines = new List <string> ();

                using (StringReader xReader = new StringReader (xValue))
                {
                    string? xLine;

                    while ((xLine = xReader.ReadLine ()) != null)
                        xLines.Add (xLine);
                }

                Console.WriteLine ($"StringReader → {xLines.Count}");

                if (xLines.Count > 0)
                    Console.WriteLine (string.Join (Environment.NewLine, xLines.Select (x => $"\x20\x20\x20\x20|{x.Replace ('\x20', '_')}|")));

                iEnumerateLinesAlt (xValue, trimsTrailingWhiteSpaces: true, reducesEmptyLines: true);
                iEnumerateLinesAlt (xValue, trimsTrailingWhiteSpaces: true, reducesEmptyLines: false);
                iEnumerateLinesAlt (xValue, trimsTrailingWhiteSpaces: false, reducesEmptyLines: true);
                iEnumerateLinesAlt (xValue, trimsTrailingWhiteSpaces: false, reducesEmptyLines: false);
            }

            // @"" にすればベタッと書けるが、好みでない

            string xNewLine = Environment.NewLine;

            // StringReader → 0
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: True) → 0
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: False) → 0
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: True) → 0
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: False) → 0

            iEnumerateLines (
                "");

            // 掃除をしないモードなら、全く同じ結果になる
            // 行末の空白系文字または空行を洗うと、それぞれ、イメージ通りの結果に
            // いずれも洗えば、中身のない文字列であることが明示される

            // StringReader → 3
            //     ||
            //     |_|
            //     ||
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: True) → 0
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: False) → 3
            //     ||
            //     ||
            //     ||
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: True) → 1
            //     |_|
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: False) → 3
            //     ||
            //     |_|
            //     ||

            iEnumerateLines (
                "" + xNewLine +
                "_" + xNewLine +
                "" + xNewLine);

            // 長くなったが、調べている内容はあまり異ならない
            // 前回との違いは、残すべき文字を残せていることと、残すべき文字の含まれる二つの行の間でも空行が洗われていることの確認

            // StringReader → 11
            //     ||
            //     |_|
            //     ||
            //     |_a_|
            //     ||
            //     |_|
            //     ||
            //     |_b_|
            //     ||
            //     |_|
            //     ||
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: True) → 3
            //     |_a|
            //     ||
            //     |_b|
            // nStringLineReader (trimsTrailingWhiteSpaces: True, reducesEmptyLines: False) → 11
            //     ||
            //     ||
            //     ||
            //     |_a|
            //     ||
            //     ||
            //     ||
            //     |_b|
            //     ||
            //     ||
            //     ||
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: True) → 9
            //     |_|
            //     ||
            //     |_a_|
            //     ||
            //     |_|
            //     ||
            //     |_b_|
            //     ||
            //     |_|
            // nStringLineReader (trimsTrailingWhiteSpaces: False, reducesEmptyLines: False) → 11
            //     ||
            //     |_|
            //     ||
            //     |_a_|
            //     ||
            //     |_|
            //     ||
            //     |_b_|
            //     ||
            //     |_|
            //     ||

            iEnumerateLines (
                "" + xNewLine +
                "_" + xNewLine +
                "" + xNewLine +
                "_a_" + xNewLine +
                "" + xNewLine +
                "_" + xNewLine +
                "" + xNewLine +
                "_b_" + xNewLine +
                "" + xNewLine +
                "_" + xNewLine +
                "" + xNewLine);
        }

        // nString.EnumerateParagraphs は、nString.EnumerateLines が大丈夫なら大丈夫
        // さまざまなファイルをザッとチェックするため、ファイルパスを受け取り、処理し、デスクトップに *.txt として保存
        // 拡張子を変更するのは、段落・行に展開され、それらの明示のための記号入りで再フォーマットされたものは、もはや本来の機能を持たないため
        // 出力先のファイルが既存なら、ハイフンと1からの連番が付けられる

        // Mac での動作を確認した

        public static void EnumerateParagraphs (string filePath)
        {
            try
            {
                var xParagraphs = nFile.ReadAllText (filePath).EnumerateParagraphs ();

                string xBorderLine = Environment.NewLine + new string ('=', 80) + Environment.NewLine,
                    xFileContents = string.Join (xBorderLine, xParagraphs.Select (x => string.Join (Environment.NewLine, x.Select (y => $"|{y}|"))));

                string xNewFilePartialPath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), Path.GetFileNameWithoutExtension (filePath)),
                    xNewFilePath = xNewFilePartialPath + ".txt";

                for (int temp = 1; nFile.CanCreate (xNewFilePath) == false; temp ++)
                    xNewFilePath = $"{xNewFilePartialPath}-{temp}.txt";

                nFile.WriteAllText (xNewFilePath, xFileContents);
            }

            catch (Exception xException)
            {
                nException.Log (xException);
                nConsole.WriteErrorHasOccurredMessage (xException);
                nConsole.WritePressAnyKeyToContinueMessage ();
            }
        }

        // Mac での動作を確認した

        public static void TestStringOptimization ()
        {
            void iOptimize (string value, nStringOptimizationOptions options)
            {
                string xValue = value
                    .Replace ('□', '\x20')
                    .Replace ('T', '\t')
                    .Replace ('N', '\xA0')
                    .Replace ('I', '\x3000');

                // テストコードなので改行はデフォルト値で
                // 一応の作法として、中でデフォルト値になるなら上でもデフォルト値にする必要はない
                nStringOptimizationResult xResult = nStringOptimizer.Default.Optimize (xValue, options, newLine: null);

                if (xResult.Lines.Count > 0)
                    Console.WriteLine (string.Join (Environment.NewLine, xResult.Lines.Select (x =>
                    {
                        return (x.IndentationString + x.VisibleString)
                            .Replace ('\x20', '□')
                            .Replace ('\t', 'T')
                            .Replace ('\xA0', 'N')
                            .Replace ('\x3000', 'I');
                    })));

                Console.WriteLine ($"VisibleLineCount: {xResult.VisibleLineCount}");
                Console.WriteLine ($"MinIndentationLength: {xResult.MinIndentationLength}");
            }

            string xNewLine = Environment.NewLine;

            // フィールドの値の変遷に注意して使い回す
            nStringOptimizationOptions xOptions = new nStringOptimizationOptions ();

            // =============================================================================

            // nStringLineReader のオプションをテスト
            // 行末の空白系文字や不要な空行を削らない

            xOptions.TrimsTrailingWhiteSpaces = false;
            xOptions.ReducesEmptyLines = false;

            // 各行については、できるだけ削るものからテスト

            xOptions.RemovesIndentation = true;
            xOptions.ReducesInlineWhiteSpaces = true;

            iOptimize (
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine +
                "TNI*TNI*TNI" + xNewLine +
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine +
                "TNI*TNI*TNI" + xNewLine +
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine,
                xOptions);

            // インデント部分では TNI の全てが失われ、インライン部分では半角空白一つに統合された
            // 行末の TNI も統合されたことについては、nStringOptimizer.Optimize を

            //
            //
            //
            // *□*□
            //
            //
            //
            // *□*□
            //
            //
            //
            // VisibleLineCount: 2
            // MinIndentationLength: 0

            Console.WriteLine ();

            // =============================================================================

            // nStringLineReader のオプションは、今後は常にオン
            // 他と互いに影響しないため、一度のテストで足りる

            xOptions.TrimsTrailingWhiteSpaces = true;
            xOptions.ReducesEmptyLines = true;

            // これらも、他と互いに影響しないため、一度で十分

            xOptions.RemovesIndentation = false;
            xOptions.ReducesInlineWhiteSpaces = false;

            // 文字を置換するときの幅は全てデフォルト値
            // 置換のテストのためにインデント幅の調整をオフに

            xOptions.IndentationTabWidth = 4;
            xOptions.IndentationNoBreakSpaceWidth = 1;
            xOptions.IndentationIdeographicSpaceWidth = 2;
            xOptions.MinIndentationLength = null;

            xOptions.InlineTabWidth = 4;
            xOptions.InlineNoBreakSpaceWidth = 1;
            xOptions.InlineIdeographicSpaceWidth = null;

            iOptimize (
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine +
                "T*T*T" + xNewLine +
                "N*N*N" + xNewLine +
                "I*I*I" + xNewLine +
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine +
                "T*T*T" + xNewLine +
                "N*N*N" + xNewLine +
                "I*I*I" + xNewLine +
                "" + xNewLine +
                "□" + xNewLine +
                "" + xNewLine,
                xOptions);

            // インデント部分では、T, N, I の全てが半角空白になった
            // インライン部分では、オプション通りに全角空白のみそのまま残った
            // その仕様については nStringOptimizationOptions に

            // MinIndentationLength が空行を除く行から正確に得られた

            // □□□□*□□□□*
            // □*□*
            // □□*I*
            //
            // □□□□*□□□□*
            // □*□*
            // □□*I*
            // VisibleLineCount: 6
            // MinIndentationLength: 1

            Console.WriteLine ();

            // =============================================================================

            // インデントを調整しないモードのまま、文字の置換を前回と反対に

            xOptions.IndentationTabWidth = null;
            xOptions.IndentationNoBreakSpaceWidth = null;
            xOptions.IndentationIdeographicSpaceWidth = null;

            xOptions.InlineTabWidth = null;
            xOptions.InlineNoBreakSpaceWidth = null;
            xOptions.InlineIdeographicSpaceWidth = 2;

            // もう空行の扱いについては見る必要がない

            iOptimize (
                "T*T*T" + xNewLine +
                "N*N*N" + xNewLine +
                "I*I*I" + xNewLine,
                xOptions);

            // オプション通りに処理された

            // MinIndentationLength の取得において T/N/I が区別されないのは仕様
            // 特に T, I は、半角空白に変換されない限り長さが「1以上の不明な数」とみなされる
            // その状況で得られる MinIndentationLength に意味付けして後続のコードで頼ることは考えにくい
            // この仕様については、nStringOptimizer.Optimize のコメントが詳しい

            // T*T*
            // N*N*
            // I*□□*
            // VisibleLineCount: 3
            // MinIndentationLength: 1

            Console.WriteLine ();

            // =============================================================================

            // インデントを増やす処理のテスト
            // 文字の置換が関係しないように半角空白のみを使う

            xOptions.MinIndentationLength = 1;

            iOptimize (
                "*" + xNewLine +
                "" + xNewLine +
                "□*" + xNewLine,
                xOptions);

            // □*
            //
            // □□*
            // VisibleLineCount: 2
            // MinIndentationLength: 1

            Console.WriteLine ();

            // =============================================================================

            // インデントを減らす

            xOptions.MinIndentationLength = 1;

            iOptimize (
                "□□□*" + xNewLine +
                "" + xNewLine +
                "□□*" + xNewLine,
                xOptions);

            // □□*
            //
            // □*
            // VisibleLineCount: 2
            // MinIndentationLength: 1

            Console.WriteLine ();

            // =============================================================================

            // オプションによりインデントを消しても、その後の調整により戻ることのテスト

            xOptions.RemovesIndentation = true;
            xOptions.MinIndentationLength = 1;

            iOptimize (
                "□a",
                xOptions);

            // □a
            // VisibleLineCount: 1
            // MinIndentationLength: 1

            Console.WriteLine ();

            // =============================================================================

            // nStringLineReader では「行なし」と認識される空の文字列
            // 内部の Min で「要素がない」として落ちたので、もう落ちないことをテスト

            iOptimize (
                "",
                xOptions);

            // VisibleLineCount: 0
            // MinIndentationLength: 0
        }

        // 文字列の結合の速度を比較

        // 結論としては、+ と string.Concat には速度差がなく、
        //     $"" も、コンパイラーによる最適化で + などと同等に速いことがあり、
        //     string.Format のみ、現時点では単純結合でも絶望的に遅い

        // + は糖衣構文のようなもので、コンパイル時に string.Concat にされるか、
        //     あるいは + と string.Concat の両方がさらに共通のメソッドを呼ぶと思っていた
        // しかし、+ には BuiltInOperators が用意されていて、深入りはしていないが、
        //     その内容は、string.Concat の単純な実装とは、ずいぶんと違っているように見えた

        // c# - where is the string operator + source code? - Stack Overflow
        // https://stackoverflow.com/questions/58924625/where-is-the-string-operator-source-code

        // BuiltInOperators.cs
        // https://sourceroslyn.io/#Microsoft.CodeAnalysis.CSharp/Compilation/BuiltInOperators.cs

        // String.Manipulation.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs

        // 数年前のノートである SV7 での結果

        // operator +: 2621.9396ms
        // string.Concat: 2620.3285ms
        // string.Format: 13631.6015ms
        // string.Format + Parallel.For: 6990.2848ms
        // interpolation: 2857.9809ms

        // M1 の MacBook Pro での実行結果
        // CPU のアーキテクチャーの違いをエミュレーションでしのいでいるのか、
        //     Parallel.For を使ってもあまり速くならないのが興味深い

        // operator +: 1868.7173ms
        // string.Concat: 1859.7751ms
        // string.Format: 9680.5629ms
        // string.Format + Parallel.For: 7360.5268ms
        // interpolation: 1933.3954ms

        // $"" は string.Format と同じくらい遅いはずだから使用を見直そうと思っていた
        // しかし、結果としては + などと同じくらい速かった
        // 次のページには、The compiler may replace String.Format with String.Concat if the analyzed behavior would be equivalent to concatenation とある
        // 明らかに単純結合で、コンパイラーによる最適化を期待できるところでは、コードの可読性のために今後も $"" を積極的に使っていく

        // $ - string interpolation - format string output | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated

        // ついでに、+ や string.Concat に null を渡した場合の挙動について調べた
        // + については、When one or both operands are of type string,
        //     the + operator concatenates the string representations of its operands (the string representation of null is an empty string) とあった
        // string.Concat については、An Empty string is used in place of any null argument とあった
        // null についても挙動が同じであることと、戻り値が null になることは絶対にないことが分かった

        // Addition operators - + and += | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/addition-operator

        // String.Concat Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.string.concat

        // null の扱いについて調べたのは、+ にマウスポインターを当てたときに表示された string string.operator + (string left, string right) が気になったため
        // left などに ? が付いていないが、null は余裕で通る
        // となると、戻り値に ? が付いていないことの信憑性も疑わざるを得ない
        // 引数に ? が付いていない理由は今も不明だが、null が返らないことが分かったので十分

        public static void CompareStringConcatenationSpeeds ()
        {
            const int xTestCount = 10,
                xConcatenationCount = 100_000_000;

            string xHoge = "hoge",
                xMoge = "moge",
                xPoge = "poge",
                xBoge = "boge",
                xDoge;

            Stopwatch xStopwatch = new Stopwatch ();

            string [] xLabels = { "operator +", "string.Concat", "string.Format", "string.Format + Parallel.For", "interpolation" };
            nMultiArray <TimeSpan> xElapsed = new nMultiArray <TimeSpan> ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xLabelIndex = 0;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xConcatenationCount; tempAlt ++)
                    xDoge = xHoge + xMoge + xPoge + xBoge;

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xConcatenationCount; tempAlt ++)
                    xDoge = string.Concat (xHoge, xMoge, xPoge, xBoge);

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xConcatenationCount; tempAlt ++)
                    xDoge = string.Format ("{0}{1}{2}{3}", xHoge, xMoge, xPoge, xBoge);

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                // 遅い string.Format が Parallel.For ならどうなるか興味本位で
                // 今回のテストでは倍ほど速くなったので、重たい処理なら積極的に並列化する

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                Parallel.For (0, xConcatenationCount, x =>
                {
                    xDoge = string.Format ("{0}{1}{2}{3}", xHoge, xMoge, xPoge, xBoge);
                });

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xConcatenationCount; tempAlt ++)
                    xDoge = $"{xHoge}{xMoge}{xPoge}{xBoge}";

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (iTester.FormatLabelsAndElapsedTimes (xLabels, xElapsed));
        }

        // 文字列を行に分割する速度の比較
        // 数年前のノートである SV7 での実行結果

        // 行末の空白系文字を削るなどする nString.EnumerateLines がそれほど遅くなかった
        // 改行文字の検索に ReadOnlySpan <char>.IndexOfAny を使うからか

        // StringReader.ReadLine: 2275.9624ms
        // MemoryExtensions.EnumerateLines: 3080.7091ms
        // nString.EnumerateLines: 3966.3628ms

        // M1 の MacBook Pro での実行結果
        // だいたいのテストで Mac の方が速いのに、ここでは全面的に負けている

        // StringReader.ReadLine: 3566.2762ms
        // MemoryExtensions.EnumerateLines: 4321.9534ms
        // nString.EnumerateLines: 5562.8636ms

        public static void CompareLineEnumerationSpeeds ()
        {
            const int xTestCount = 10,
                xEnumerationCount = 10_000_000;

            string xNewLine = Environment.NewLine,
                xValue =
                    "" + xNewLine +
                    "_" + xNewLine +
                    "" + xNewLine +
                    "_a_" + xNewLine +
                    "" + xNewLine +
                    "_" + xNewLine +
                    "" + xNewLine +
                    "_b_" + xNewLine +
                    "" + xNewLine +
                    "_" + xNewLine +
                    "" + xNewLine;

            List <string> xLines;

            Stopwatch xStopwatch = new Stopwatch ();

            string [] xLabels = { "StringReader.ReadLine", "MemoryExtensions.EnumerateLines", "nString.EnumerateLines" };
            nMultiArray <TimeSpan> xElapsed = new nMultiArray <TimeSpan> ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xLabelIndex = 0;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xEnumerationCount; tempAlt ++)
                {
                    xLines = new List <string> ();

                    using (StringReader xReader = new StringReader (xValue))
                    {
                        string? xLine;

                        while ((xLine = xReader.ReadLine ()) != null)
                            xLines.Add (xLine);
                    }
                }

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xEnumerationCount; tempAlt ++)
                {
                    xLines = new List <string> ();

                    var xEnumerator = MemoryExtensions.EnumerateLines (xValue.AsSpan ());

                    while (xEnumerator.MoveNext ())
                        xLines.Add (xEnumerator.Current.ToString ());
                }

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xEnumerationCount; tempAlt ++)
                    nString.EnumerateLines (xValue).ToList ();

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (iTester.FormatLabelsAndElapsedTimes (xLabels, xElapsed));
        }

        // 数年前のノートである SV7 での結果

        // nString.Optimize: 1569.5653ms

        // 1ミリ秒あたり637回くらい実行できたようだ
        // 名前やメールアドレスなど、もっと短いものがほとんどの、数項目から数十項目ほどのフォームで全てを最適化しても1ミリ秒も掛からない
        // ユーザーの意図を損なう過度な最適化を避けながらも、セキュリティーの向上などのため、nString.Optimize を積極的に使う

        // M1 の MacBook Pro での実行結果
        // CompareLineEnumerationSpeeds が遅かったのに最適化が速い理由は分からない

        // nString.Optimize: 1274.7906ms

        public static void TestStringOptimizationSpeed ()
        {
            const int xTestCount = 10,
                xOptimizationCount = 1_000_000;

            string xNewLine = Environment.NewLine,
                xValue =
                    ("" + xNewLine +
                    "□" + xNewLine +
                    "" + xNewLine +
                    "T*T*T" + xNewLine +
                    "N*N*N" + xNewLine +
                    "I*I*I" + xNewLine +
                    "" + xNewLine +
                    "□" + xNewLine +
                    "" + xNewLine +
                    "T*T*T" + xNewLine +
                    "N*N*N" + xNewLine +
                    "I*I*I" + xNewLine +
                    "" + xNewLine +
                    "□" + xNewLine +
                    "" + xNewLine)
                        .Replace ('□', '\x20')
                        .Replace ('T', '\t')
                        .Replace ('N', '\xA0')
                        .Replace ('I', '\x3000');

            Stopwatch xStopwatch = new Stopwatch ();

            string [] xLabels = { "nString.Optimize" };
            nMultiArray <TimeSpan> xElapsed = new nMultiArray <TimeSpan> ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xLabelIndex = 0;

                // =============================================================================

                xStopwatch.Reset ();
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xOptimizationCount; tempAlt ++)
                    xValue.Optimize ();

                xElapsed [xLabelIndex ++, temp] = xStopwatch.Elapsed;

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (iTester.FormatLabelsAndElapsedTimes (xLabels, xElapsed));
        }

        // 後続の TestStringOptimizationAlt とセットで、与えたディレクトリー内の（ほとんど）全てのテキストファイルの内容を最適化し、
        //     パスの階層構造を保ちながらデスクトップに新規保存するテストを行う

        // これにより Nekote のディレクトリーなどを丸ごと最適化し、WinMerge などで比較したところ、ほとんどのファイルが完全一致した

        // 自作タスク管理ソフトの過去ログを変換したものである taskKiller.txt にハードタブの混入が認められたが、実害がないため対処しない

        private static void iHandleDirectoryForStringOptimizationTest (DirectoryInfo sourceDirectory, DirectoryInfo destDirectory)
        {
            // .git などを回避
            // こういう命名のディレクトリーにユーザーが手作業で作ったファイルが多数含まれることは考えにくい

            if (sourceDirectory.Name.StartsWith ('.'))
                return;

            // 保存先のディレクトリーを作るより先に、元々のディレクトリーが存在するか見る

            var xSubdirectories = sourceDirectory.GetDirectories ();

            nDirectory.Create (destDirectory.FullName);

            foreach (DirectoryInfo xSubdirectory in xSubdirectories)
                iHandleDirectoryForStringOptimizationTest (xSubdirectory, new DirectoryInfo (nPath.Join (destDirectory.FullName, xSubdirectory.Name)));

            foreach (FileInfo xSourceFile in sourceDirectory.GetFiles ())
            {
                // .gitignore は処理されてもよいが、それくらいしか思い当たらない
                // ディレクトリーの回避と挙動を整合させておく

                if (xSourceFile.Name.StartsWith ('.'))
                    continue;

                // バイナリーからエンコーディングを検出し、可能ならそれでテキストを別ルートでバイナリーにする
                // BOM 分を除く部分が一致すれば、ラウンドトリップの成功によりテキスト系ファイルの可能性が高いと判断
                // 不一致部分が BOM のみなのか調べないし、Shift-JIS などを正しく扱えないだろうから、今のところメソッド化しない
                // エンコーディングを判別するライブラリーを Nekote に組み込むことも検討したが、
                //     今はむしろローカルのエンコーディングの使用を減らしていく時代

                string xText = nFile.ReadAllText (xSourceFile.FullName);
                byte [] xBinary = nFile.ReadAllBytes (xSourceFile.FullName);
                Encoding? xEncoding = nBom.GetEncoding (xBinary, 0, xBinary.Length);
                byte [] xBinaryFromText = (xEncoding ?? Encoding.UTF8).GetBytes (xText);

                if (xBinaryFromText.Length <= xBinary.Length)
                {
                    if (nArray.Equals (xBinary, xBinary.Length - xBinaryFromText.Length, xBinaryFromText, 0, xBinaryFromText.Length))
                    {
                        string xDestFilePath = nPath.Join (destDirectory.FullName, xSourceFile.Name),
                            xDestFileContents = xText.Optimize ()!;

                        // 最適化すると、「段落の集合」のような扱いになり、区切りとして空行は入るが、末尾の改行は落ちる
                        // 末尾の改行だけで diff ソフトで「不一致」と表示されないようにしておく

                        if (xDestFileContents != xText && (xDestFileContents + Environment.NewLine) == xText)
                            xDestFileContents += Environment.NewLine;

                        nFile.WriteAllText (xDestFilePath, xDestFileContents, xEncoding ?? Encoding.UTF8);

                        // diff ソフトでチェック済みのディレクトリーを同期するときに「保存先のファイルの方が新しいようですが」と毎回言われないように
                        File.SetLastWriteTimeUtc (xDestFilePath, xSourceFile.LastWriteTimeUtc);

                        Console.WriteLine ("ファイルを作成しました: " + xDestFilePath);
                    }
                }
            }
        }

        // Mac での動作を確認した

        public static void TestStringOptimizationAlt (string directoryPath)
        {
            DirectoryInfo xSourceDirectory = new DirectoryInfo (directoryPath);

            string xDestDirectoryPartialPath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), xSourceDirectory.Name),
                xDestDirectoryPath = xDestDirectoryPartialPath;

            for (int temp = 1; nDirectory.CanCreate (xDestDirectoryPath) == false; temp ++)
                xDestDirectoryPath = $"{xDestDirectoryPartialPath}-{temp}";

            iHandleDirectoryForStringOptimizationTest (xSourceDirectory, new DirectoryInfo (xDestDirectoryPath));
        }

        // Mac での動作を確認した

        public static void TestNameValueCollection ()
        {
            nNameValueCollection xCollection = new nNameValueCollection (new NameValueCollection ());

            // (Empty) になるのを確認した
            // Console.WriteLine (xCollection.ToFriendlyString ()); // OK

            // Console.WriteLine (xCollection.Keys.Count); // 0
            // Console.WriteLine (xCollection.AllKeys.Length); // 0
            // Console.WriteLine (xCollection.ContainsKey (null)); // False

            xCollection.SetString (null, null);
            // Console.WriteLine (xCollection.Keys.Count); // 1
            // Console.WriteLine (xCollection.AllKeys.Length); // 1
            // Console.WriteLine (xCollection.ContainsKey (null)); // True

            // ソートのテストのため、キーの連番を降順に

            xCollection.SetString ("Name-2", null); // OK
            xCollection.SetString ("Name-2", string.Empty); // OK
            xCollection.SetString ("Name-2", "hoge"); // OK
            xCollection.SetString ("Name-2", $"hoge{Environment.NewLine}moge"); // OK

            // AddString が、キーが既存がどうかに関わらず null を保持するのを確認

            xCollection.AddString ("Name-1", null); // OK
            xCollection.AddString ("Name-1", null); // OK
            xCollection.AddString ("Name-1", string.Empty); // OK
            xCollection.AddString ("Name-1", "hoge"); // OK
            xCollection.AddString ("Name-1", $"hoge{Environment.NewLine}moge"); // OK

            string xString = xCollection.ToFriendlyString ();

            // (Null): (Null)
            // Name-1:
            //     [0] (Null)
            //     [1] (Null)
            //     [2] (Empty)
            //     [3] hoge
            //     [4]
            //         hoge
            //         moge
            // Name-2:
            //     hoge
            //     moge

            Console.WriteLine (xString);

            // 末尾に余計な改行などが付いていないことを確認した
            // Console.WriteLine (xString.Optimize () == xString); // True
        }

        // DiffMatchPatch.cs は、2022年12月26日現在、2018年7月31日から更新が止まっている
        // その間の C# の変化により Nullable 関連の警告が多い
        // GitHub の Issues を見る限り、バグが落ちきっているコード
        // 警告を無効化し、Nekote での使用を継続する

        // 何かをテストするわけでないため、ここでよいのか迷う
        // ここでは、ここでダメな特段の理由がないという消極的判断を

        public static void PatchDiffMatchPatch ()
        {
            string? xFilePath = iTester.FindFileOrDirectory ("DiffMatchPatch.cs");
            Encoding? xEncoding = nFile.GetEncoding (xFilePath!);

            // ASCII かと思えば、BOM 付きの UTF-8 だった

            // if (xEncoding != null)
            //     Console.WriteLine (xEncoding.EncodingName); // → Unicode (UTF-8)

            string xFileContents = nFile.ReadAllText (xFilePath!);

            int xIndex = xFileContents.IndexOf ("using System;", StringComparison.Ordinal),
                xIndexAlt = xFileContents.IndexOf ("#pragma warning disable CS8600", 0, xIndex, StringComparison.Ordinal);

            if (xIndexAlt >= 0)
            {
                Console.WriteLine ("既にパッチされています。");
                return;
            }

            xIndexAlt = xFileContents.IndexOf ('\n', StringComparison.Ordinal);
            string xNewLine = xIndexAlt >= 1 && xFileContents [xIndexAlt - 1] == '\r' ? "\r\n" : "\n";

            xFileContents = xFileContents.Insert (xIndex,
                "// iStringTester.PatchDiffMatchPatch" + xNewLine +
                xNewLine +
                "#pragma warning disable CS8600" + xNewLine +
                "#pragma warning disable CS8602" + xNewLine +
                "#pragma warning disable CS8603" + xNewLine +
                "#pragma warning disable CS8765" + xNewLine +
                xNewLine);

            xFileContents +=
                xNewLine +
                "#pragma warning restore CS8600" + xNewLine +
                "#pragma warning restore CS8602" + xNewLine +
                "#pragma warning restore CS8603" + xNewLine +
                "#pragma warning restore CS8765" + xNewLine;

            nFile.WriteAllText (xFilePath!, xFileContents, xEncoding);
        }

        // 2022年12月28日の実行結果

        // Encodings-20221228T140339Z.txt (Windows 11)
        // Encodings-20221228T140516Z.txt (Windows 10)
        // Encodings-20221228T140856Z.txt (Mac)

        // .NET 内部のデータを取得しただけなのか、三つとも内容が同じ
        // ファイルサイズが少し異なるのは、改行の違いによる

        public static void GetEncodings ()
        {
            // Console.WriteLine (Encoding.GetEncodings ().Max (x => x.CodePage)); // → 65001

            StringBuilder xBuilder = new StringBuilder ();

            // Encoding.GetEncodings Method (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencodings

            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);

            foreach (EncodingInfo xEncoding in Encoding.GetEncodings ().OrderBy (x => x.CodePage))
            {
                if (xBuilder.Length > 0)
                    xBuilder.AppendLine ();

                // EncodingInfo の三つは、必ず取得できる
                // Encoding.GetEncoding には、こちらの CodePage または Name を与える

                // EncodingInfo Class (System.Text) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.text.encodinginfo

                xBuilder.AppendLine (FormattableString.Invariant ($"CodePage: 0x{xEncoding.CodePage:X4} ({xEncoding.CodePage})"));
                xBuilder.AppendLine ($"DisplayName: {xEncoding.DisplayName}");
                xBuilder.AppendLine ($"Name: {xEncoding.Name}");

                Encoding xEncodingAlt = xEncoding.GetEncoding ();

                xBuilder.AppendLine ("\x20\x20\x20 Encoding:");

                string xIndentationString = "\x20\x20\x20\x20\x20\x20\x20\x20";

                // Encoding クラスの方は、データを取得しようとすると落ちるプロパティーが多々ある

                // Encoding Class (System.Text) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding

                // EncodingTable.InternalGetCodePageDataItem によると、WebName → BodyName/HeaderName
                // データの重複になるが、プロパティーの存在は確かなので一応そのまま出力

                // EncodingTable.cs
                // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Text/EncodingTable.cs

                try { xBuilder.AppendLine (xIndentationString + "BodyName: " + xEncodingAlt.BodyName); } catch {}

                try { xBuilder.AppendLine (xIndentationString + FormattableString.Invariant ($"CodePage: 0x{xEncodingAlt.CodePage:X4} ({xEncodingAlt.CodePage})")); } catch {}

                // 日本語のエンコーディング名を取得できなくなっていることについて

                // 以前は「日本語 (シフト JIS)」のようなエンコーディング名を取得できたが、今では Japanese (Shift-JIS) のみ
                // EncodingTable.GetDisplayName では、SR.GetResourceString ("Globalization_cp_" + codePage.ToString ()) により名前を得られない場合、s_englishNames のものが設定される
                // SR.GetResourceString を呼ぶ方法を探したところ、Environment に internal static string? GetResourceStringLocal (string key) => SR.GetResourceString (key) を発見

                // Environment.CoreCLR.cs
                // https://source.dot.net/#System.Private.CoreLib/src/System/Environment.CoreCLR.cs

                // これは、typeof (Environment).GetMethod ("GetResourceStringLocal", BindingFlags.NonPublic | BindingFlags.Static)
                //     .Invoke (null, new object [] { "Globalization_cp_" }) のようにすれば使えた
                // SR クラスにキーがたくさんあり、適当に指定してみたところ日本語のメッセージを得られた

                // System.SR.cs
                // https://source.dot.net/#System.Private.CoreLib/artifacts/obj/coreclr/System.Private.CoreLib/x64/Debug/System.SR.cs

                // しかし、"Globalization_cp_" + codePage.ToString () ではダメだった
                // Encoding.RegisterProvider (CodePagesEncodingProvider.Instance) が必要になったことからも、ただデータがないのだろう

                // ResourceManager を使うために NuGet で System.Resources.ResourceManager を入れてみたが、こちらは動かなかった
                // 互換性の問題か、何かミスっていたのか不詳だが、GetResourceStringLocal でダメだったので深入りしなかった

                // ResourceManager Class (System.Resources) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.resources.resourcemanager

                // NuGet Gallery | System.Resources.ResourceManager 4.3.0
                // https://www.nuget.org/packages/System.Resources.ResourceManager/

                try { xBuilder.AppendLine (xIndentationString + "EncodingName: " + xEncodingAlt.EncodingName); } catch {}

                try { xBuilder.AppendLine (xIndentationString + "HeaderName: " + xEncodingAlt.HeaderName); } catch {}

                // 多くのエンコーディングで get 時に落ちるプロパティーが四つ続く
                // イマイチよく分からないものばかりだが、とりあえず出力しておく

                try { xBuilder.AppendLine (xIndentationString + "IsBrowserDisplay: " + xEncodingAlt.IsBrowserDisplay); } catch {}
                try { xBuilder.AppendLine (xIndentationString + "IsBrowserSave: " + xEncodingAlt.IsBrowserSave); } catch {}
                try { xBuilder.AppendLine (xIndentationString + "IsMailNewsDisplay: " + xEncodingAlt.IsMailNewsDisplay); } catch {}
                try { xBuilder.AppendLine (xIndentationString + "IsMailNewsSave: " + xEncodingAlt.IsMailNewsSave); } catch {}

                try { xBuilder.AppendLine (xIndentationString + "IsReadOnly: " + xEncodingAlt.IsReadOnly); } catch {}
                try { xBuilder.AppendLine (xIndentationString + "IsSingleByte: " + xEncodingAlt.IsSingleByte); } catch {}

                if (xEncodingAlt.Preamble.Length > 0)
                {
                    try { xBuilder.AppendLine (xIndentationString + "Preamble: " + string.Join (" ", xEncodingAlt.Preamble.ToArray ().Select (x => $"0x{x:X2}"))); } catch {}
                }

                try { xBuilder.AppendLine (xIndentationString + "WebName: " + xEncodingAlt.WebName); } catch {}

                // 80～90年代に使われ、Unicode の台頭により使われなくなったとのこと

                // Windows code page - Wikipedia
                // https://en.wikipedia.org/wiki/Windows_code_page

                try { xBuilder.AppendLine (xIndentationString + FormattableString.Invariant ($"WindowsCodePage: 0x{xEncodingAlt.WindowsCodePage:X4} ({xEncodingAlt.WindowsCodePage})")); } catch {}
            }

            string xFilePath = nPath.Map (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), $"Encodings-{DateTime.UtcNow.ToMinimalUniversalDateTimeString ()}.txt");
            nFile.WriteAllText (xFilePath, xBuilder.ToString ());

            Console.WriteLine (xBuilder.ToString ());
        }

        public static void TestNumericStringComparison ()
        {
            Console.WriteLine (nString.CompareNumericStrings ("2", "10")); // → -1
            Console.WriteLine (nString.CompareNumericStrings ("1", "02")); // → -1
            Console.WriteLine (nString.CompareNumericStrings ("0123456789", "０１２３４５６７８９")); // → 0
        }

        // 左は nAlphanumericComparer.OrdinalIgnoreCase、右は StringComparer.OrdinalIgnoreCase によるソート

        // ASCII では、小さいものから、数字 → 大文字 → 小文字
        // その点が共通なので、ソート結果は、パッと見、あまり異ならない
        // 左では、二つの文字列が数字以外として一致してきた状況で双方に数字が見付かると、数値としての比較に切り替わる
        // 0詰めや半角・全角も考慮されるため、たとえば「01月01日」と（全角の）「１月１日」が一致する

        // 0aa4 03Y1
        // 0D6M 0626
        // 1FJ4 07P5
        // 1y76 09B3
        // 2A4r 0aa4
        // 2s94 0D6M
        // 2V30 14Jc
        // 3B4s 19U3
        // 3B68 1FJ4
        // 3N6o 1y76
        // 3n17 25b9
        // 03Y1 25gS
        // 4E20 263m
        // 4Z54 2A4r
        // 5t3v 2s94
        // 6e73 2V30
        // 6t06 3290
        // 6u97 361T
        // 6WL0 39V1
        // 7c19 3B4s
        // 07P5 3B68
        // 8a87 3n17
        // 8e29 3N6o
        // 09B3 40B2
        // 9E0g 431v
        // 9E60 46X5
        // 9k8p 48i0
        // 9Z2u 49B2
        // 14Jc 4E20
        // 19U3 4Z54
        // 25b9 50o2
        // 25gS 5213
        // 39V1 554i
        // 40B2 573z
        // 46X5 5t3v
        // 48i0 60L0
        // 49B2 64oi
        // 50o2 6526
        // 60L0 660w
        // 64oi 66Hs
        // 66Hs 680l
        // 73fo 6e73
        // 75K6 6t06
        // 91S7 6u97
        // 99cf 6WL0
        // 263m 73fo
        // 361T 753T
        // 431v 75K6
        // 554i 795K
        // 573z 7c19
        // 0626 808C
        // 660w 8a87
        // 680l 8e29
        // 753T 913I
        // 795K 918T
        // 808C 91S7
        // 913I 978v
        // 918T 99cf
        // 978v 9E0g
        // 3290 9E60
        // 5213 9k8p
        // 6526 9Z2u
        // A2u5 A2u5
        // ao21 ao21
        // aw63 aw63
        // b20a b20a
        // C2C0 C2C0
        // DB32 DB32
        // EP15 EP15
        // G0q5 G0q5
        // g6d7 G687
        // g8j9 g6d7
        // G687 g8j9
        // Gr36 Gr36
        // H1t8 H1t8
        // H395 H395
        // I7t4 I7t4
        // j640 j640
        // l236 l236
        // l634 l634
        // mp52 mp52
        // N808 N808
        // O97p O793
        // O793 O97p
        // Ov76 Ov76
        // P8k4 p24h
        // p24h P8k4
        // R01J R01J
        // R6n5 R6n5
        // Sc29 Sc29
        // t3X9 t3X9
        // u7A4 u45P
        // u45P u7A4
        // UO38 UO38
        // W701 W701
        // Wa80 Wa80
        // Y1H3 Y1H3
        // y8T2 Y60A
        // Y60A y8T2
        // z8A4 z8A4

        public static void TestAlphanumericStringComparison ()
        {
            const int xCount = 100;

            var xStrings = Enumerable.Range (0, xCount).Select (x => nRandom.GenerateAsciiPassword
            (
                length: 4,
                minDigitCount: 2,
                minUpperCount: 0,
                minLowerCount: 0,
                minNonAlphanumericCount: 0,
                maxNonAlphanumericCount: 0
            ))
            .ToArray ();

            // 次の2行に ToArray を付けなかったことで、ElementAt 時の遅延実行がうまくいかず、双方で、中途半端にソートされたリストが出力された
            // ほどほどにソートされていたが、完璧でなかった
            // 2行に ToArray を付けたところ、ソートはうまくいった

            // しかし、よく見ると、左右で全く異なるリストのソートになっていた
            // 当たり前だが、もっと前半の Select も遅延実行なので、
            //     二つの ToArray 時にそれぞれでやっと nRandom.GenerateAsciiPassword が呼ばれていたようだ

            var xAlphanumericallySorted = xStrings.OrderBy (x => x, nAlphanumericComparer.OrdinalIgnoreCase);
            var xNormallySorted = xStrings.OrderBy (x => x, StringComparer.OrdinalIgnoreCase);

            Console.WriteLine (string.Join (Environment.NewLine, Enumerable.Range (0, xCount)
                .Select (x => $"{xAlphanumericallySorted.ElementAt (x)} {xNormallySorted.ElementAt (x)}")));
        }
    }
}
