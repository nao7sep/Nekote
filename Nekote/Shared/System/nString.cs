using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nString
    {
        // CR, LF 以外にもあるようだが、StringReader.ReadLine がこれらしか見ないのでそれにならう

        // Newline - Wikipedia
        // https://en.wikipedia.org/wiki/Newline

        // StringReader.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/StringReader.cs

        public static readonly char [] NewLineChars = { '\r', '\n' };

        // InvariantCulture での null や "" の文字列化と位置付け

        public static string GetLiteralIfNullOrEmpty (string? value)
        {
            if (value == null)
                return nStringLiterals.NullLabel;

            if (value.Length == 0)
                return nStringLiterals.EmptyLabel;

            return value;
        }

        // 文字列の処理のメソッドを集めていく

        // できるだけ拡張メソッドにする
        // インスタンスがあってのメソッドのように呼ばれるため、引数が null なら基本的には落ちるに任せる

        // .NET に同じ名前のメソッドがあるなら、名前を異ならせ、可能なら戻り値の型でも区別が付くようにする
        // 区別を付けるにおいては、引数の違いに頼ることを避ける
        // たとえば、ToString の場合、引数を取るものが .NET にないとしても、「引数を取るから同じ名前でも」というのは適さない
        // .NET 側の多重定義を全て把握しているわけでないし、今後の変更による名前の衝突も考えられるため
        // ToStringEx のように名前を異ならせるのが無難だし、IntelliSense でも分かりやすい

        // Trim*AsSpan は、トリミングの結果を文字列でなく ReadOnlySpan としてもらいたいときに役立つ
        // 返すのは ReadOnlySpan だが、MemoryExtensions.AsSpan も ReadOnlySpan を返すため、名前は問題でない

        // MemoryExtensions.AsSpan Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.asspan

        public static ReadOnlySpan <char> TrimAsSpan (this string value, char trimChar)
        {
            return MemoryExtensions.Trim (value.AsSpan (), trimChar);
        }

        public static ReadOnlySpan <char> TrimAsSpan (this string value, params char [] trimChars)
        {
            return MemoryExtensions.Trim (value.AsSpan (), trimChars.AsSpan ());
        }

        public static ReadOnlySpan <char> TrimStartAsSpan (this string value, char trimChar)
        {
            return MemoryExtensions.TrimStart (value.AsSpan (), trimChar);
        }

        public static ReadOnlySpan <char> TrimStartAsSpan (this string value, params char [] trimChars)
        {
            return MemoryExtensions.TrimStart (value.AsSpan (), trimChars.AsSpan ());
        }

        public static ReadOnlySpan <char> TrimEndAsSpan (this string value, char trimChar)
        {
            return MemoryExtensions.TrimEnd (value.AsSpan (), trimChar);
        }

        public static ReadOnlySpan <char> TrimEndAsSpan (this string value, params char [] trimChars)
        {
            return MemoryExtensions.TrimEnd (value.AsSpan (), trimChars.AsSpan ());
        }

        // null を明示的に通す理由については nString.Optimize のコメントが詳しい

        public static IEnumerable <string> EnumerateLines (this string? value, bool trimsTrailingWhiteSpaces = true, bool reducesEmptyLines = true)
        {
            if (string.IsNullOrEmpty (value))
                yield break;

            nStringLineReader xReader = new nStringLineReader (value, trimsTrailingWhiteSpaces, reducesEmptyLines);

            while (xReader.ReadLine (out ReadOnlySpan <char> xResult))
                yield return xResult.ToString ();
        }

        // 一つ以上の段落のそれぞれに一つ以上の行が入っている文字列を分解
        // 各部で LINQ が利くように、入れ子の IEnumerable にした

        public static IEnumerable <IEnumerable <string>> EnumerateParagraphs (this string? value, bool trimsTrailingWhiteSpaces = true, bool reducesEmptyLines = true)
        {
            if (string.IsNullOrEmpty (value))
                yield break;

            nStringLineReader xReader = new nStringLineReader (value, trimsTrailingWhiteSpaces, reducesEmptyLines);

            List <string>? xParagraph = null;

            while (xReader.ReadLine (out ReadOnlySpan <char> xResult))
            {
                if (xResult.Length > 0)
                {
                    if (xParagraph == null)
                        xParagraph = new List <string> ();

                    xParagraph.Add (xResult.ToString ());
                }

                else
                {
                    if (xParagraph != null)
                    {
                        List <string> xParagraphAlt = xParagraph;
                        xParagraph = null;
                        yield return xParagraphAlt;
                    }
                }
            }

            if (xParagraph != null)
                yield return xParagraph;
        }

        // 自分は、null を「値が設定されていない」「初期化されていない」と、"" を「"" が設定された」「初期化されている」とみなしている
        // では、「拡張子がないということ」を示すときに xExtension が null と "" のいずれであるべきかを考えると難しい
        // 「ない」から null なのか、"" という拡張子があり、それが設定されているから "" なのか、どちらも論理的には成立するため

        // 拡張子を Optimize に通すことは考えにくい
        // しかし、拡張子のような、どちらでも論理的には成立するものが通るメソッドと同種のメソッドである点には考慮が必要
        // .NET の WebUtility.HtmlEncode も、個人的には「null なら HTML エンコードがそもそも不要」と思うが、null が通る

        // WebUtility.HtmlEncode Method (System.Net) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.net.webutility.htmlencode

        // 構文解析による型変換など、null はもちろん、"" も入力として適さないところでは、null も落ちるべき
        // 一方、型変換が起こらず、"" が完全に正常値とみなされるところでは、他の多くで null が通るため、特段の理由がなければ null が通るべき
        // そのチェックにおいて、"" のときにすぐにメソッドを抜けても最後まで処理させても返るものが完全に同一なら、"" でも抜けてよいだろう

        public static string? Optimize (this string? value, nStringOptimizationOptions? options = null, string? newLine = null)
        {
            // nStringOptimizer.Optimize では null でも "" が返る
            // しかし、こちらは、WebUtility.HtmlEncode などに近いメソッドなので null が返るべき

            if (string.IsNullOrEmpty (value))
                return value;

            return nStringOptimizer.Default.Optimize (value, options, newLine).Value;
        }

        /// <summary>
        /// 半角と全角の両方の数字に対応。
        /// </summary>
        public static int CompareNumericStrings (ReadOnlySpan <char> value1, ReadOnlySpan <char> value2)
        {
            // 短い方の左端に0詰めするなどを避けたく、負のインデックスに
            // 負なら '0' を使う

            int xMaxLength = Math.Max (value1.Length, value2.Length),
                xIndex1 = value1.Length - xMaxLength,
                xIndex2 = value2.Length - xMaxLength;

            while (xIndex1 < value1.Length)
            {
                int xResult = nChar.CompareSupportedDigits (xIndex1 >= 0 ? value1 [xIndex1] : '0', xIndex2 >= 0 ? value2 [xIndex2] : '0');

                if (xResult != 0)
                    return xResult;

                xIndex1 ++;
                xIndex2 ++;
            }

            return 0;
        }
    }
}
