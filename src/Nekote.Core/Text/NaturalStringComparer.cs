using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列を自然順（例: "file 2.txt"が"file 10.txt"より前に来る）で比較するための機能を提供します。
    /// このクラスは、StringComparerと同様の静的プロパティとファクトリメソッドを提供します。
    /// </summary>
    /// <remarks>
    /// 設計について: abstract class vs. interface
    ///
    /// このAPIは、インターフェースではなく抽象クラスとして設計されています。
    /// 主な理由は、<see cref="System.StringComparer"/> のような使い慣れたファクトリパターンを提供するためです。
    /// これにより、<see cref="InvariantCulture"/> のような静的プロパティを通じて、事前に構成されたインスタンスに簡単にアクセスできます。
    /// 抽象クラスはまた、実装の詳細をライブラリの利用者に公開することなく、内部で管理することを可能にします。
    ///
    /// デフォルトの動作: Unicode正規化
    ///
    /// このクラスの静的プロパティ（例：<see cref="InvariantCulture"/>）によって返されるデフォルトのコンパレータは、Unicode正規化を実行します。
    /// これは、全角数字（例：「１２３」）と半角数字（例：「123」）を等しいものとして扱うことを意味します。
    /// この動作は、特に日本語のような全角文字が一般的に使用される環境での直感的なソート順序を提供します。
    /// パフォーマンスが最優先で、入力文字列に全角数字などが含まれないことが確実な場合は、<see cref="Create(StringComparer, bool)"/> メソッドで正規化を無効にしたインスタンスを生成できます。
    /// 例：NaturalStringComparer.Create(StringComparer.Ordinal, normalize: false)
    ///
    /// 現在の実装の制限事項
    ///
    /// 現在の実装は、符号なし整数を効率的に処理することに特化しています。
    /// 以下の要素はサポートされていません：
    ///
    /// - 符号: 正（+）または負（-）の符号は数字の一部として解釈されません。
    /// - 浮動小数点数: 小数点（.や,）は数字の一部として認識されず、文字列として扱われます。例えば、"1.5" は "1"、"."、"5" の3つの部分として比較されます。
    /// - 桁区切り文字: 桁区切り文字（,や.）はサポートされていません。例えば、"1,000" は数値の "1" と、文字列の ","、数値の "000" として解釈されます。
    ///
    /// これらの機能をサポートするには、カルチャに依存した数値解析が必要となり、大幅に複雑さが増します。
    /// 例えば、"1,000" は英語圏では「千」を意味しますが、ドイツ語圏では「1.000」と表記され、逆に "," は小数点を意味します。
    /// このような曖昧さを解決するには、<see cref="System.Globalization.CultureInfo"/> をコンパレータに渡し、<see cref="decimal.TryParse(string?, System.Globalization.NumberStyles, System.IFormatProvider?, out decimal)"/> のような高度な解析を行う必要があります。
    /// このような機能は、パフォーマンスへの影響と設計の複雑化を伴うため、現在の実装では意図的に除外されています。
    /// </remarks>
    public abstract class NaturalStringComparer : IComparer<string>, IEqualityComparer<string>
    {
        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer InvariantCulture { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCulture, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer InvariantCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.InvariantCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer CurrentCulture { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCulture, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer CurrentCultureIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.CurrentCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer Ordinal { get; } = new NaturalStringComparerImplementation(StringComparer.Ordinal, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        public static NaturalStringComparer OrdinalIgnoreCase { get; } = new NaturalStringComparerImplementation(StringComparer.OrdinalIgnoreCase, normalize: true);

        /// <summary>
        /// 指定した StringComparer と正規化オプションを使用して NaturalStringComparer のインスタンスを作成します。
        /// </summary>
        /// <param name="baseComparer">基本的な文字列比較（大文字小文字の区別、カルチャなど）を行うための StringComparer。</param>
        /// <param name="normalize">比較前にUnicode正規化（例：全角数字を半角に変換）を行うかどうか。デフォルトは <c>true</c> です。</param>
        /// <returns>NaturalStringComparer の新しいインスタンス。</returns>
        public static NaturalStringComparer Create(StringComparer baseComparer, bool normalize = true)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            return new NaturalStringComparerImplementation(baseComparer, normalize);
        }

        /// <summary>
        /// 2つの文字列を比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="x">比較する最初のオブジェクト。</param>
        /// <param name="y">比較する 2 番目のオブジェクト。</param>
        /// <returns>
        /// 0未満: xはyより小さい。
        /// 0: xはyと等しい。
        /// 0より大きい: xはyより大きい。
        /// </returns>
        public abstract int Compare(string? x, string? y);

        /// <summary>
        /// 2つの文字列が等しいかどうかを判断します。
        /// </summary>
        /// <param name="x">比較する最初の文字列。</param>
        /// <param name="y">比較する 2 番目の文字列。</param>
        /// <returns>文字列が等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(string? x, string? y);

        /// <summary>
        /// 指定した文字列のハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字列。</param>
        /// <returns>指定した文字列のハッシュコード。</returns>
        public abstract int GetHashCode(string obj);

        /// <summary>
        /// 2つの文字スパンを比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="x">比較する最初の文字スパン。</param>
        /// <param name="y">比較する 2 番目の文字スパン。</param>
        /// <returns>
        /// 0未満: xはyより小さい。
        /// 0: xはyと等しい。
        /// 0より大きい: xはyより大きい。
        /// </returns>
        public abstract int Compare(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        /// <summary>
        /// 2つの文字スパンが等しいかどうかを判断します。
        /// </summary>
        /// <param name="x">比較する最初の文字スパン。</param>
        /// <param name="y">比較する 2 番目の文字スパン。</param>
        /// <returns>文字スパンが等しい場合はtrue、それ以外の場合はfalse。</returns>
        public abstract bool Equals(ReadOnlySpan<char> x, ReadOnlySpan<char> y);

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <param name="obj">ハッシュコードを取得する対象の文字スパン。</param>
        /// <returns>指定した文字スパンのハッシュコード。</returns>
        public abstract int GetHashCode(ReadOnlySpan<char> obj);
    }
}
