using System.Text;
using Nekote.Core.Operations;
using Nekote.Core.Text.Processing;

namespace Nekote.Core.Text
{
    /// <summary>
    /// セグメント間の区切りを自動管理する文字列ビルダー。
    /// </summary>
    /// <remarks>
    /// このクラスは、複数のセグメント (テキストのブロック) を構築する際に、
    /// セグメント間に自動的に区切り (例: 空行) を挿入する機能を提供します。
    /// 各セグメントは、次のセグメントが追加される場合にのみ区切りを挿入するため、
    /// 最後のセグメントの後には不要な区切りが残りません。
    /// また、登録されたアクションを通じて、テキストをリアルタイムで出力することもできます。
    /// </remarks>
    public class SegmentedStringBuilder
    {
        private readonly StringBuilder _stringBuilder;
        private readonly List<Action<string>> _outputActions;
        private readonly LineReaderConfiguration _lineReaderConfiguration;
        private readonly KeyValueFormatter _keyValueFormatter;
        private readonly bool _omitKeyValuePairIfValueIsEmpty;
        private readonly ActionErrorHandling _errorHandling;
        private string? _pendingSeparator;

        /// <summary>
        /// SegmentedStringBuilder の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="stringBuilder">使用する StringBuilder。null の場合、新しいインスタンスが作成されます。</param>
        /// <param name="lineReaderConfiguration">複数行テキストを処理する際に使用する <see cref="LineReaderConfiguration"/>。</param>
        /// <param name="keyValueFormatter">キーと値のペアをフォーマットする際に使用する <see cref="KeyValueFormatter"/>。null の場合、<see cref="KeyValueFormatter.Default"/> が使用されます。</param>
        /// <param name="omitKeyValuePairIfValueIsEmpty">true の場合、<see cref="AppendKeyValuePair"/> で値が空のときに行全体を省略します。デフォルトは false です。</param>
        /// <param name="errorHandling">出力アクション実行時のエラー処理方法。</param>
        public SegmentedStringBuilder(
            StringBuilder? stringBuilder = null,
            LineReaderConfiguration lineReaderConfiguration = LineReaderConfiguration.Default,
            KeyValueFormatter? keyValueFormatter = null,
            bool omitKeyValuePairIfValueIsEmpty = false,
            ActionErrorHandling errorHandling = ActionErrorHandling.StopOnFirstException)
        {
            _stringBuilder = stringBuilder ?? new StringBuilder();
            _outputActions = new List<Action<string>>();
            _lineReaderConfiguration = lineReaderConfiguration;
            _keyValueFormatter = keyValueFormatter ?? KeyValueFormatter.Default;
            _omitKeyValuePairIfValueIsEmpty = omitKeyValuePairIfValueIsEmpty;
            _errorHandling = errorHandling;
            _pendingSeparator = null;
        }

        /// <summary>
        /// 内部の StringBuilder を取得します。
        /// </summary>
        /// <remarks>
        /// このプロパティは、内部の <see cref="StringBuilder"/> に直接アクセスします。
        /// このインスタンスを介した操作は、保留中の区切りに影響せず、出力アクションも呼び出しません。
        /// このクラスの機能をバイパスする場合は、注意して使用してください。
        /// </remarks>
        public StringBuilder StringBuilder => _stringBuilder;

        /// <summary>
        /// 複数行テキストを処理する際に使用される <see cref="LineReaderConfiguration"/> を取得します。
        /// </summary>
        public LineReaderConfiguration LineReaderConfiguration => _lineReaderConfiguration;

        /// <summary>
        /// キーと値のペアをフォーマットする際に使用される <see cref="KeyValueFormatter"/> を取得します。
        /// </summary>
        public KeyValueFormatter KeyValueFormatter => _keyValueFormatter;

        /// <summary>
        /// キーと値のペアを追加する際に、値が空の場合に行全体を省略するかどうかを取得します。
        /// </summary>
        public bool OmitKeyValuePairIfValueIsEmpty => _omitKeyValuePairIfValueIsEmpty;

        /// <summary>
        /// 現在のバッファー容量を取得します。
        /// </summary>
        public int Capacity => _stringBuilder.Capacity;

        /// <summary>
        /// 現在の文字列の長さを取得します。
        /// </summary>
        public int Length => _stringBuilder.Length;

        /// <summary>
        /// この StringBuilder が拡張できる最大容量を取得します。
        /// </summary>
        /// <remarks>
        /// この値は StringBuilder の作成時に決定され、変更できません。
        /// デフォルトでは Int32.MaxValue です。
        /// </remarks>
        public int MaxCapacity => _stringBuilder.MaxCapacity;

        /// <summary>
        /// 指定した文字位置にある文字を取得します。
        /// </summary>
        /// <param name="index">文字の位置。インデックスは 0 から始まります。</param>
        /// <returns>位置 <paramref name="index"/> にある Unicode 文字。</returns>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> がこのインスタンスの範囲外です。
        /// </exception>
        /// <remarks>
        /// このプロパティは、内部の <see cref="StringBuilder"/> に直接アクセスします。
        /// 文字の取得は、保留中の区切りに影響しません。
        /// また、出力アクションも呼び出されません。
        /// </remarks>
        public char this[int index]
        {
            get => _stringBuilder[index];
        }

        /// <summary>
        /// 出力アクション実行時のエラー処理方法を取得します。
        /// </summary>
        public ActionErrorHandling ErrorHandling => _errorHandling;

        /// <summary>
        /// 出力アクションを登録します。
        /// </summary>
        /// <param name="action">テキストが追加されたときに呼び出されるアクション。</param>
        /// <remarks>
        /// 登録されたアクションは、テキストが追加されるたびに呼び出されます。
        /// 例えば、Console.Write を登録することで、リアルタイムでコンソールに出力できます。
        /// </remarks>
        public void RegisterOutputAction(Action<string> action)
        {
            _outputActions.Add(action);
        }

        /// <summary>
        /// 保留中の区切り文字列が存在するかどうかを取得します。
        /// </summary>
        /// <remarks>
        /// 空文字列は区切りとして意味を持たないため、null または空文字列の場合は false を返します。
        /// </remarks>
        public bool HasPendingSeparator => !string.IsNullOrEmpty(_pendingSeparator);

        /// <summary>
        /// 次のセグメントの前に挿入される区切り文字列を設定します。
        /// </summary>
        /// <param name="separator">区切り文字列。</param>
        /// <remarks>
        /// この区切りは、次にテキストが追加されるときにのみ挿入されます。
        /// 次のテキストが追加されない場合、区切りは挿入されません。
        /// </remarks>
        public void SetPendingSeparator(string separator)
        {
            _pendingSeparator = separator;
        }

        /// <summary>
        /// 次のセグメントの前に空行を挿入するように設定します。
        /// </summary>
        public void SetPendingEmptyLine()
        {
            _pendingSeparator = Environment.NewLine;
        }

        /// <summary>
        /// 保留中の区切り文字列をクリアします。
        /// </summary>
        public void ClearPendingSeparator()
        {
            _pendingSeparator = null;
        }

        /// <summary>
        /// 保留中の区切り文字列を即座に出力します。
        /// </summary>
        /// <remarks>
        /// このメソッドは、保留中の区切りを出力した後、
        /// StringBuilder の組み込みメソッドを直接使用したい場合に便利です。
        /// 区切りが存在しない場合、このメソッドは何も行いません。
        /// </remarks>
        public void FlushPendingSeparator()
        {
            if (string.IsNullOrEmpty(_pendingSeparator))
                return;

            _stringBuilder.Append(_pendingSeparator);
            InvokeOutputActions(_pendingSeparator);
            _pendingSeparator = null;
        }

        /// <summary>
        /// すべての文字を削除します。
        /// </summary>
        /// <remarks>
        /// 保留中の区切りもクリアされます。
        /// </remarks>
        public void Clear()
        {
            _stringBuilder.Clear();
            _pendingSeparator = null;
        }

        /// <summary>
        /// このインスタンスの指定したセグメントから、
        /// コピー先の配列の指定した位置に文字をコピーします。
        /// </summary>
        /// <param name="sourceIndex">
        /// このインスタンス内でコピーを開始する位置。インデックスは 0 から始まります。
        /// </param>
        /// <param name="destination">文字のコピー先となる配列。</param>
        /// <param name="destinationIndex">
        /// <paramref name="destination"/> 内でコピーを開始する位置。インデックスは 0 から始まります。
        /// </param>
        /// <param name="count">コピーする文字数。</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="destination"/> が null です。
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="sourceIndex"/>、<paramref name="destinationIndex"/>、
        /// または <paramref name="count"/> が 0 未満です。
        /// または、<paramref name="sourceIndex"/> がこのインスタンスの長さより大きいです。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="sourceIndex"/> + <paramref name="count"/> が
        /// このインスタンスの長さより大きいです。
        /// または、<paramref name="destinationIndex"/> + <paramref name="count"/> が
        /// <paramref name="destination"/> の長さより大きいです。
        /// </exception>
        public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            _stringBuilder.CopyTo(sourceIndex, destination, destinationIndex, count);
        }

        /// <summary>
        /// このインスタンスの指定したセグメントから、
        /// コピー先のスパンに文字をコピーします。
        /// </summary>
        /// <param name="sourceIndex">
        /// このインスタンス内でコピーを開始する位置。インデックスは 0 から始まります。
        /// </param>
        /// <param name="destination">文字のコピー先となる書き込み可能なスパン。</param>
        /// <param name="count">コピーする文字数。</param>
        public void CopyTo(int sourceIndex, Span<char> destination, int count)
        {
            _stringBuilder.CopyTo(sourceIndex, destination, count);
        }

        /// <summary>
        /// 指定された最小容量を確保します。
        /// </summary>
        /// <param name="capacity">確保する最小容量。</param>
        public void EnsureCapacity(int capacity)
        {
            _stringBuilder.EnsureCapacity(capacity);
        }

        /// <summary>
        /// テキストを追加します。
        /// </summary>
        /// <param name="text">追加するテキスト。</param>
        public void Append(string text)
        {
            if (text == null)
                return;

            FlushPendingSeparator();

            _stringBuilder.Append(text);
            InvokeOutputActions(text);
        }

        /// <summary>
        /// テキストを追加し、改行を追加します。
        /// </summary>
        /// <param name="text">追加するテキスト。</param>
        public void AppendLine(string text)
        {
            if (text == null)
            {
                AppendLine();
                return;
            }

            FlushPendingSeparator();

            _stringBuilder.Append(text);
            _stringBuilder.Append(Environment.NewLine);
            InvokeOutputActions(text);
            InvokeOutputActions(Environment.NewLine);
        }

        /// <summary>
        /// 改行を追加します。
        /// </summary>
        public void AppendLine()
        {
            FlushPendingSeparator();

            _stringBuilder.Append(Environment.NewLine);
            InvokeOutputActions(Environment.NewLine);
        }

        /// <summary>
        /// 複数行のテキストを追加します。
        /// オプションで各行に一貫したインデントを追加できます。
        /// </summary>
        /// <param name="text">追加するテキスト。</param>
        /// <param name="indentation">各行の先頭に追加するインデント文字列。null の場合、インデントは追加されません。</param>
        /// <remarks>
        /// このメソッドは、内部で <see cref="LineReader"/> を使用してテキストを行ごとに処理します。
        /// 行の処理方法は、コンストラクターで指定された <see cref="LineReaderConfiguration"/> に基づきます。
        /// 意味のある内容を持つ行には、インデント、行の内容、改行が追加されます。
        /// 空行（意味のある内容がない行）には、改行のみが追加されます。
        /// 注: <see cref="LineReaderConfiguration.Default"/> を使用している場合、
        /// 空白文字のみで構成される文字列（例: " "）は空行として扱われ、
        /// 先頭の空行として無視されるため、何も出力されません。
        /// </remarks>
        public void AppendLines(string text, string? indentation = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            var lineReader = LineReader.Create(_lineReaderConfiguration, text.AsMemory());

            while (lineReader.ReadLine(out var line))
            {
                FlushPendingSeparator();

                // 行に意味のある内容がある場合
                if (!StringHelper.IsWhiteSpace(line))
                {
                    if (indentation != null)
                    {
                        _stringBuilder.Append(indentation);
                    }
                    _stringBuilder.Append(line);
                    _stringBuilder.Append(Environment.NewLine);

                    if (indentation != null)
                    {
                        InvokeOutputActions(indentation);
                    }
                    InvokeOutputActions(line.ToString());
                    InvokeOutputActions(Environment.NewLine);
                }
                else
                {
                    // 空行の場合は改行のみ追加
                    _stringBuilder.Append(Environment.NewLine);
                    InvokeOutputActions(Environment.NewLine);
                }
            }
        }

        /// <summary>
        /// キーと値のペアを1行として追加します。
        /// </summary>
        /// <param name="key">キー。</param>
        /// <param name="value">値。</param>
        /// <param name="indentation">行の先頭に追加するインデント文字列。null の場合、インデントは追加されません。</param>
        /// <param name="suffix">値の後に追加するサフィックス文字列。null の場合、サフィックスは追加されません。</param>
        /// <remarks>
        /// このメソッドは、コンストラクターで指定された <see cref="KeyValueFormatter"/> を使用して
        /// キーと値をフォーマットします。
        /// フォーマッターは、値が空かどうかに応じて適切なセパレーターを自動的に選択します。
        /// コンストラクターで <see cref="OmitKeyValuePairIfValueIsEmpty"/> が true に設定されている場合、
        /// 値が空のときに行全体が省略されます。
        /// false の場合、値が空でも行は常に出力されます。
        /// </remarks>
        public void AppendKeyValuePair(
            string key,
            string? value,
            string? indentation = null,
            string? suffix = null)
        {
            if (_omitKeyValuePairIfValueIsEmpty && string.IsNullOrWhiteSpace(value))
                return;

            FlushPendingSeparator();

            if (indentation != null)
            {
                _stringBuilder.Append(indentation);
            }

            string formattedPair = _keyValueFormatter.Format(key, value);
            _stringBuilder.Append(formattedPair);

            if (suffix != null)
            {
                _stringBuilder.Append(suffix);
            }

            _stringBuilder.Append(Environment.NewLine);

            // 出力アクションを呼び出す
            if (indentation != null)
            {
                InvokeOutputActions(indentation);
            }
            InvokeOutputActions(formattedPair);
            if (suffix != null)
            {
                InvokeOutputActions(suffix);
            }
            InvokeOutputActions(Environment.NewLine);
        }

        /// <summary>
        /// 登録されたすべての出力アクションを呼び出します。
        /// </summary>
        /// <param name="text">アクションに渡すテキスト。</param>
        /// <remarks>
        /// このメソッドは、StringBuilder を直接操作した後に、
        /// 登録されたアクションに手動で通知する必要がある場合に使用します。
        /// 例: stringBuilder.StringBuilder.Append("text"); stringBuilder.InvokeOutputActions("text");
        /// </remarks>
        public void InvokeOutputActions(string text)
        {
            if (_outputActions.Count == 0)
                return;

            switch (_errorHandling)
            {
                case ActionErrorHandling.StopOnFirstException:
                    foreach (var action in _outputActions)
                    {
                        action(text);
                    }
                    break;

                case ActionErrorHandling.CollectAndThrowAll:
                    var exceptions = new List<Exception>();
                    foreach (var action in _outputActions)
                    {
                        try
                        {
                            action(text);
                        }
                        catch (Exception exception)
                        {
                            exceptions.Add(exception);
                        }
                    }
                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException(exceptions);
                    }
                    break;

                case ActionErrorHandling.SuppressAll:
                    foreach (var action in _outputActions)
                    {
                        try
                        {
                            action(text);
                        }
                        catch
                        {
                            // 例外を破棄
                        }
                    }
                    break;

                default:
                    throw new InvalidOperationException($"Unknown error handling mode: {_errorHandling}");
            }
        }

        /// <summary>
        /// 現在の内容を文字列として取得します。
        /// </summary>
        /// <param name="trimEnd">末尾の空白文字を削除するかどうか。</param>
        /// <returns>構築された文字列。</returns>
        /// <remarks>
        /// このクラスは、セグメント区切りと行区切りの両方を保留中の区切りとして扱うと
        /// 複雑になりすぎるため、行区切りは管理しません。
        /// AppendLine の呼び出しによって生成される末尾の改行は、
        /// trimEnd パラメータを true にすることで削除できます。
        /// デフォルトでは false です。
        /// </remarks>
        public string ToString(bool trimEnd)
        {
            if (!trimEnd)
                return _stringBuilder.ToString();

            return _stringBuilder.ToString().TrimEnd();
        }

        /// <summary>
        /// 現在の内容を文字列として取得します。
        /// </summary>
        /// <returns>構築された文字列。</returns>
        public override string ToString()
        {
            return ToString(trimEnd: false);
        }
    }
}
