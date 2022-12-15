using System;
using System.Collections.Generic;
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
                        return x
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
    }
}
