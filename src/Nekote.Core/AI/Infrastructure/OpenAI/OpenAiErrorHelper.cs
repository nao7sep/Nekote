using System.Net;
using System.Text.Json;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI
{
    /// <summary>
    /// OpenAI API エラーハンドリングのヘルパーメソッドを提供します。
    /// </summary>
    internal static class OpenAiErrorHelper
    {
        /// <summary>
        /// OpenAI API のエラーレスポンスをデシリアライズして例外をスローします。
        /// </summary>
        /// <param name="statusCode">HTTP ステータスコード。</param>
        /// <param name="responseBody">レスポンスボディの JSON 文字列。</param>
        /// <exception cref="AiProviderApiException">常にスローされます。</exception>
        public static void ThrowApiException(HttpStatusCode statusCode, string responseBody)
        {
            OpenAiErrorResponseDto? errorResponse = null;

            try
            {
                errorResponse = JsonSerializer.Deserialize<OpenAiErrorResponseDto>(
                    responseBody,
                    JsonDefaults.Options);
            }
            catch
            {
                // デシリアライズに失敗した場合は生のレスポンスを使用
            }

            if (errorResponse?.Error != null)
            {
                throw new AiProviderApiException(
                    statusCode: statusCode,
                    providerErrorType: errorResponse.Error.Type,
                    providerErrorCode: errorResponse.Error.Code,
                    providerErrorMessage: errorResponse.Error.Message,
                    errorParam: errorResponse.Error.Param,
                    responseBody: responseBody);
            }
            else
            {
                throw new AiProviderApiException(
                    statusCode: statusCode,
                    responseBody: responseBody);
            }
        }
    }
}
