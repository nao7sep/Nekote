using Microsoft.Extensions.DependencyInjection;

namespace Nekote.Core.Randomization.DependencyInjection
{
    /// <summary>
    /// IServiceCollection に IRandomProvider 関連のサービスを登録するための拡張メソッドを提供します。
    ///
    /// System.Random はスレッドセーフではないため、このライブラリの他のプロバイダー（SystemClock, SystemGuidProvider）と
    /// 同様にシングルトンとして安全に登録するためには、スレッドセーフなラッパーが必要です。
    /// SystemRandomProvider は、内部でロックを使用してスレッドセーフ性を保証します。
    ///
    /// .NET 6以降で利用可能な Random.Shared はスレッドセーフですが、シード値を指定できないという制約があります。
    /// テストの決定性を確保するためにはシード値の指定が不可欠であるため、このライブラリでは Random.Shared を直接使用せず、
    /// シード可能な独自のスレッドセーフな実装を提供します。これにより、アプリケーションの要求とテストの要求の両方を満たします。
    /// </summary>
    public static class RandomProviderServiceCollectionExtensions
    {
        /// <summary>
        /// スレッドセーフな IRandomProvider サービスをシングルトンとして DI コンテナに登録します。
        /// このオーバーロードは、時間に依存したデフォルトのシードを使用します。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <returns>チェイン用のサービスコレクション。</returns>
        public static IServiceCollection AddSystemRandomProvider(this IServiceCollection services)
        {
            services.AddSingleton<IRandomProvider, SystemRandomProvider>();
            return services;
        }

        /// <summary>
        /// 指定されたシードを使用して、スレッドセーフな IRandomProvider サービスをシングルトンとして DI コンテナに登録します。
        /// これにより、決定論的な乱数系列が生成されるため、テストに役立ちます。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <param name="seed">擬似乱数系列の開始値を計算するために使用する数値。</param>
        /// <returns>チェイン用のサービスコレクション。</returns>
        public static IServiceCollection AddSystemRandomProvider(this IServiceCollection services, int seed)
        {
            services.AddSingleton<IRandomProvider>(sp => new SystemRandomProvider(seed));
            return services;
        }
    }
}
