using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public class nGuidAndIniLikeFileBasedCrudDataProvider: nIniLikeFileBasedCrudDataProvider <Guid>
    {
        public nGuidAndIniLikeFileBasedCrudDataProvider (string directoryPath): base (EqualityComparer <Guid>.Default, directoryPath)
        {
        }

        public override Guid GenerateKey ()
        {
            // The returned Guid is guaranteed to not equal Guid.Empty とのこと
            // 0やそれに類するものからの連番でなく、ランダム性のあるところでは、基本的には0以外が望ましい

            // Guid.NewGuid Method (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.guid.newguid

            return Guid.NewGuid ();
        }

        public override string KeyToFilePath (Guid key)
        {
            // Guid.ToString Method (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.guid.tostring

            // 自分は、可読性を考えて16進数には大文字を使うことが多いが、ルール化しているわけでない
            // GUID は、16進数をベースとするものだが、人間が読むものでないし、ToString ("D") で小文字で出てくるため、小文字のまま
            // いずれ最速のラウンドトリップを考えるにおいても「小文字のまま」という結論が出るため、今からそれに整合させておく

            return nPath.Join (DirectoryPath, $"{key.ToString ("D")}.nini");
        }

        public override void SetKeyToEntry (nStringDictionary entry, Guid key)
        {
            entry.SetString ("Key", key.ToString ("D"));
        }

        public override bool TryGetKeyFromEntry (nStringDictionary entry, out Guid key)
        {
            try
            {
                if (entry.TryGetValue ("Key", out string? xResult) && // ? を付けないと、叱られる
                    xResult != null &&
                    Guid.TryParseExact (xResult, "D", out Guid xResultAlt))
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

        public override bool TryParseFileName (string name, out Guid key)
        {
            try
            {
                if (name != null &&
                        name.EndsWith (".nini", StringComparison.OrdinalIgnoreCase) &&
                        Guid.TryParseExact (name.AsSpan (0, name.Length - ".nini".Length), "D", out key))
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
