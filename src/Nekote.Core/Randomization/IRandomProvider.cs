using System;

namespace Nekote.Core.Randomization
{
    /// <summary>
    /// 乱数生成を抽象化し、テスト容易性を向上させるためのインターフェース。
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>
        /// 0以上のランダムな整数を返します。
        /// </summary>
        int Next();

        /// <summary>
        /// 指定した最大値未満の0以上のランダムな整数を返します。
        /// </summary>
        int Next(int maxValue);

        /// <summary>
        /// 指定した範囲内のランダムな整数を返します。
        /// </summary>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// 0以上のランダムな64ビット整数を返します。
        /// </summary>
        long NextInt64();

        /// <summary>
        /// 指定した最大値未満の0以上のランダムな64ビット整数を返します。
        /// </summary>
        long NextInt64(long maxValue);

        /// <summary>
        /// 指定した範囲内のランダムな64ビット整数を返します。
        /// </summary>
        long NextInt64(long minValue, long maxValue);

        /// <summary>
        /// 0.0と1.0の間のランダムな浮動小数点数を返します。
        /// </summary>
        double NextDouble();

        /// <summary>
        /// 0.0Fと1.0Fの間のランダムな浮動小数点数を返します。
        /// </summary>
        float NextSingle();

        // Sample() メソッドは、System.Random の 'protected virtual' メンバーであり、直接の公開使用ではなく、
        // 派生クラスでのオーバーライドを目的としているため、このインターフェースでは定義されていません。

        /// <summary>
        /// バイト配列をランダムな数値で埋めます。
        /// </summary>
        void NextBytes(byte[] buffer);

        /// <summary>
        /// バイトスパンをランダムな数値で埋めます。
        /// </summary>
        void NextBytes(Span<byte> buffer);

        /// <summary>
        /// ソース配列からランダムに選択された項目を含む新しい配列を作成します。
        /// </summary>
        T[] GetItems<T>(T[] choices, int length);

        /// <summary>
        /// ソーススパンからランダムに選択された項目で宛先スパンを埋めます。
        /// </summary>
        void GetItems<T>(ReadOnlySpan<T> choices, Span<T> destination);

        /// <summary>
        /// ソーススパンからランダムに選択された項目を含む新しい配列を作成します。
        /// </summary>
        T[] GetItems<T>(ReadOnlySpan<T> choices, int length);

        /// <summary>
        /// 配列の要素の順序をランダム化します。
        /// </summary>
        void Shuffle<T>(T[] values);

        /// <summary>
        /// スパンの要素の順序をランダム化します。
        /// </summary>
        void Shuffle<T>(Span<T> values);
    }
}
