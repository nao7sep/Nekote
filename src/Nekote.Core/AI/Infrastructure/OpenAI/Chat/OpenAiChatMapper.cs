using System.Text.Json;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat.Dtos;

namespace Nekote.Core.AI.Infrastructure.OpenAI.Chat
{
    /// <summary>
    /// OpenAI の DTO とドメインモデル間の変換を行います。
    /// </summary>
    internal static class OpenAiChatMapper
    {
        /// <summary>
        /// ドメインモデルを OpenAI API のリクエスト DTO に変換します。
        /// </summary>
        /// <param name="messages">チャットメッセージのリスト。</param>
        /// <param name="options">オプション設定。</param>
        /// <param name="modelName">使用するモデル名。</param>
        /// <returns>OpenAI API リクエスト DTO。</returns>
        /// <exception cref="ArgumentException">モデル名が null または空白の場合。</exception>
        public static OpenAiChatRequestDto ToRequestDto(
            IReadOnlyList<ChatMessage> messages,
            ChatCompletionOptions? options,
            string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName))
            {
                throw new ArgumentException("Model name cannot be null or empty.", nameof(modelName));
            }

            var requestDto = new OpenAiChatRequestDto
            {
                Model = modelName,
                Messages = messages.Select(m => new OpenAiMessageDto
                {
                    Role = MapRole(m.Role),
                    Content = m.Content,
                    Name = m.Name
                }).ToList(),
                Temperature = options?.Temperature,
                MaxTokens = options?.MaxTokens,
                TopP = options?.TopP,
                Stream = false
            };

            // プロバイダー固有のパラメータを ExtensionData に追加
            if (options?.ProviderSpecificParameters != null && options.ProviderSpecificParameters.Count > 0)
            {
                requestDto.ExtensionData = options.ProviderSpecificParameters
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => JsonSerializer.SerializeToElement(kvp.Value));
            }

            return requestDto;
        }

        /// <summary>
        /// OpenAI API のレスポンス DTO をドメインモデルに変換します。
        /// </summary>
        /// <param name="dto">OpenAI API レスポンス DTO。</param>
        /// <returns>ドメインモデルの ChatResponse。</returns>
        /// <exception cref="InvalidOperationException">レスポンスが不正な形式の場合。</exception>
        public static ChatResponse ToDomainModel(OpenAiChatResponseDto dto)
        {
            if (dto.Choices == null || dto.Choices.Count == 0)
            {
                throw new InvalidOperationException(
                    "OpenAI response is malformed: 'choices' array is null or empty.");
            }

            var choices = new List<ChatChoice>();

            foreach (var choice in dto.Choices)
            {
                if (choice.Index == null)
                {
                    throw new InvalidOperationException(
                        "OpenAI response is malformed: 'index' is null in choice.");
                }

                if (choice.Message == null)
                {
                    throw new InvalidOperationException(
                        $"OpenAI response is malformed: 'message' object is null at choice index {choice.Index}.");
                }

                if (string.IsNullOrWhiteSpace(choice.Message.Content))
                {
                    throw new InvalidOperationException(
                        $"OpenAI response is malformed: 'content' is null or empty at choice index {choice.Index}.");
                }

                if (string.IsNullOrWhiteSpace(choice.FinishReason))
                {
                    throw new InvalidOperationException(
                        $"OpenAI response is malformed: 'finish_reason' is null or empty at choice index {choice.Index}.");
                }

                choices.Add(new ChatChoice
                {
                    Index = choice.Index.Value,
                    Content = choice.Message.Content,
                    FinishReason = choice.FinishReason
                });
            }

            return new ChatResponse
            {
                Choices = choices,
                Usage = dto.Usage != null ? new TokenUsage
                {
                    PromptTokens = dto.Usage.PromptTokens ?? 0,
                    CompletionTokens = dto.Usage.CompletionTokens ?? 0,
                    TotalTokens = dto.Usage.TotalTokens ?? 0
                } : null,
                ResponseId = dto.Id
            };
        }

        /// <summary>
        /// ドメインの <see cref="ChatRole"/> を OpenAI の文字列表現にマッピングします。
        /// </summary>
        /// <param name="role">ドメインの ChatRole。</param>
        /// <returns>OpenAI API の role 文字列。</returns>
        /// <exception cref="ArgumentException">サポートされていない役割の場合。</exception>
        private static string MapRole(ChatRole role)
        {
            return role switch
            {
                ChatRole.System => "system",
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                _ => throw new ArgumentException($"Unsupported chat role: {role}", nameof(role))
            };
        }
    }
}
