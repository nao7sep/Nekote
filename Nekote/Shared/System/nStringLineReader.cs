using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // 文字列の行単位での読み出しに特化したクラス
    // 行末の空白系文字を削ったり、不要な空行を省いたりが可能
    // そのうち後者は、文字列全体の先頭や末尾の空行や、中間部で二つ以上連続するもの
    // そういった掃除をしないなら StringReader を使えばよいので、これらの機能はデフォルトでオン

    // 実装においては、StringReader と StreamReader の ReadLine を参考にした
    // 空白系文字や空行を考慮しないモードなら File.ReadAllLines などと結果が一致してほしいため

    // StringReader.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/StringReader.cs

    // StreamReader.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/StreamReader.cs

    // ReadLine は string? でなく ReadOnlySpan <char> を out で返す
    // 改行文字の検索に ReadOnlySpan <char> を使うため、Substring を呼ばず、いったん Slice で返す
    // それを呼び出し側で ToString するのは手間でなく、一方、ReadOnlySpan <char> のまま使えるなら効率的

    public class nStringLineReader
    {
        private string mValue;

        // 二つ以上の連続する空行のうち一つだけを出力するモードなら、二つ目以降が無視される
        // 文字列の先頭を見る時点で既に一つ目が見付かっているとして、先頭の空行を無視
        private bool mHasDetectedEmptyLine = true;

        private int mCurrentIndex;

        // inclusive
        // length 系でもよかったが、コンストラクターで末尾の文字にアクセスするので、それに最適化
        private int mLastIndex;

        private bool mTrimsTrailingWhiteSpaces;

        private bool mReducesEmptyLines;

        public nStringLineReader (string value, bool trimsTrailingWhiteSpaces = true, bool reducesEmptyLines = true)
        {
            // value が null でも落ちないようにした

            mValue = value ?? string.Empty;
            mCurrentIndex = 0;
            mLastIndex = mValue.Length - 1;
            mTrimsTrailingWhiteSpaces = trimsTrailingWhiteSpaces;
            mReducesEmptyLines = reducesEmptyLines;

            if (mTrimsTrailingWhiteSpaces)
            {
                if (mReducesEmptyLines)
                {
                    while (mLastIndex >= mCurrentIndex)
                    {
                        if (char.IsWhiteSpace (mValue [mLastIndex]) == false)
                            break;

                        mLastIndex --;
                    }

                    // 全てが空白系文字の場合、mLastIndex は -1 になる
                    // ReadLine では問題なく null が返る
                }
            }

            else
            {
                if (mReducesEmptyLines)
                {
                    while (mLastIndex >= mCurrentIndex)
                    {
                        if (nString.NewLineChars.Contains (mValue [mLastIndex]) == false)
                            break;

                        mLastIndex --;
                    }
                }
            }
        }

        // 戻り値の型を ReadOnlySpan にすると、null を返してもコンパイルエラーにならない
        // null は、暗黙的に char []? の null とみなされて、コンストラクターで this = default により空の ReadOnlySpan になる
        // それを戻り値として受け取っても、読めなかったのか、0文字を読めたのか分からない可能性があるため、成否を bool で扱う

        // ReadOnlySpan<T>.Implicit Operator (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.readonlyspan-1.op_implicit

        // ReadOnlySpan.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/ReadOnlySpan.cs

        public bool ReadLine (out ReadOnlySpan <char> result)
        {
            // 不要な空行を無視するモードのときに、読み出したものが空行なら、その回のうちに空行でないものを探して返さないといけない
            // その処理に再帰コールを使うと改行だけが延々と続く文字列による攻撃が成立するため、goto にして様子見

            // goto なら大丈夫というのを一応テストした
            // 次のコードが問題なく走り、OK までが表示された
            // goto が行われているのも確認した

            // string xValue = string.Concat (Enumerable.Range (0, 100_000_000).Select (x => Environment.NewLine)) + 'a'; // こうしないと、コンストラクターで後ろから全て削られる
            // Console.WriteLine (xValue.Length); // → 200000001
            // nString.EnumerateLines (xValue).ToArray (); // こうしないと、ここでも遅延実行が有効で何も行われない
            // Console.WriteLine ("OK");

            // なお、Range の count をさらに10倍にすると、ArgumentOutOfRangeException: Specified argument was out of the range of valid values. (Parameter 'minimumLength') が飛んだ
            // 10億というと int.MaxValue の半分くらいの値
            // xValue の長さは int.MaxValue に迫っただろうから、LINQ の実装のどこかで int.MaxValue を超えたのかもしれない
            // スタックオーバーフローにはならなかったため、goto でよい

        Beginning:

            if (mCurrentIndex > mLastIndex)
            {
                result = default;
                return false;
            }

            ReadOnlySpan <char> xRemaining = mValue.AsSpan (mCurrentIndex, mLastIndex - mCurrentIndex + 1);
            int xLineLength = xRemaining.IndexOfAny (nString.NewLineChars);

            // mCurrentIndex を更新
            // 改行文字が見付からなければ、残りの文字を一度に処理

            if (xLineLength >= 0)
            {
                if (xRemaining [xLineLength] == '\r' && xLineLength + 1 < xRemaining.Length && xRemaining [xLineLength + 1] == '\n')
                    mCurrentIndex += xLineLength + 2;

                else mCurrentIndex += xLineLength + 1;
            }

            else
            {
                xLineLength = xRemaining.Length;
                mCurrentIndex += xLineLength;
            }

            if (mTrimsTrailingWhiteSpaces)
            {
                while (xLineLength > 0)
                {
                    if (char.IsWhiteSpace (xRemaining [xLineLength - 1]) == false)
                        break;

                    xLineLength --;
                }

                // 全て空白系文字なら xLineLength は0になる
                // -1 ではないので、そのまま処理できる
            }

            if (xLineLength > 0)
            {
                // モードを見ず、常に代入してもよいが、else の方とのバランスを考えて

                if (mReducesEmptyLines)
                    mHasDetectedEmptyLine = false;
            }

            else
            {
                if (mReducesEmptyLines)
                {
                    // 不要な空行を削るモードのときに既に一つは見付かっていれば、goto により、更新された mCurrentIndex から、返せるものを探す
                    // その後も空行が続けば、メソッドの今回の呼び出しにおいて mCurrentIndex > mLastIndex が成立して false が返る

                    if (mHasDetectedEmptyLine)
                        goto Beginning;

                    mHasDetectedEmptyLine = true;
                }
            }

            result = xRemaining.Slice (0, xLineLength);
            return true;
        }

        // ReadLineAsync を作らない
        // メモリー上の文字列から行ごとの読み込みなので、Task を作るコストの方が大きい場合がほとんど
        // .NET には StringReader.ReadLineAsync があるが、他と揃えただけの感が大きい
    }
}
