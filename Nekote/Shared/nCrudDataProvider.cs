using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // abstract - C# Reference | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/abstract

    public abstract class nCrudDataProvider <KeyType, EntryType>: Dictionary <KeyType, EntryType>
        where KeyType: notnull
    {
        // comparer に ? を付けてデフォルト値を null にする考えがあるが、やめておく
        // abstract class なので、上位クラスでの仕様の明示的な指定を要求する
        // たいてい IEqualityComparer <KeyType>.Default だが、それも上位クラスで指定してもらう

        public nCrudDataProvider (IEqualityComparer <KeyType> comparer): base (comparer)
        {
        }

        public abstract KeyType CreateEntry (EntryType entry);

        public abstract bool TryCreateEntry (EntryType entry, out KeyType key);

        public abstract EntryType ReadEntry (KeyType key);

        public abstract bool TryReadEntry (KeyType key, out EntryType entry);

        public abstract void UpdateEntry (KeyType key, EntryType entry);

        public abstract bool TryUpdateEntry (KeyType key, EntryType entry);

        public abstract void DeleteEntry (KeyType key);

        public abstract bool TryDeleteEntry (KeyType key);
    }
}
