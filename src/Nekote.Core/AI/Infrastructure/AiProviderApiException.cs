using System.Net;
using Nekote.Core.Text;

namespace Nekote.Core.AI.Infrastructure
{
    /// <summary>
    /// AI プロバイダー API からのエラーレスポンスを表す例外。
    /// </summary>
    public sealed class AiProviderApiException : Exception
    {
        private readonly Lazy<string> _lazyToString;

        /// <summary>
        /// <see cref="AiProviderApiException"/> の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">例外の短いメッセージ。</param>
        /// <param name="statusCode">HTTP ステータスコード。</param>
        /// <param name="sourceData">エラーの原因となったソースデータへの参照。</param>
        public AiProviderApiException(
            string message,
            HttpStatusCode statusCode,
            object? sourceData = null)
            : base(message)
        {
            StatusCode = statusCode;
            SourceData = sourceData;
            _lazyToString = new Lazy<string>(BuildToString);
        }

        /// <summary>
        /// HTTP ステータスコードを取得します。
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// エラーの原因となったソースデータへの参照を取得します。
        /// </summary>
        /// <remarks>
        /// このプロパティは、デバッグやエラー分析のために、エラーが発生した際のオリジナルのデータオブジェクトへの参照を保持します。
        /// </remarks>
        public object? SourceData { get; }

        /// <summary>
        /// 例外の文字列表現を取得します。
        /// </summary>
        /// <returns>すべてのエラー情報を含む文字列。</returns>
        /// <remarks>
        /// この実装は遅延初期化されます。
        /// 文字列表現は最初に要求されたときにのみ構築されます。
        /// </remarks>
        public override string ToString()
        {
            return _lazyToString.Value;
        }

        private string BuildToString()
        {
            var builder = new SegmentedStringBuilder();

            builder.AppendLine($"{GetType().FullName}: {Message}");
            builder.AppendKeyValuePair("Status Code", $"{(int)StatusCode} ({StatusCode})", indentation: "  ");

            // Exception.Data は ListDictionary によってバックアップされています。
            // 公式ドキュメントでは挿入順序の保証はありませんが、実装は単方向リンクリスト構造であり、
            // Add メソッドは last.next に新しいノードを設定し、
            // 列挙子は current.next を参照して次の要素に移動します。
            // このため、実際には挿入順序で列挙されます。
            // 仮に Microsoft がこの古いクラスの実装を変更したとしても、
            // 順序が変わるだけで、このメソッドに致命的な影響はありません。
            // 注: ListDictionary は古い実装であり、より新しい OrderedDictionary などの
            // 代替手段もありますが、少数のキーと値のペアを扱うだけのために
            // 新しいプロパティを追加することは過剰設計です。基底クラスの Data プロパティを使用します。
            if (Data.Count > 0)
            {
                builder.AppendLine("  Data:");

                foreach (var key in Data.Keys)
                {
                    // ListDictionary の型シグネチャではキーは null 許容オブジェクトですが、
                    // Add メソッドは ArgumentNullException.ThrowIfNull(key) を呼び出すため、
                    // null キーは明示的に禁止されています。
                    // したがって、実際には key は常に非 null です。
                    // ただし、ToString のような診断メソッドでは例外をスローすべきではないため、
                    // 防御的にコーディングし、万が一 null キーが存在した場合でも
                    // 空文字列に変換して表示します。
                    var keyString = key?.ToString() ?? string.Empty;

                    // Data.Keys 内のすべてのキーには対応する値が存在します。
                    // null-forgiving 演算子を使用してコンパイラの警告を回避します。
                    builder.AppendKeyValuePair(keyString, Data[key!]?.ToString(), indentation: "    ");
                }
            }

            if (SourceData != null)
            {
                builder.AppendKeyValuePair("Source Data Type", SourceData.GetType().FullName, indentation: "  ");
            }

            if (StackTrace != null)
            {
                builder.AppendLine("  Stack Trace:");
                builder.AppendLines(StackTrace, indentation: "    ");
            }

            // Exception.ToString は末尾に改行を含まない文字列を返すという設計を継承します。
            // これにより、呼び出し側が必要に応じて改行を追加できます。
            return builder.ToString(trimEnd: true);
        }
    }
}
