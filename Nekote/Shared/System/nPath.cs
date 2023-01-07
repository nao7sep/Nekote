using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nPath
    {
        public static readonly char [] DirectorySeparators = { '\\', '/' };

        // Path.Combine は、二つ目以降の引数に絶対パスを与えると、それがベースパスになる
        // ユーザー入力をそのままつなげるなどがそもそもあり得ないが、仕様としてはセキュリティーリスク
        // だから Path.Join を使えとのこと
        // しかし、これは、ディレクトリーの区切り文字が PathInternal.DirectorySeparatorChar に決め打ちで、たとえば Windows で Mac/Linux 用のパスを吐けない
        // そのため、区切り文字を指定できるシンプルな実装を用意しておく

        // Path.Combine Method (System.IO) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.io.path.combine

        // Path.Join Method (System.IO) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.io.path.join

        // Path.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/Path.cs

        // nPath.Join では、それぞれのパスに null が通らないようになっている → 通らないように作ったつもりだが、通ることが判明
        // 過去、Path.Combine や Path.Join を使っていて、null かもしれないものをつなげたことがない
        // 各部が null かどうかは呼び出し側で把握し、きちんと条件分岐して安全なパスを作るべき

        // 追記: 詳しくは nString.TrimAsSpan のところに書いたが、MemoryExtensions.AsSpan は、引数が null なら default を返し、落ちることはない
        // Join や Map は、意図しなかったことだが、（Map の一つ目を除き）各部に null を渡しても落ちないようになっている

        // ReadOnlySpan <char> と string interpolation の関係について詳細を知らなかったので調べた
        // .NET 6 == C# 10 のようで、C# 10 からは、string interpolation に DefaultInterpolatedStringHandler が使われ、パフォーマンスが向上しているとのこと
        // この構造体には AppendLiteral (String) や AppendFormatted (ReadOnlySpan <Char>) があるため、StringBuilder のように動作するのだろう
        // ソースを見ると、DefaultInterpolatedStringHandler は、GrowThenCopyString を呼びながら Span <char> に文字列をコピーする、興味深い実装になっていた
        // string.Format 的な（重たい）動作を何となくイメージしていたが、AppendFormatted (ReadOnlySpan <Char>) によりネイティブ（？）に処理されるようなので問題なし

        // Formatting byte[] or ReadOnlySpan<byte> to string using C# 10 and .NET 6 string interpolation, compiler handler lowering pattern - Stack Overflow
        // https://stackoverflow.com/questions/71729980/formatting-byte-or-readonlyspanbyte-to-string-using-c-sharp-10-and-net-6-st

        // DefaultInterpolatedStringHandler Struct (System.Runtime.CompilerServices) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.defaultinterpolatedstringhandler

        // DefaultInterpolatedStringHandler.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Runtime/CompilerServices/DefaultInterpolatedStringHandler.cs

        // $ - string interpolation - format string output | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/tokens/interpolated

        // Improved interpolated strings - C# 10.0 draft specifications | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/improved-interpolated-strings#the-handler-pattern

        private static string iJoin (string path1, char directorySeparator, string path2)
        {
            return $"{nString.TrimEndAsSpan (path1, DirectorySeparators)}{directorySeparator}{nString.TrimStartAsSpan (path2, DirectorySeparators)}";
        }

        private static string iJoin (string path1, char directorySeparator, ReadOnlySpan <char> trimmedPath2, string path3)
        {
            return $"{nString.TrimEndAsSpan (path1, DirectorySeparators)}{directorySeparator}{trimmedPath2}{directorySeparator}{nString.TrimStartAsSpan (path3, DirectorySeparators)}";
        }

        private static string iJoin (string path1, char directorySeparator, ReadOnlySpan <char> trimmedPath2, ReadOnlySpan <char> trimmedPath3, string path4)
        {
            return $"{nString.TrimEndAsSpan (path1, DirectorySeparators)}{directorySeparator}{trimmedPath2}{directorySeparator}{trimmedPath3}{directorySeparator}{nString.TrimStartAsSpan (path4, DirectorySeparators)}";
        }

        public static string Join (string path1, char directorySeparator, string path2)
        {
            ReadOnlySpan <char>
                xTrimmedPath1 = nString.TrimAsSpan (path1, DirectorySeparators),
                xTrimmedPath2 = nString.TrimAsSpan (path2, DirectorySeparators);

            if (xTrimmedPath1.Length > 0)
            {
                if (xTrimmedPath2.Length > 0)
                    return iJoin (path1, directorySeparator, path2); // 全てあり

                else return path1; // 二つ目がなし
            }

            else
            {
                if (xTrimmedPath2.Length > 0)
                    return path2; // 一つ目がなし

                else return string.Empty; // 全てなし
            }
        }

        public static string Join (string path1, string path2)
        {
            return Join (path1, Path.DirectorySeparatorChar, path2);
        }

        public static string Join (string path1, char directorySeparator, string path2, string path3)
        {
            ReadOnlySpan <char>
                xTrimmedPath1 = nString.TrimAsSpan (path1, DirectorySeparators),
                xTrimmedPath2 = nString.TrimAsSpan (path2, DirectorySeparators),
                xTrimmedPath3 = nString.TrimAsSpan (path3, DirectorySeparators);

            if (xTrimmedPath1.Length > 0)
            {
                if (xTrimmedPath2.Length > 0)
                {
                    if (xTrimmedPath3.Length > 0)
                        return iJoin (path1, directorySeparator, xTrimmedPath2, path3); // 全てあり

                    else return iJoin (path1, directorySeparator, path2); // 三つ目がなし
                }

                else
                {
                    if (xTrimmedPath3.Length > 0)
                        return iJoin (path1, directorySeparator, path3); // 二つ目がなし

                    else return path1; // 二つ目、三つ目がなし
                }
            }

            else
            {
                if (xTrimmedPath2.Length > 0)
                {
                    if (xTrimmedPath3.Length > 0)
                        return iJoin (path2, directorySeparator, path3); // 一つ目がなし

                    else return path2; // 一つ目、三つ目がなし
                }

                else
                {
                    if (xTrimmedPath3.Length > 0)
                        return path3; // 一つ目、二つ目がなし

                    else return string.Empty; // 全てなし
                }
            }
        }

        public static string Join (string path1, string path2, string path3)
        {
            return Join (path1, Path.DirectorySeparatorChar, path2, path3);
        }

        public static string Join (string path1, char directorySeparator, string path2, string path3, string path4)
        {
            ReadOnlySpan <char>
                xTrimmedPath1 = nString.TrimAsSpan (path1, DirectorySeparators),
                xTrimmedPath2 = nString.TrimAsSpan (path2, DirectorySeparators),
                xTrimmedPath3 = nString.TrimAsSpan (path3, DirectorySeparators),
                xTrimmedPath4 = nString.TrimAsSpan (path4, DirectorySeparators);

            if (xTrimmedPath1.Length > 0)
            {
                if (xTrimmedPath2.Length > 0)
                {
                    if (xTrimmedPath3.Length > 0)
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path1, directorySeparator, xTrimmedPath2, xTrimmedPath3, path4); // 全てあり

                        else return iJoin (path1, directorySeparator, xTrimmedPath2, path3); // 四つ目がなし
                    }

                    else
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path1, directorySeparator, xTrimmedPath2, path4); // 三つ目がなし

                        else return iJoin (path1, directorySeparator, path2); // 三つ目、四つ目がなし
                    }
                }

                else
                {
                    if (xTrimmedPath3.Length > 0)
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path1, directorySeparator, xTrimmedPath3, path4); // 二つ目がなし

                        else return iJoin (path1, directorySeparator, path3); // 二つ目、四つ目がなし
                    }

                    else
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path1, directorySeparator, path4); // 二つ目、三つ目がなし

                        else return path1; // 二つ目、三つ目、四つ目がなし
                    }
                }
            }

            else
            {
                if (xTrimmedPath2.Length > 0)
                {
                    if (xTrimmedPath3.Length > 0)
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path2, directorySeparator, xTrimmedPath3, path4); // 一つ目がなし

                        else return iJoin (path2, directorySeparator, path3); // 一つ目、四つ目がなし
                    }

                    else
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path2, directorySeparator, path4); // 一つ目、三つ目がなし

                        else return path2; // 一つ目、三つ目、四つ目がなし
                    }
                }

                else
                {
                    if (xTrimmedPath3.Length > 0)
                    {
                        if (xTrimmedPath4.Length > 0)
                            return iJoin (path3, directorySeparator, path4); // 一つ目、二つ目がなし

                        else return path3; // 一つ目、二つ目、四つ目がなし
                    }

                    else
                    {
                        if (xTrimmedPath4.Length > 0)
                            return path4; // 一つ目、二つ目、三つ目がなし

                        else return string.Empty; // 全てなし
                    }
                }
            }
        }

        public static string Join (string path1, string path2, string path3, string path4)
        {
            return Join (path1, Path.DirectorySeparatorChar, path2, path3, path4);
        }

        public static string Join (char directorySeparator, params string [] paths)
        {
            // ToString を入れないと、ReadOnlySpan <char> を型引数として使えないとのエラーが出る

            // c# - Why ReadOnlySpan may not be used as a type argument for generic delegates and generic methods? - Stack Overflow
            // https://stackoverflow.com/questions/53155438/why-readonlyspan-may-not-be-used-as-a-type-argument-for-generic-delegates-and-ge

            var xPaths = paths.Select (x => (Path: x, TrimmedPath: nString.TrimAsSpan (x, DirectorySeparators).ToString ())).Where (x => x.TrimmedPath.Length > 0);
            int xCount = xPaths.Count ();

            if (xCount >= 3)
            {
                // ^0 は、最後の要素のインデックスでなく、使うと範囲外になる「長さ」に相当するもの
                // A..B において A は inclusive、B は exclusive
                // 0 <= index < length 的に考える

                // Explore ranges of data using indices and ranges | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes

                string xJoinedMiddlePaths = string.Join (directorySeparator, xPaths.Take (1..^1).Select (x => x.TrimmedPath));
                return iJoin (xPaths.First ().Path, directorySeparator, xJoinedMiddlePaths, xPaths.Last ().Path);
            }

            if (xCount == 2)
                return iJoin (xPaths.First ().Path, directorySeparator, xPaths.Last ().Path);

            if (xCount == 1)
                return xPaths.First ().Path;

            return string.Empty;
        }

        public static string Join (params string [] paths)
        {
            return Join (Path.DirectorySeparatorChar, paths);
        }

        // Join では、各部のトリミングなどにより長さが0になれば、その部分は初めからなかったことにされる
        // たとえば、nPath.Join ("/", "hoge", "\\", "moge") は、問題なく hoge\moge になる

        // Map は、Join と似ているが、絶対パスに相対パスをつなげてマッピングするメソッドのため、一つ目の引数が絶対パスでないと例外が飛ぶ
        // そこまでするべきか迷ったが、たとえば nApp.DirectoryPath は、Environment.GetCommandLineArgs の戻り値にパスが含まれるかなどをチェックする
        // Map は、path が絶対パスであるというのが処理に不可欠の条件なので、そのことが null 参照や範囲外のインデックスと同様にチェックされてよい

        public static string Map (string path, char directorySeparator, params string [] paths)
        {
            if (Path.IsPathFullyQualified (path) == false)
                throw new nArgumentException ();

            var xPaths = paths.Select (x => (Path: x, TrimmedPath: nString.TrimAsSpan (x, DirectorySeparators).ToString ())).Where (x => x.TrimmedPath.Length > 0);
            int xCount = xPaths.Count ();

            if (xCount >= 2)
            {
                string xJoinedMiddlePaths = string.Join (directorySeparator, xPaths.Take (0..^1).Select (x => x.TrimmedPath));
                return iJoin (path, directorySeparator, xJoinedMiddlePaths, xPaths.Last ().Path);
            }

            if (xCount == 1)
                return iJoin (path, directorySeparator, xPaths.Last ().Path);

            // if (xCount == 0)
                return path;
        }

        public static string Map (string path, params string [] paths)
        {
            return Map (path, Path.DirectorySeparatorChar, paths);
        }
    }
}
