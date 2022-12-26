using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nEnumerable
    {
        // 文字「列」でなく文字のソートについて有用な情報が得られたため、ここにザッとまとめておく

        // char の配列や IEnumerable などを StringComparer で並び替えるには、char をいったん string にするしかない
        // 内部的には文字単位の比較だろうから、そういうメソッドがあると期待したが、どのコースでも LibraryImport のメソッドが呼ばれる

        // CompareInfo.Compare → GlobalizationMode.UseNls により NlsCompareString または IcuCompareString に
        // NlsCompareString →
        //     [LibraryImport ("kernel32.dll", EntryPoint = "CompareStringEx")]
        //     internal static unsafe partial int CompareStringEx (char* lpLocaleName, uint dwCmpFlags, char* lpString1, int cchCount1,
        //         char* lpString2, int cchCount2, void* lpVersionInformation, void* lpReserved, IntPtr lParam);
        // IcuCompareString →
        //     [LibraryImport (Libraries.GlobalizationNative, EntryPoint = "GlobalizationNative_CompareString", StringMarshalling = StringMarshalling.Utf16)]
        //     internal static unsafe partial int CompareString (IntPtr sortHandle, char* lpStr1, int cwStr1Len, char* lpStr2, int cwStr2Len, CompareOptions options);

        // 他のところにも書いたが、Unix 系の OS では ICU が使われる
        // Windows では、以前は NLS だったが、Windows 10 May 2019 Update からは ICU が使われる

        // Globalization and ICU - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu

        // ICU については、次のページが詳しい
        // Microsoft のページでは http://site.icu-project.org/home へのリンクが張られているが、一度も開けたことがない

        // ICU - International Components for Unicode
        // https://icu.unicode.org/

        // 文字「列」でなく文字での比較ができないのは、ICU による制限なのか、Microsoft なのか、.NET なのか、現時点では不明
        // サロゲートペアの文字が char 一つに収まらず、char 同士の比較では ICU が期する精度の処理にならないことが関係しているはず
        // それでも、既に char に収まっている文字があるなら、それらの比較も一つの選択肢として可能であってほしい
        // それができないようなのは、文字単位での処理のオーバーヘッドの大きさなども懸念されてのことか

        // 今さら初歩の初歩だが、Array.Sort は、並び替える配列の型により結果が異なる
        // .NET での意味付けとしては、char は、「文字」でなく、「文字のコードポイント」、つまり、あくまでただの数値なのだろう
        // string の方は「文字のコードポイントの配列」でなく「文字列」であり、処理においてカルチャーなどが意識される
        // char は、型のチェックが可能で、多重定義を用意しやすい「特殊な ushort」くらいに考えてよいか

        // char [] xHoge = { '亜', '井' };
        // Array.Sort (xHoge);
        // Console.WriteLine (string.Join (", ", xHoge)); // → 井, 亜

        // string [] xMoge = { "亜", "井" };
        // Array.Sort (xMoge);
        // Console.WriteLine (string.Join (", ", xMoge)); // → 亜, 井

        // Nekote としては、char を StringComparer などで並び替えるのを非推奨とする
        // やれるし、やらないといけないところもあるが、一度しか実行されないコードなど、パフォーマンスに響かないものに限られるべき
        // という考え方により、拡張メソッドでなく、引数の型が決め打ちで、keySelector を持たない OrderBy を用意しておく

        // comparer の型は、StringComparer が IComparer <string?> を継承していることと、
        //     LINQ の OrderBy の引数が IComparer <TKey>? comparer であることに基づく
        // If comparer is null, the default comparer Default is used to compare keys というのを尊重

        // StringComparer.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/StringComparer.cs

        // Enumerable.OrderBy Method (System.Linq) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.orderby

        public static IOrderedEnumerable <(char Value, string SortKey)> OrderBy (IEnumerable <char> values, IComparer <string?>? comparer)
        {
            // char.ToString → string.CreateFromChar → FastAllocateString となっている
            // 最後のものは、[MethodImpl (MethodImplOptions.InternalCall)] のようだ

            // new string (char c, int count) には、[MethodImpl (MethodImplOptions.InternalCall)] と
            //     [DynamicDependency ("Ctor (System.Char, System.Int32)")] が付いている

            // DynamicDependency については深入りしないでおくが、パッと見、new string の方がオーバーヘッドが少なそう

            // ただ、char.ToString で済むものを、わずかなパフォーマンス向上のために new string (char, 1) と書きたくない
            // めんどくさいし、コードがうるさくなるし、ToString とのわずかな速度差がいつまでもあるとも限らないため

            // ということから、Nekote では、char → string には ToString を使う

            // c# - How do I convert a single char to a string? - Stack Overflow
            // https://stackoverflow.com/questions/13736480/how-do-i-convert-a-single-char-to-a-string

            // String.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.cs

            // String.CoreCLR.cs
            // https://source.dot.net/#System.Private.CoreLib/src/System/String.CoreCLR.cs

            // Use the dynamic dependency API to reference framework packages at run time - Windows apps | Microsoft Learn
            // https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/framework-packages/use-the-dynamic-dependency-api

            return values.Select (x => (Value: x, SortKey: x.ToString ())).OrderBy (y => y.SortKey, comparer);
        }
    }
}
