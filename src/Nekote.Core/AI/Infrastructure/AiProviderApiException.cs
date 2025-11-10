using System.Net;
using System.Text;
using Nekote.Core.Text;

namespace Nekote.Core.AI.Infrastructure
{
    /// <summary>
    /// AI プロバイダー API からのエラーレスポンスを表す例外。
    /// </summary>
    public sealed class AiProviderApiException : Exception
    {
        /// <summary>
        /// HTTP ステータスコードを取得します。
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// プロバイダー固有のエラータイプを取得します。
        /// </summary>
        public string? ProviderErrorType { get; }

        /// <summary>
        /// プロバイダー固有のエラーコードを取得します。
        /// </summary>
        public string? ProviderErrorCode { get; }

        /// <summary>
        /// プロバイダーが返したエラーメッセージを取得します。
        /// </summary>
        public string? ProviderErrorMessage { get; }

        /// <summary>
        /// エラーが発生したパラメータ名を取得します。
        /// </summary>
        public string? ErrorParam { get; }

        /// <summary>
        /// 生のレスポンスボディを取得します。
        /// </summary>
        public string? ResponseBody { get; }

        /// <summary>
        /// AiProviderApiException の新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="statusCode">HTTP ステータスコード。</param>
        /// <param name="providerErrorType">プロバイダー固有のエラータイプ。</param>
        /// <param name="providerErrorCode">プロバイダー固有のエラーコード。</param>
        /// <param name="providerErrorMessage">プロバイダーが返したエラーメッセージ。</param>
        /// <param name="errorParam">エラーが発生したパラメータ名。</param>
        /// <param name="responseBody">生のレスポンスボディ。</param>
        public AiProviderApiException(
            HttpStatusCode statusCode,
            string? providerErrorType = null,
            string? providerErrorCode = null,
            string? providerErrorMessage = null,
            string? errorParam = null,
            string? responseBody = null)
            : base(BuildMessage(statusCode, providerErrorType, providerErrorCode, providerErrorMessage, errorParam))
        {
            StatusCode = statusCode;
            ProviderErrorType = providerErrorType;
            ProviderErrorCode = providerErrorCode;
            ProviderErrorMessage = providerErrorMessage;
            ErrorParam = errorParam;
            ResponseBody = responseBody;
        }

        /// <summary>
        /// 例外の文字列表現を取得します。
        /// </summary>
        /// <returns>すべてのエラー情報を含む文字列。</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{GetType().FullName}: {Message}");

            builder.AppendLineIfNotEmpty(key: "Provider Error Type", value: ProviderErrorType);
            builder.AppendLineIfNotEmpty(key: "Provider Error Code", value: ProviderErrorCode);
            builder.AppendLineIfNotEmpty(key: "Provider Error Message", value: ProviderErrorMessage);
            builder.AppendLineIfNotEmpty(key: "Error Parameter", value: ErrorParam);

            builder.AppendLine($"  HTTP Status Code: {(int)StatusCode} ({StatusCode})");

            if (!string.IsNullOrWhiteSpace(ResponseBody))
            {
                builder.AppendLine("  Response Body:");
                builder.AppendLine($"    {ResponseBody}");
            }

            if (StackTrace != null)
            {
                builder.AppendLine("  Stack Trace:");
                builder.AppendLine(StackTrace);
            }

            return builder.ToString();
        }

        private static string BuildMessage(
            HttpStatusCode statusCode,
            string? providerErrorType,
            string? providerErrorCode,
            string? providerErrorMessage,
            string? errorParam)
        {
            var builder = new StringBuilder();
            builder.Append("AI provider API request failed with status");
            builder.Append(": ");
            builder.Append($"{(int)statusCode} ({statusCode})");

            builder.AppendIfNotEmpty(key: "Type", value: providerErrorType);
            builder.AppendIfNotEmpty(key: "Code", value: providerErrorCode);
            builder.AppendIfNotEmpty(key: "Message", value: providerErrorMessage);
            builder.AppendIfNotEmpty(key: "Param", value: errorParam);

            builder.RemoveTrailingSuffix();

            return builder.ToString();
        }
    }
}
