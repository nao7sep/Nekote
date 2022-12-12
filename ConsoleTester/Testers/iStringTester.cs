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
    }
}
