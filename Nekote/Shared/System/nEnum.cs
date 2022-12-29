using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Enumeration types - C# reference | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum

    // Enum Design - Framework Design Guidelines | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum

    // Names of Classes, Structs, and Interfaces - Framework Design Guidelines | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces#naming-enumerations

    // Enum Class (System) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.enum

    // Enum.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Enum.cs

    // FlagsAttribute Class (System) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.flagsattribute

    // Enumeration format strings | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/standard/base-types/enumeration-format-strings

    public static class nEnum
    {
        // 一つの値を受け取り、それが enum 内に存在するかチェック

        // struct と Enum のどちらか一つでも外すとコンパイルできない

        public static bool ValidateValue <EnumType> (EnumType value) where EnumType: struct, Enum
        {
            return Enum.GetValues <EnumType> ().Contains (value);
        }

        // 一つの値または複数の値の OR を受け取り、それら全てが enum 内に存在するかチェック

        // enum の側が正しく設計されていて、メソッドの使い方も正しいことを必要とする
        // たとえば、Red = 1, Green = 2 の enum で3を検証すると、Green の次は Blue = 4 でも3で True が返る
        // この場合、使うべきは、ValidateValue の方

        public static bool ValidateAllValues <EnumType> (EnumType values) where EnumType: struct, Enum
        {
            Type xUnderlyingType = Enum.GetUnderlyingType (typeof (EnumType));

            // Enum.GetValuesAsUnderlyingType に次のコードがある

            // return InternalGetCorElementType (enumType) switch
            // {
            //     CorElementType.ELEMENT_TYPE_I1 => GetEnumInfo <sbyte> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_U1 => GetEnumInfo <byte> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_I2 => GetEnumInfo <short> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_U2 => GetEnumInfo <ushort> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_I4 => GetEnumInfo <int> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_U4 => GetEnumInfo <uint> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_I8 => GetEnumInfo <long> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_U8 => GetEnumInfo <ulong> (enumType, getNames: false).CloneValues (),
            // #if RARE_ENUMS
            //     CorElementType.ELEMENT_TYPE_R4 => GetEnumInfo <float> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_R8 => GetEnumInfo <double> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_I => GetEnumInfo <nint> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_U => GetEnumInfo <nuint> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_CHAR => GetEnumInfo <char> (enumType, getNames: false).CloneValues (),
            //     CorElementType.ELEMENT_TYPE_BOOLEAN => CopyByteArrayToNewBoolArray (GetEnumInfo <byte> (enumType, getNames: false).Values),
            // #endif
            //     _ => throw CreateUnknownEnumTypeException (),
            // };

            // このうち、RARE_ENUMS の型には、現時点では対応しない
            // .NET のガイドラインでも非推奨のようだし、int, long, short くらいしか理由がないため
            // 対応する型については、上記コードの順で見ていくが、int のみ、頻度が高いので最初に

            // Enum.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Enum.cs

            if (xUnderlyingType == typeof (int) ||
                xUnderlyingType == typeof (sbyte) ||
                xUnderlyingType == typeof (byte) ||
                xUnderlyingType == typeof (short) ||
                xUnderlyingType == typeof (ushort))
            {
                // コードの共通化のために <UnderlyingType> の補助メソッドを作ってみたが、
                //     UnderlyingType へのビット演算ができないとのエラーに
                // ソースでは where TUnderlyingValue: struct, INumber <TUnderlyingValue>, IBitwiseOperators <TUnderlyingValue, TUnderlyingValue, TUnderlyingValue> となっているが、
                //     これらは .NET 7 からのインターフェースのようだ
                // それぞれ、むしろ .NET 6 でまだそれらがないことに驚くほど基本的なもの

                // INumber<TSelf> Interface (System.Numerics) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.numerics.inumber-1

                // IBitwiseOperators<TSelf,TOther,TResult>.BitwiseAnd(TSelf, TOther) Operator (System.Numerics) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.numerics.ibitwiseoperators-3.op_bitwiseand

                // .NET 6 ではジェネリックの型へのビット演算が難しいようなので、埋め込みの型を並べてベタ書きに

                // object を通る点などが気になるが、Convert.ChangeType なら動くようなので様子見

                // Convert.ChangeType Method (System) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.convert.changetype

                int xValues = (int) Convert.ChangeType (values, typeof (int));

                foreach (EnumType xValue in Enum.GetValues <EnumType> ())
                    xValues &= ~((int) Convert.ChangeType (xValue, typeof (int)));

                if (xValues == 0)
                    return true;
            }

            else if (xUnderlyingType == typeof (uint))
            {
                uint xValues = (uint) Convert.ChangeType (values, typeof (uint));

                foreach (EnumType xValue in Enum.GetValues <EnumType> ())
                    xValues &= ~((uint) Convert.ChangeType (xValue, typeof (uint)));

                if (xValues == 0)
                    return true;
            }

            else if (xUnderlyingType == typeof (long))
            {
                long xValues = (long) Convert.ChangeType (values, typeof (long));

                foreach (EnumType xValue in Enum.GetValues <EnumType> ())
                    xValues &= ~((long) Convert.ChangeType (xValue, typeof (long)));

                if (xValues == 0)
                    return true;
            }

            else if (xUnderlyingType == typeof (ulong))
            {
                ulong xValues = (ulong) Convert.ChangeType (values, typeof (ulong));

                foreach (EnumType xValue in Enum.GetValues <EnumType> ())
                    xValues &= ~((ulong) Convert.ChangeType (xValue, typeof (ulong)));

                if (xValues == 0)
                    return true;
            }

            else throw new nNotSupportedException ();

            return false;
        }
    }
}
