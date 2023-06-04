using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nChar
    {
        // string.IndexOfAny に必要
        public static readonly char [] SupportedDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '０', '１', '２', '３', '４', '５', '６', '７', '８', '９' };

        public static bool IsSupportedDigit (char value)
        {
            // Unicode の BMP 面では、半角と全角の20文字に対応すれば十分
            // BMP 外では、MATHEMATICAL BOLD DIGIT ZERO など、位置付けの不詳なものもある
            // それらまで Nekote が比較などに対応しないことのデメリットは限定的

            // Unicode Characters in the 'Number, Decimal Digit' Category
            // https://www.fileformat.info/info/unicode/category/Nd/list.htm

            // Numerals in Unicode - Wikipedia
            // https://en.wikipedia.org/wiki/Numerals_in_Unicode

            // 少し高速化した
            // 半角空白の0より小さいなら絶対に false

            // return (value >= '0' && value <= '9') || (value >= '０' && value <= '９');
            return value >= '0' && (value <= '9' || (value >= '０' && value <= '９'));
        }

        public static int CompareSupportedDigits (char value1, char value2)
        {
            // ループで何度も行われる処理なので、ベタ書きにより少しでも速く

            // IsSupportedDigit の方を少し高速化したが、こちらは、ややこしくなるのでそのまま
            // そもそも、両方が数字だと分かっていて呼ばれることが多い

            if (value1 >= '0' && value1 <= '9')
            {
                if (value2 >= '0' && value2 <= '9')
                    return value1.CompareTo (value2);

                else if (value2 >= '０' && value2 <= '９')
                    return value1.CompareTo ((char) (value2 - '０' + '0'));
            }

            else if (value1 >= '０' && value1 <= '９')
            {
                if (value2 >= '0' && value2 <= '9')
                    return ((char) (value1 - '０' + '0')).CompareTo (value2);

                else if (value2 >= '０' && value2 <= '９')
                    return value1.CompareTo (value2);
            }

            throw new nArgumentException ();
        }
    }
}
