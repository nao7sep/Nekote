using System.Net;
using System.Text;

namespace Nekote.Core.AI.Infrastructure
{
    /// <summary>
    /// AI プロバイダー API からのエラーレスポンスを表す例外。
    /// </summary>
    public sealed class AiProviderApiException : Exception
    {
        private readonly Lazy<string> _lazyToString;

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
        /// AiProviderApiException の新しいインスタンスを初期化します。
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
            var builder = new StringBuilder();
            builder.AppendLine($"{GetType().FullName}: {Message}");

            builder.AppendLine($"  HTTP Status Code: {(int)StatusCode} ({StatusCode})");

            // Exception.Data は ListDictionary によってバックアップされており、
            // 挿入順序を保持します。これは、スローする側が追加した順序で
            // キーと値のペアが表示されることを意味します。
            if (Data.Count > 0)
            {
                builder.AppendLine("  Data:");
                foreach (var key in Data.Keys)
                {
                    builder.AppendLine($"    {key}: {Data[key]}");
                }
            }

            if (SourceData != null)
            {
                builder.AppendLine($"  Source Data Type: {SourceData.GetType().FullName}");
            }

            if (StackTrace != null)
            {
                builder.AppendLine("  Stack Trace:");
                builder.AppendLine(StackTrace);
            }

            return builder.ToString();
        }
    }
}
