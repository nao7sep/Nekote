using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat
{
    /// <summary>
    /// OpenAI の Chat Completions API を使用してチャット補完を実行します。
    /// </summary>
    internal sealed class OpenAiChatRepository : IChatCompletionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAiChatRepository> _logger;
        private readonly OpenAiConfiguration _config;

        public OpenAiChatRepository(
            IHttpClientFactory httpClientFactory,
            ILogger<OpenAiChatRepository> logger,
            IOptions<OpenAiConfiguration> configuration)
        {
            _logger = logger;
            _config = configuration.Value;

            // API キーをフォールバックチェーンで解決
            var apiKey = _config.ChatApiKey ?? _config.DefaultApiKey
                ?? throw new InvalidOperationException(
                    "OpenAI API key is not configured. Provide either 'DefaultApiKey' or 'ChatApiKey'.");

            // エンドポイントをフォールバックチェーンで解決
            var endpoint = _config.ChatEndpoint
                ?? (_config.BaseUrl != null ? $"{_config.BaseUrl}/v1/chat/completions" : null)
                ?? "https://api.openai.com/v1/chat/completions";

            _httpClient = httpClientFactory.CreateClient("OpenAI-Chat");
            _httpClient.BaseAddress = new Uri(endpoint);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<ChatResponse> GetCompletionAsync(
            IReadOnlyList<ChatMessage> messages,
            ChatCompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // モデル名をフォールバックチェーンで解決
            var modelName = options?.ModelName ?? _config.ChatModelName ?? _config.DefaultModelName
                ?? throw new InvalidOperationException(
                    "OpenAI model name is not configured. Provide 'DefaultModelName' or 'ChatModelName'.");

            // ドメインモデルをプロバイダー固有の DTO に変換
            var requestDto = OpenAiChatMapper.ToRequestDto(messages, options, modelName);

            // API 呼び出しのためにリクエストをシリアライズ
            var json = JsonSerializer.Serialize(requestDto, JsonDefaults.FormattedOptions);
            _logger.LogDebug("Sending chat completion request to OpenAI: {Json}", json);

            // プロバイダーエンドポイントに HTTP リクエストを送信
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken)
                .ConfigureAwait(false);

            // レスポンスボディを読み取り
            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            // HTTP エラーをチェック
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenAI API returned error: {StatusCode} - {Body}",
                    (int)response.StatusCode,
                    responseJson);

                OpenAiErrorHelper.ThrowApiException(response.StatusCode, responseJson);
            }

            // API レスポンスをデシリアライズ
            var responseDto = JsonSerializer.Deserialize<OpenAiChatResponseDto>(
                responseJson,
                JsonDefaults.Options)
                ?? throw new InvalidOperationException("Failed to deserialize OpenAI response.");

            // DTO をドメインモデルに変換
            return OpenAiChatMapper.ToDomainModel(responseDto);
        }

        public IAsyncEnumerable<ChatStreamChunk> GetCompletionStreamAsync(
            IReadOnlyList<ChatMessage> messages,
            ChatCompletionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // ストリーミングは Phase 5 で実装
            throw new NotImplementedException(
                "Streaming is not implemented yet. Use GetCompletionAsync for now.");
        }
    }
}
