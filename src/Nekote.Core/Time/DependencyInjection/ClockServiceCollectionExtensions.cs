using Microsoft.Extensions.DependencyInjection;

namespace Nekote.Core.Time.DependencyInjection
{
    /// <summary>
    /// IServiceCollection に IClock 関連のサービスを登録するための拡張メソッドを提供します。
    /// </summary>
    public static class ClockServiceCollectionExtensions
    {
        /// <summary>
        /// IClock サービスをシングルトンとして DI コンテナに登録します。
        /// SystemClockの実装は、DateTime.NowやDateTimeOffset.Nowなどの静的プロパティに依存しており、
        /// これらはスレッドセーフであるため、SystemClock自体もスレッドセーフです。
        /// そのため、シングルトンとして登録することで、不要なインスタンス化を避け、効率性を高めます。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <returns>チェイン用のサービスコレクション。</returns>
        public static IServiceCollection AddSystemClock(this IServiceCollection services)
        {
            services.AddSingleton<IClock, SystemClock>();
            return services;
        }
    }
}
