using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // key を文字列で受けるとすれば、long 型の Ticks を文字列化して Z を付けたものになる
    // Z は冗長と思えるが、データの欠損がないことの確認になるし、ローカル日時との混同の回避にもつながる
    // クラス名にも Utc を入れ、ローカル日時との混同のリスクをさらに低めておく

    // <DateTime> とすることも考えたが、Ticks を DateTime.UtcNow.Ticks により取得するだけであり、DateTime 型を主体的に使う実装でない
    // キーは、その値が特定の型であることが実装に不可欠でないなら、できるだけコストの低い型で扱われるべき

    public class nUtcTicksAndIniLikeFileBasedCrudDataProvider: nIniLikeFileBasedCrudDataProvider <long>
    {
        public nUtcTicksAndIniLikeFileBasedCrudDataProvider (string directoryPath): base (EqualityComparer <long>.Default, directoryPath)
        {
        }

        public override long GenerateKey ()
        {
            return DateTime.UtcNow.Ticks;
        }

        public override string KeyToFilePath (long key)
        {
            return nPath.Join (DirectoryPath, $"{key.ToString (CultureInfo.InvariantCulture)}Z{nStringLiterals.IniLikeFileExtension}");
        }

        public override void SetKeyToEntry (nStringDictionary entry, long key)
        {
            entry.SetString ("Key", $"{key.ToString (CultureInfo.InvariantCulture)}Z");
        }

        public override bool TryGetKeyFromEntry (nStringDictionary entry, out long key)
        {
            try
            {
                if (entry.TryGetValue ("Key", out string? xResult) && // ? を付けないと、叱られる
                    xResult != null &&
                    (xResult [xResult.Length - 1] == 'Z' || xResult [xResult.Length - 1] == 'z') && // 大文字・小文字が区別されない
                    long.TryParse (xResult.AsSpan (0, xResult.Length - 1), NumberStyles.None, CultureInfo.InvariantCulture, out long xResultAlt))
                {
                    key = xResultAlt;
                    return true;
                }
            }

            catch
            {
            }

            key = default;
            return false;
        }

        public override bool TryParseFileName (string name, out long key)
        {
            try
            {
                if (name != null &&
                        name.EndsWith ($"Z{nStringLiterals.IniLikeFileExtension}", StringComparison.OrdinalIgnoreCase) &&
                        long.TryParse (name.AsSpan (0, name.Length - $"Z{nStringLiterals.IniLikeFileExtension}".Length), NumberStyles.None, CultureInfo.InvariantCulture, out key))
                    return true;
            }

            catch
            {
            }

            key = default;
            return false;
        }
    }
}
