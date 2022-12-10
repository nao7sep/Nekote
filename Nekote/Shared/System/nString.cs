﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nString
    {
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
    }
}
