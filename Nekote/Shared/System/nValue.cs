using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // 値型の構造体の処理に関するものを雑多に放り込んでいくところ
    // Enum もそうだが、Enum には個別のクラスを用意する

    // ValueType Class (System) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.valuetype

    public static class nValue
    {
        // YouTube のビデオには、v= で11桁のキーが付いている
        // これは [0-9A-Za-z-_] の64文字で6ビットずつを11文字の可能性が高い

        // ビット演算において、エンディアンについては考える必要がないとのこと
        // 一応、Mac でも試す → 同じキーを経てのラウンドトリップに成功

        // c - Does bit-shift depend on endianness? - Stack Overflow
        // https://stackoverflow.com/questions/7184789/does-bit-shift-depend-on-endianness

        // 文字コード順に追加していくが、それだと (long) 0 が - の連続になるため、記号を最後に

        // List <char> xChars = new List <char> ();

        // xChars.AddRange (Enumerable.Range (0, 10).Select (x => (char) ('0' + x))); // 0x30 から
        // xChars.AddRange (Enumerable.Range (0, 26).Select (x => (char) ('A' + x))); // 0x41 から
        // xChars.AddRange (Enumerable.Range (0, 26).Select (x => (char) ('a' + x))); // 0x61 から

        // xChars.Add ('-'); // 0x2D
        // xChars.Add ('_'); // 0x5F

        // Console.WriteLine (new string (xChars.ToArray ()));

        private static readonly string iYouTubeLikeKeyChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_";

        public static string ToYouTubeLikeKey (this long value)
        {
            char [] xChars = new char [11];

            // & は >> より強い
            // C 言語についてのページだが、C# でも同じだろう

            // Precedence and order of evaluation | Microsoft Learn
            // https://learn.microsoft.com/en-us/cpp/c-language/precedence-and-order-of-evaluation

            // 欲しい6ビットを右端に移動し、0x3F との AND により切り抜き
            // 最初の5文字分は、24 + 6 == 30 <= 32 なので int にキャストしてから処理

            // 最後、残り4ビットから6ビット分を読もうとする
            // ない分については、For unsigned numbers, the bit positions that have been vacated by the shift operation are zero-filled
            //     For signed numbers, the sign bit is used to fill the vacated bit positions
            //     In other words, if the number is positive, 0 is used, and if the number is negative, 1 is used とのこと

            // Left shift and right shift operators ('<<' and '>>') | Microsoft Learn
            // https://learn.microsoft.com/en-us/cpp/cpp/left-shift-and-right-shift-operators-input-and-output

            // 文字の順序を反転させた
            // 元が整数なのだから、64進数のように桁が上がっていく方が分かりやすい

            xChars [10] = iYouTubeLikeKeyChars [0x3F & ((int) value >> 0)];
            xChars [9] = iYouTubeLikeKeyChars [0x3F & ((int) value >> 6)];
            xChars [8] = iYouTubeLikeKeyChars [0x3F & ((int) value >> 12)];
            xChars [7] = iYouTubeLikeKeyChars [0x3F & ((int) value >> 18)];
            xChars [6] = iYouTubeLikeKeyChars [0x3F & ((int) value >> 24)];

            xChars [5] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 30))];
            xChars [4] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 36))];
            xChars [3] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 42))];
            xChars [2] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 48))];
            xChars [1] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 54))];
            xChars [0] = iYouTubeLikeKeyChars [(int) (0x3F & (value >> 60))];

            return new string (xChars);
        }

        // char [] xValues = new char [0x7F];

        // for (int temp = 0; temp < 64; temp ++)
        //     xValues ["0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz-_" [temp]] = (char) temp;

        // Console.WriteLine (string.Join (", ", xValues.Select (x => FormattableString.Invariant (@$"'\x{(int) x:X2}'"))));

        // ビット演算時の int への変換を減らすため、初めから int [] で用意しておく

        private static readonly int [] iYouTubeLikeKeyValues = { '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x3E', '\x00', '\x00', '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08', '\x09', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x00', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F', '\x10', '\x11', '\x12', '\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', '\x1E', '\x1F', '\x20', '\x21', '\x22', '\x23', '\x00', '\x00', '\x00', '\x00', '\x3F', '\x00', '\x24', '\x25', '\x26', '\x27', '\x28', '\x29', '\x2A', '\x2B', '\x2C', '\x2D', '\x2E', '\x2F', '\x30', '\x31', '\x32', '\x33', '\x34', '\x35', '\x36', '\x37', '\x38', '\x39', '\x3A', '\x3B', '\x3C', '\x3D', '\x00', '\x00', '\x00', '\x00' };

        private static bool iIsValidYouTubeLikeKey (string key)
        {
            return key.Length == 11 && key.Any (x => iYouTubeLikeKeyChars.Contains (x, StringComparison.Ordinal) == false) == false;
        }

        private static long iParseYouTubeLikeKey (string key)
        {
            // iYouTubeLikeKeyValues が int [] なので、0～30ビットの部分は、int で OR を取り、最後に long にキャスト
            // 30～36ビットの部分は、int の切れ目にまたがるため、最初に long にキャストしてからビットシフト
            // 36～64ビットの部分は、シフト量を減らすことで int として処理できるため、OR を取ってから long にキャスト
            // 元の値が負なら0でなく1が詰められることについては、ここで上位2ビットが破棄されるため問題なし

            return
                (long)
                    (iYouTubeLikeKeyValues [key [10]] << 0 |
                    iYouTubeLikeKeyValues [key [9]] << 6 |
                    iYouTubeLikeKeyValues [key [8]] << 12 |
                    iYouTubeLikeKeyValues [key [7]] << 18 |
                    iYouTubeLikeKeyValues [key [6]] << 24) |
                (long)
                    iYouTubeLikeKeyValues [key [5]] << 30 |
                (long)
                    (iYouTubeLikeKeyValues [key [4]] << 4 |
                    iYouTubeLikeKeyValues [key [3]] << 10 |
                    iYouTubeLikeKeyValues [key [2]] << 16 |
                    iYouTubeLikeKeyValues [key [1]] << 22 |
                    iYouTubeLikeKeyValues [key [0]] << 28) << 32;
        }

        public static long ParseYouTubeLikeKey (string key)
        {
            if (iIsValidYouTubeLikeKey (key) == false)
                throw new nFormatException ();

            return iParseYouTubeLikeKey (key);
        }

        public static bool TryParseYouTubeLikeKey (string key, out long result)
        {
            if (iIsValidYouTubeLikeKey (key) == false)
            {
                result = default;
                return false;
            }

            result = iParseYouTubeLikeKey (key);
            return true;
        }
    }
}
