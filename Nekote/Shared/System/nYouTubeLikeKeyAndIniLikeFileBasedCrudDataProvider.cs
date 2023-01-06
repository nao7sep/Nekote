using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // キーは string で扱われる
    // long 型のランダムな値をキーとする辞書をイメージするなら long で扱った方が効率性にも優れるが、
    //     これは、YouTubeLikeKey が主キーであり、CRUD においても文字列のキーにより処理が行われるクラス

    public class nYouTubeLikeKeyAndIniLikeFileBasedCrudDataProvider: nIniLikeFileBasedCrudDataProvider <string>
    {
        public readonly Random Random;

        public nYouTubeLikeKeyAndIniLikeFileBasedCrudDataProvider (string directoryPath, Random? random = null): base (EqualityComparer <string>.Default, directoryPath)
        {
            Random = random ?? Random.Shared;
        }

        public override string GenerateKey ()
        {
            while (true)
            {
                long xKey = Random.NextInt64 ();

                // nGuidAndIniLikeFileBasedCrudDataProvider のコメントにも書いたが、
                //     ランダム性のあるキーなら、特段の理由がなければ0が避けられるべき
                // Random.NextInt64 では long.MaxValue は出ないし、
                //     Guid.NewGuid でも Guid.Empty は出ない

                if (xKey != 0)
                    return xKey.ToYouTubeLikeKey ();
            }
        }

        public override string KeyToFilePath (string key)
        {
            // パスに組み込まれる文字列なので、正当性をチェック

            // Discards - unassigned discardable variables | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/discards

            if (nValue.TryParseYouTubeLikeKey (key, out _) == false)
                throw new nArgumentException ();

            return nPath.Join (DirectoryPath, $"{key}.nini");
        }

        public override void SetKeyToEntry (nStringDictionary entry, string key)
        {
            // こちらでは、パスに使われるわけでないため、チェックは不要
            // 巡り巡って……ということ考えると、際限なくチェックすることになる

            entry.SetString ("Key", key);
        }

        public override bool TryGetKeyFromEntry (nStringDictionary entry, out string key)
        {
            try
            {
                if (entry.TryGetValue ("Key", out string? xResult) && // ? を付けないと、叱られる
                    xResult != null &&
                    nValue.TryParseYouTubeLikeKey (xResult, out _))
                {
                    key = xResult; // long からの再変換は不要
                    return true;
                }
            }

            catch
            {
            }

            key = default!;
            return false;
        }

        public override bool TryParseFileName (string name, out string key)
        {
            try
            {
                if (name != null &&
                    name.EndsWith (".nini", StringComparison.OrdinalIgnoreCase) &&
                    nValue.TryParseYouTubeLikeKey (name.Substring (0, name.Length - ".nini".Length), out long xResult))
                {
                    key = xResult.ToYouTubeLikeKey ();
                    return true;
                }
            }

            catch
            {
            }

            key = default!;
            return false;
        }
    }
}
