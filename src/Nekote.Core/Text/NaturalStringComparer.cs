using System;
using System.Collections.Generic;

namespace Nekote.Core.Text
{
    /// <summary>
    /// 文字列を自然順（natural order）で比較するための機能を提供します。この順序は、ファイル名やバージョン番号など、
    /// 人間が直感的に期待するソート順（例: "file 2.txt"が"file 10.txt"より前に来る）を実現します。
    /// このクラスは不変（immutable）であり、その静的インスタンスはスレッドセーフです。
    /// </summary>
    /// <remarks>
    /// 設計思想
    ///
    /// このAPIは、.NET標準の <see cref="System.StringComparer"/> と同様の使い慣れた静的プロパティとファクトリパターンを提供します。
    /// これにより、<see cref="InvariantCulture"/> のような定義済みインスタンスに簡単にアクセスできます。
    ///
    /// Unicodeの取り扱い
    ///
    /// 内部実装では、<see cref="GraphemeReader"/> を使用して、文字列を文字素クラスタ（grapheme cluster）単位で
    /// 安全に反復処理します。これにより、サロゲートペアで表現される絵文字や、結合文字（例: "e" + "´" = "é"）などが
    /// 1つの文字単位として正しく認識され、比較アルゴリズムが破壊されるのを防ぎます。この堅牢なUnicode処理が、このクラスの重要な特徴です。
    ///
    /// デフォルトの動作: Unicode正規化
    ///
    /// 静的プロパティ（例：<see cref="InvariantCulture"/>）によって返されるデフォルトのコンパレータは、Unicode正規化を実行します。
    /// これにより、全角数字（例：「１２３」）と半角数字（例：「123」）が等価として扱われ、特に日本語環境で直感的なソート順序を提供します。
    /// パフォーマンスが最優先で、入力文字列に全角文字などが含まれないことが確実な場合は、<see cref="Create(StringComparer, bool)"/> メソッドで
    /// 正規化を無効にしたインスタンスを生成できます。（例: NaturalStringComparer.Create(StringComparer.Ordinal, normalize: false)）
    ///
    /// 現在の実装の制限事項
    ///
    /// 現在の実装は、符号なし整数を効率的に処理することに特化しており、以下の要素はサポートされていません：
    ///
    /// - 符号: 正（+）または負（-）の符号は数字の一部として解釈されません。
    /// - 浮動小数点数: 小数点（.や,）は数字の一部として認識されず、文字列として扱われます。
    /// - 桁区切り文字: 桁区切り文字（,や.）はサポートされていません。
    ///
    /// これらの機能をサポートするには、カルチャに依存した高度な数値解析が必要となり、パフォーマンスへの影響と設計の複雑化を伴うため、
    /// 現在の実装では意図的に除外されています。
    /// </remarks>
    public sealed class NaturalStringComparer : IComparer<string>, IEqualityComparer<string>
    {
        private readonly StringComparer _baseComparer;
        private readonly bool _normalize;

        private NaturalStringComparer(StringComparer baseComparer, bool normalize)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            _baseComparer = baseComparer;
            _normalize = normalize;
        }

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別する自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: ファイルパス、URI、プロトコルメッセージ、内部キーなど、機械が処理する識別子の比較に適しています。
        /// 動作: 文字をUnicodeコードポイントとして直接比較するため、高速で予測可能です。カルチャに依存しないため、環境を問わず一貫した結果が得られます。
        /// 注意: 'f' と 'F' を区別します。言語的な等価性（例: 'é' と 'e' + '´'）を考慮しないため、ユーザー向けの表示には不向きです。Linuxのような大文字と小文字を区別するファイルシステムで特に有用です。
        /// </remarks>
        public static NaturalStringComparer Ordinal { get; } = new NaturalStringComparer(StringComparer.Ordinal, normalize: true);

        /// <summary>
        /// 序数（バイナリ）ルールを使用して、大文字と小文字を区別しない自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="Ordinal"/> と同様に機械が処理する識別子向けですが、大文字と小文字を区別しません。
        /// 動作: <see cref="Ordinal"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: Windowsのように大文字と小文字を区別しないファイルシステムでのファイル名比較に最適です。
        /// </remarks>
        public static NaturalStringComparer OrdinalIgnoreCase { get; } = new NaturalStringComparer(StringComparer.OrdinalIgnoreCase, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別する自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: 言語的に意味のあるが、特定のカルチャに依存しない方法で表示・ソートするデータに適しています。
        /// 動作: 言語的な規則に基づいて比較します。例えば、カノニカル等価な文字列（'é' と 'e' + '´'）を正しく等価と判断します。
        /// 注意: <see cref="Ordinal"/> よりも低速です。また、ファイル名のような技術的識別子に使用すると、直感に反する結果を返すことがあります。
        /// 例えば、`StringComparer.InvariantCulture`が"File1.txt"を"file2.txt"より小さいと判断するのに対し、
        /// この自然順比較では"File1.txt"がより大きいと判断されます。
        /// これは、自然順アルゴリズムが文字列を「File」と「1」、および「file」と「2」のようにチャンクに分割するためです。
        /// この分割により、.NETの`StringComparer`が持つ、大文字小文字以外の部分が同一の場合に数値を優先する能力が妨げられます。
        /// 結果として、最初のテキストチャンク（"File"と"file"）の比較が全体の順序を決定してしまい、直感に反する順序が生まれます。
        /// </remarks>
        public static NaturalStringComparer InvariantCulture { get; } = new NaturalStringComparer(StringComparer.InvariantCulture, normalize: true);

        /// <summary>
        /// インバリアントカルチャを使用して、大文字と小文字を区別しない自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="InvariantCulture"/> と同様ですが、大文字と小文字を区別しません。
        /// 動作: <see cref="InvariantCulture"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: ユーザーに表示するリストなどで、カルチャに依存しないが、大文字・小文字を区別しないソートが必要な場合に適しています。
        /// </remarks>
        public static NaturalStringComparer InvariantCultureIgnoreCase { get; } = new NaturalStringComparer(StringComparer.InvariantCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別し、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: 現在のシステムカルチャに固有の規則で、ユーザーに表示するデータをソートする場合にのみ使用します。
        /// 動作: 実行環境のカルチャ設定に依存するため、結果が環境によって変わる可能性があります。
        /// 注意: 結果の再現性が保証されないため、データの永続化や内部キーの比較には絶対に使用しないでください。
        /// </remarks>
        public static NaturalStringComparer CurrentCulture { get; } = new NaturalStringComparer(StringComparer.CurrentCulture, normalize: true);

        /// <summary>
        /// 現在のカルチャを使用して、大文字と小文字を区別せず、Unicode正規化を行う自然順比較のインスタンスを取得します。
        /// </summary>
        /// <remarks>
        /// 用途: <see cref="CurrentCulture"/> と同様ですが、大文字と小文字を区別しません。
        /// 動作: <see cref="CurrentCulture"/> の大文字・小文字を区別しないバージョンです。
        /// 注意: <see cref="CurrentCulture"/> と同じく、結果の再現性が保証されないため、データの永続化や内部キーには使用しないでください。
        /// </remarks>
        public static NaturalStringComparer CurrentCultureIgnoreCase { get; } = new NaturalStringComparer(StringComparer.CurrentCultureIgnoreCase, normalize: true);

        /// <summary>
        /// 指定した StringComparer と正規化オプションを使用して NaturalStringComparer のインスタンスを作成します。
        /// </summary>
        /// <param name="baseComparer">基本的な文字列比較（大文字小文字の区別、カルチャなど）を行うための StringComparer。</param>
        /// <param name="normalize">比較前にUnicode正規化（例：全角数字を半角に変換）を行うかどうか。デフォルトは true です。</param>
        /// <returns>NaturalStringComparer の新しいインスタンス。</returns>
        public static NaturalStringComparer Create(StringComparer baseComparer, bool normalize = true)
        {
            if (baseComparer is null)
            {
                throw new ArgumentNullException(nameof(baseComparer));
            }
            return new NaturalStringComparer(baseComparer, normalize);
        }

        /// <summary>
        /// 2つの文字列を比較し、並べ替え順序での相対的な位置を示す値を返します。
        /// </summary>
        /// <param name="left">比較する最初のオブジェクト。</param>
        /// <param name="right">比較する 2 番目のオブジェクト。</param>
        /// <returns>
        /// 0未満: leftはrightより小さい。
        /// 0: leftはrightと等しい。
        /// 0より大きい: leftはrightより大きい。
        /// </returns>
        public int Compare(string? left, string? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }
            if (left is null)
            {
                return -1;
            }
            if (right is null)
            {
                return 1;
            }

            return Compare(left.AsSpan(), right.AsSpan());
        }

        /// <summary>
        /// 2つの文字列が等しいかどうかを判断します。
        /// </summary>
        /// <param name="left">比較する最初の文字列。</param>
        /// <param name="right">比較する 2 番目の文字列。</param>
        /// <returns>文字列が等しい場合はtrue、それ以外の場合はfalse。</returns>
        public bool Equals(string? left, string? right)
        {
            return Compare(left, right) == 0;
        }

        /// <summary>
        /// 指定した文字列のハッシュコードを返します。
        /// </summary>
        /// <param name="text">ハッシュコードを取得する対象の文字列。</param>
        /// <returns>指定した文字列のハッシュコード。</returns>
        public int GetHashCode(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            return GetHashCode(text.AsSpan());
        }

        /// <summary>
        /// 2つの文字スパンを自然順で比較します。
        /// </summary>
        /// <remarks>
        /// パフォーマンスに関する注意:
        /// 現在の実装は、Unicode書記素クラスタを正しく処理するために内部的に一時的な文字列とバッファを割り当てます。
        /// これは .NET の StringInfo API の制限によるものです。
        /// 将来的に .NET がスパンベースの書記素列挙をサポートした場合、ゼロアロケーション実装に更新される予定です。
        ///
        /// アルゴリズム戦略:
        /// 1. GraphemeReaderを使用して、文字列を文字単位ではなく書記素クラスタ単位で反復処理します。
        ///    これにより、サロゲートペア（絵文字など）や結合文字が正しく1つの単位として扱われます。
        /// 2. 文字列を「数値チャンク」と「非数値チャンク」に分割します。
        /// 3. 対応するチャンク同士を比較します。
        ///    - 両方が数値チャンクの場合、数値として比較します（例: 2は10より小さい）。
        ///    - 両方が非数値チャンクの場合、指定された基本コンパレータ（_baseComparer）を使用して文字列として比較します。
        /// 4. チャンクの種類が異なる場合（例: 数字と文字）、比較を基本コンパレータに委ね、一貫した順序付けを保証します。
        /// 5. 非数値チャンクの比較では、一方がもう一方の接頭辞であるケースを正しく処理します。
        ///    （例: "file"と"file.txt"）共通部分を比較した後、残りの部分の比較を続行します。
        /// </remarks>
        public int Compare(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            var leftReader = new GraphemeReader(left);
            var rightReader = new GraphemeReader(right);

            while (!leftReader.IsEndOfText && !rightReader.IsEndOfText)
            {
                var isLeftDigit = IsCurrentGraphemeDigit(leftReader);
                var isRightDigit = IsCurrentGraphemeDigit(rightReader);

                if (isLeftDigit && isRightDigit)
                {
                    // シナリオ1: 両方の現在位置が数字。
                    // この場合、両方の文字列から完全な数値チャンク（連続する数字の並び）を抽出し、
                    // それらを数値として比較します。例えば "2" と "10" を比較する場合、
                    // "2" は "10" より小さいと正しく判断されます。
                    // これが自然順ソートの核となるロジックです。
                    var leftStartPosition = leftReader.Position;
                    while (!leftReader.IsEndOfText && IsCurrentGraphemeDigit(leftReader))
                    {
                        leftReader.Advance();
                    }
                    var leftChunk = leftReader.Slice(leftStartPosition, leftReader.Position - leftStartPosition);

                    var rightStartPosition = rightReader.Position;
                    while (!rightReader.IsEndOfText && IsCurrentGraphemeDigit(rightReader))
                    {
                        rightReader.Advance();
                    }
                    var rightChunk = rightReader.Slice(rightStartPosition, rightReader.Position - rightStartPosition);

                    var result = CompareNumeric(leftChunk, rightChunk);
                    if (result != 0) return result;
                }
                else if (!isLeftDigit && !isRightDigit)
                {
                    // シナリオ2: 両方の現在位置が非数字。
                    // ここでの課題は、"file.txt" と "file1.txt" のようなケースを正しく扱うことです。
                    // もし単純に非数値チャンク全体（この例では "file.txt" と "file"）を比較すると、
                    // "file" が小さいと判断され、"file1.txt" が "file.txt" の前に来てしまい、誤った順序になります。
                    //
                    // この問題を回避するため、ここでは両方の非数値チャンクの「最短の長さ」ぶんだけを比較します。
                    // "file.txt" と "file1.txt" の例では、"file" 同士を比較し、結果は等価（0）になります。
                    // このブロックの処理でリーダーが共通部分の末尾に進むため、ループの次のイテレーションでは
                    // 状況が「非数字 vs 数字」（".txt" vs "1.txt"）に変わり、そこで最終的な順序が決定されます。
                    var leftStartPosition = leftReader.Position;
                    while (!leftReader.IsEndOfText && !IsCurrentGraphemeDigit(leftReader))
                    {
                        leftReader.Advance();
                    }
                    var leftChunkGraphemeCount = leftReader.Position - leftStartPosition;

                    var rightStartPosition = rightReader.Position;
                    while (!rightReader.IsEndOfText && !IsCurrentGraphemeDigit(rightReader))
                    {
                        rightReader.Advance();
                    }
                    var rightChunkGraphemeCount = rightReader.Position - rightStartPosition;

                    var minGraphemeCount = Math.Min(leftChunkGraphemeCount, rightChunkGraphemeCount);

                    var leftCommonPrefix = leftReader.Slice(leftStartPosition, minGraphemeCount);
                    var rightCommonPrefix = rightReader.Slice(rightStartPosition, minGraphemeCount);
                    var result = _baseComparer.Compare(leftCommonPrefix.ToString(), rightCommonPrefix.ToString());
                    if (result != 0) return result;

                    // 共通接頭辞が等しい場合、リーダーの位置を共通部分の末尾まで進めて、
                    // 次のチャンクの比較を継続できるようにします。
                    leftReader.Position = leftStartPosition + minGraphemeCount;
                    rightReader.Position = rightStartPosition + minGraphemeCount;
                }
                else
                {
                    // シナリオ3: 一方が数字で、もう一方が非数字。
                    // この時点で、2つの文字列は根本的に異なる構造を持つことが確定します。
                    // 例えば "a1" と "aa" を比較する場合、最初の 'a' は共通ですが、
                    // 次に '1' と 'a' が比較されます。
                    //
                    // このような場合、残りの部分文字列全体を単純に比較するのが最も安全で確実です。
                    // これにより、Unicodeの将来のバージョンで複数のコードポイントから成る新しい数字が導入されたとしても、
                    // アルゴリズムの堅牢性が保たれます。
                    var leftRemainder = leftReader.Slice(leftReader.Position, leftReader.Count - leftReader.Position);
                    var rightRemainder = rightReader.Slice(rightReader.Position, rightReader.Count - rightReader.Position);
                    return _baseComparer.Compare(leftRemainder.ToString(), rightRemainder.ToString());
                }
            }

            // 一方の文字列がもう一方の末尾に到達した場合、短い方が小さいと見なされます。
            return leftReader.IsEndOfText == rightReader.IsEndOfText ? 0 : (leftReader.IsEndOfText ? -1 : 1);
        }

        /// <summary>
        /// 2 つの文字スパンが自然順で等しいかどうかを判定します。
        /// </summary>
        /// <remarks>
        /// 注意: このメソッドは内部的にメモリ割り当てを行います。詳細は <see cref="Compare(ReadOnlySpan{char}, ReadOnlySpan{char})"/> を参照してください。
        /// </remarks>
        /// <param name="left">比較する最初の文字スパン。</param>
        /// <param name="right">比較する 2 番目の文字スパン。</param>
        /// <returns>文字スパンが等しい場合はtrue、それ以外の場合はfalse。</returns>
        public bool Equals(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
        {
            return Compare(left, right) == 0;
        }

        /// <summary>
        /// 指定した文字スパンのハッシュコードを返します。
        /// </summary>
        /// <remarks>
        /// パフォーマンスに関する注意:
        /// このメソッドは内部的にメモリ割り当てを行います。詳細は <see cref="Compare(ReadOnlySpan{char}, ReadOnlySpan{char})"/> を参照してください。
        ///
        /// このハッシュコード計算は、Compareメソッドのロジックと一貫性があります。
        /// 文字列を同じように数値チャンクと非数値チャンクに分割し、各チャンクのハッシュ値を計算して結合します。
        /// これにより、Compareが0（等しい）を返す2つの文字列は、同じハッシュコードを持つことが保証されます。
        /// </remarks>
        /// <param name="text">ハッシュコードを取得する対象の文字スパン。</param>
        /// <returns>指定した文字スパンのハッシュコード。</returns>
        public int GetHashCode(ReadOnlySpan<char> text)
        {
            var hashCode = new HashCode();
            var reader = new GraphemeReader(text);

            while (!reader.IsEndOfText)
            {
                var isDigit = IsCurrentGraphemeDigit(reader);
                var startPosition = reader.Position;

                while (!reader.IsEndOfText && IsCurrentGraphemeDigit(reader) == isDigit)
                {
                    reader.Advance();
                }
                var chunk = reader.Slice(startPosition, reader.Position - startPosition);

                if (isDigit)
                {
                    // 数値チャンクの場合、先行ゼロをトリムして数値としてハッシュ値を追加します。
                    // これにより "01" と "1" が同じハッシュ値を持つようになります。
                    var spanToHash = _normalize ? Normalize(chunk) : chunk;
                    var trimmedSpan = spanToHash.TrimStart('0');
                    if (long.TryParse(trimmedSpan, out var numericValue))
                    {
                        hashCode.Add(numericValue);
                    }
                    else
                    {
                        // longに収まらない巨大な数の場合は文字列としてハッシュコードを計算します。
                        hashCode.Add(trimmedSpan.ToString());
                    }
                }
                else
                {
                    // 非数値チャンクの場合、基本コンパレータを使用してハッシュ値を追加します。
                    hashCode.Add(chunk.ToString(), _baseComparer);
                }
            }
            return hashCode.ToHashCode();
        }

        /// <summary>
        /// 書記素クラスタが数字として扱われるべきかどうかを判断します。
        /// </summary>
        private bool IsCurrentGraphemeDigit(GraphemeReader reader)
        {
            if (reader.IsEndOfText)
            {
                return false;
            }

            // 自然順ソートの目的では、書記素クラスタの最初の文字が数字であれば、
            // そのクラスタ全体を「数字」として扱います。
            // 複数の文字から成る書記素クラスタ（例：絵文字）が数字として解釈されることは
            // ほとんどないため、このヒューリスティックは効率的かつ実用的です。
            var grapheme = reader.PeekAsSpan();
            if (grapheme.IsEmpty) return false;

            char firstChar = grapheme[0];

            // ASCII数字は常に数字として扱われます。
            if (firstChar >= '0' && firstChar <= '9')
            {
                return true;
            }

            // 正規化が有効な場合にのみ、全角数字を数字として扱います。
            if (_normalize && firstChar >= '０' && firstChar <= '９')
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 文字列の数値部分を自然順で比較します。
        /// </summary>
        /// <remarks>
        /// このメソッドは、2つの文字列スパンを数値として比較します。
        /// 例：「2」は「10」より小さい。
        ///
        /// 先行するゼロは無視されます（例：「01」と「1」は等しい）。
        /// まず有効な桁数で比較し、桁数が同じ場合は辞書順で比較します。
        /// </remarks>
        private int CompareNumeric(ReadOnlySpan<char> leftNumeric, ReadOnlySpan<char> rightNumeric)
        {
            ReadOnlySpan<char> leftToCompare = leftNumeric;
            ReadOnlySpan<char> rightToCompare = rightNumeric;

            if (_normalize)
            {
                leftToCompare = Normalize(leftNumeric);
                rightToCompare = Normalize(rightNumeric);
            }

            var trimmedLeft = leftToCompare.TrimStart('0');
            var trimmedRight = rightToCompare.TrimStart('0');

            if (trimmedLeft.Length != trimmedRight.Length)
            {
                return trimmedLeft.Length.CompareTo(trimmedRight.Length);
            }

            return trimmedLeft.SequenceCompareTo(trimmedRight);
        }

        /// <summary>
        /// 数字スパンを正規化します（例：全角を半角に）。
        /// </summary>
        /// <remarks>
        /// このメソッドは、パフォーマンスのために、全角数字が含まれている場合にのみ新しいメモリ割り当てを行います。
        /// </remarks>
        private static ReadOnlySpan<char> Normalize(ReadOnlySpan<char> span)
        {
            bool hasFullWidth = false;
            for (int charIndex = 0; charIndex < span.Length; charIndex++)
            {
                if (span[charIndex] >= '０' && span[charIndex] <= '９')
                {
                    hasFullWidth = true;
                    break;
                }
            }

            if (!hasFullWidth)
            {
                return span;
            }

            var buffer = new char[span.Length];
            for (int charIndex = 0; charIndex < span.Length; charIndex++)
            {
                char currentChar = span[charIndex];
                if (currentChar >= '０' && currentChar <= '９')
                {
                    buffer[charIndex] = (char)('0' + (currentChar - '０'));
                }
                else
                {
                    buffer[charIndex] = currentChar;
                }
            }
            return buffer;
        }
    }
}
