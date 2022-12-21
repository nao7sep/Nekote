using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Enumerable.Except には、引数として IEqualityComparer を取るものがある
    // パフォーマンスの問われない一度きりのテストコードなどでは、こういうところをラムダ式でパッと書きたい

    // Enumerable.Except Method (System.Linq) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.linq.enumerable.except

    // We recommend that you derive from the EqualityComparer<T> class instead of implementing the IEqualityComparer<T> interface,
    //     because the EqualityComparer<T> class tests for equality using the IEquatable<T>.Equals method instead of the Object.Equals method とある

    // IEqualityComparer<T> Interface (System.Collections.Generic) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1

    // EqualityComparer<T> Class (System.Collections.Generic) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.equalitycomparer-1

    public class nEqualityComparer <ElementType>: EqualityComparer <ElementType>
    {
        public readonly Func <ElementType?, ElementType?, bool> EqualsPredicate;

        // なくてよいところでは省略できるように
        public readonly Func <ElementType, int>? GetHashCodePredicate = null;

        public nEqualityComparer (Func <ElementType?, ElementType?, bool> equalsPredicate, Func <ElementType, int>? getHashCodePredicate = null)
        {
            EqualsPredicate = equalsPredicate;
            GetHashCodePredicate = getHashCodePredicate;
        }

        public override bool Equals (ElementType? value1, ElementType? value2)
        {
            return EqualsPredicate (value1, value2);
        }

        // IDE にコードを生成してもらうと引数の型名に [DisallowNull] が付く
        // こういうのは、付けられる全てのところに付けようと徹底するコストが利益に見合わないため、あまり付けたくない
        // 呼び出し側に問題があれば、value! のところで落ちる

        public override int GetHashCode (ElementType value)
        {
            if (GetHashCodePredicate != null)
                return GetHashCodePredicate (value);

            else return value!.GetHashCode ();
        }
    }
}
