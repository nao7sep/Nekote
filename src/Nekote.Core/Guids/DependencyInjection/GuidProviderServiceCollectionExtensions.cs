using Microsoft.Extensions.DependencyInjection;

namespace Nekote.Core.Guids.DependencyInjection
{
    /// <summary>
    /// IServiceCollection に IGuidProvider 関連のサービスを登録するための拡張メソッドを提供します。
    /// </summary>
    public static class GuidProviderServiceCollectionExtensions
    {
        /// <summary>
        /// IGuidProvider サービスをシングルトンとして DI コンテナに登録します。
        /// SystemGuidProviderの実装は、Guid.NewGuid()に依存しており、このメソッドはスレッドセーフです。
        /// そのため、SystemGuidProvider自体もスレッドセーフであり、シングルトンとして登録することで効率性を高めます。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <returns>チェイン用のサービスコレクション。</returns>
        public static IServiceCollection AddSystemGuidProvider(this IServiceCollection services)
        {
            services.AddSingleton<IGuidProvider, SystemGuidProvider>();
            return services;
        }
    }
}
