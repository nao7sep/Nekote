using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nString
    {
        // 最初は TrimAndGetSlice または *Return* を考えたが、シンプルにした
        // ReadOnlySpan が .NET の実装でよく使われているのは、おそらく速いのだろう

        public static ReadOnlySpan <char> TrimAndSlice (string value, bool trimsStart, bool trimsEnd, char trimChar)
        {
            int xLength = value.Length,
                xFirstIndex = 0;

            if (trimsStart)
            {
                while (xFirstIndex < xLength)
                {
                    if (value [xFirstIndex] != trimChar)
                        break;

                    xFirstIndex ++;
                }

                // 全てが削れる文字なら場外に
                // 一応、value から生成

                if (xFirstIndex >= xLength)
                    return value.AsSpan (0, 0);
            }

            // xLength == 0 でも問題なし
            int xLastIndex = xLength - 1;

            if (trimsEnd)
            {
                // 先頭のトリミングが必ず行われるなら、場外に出なかった時点で xFirstIndex には削れない文字があるのが確定し、xLastIndex > xFirstIndex で足りる
                // 先頭のトリミングが行われなかったなら、xFirstIndex == 0 のところの文字が分からないため、>= によりそこも見る必要がある
                // 引数により1文字分、無駄な処理になるが、この方が実装がシンプル

                while (xLastIndex >= xFirstIndex)
                {
                    if (value [xLastIndex] != trimChar)
                        break;

                    xLastIndex --;
                }

                // 逆向きに場外になった

                if (xLastIndex < 0)
                    return value.AsSpan (0, 0);
            }

            return value.AsSpan (xFirstIndex, xLastIndex - xFirstIndex + 1);
        }

        public static ReadOnlySpan <char> TrimAndSlice (string value, bool trimsStart, bool trimsEnd, params char [] trimChars)
        {
            // 削る文字のチェック以外は、多重定義のものと同一
            // 共通化すると文字ごとに if 文が入って遅くなりそう

            int xLength = value.Length,
                xFirstIndex = 0;

            if (trimsStart)
            {
                while (xFirstIndex < xLength)
                {
                    if (trimChars.Contains (value [xFirstIndex]) == false)
                        break;

                    xFirstIndex ++;
                }

                if (xFirstIndex >= xLength)
                    return value.AsSpan (0, 0);
            }

            int xLastIndex = xLength - 1;

            if (trimsEnd)
            {
                while (xLastIndex >= xFirstIndex)
                {
                    if (trimChars.Contains (value [xLastIndex]) == false)
                        break;

                    xLastIndex --;
                }

                if (xLastIndex < 0)
                    return value.AsSpan (0, 0);
            }

            return value.AsSpan (xFirstIndex, xLastIndex - xFirstIndex + 1);
        }

        public static ReadOnlySpan <char> TrimAndSlice (string value, char trimChar)
        {
            return TrimAndSlice (value, true, true, trimChar);
        }

        public static ReadOnlySpan <char> TrimAndSlice (string value, params char [] trimChars)
        {
            return TrimAndSlice (value, true, true, trimChars);
        }

        public static ReadOnlySpan <char> TrimStartAndSlice (string value, char trimChar)
        {
            return TrimAndSlice (value, true, false, trimChar);
        }

        public static ReadOnlySpan <char> TrimStartAndSlice (string value, params char [] trimChars)
        {
            return TrimAndSlice (value, true, false, trimChars);
        }

        public static ReadOnlySpan <char> TrimEndAndSlice (string value, char trimChar)
        {
            return TrimAndSlice (value, false, true, trimChar);
        }

        public static ReadOnlySpan <char> TrimEndAndSlice (string value, params char [] trimChars)
        {
            return TrimAndSlice (value, false, true, trimChars);
        }
    }
}
