using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // nEqualityComparer のコメントも参照

    // IComparer<T> Interface (System.Collections.Generic) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.icomparer-1

    // Comparer<T> Class (System.Collections.Generic) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.comparer-1

    public class nComparer <ElementType>: Comparer <ElementType>
    {
        // nEqualityComparer には二つあり、メソッド名 + Predicate なので、一応、整合させておく
        public readonly Func <ElementType?, ElementType?, int> ComparePredicate;

        public nComparer (Func <ElementType?, ElementType?, int> comparePredicate)
        {
            ComparePredicate = comparePredicate;
        }

        public override int Compare (ElementType? value1, ElementType? value2)
        {
            return ComparePredicate (value1, value2);
        }
    }
}
