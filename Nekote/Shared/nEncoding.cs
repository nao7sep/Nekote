using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nEncoding
    {
        // 理由は不詳だが、private static Encoding BigEndianUTF32 => UTF32Encoding.s_bigEndianDefault と、private になっている
        // その実体は new UTF32Encoding (bigEndian: true, byteOrderMark: true) なので、その通りに実装

        // UnicodeEncoding.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Text/UnicodeEncoding.cs

        /// <summary>
        /// .NET のものが private になっているので用意した。他は Encoding クラスのものを使う。
        /// </summary>
        public static readonly Encoding BigEndianUTF32 = new UTF32Encoding (bigEndian: true, byteOrderMark: true);
    }
}
