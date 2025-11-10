using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nekote.Core.AI.Domain.Chat;
using Nekote.Core.AI.Infrastructure.OpenAI;
using Nekote.Core.AI.Infrastructure.OpenAI.Chat;

namespace Nekote.Core.AI.Infrastructure.DependencyInjection
{
    /// <summary>
    /// OpenAI サービスを DI コンテナに登録するための拡張メソッドを提供します。
    /// </summary>
    public static class OpenAiServiceCollectionExtensions
    {
        /// <summary>
        /// OpenAI Chat サービスを DI コンテナに登録します。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <param name="configuration">設定オブジェクト。</param>
        /// <param name="configurationSectionPath">
        /// 設定セクションのパス。デフォルトは "AI:OpenAI"。
        /// </param>
        /// <returns>サービスコレクション (メソッドチェーン用)。</returns>
        public static IServiceCollection AddOpenAiChat(
            this IServiceCollection services,
            IConfiguration configuration,
            string configurationSectionPath = "AI:OpenAI")
        {
            // 設定をバインド
            services.Configure<OpenAiConfiguration>(
                configuration.GetSection(configurationSectionPath));

            // HttpClient を登録 (名前付き)
            services.AddHttpClient("OpenAI-Chat");

            // Chat サービスを登録
            services.AddTransient<IChatCompletionService, OpenAiChatRepository>();

            return services;
        }
    }
}
