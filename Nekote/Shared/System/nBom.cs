using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nBom
    {
        // Wikipedia のページ内の表に含まれるエンコーディングのうち、今まで遭遇したことのある UTF-7 までを表の順序のまま定義

        // Byte order mark - Wikipedia
        // https://en.wikipedia.org/wiki/Byte_order_mark

        // Encoding Class (System.Text) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding

        // UTF7 をコメントアウト
        // 安全でないから使うなとコンパイラーに言われた

        public static readonly byte []
            UTF8 = new byte [] { 0xEF, 0xBB, 0xBF },
            BigEndianUnicode = new byte [] { 0xFE, 0xFF },
            Unicode = new byte [] { 0xFF, 0xFE },
            BigEndianUTF32 = new byte [] { 0x00, 0x00, 0xFE, 0xFF },
            UTF32 = new byte [] { 0xFF, 0xFE, 0x00, 0x00 };
            // UTF7 = new byte [] { 0x2B, 0x2F, 0x76 };

        // UTF は、もう十分に枯れている
        // 文字の追加は続いても、長さ5以上の BOM を必要とする新しい形式は考えにくい
        public const int MaxLength = 4;

        public static Encoding? GetEncoding (byte [] values, int firstIndex, int length)
        {
            // UTF32 のものは、最初だけ見ると Unicode と誤検出される
            // 仕様に欠陥がある
            // 確実に判別するなら、2バイトごとと4バイトごとで読み比べ、どちらがより正しく読めていそうか調べることになる
            // 以下は簡易的な判別なので、そこまでは不要

            // 圧倒的によく見る UTF-8 をまず見る
            // それからは、上記の件を念頭に、4バイトのものから探す
            // それぞれの長さにおいては、Windows がリトルエンディアンなのでそちらから
            // .NET は Mac/Linux でも動くが、Windows がメインの環境であることは今後も変わらない

            if (length >= 3)
            {
                if (nArray.Equals (values, firstIndex, UTF8, 0, 3))
                    return Encoding.UTF8;
            }

            if (length >= 4)
            {
                if (nArray.Equals (values, firstIndex, UTF32, 0, 4))
                    return Encoding.UTF32;

                if (nArray.Equals (values, firstIndex, BigEndianUTF32, 0, 4))
                    return nEncoding.BigEndianUTF32;
            }

            if (length >= 2)
            {
                if (nArray.Equals (values, firstIndex, Unicode, 0, 2))
                    return Encoding.Unicode;

                if (nArray.Equals (values, firstIndex, BigEndianUnicode, 0, 2))
                    return Encoding.BigEndianUnicode;
            }

            return null;
        }
    }
}
