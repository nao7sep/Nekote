using Microsoft.Extensions.DependencyInjection;

namespace Nekote.Core.Randomization.DependencyInjection
{
    /// <summary>
    /// <see cref="IServiceCollection"/> に <see cref="IRandomProvider"/> 関連のサービスを登録するための拡張メソッドを提供します。
    /// <see cref="SystemRandomProvider"/> は、シード値が指定されていない場合は高性能な <see cref="Random.Shared"/> を使用し、
    /// シード値が指定されている場合は決定論的な乱数列を生成します。
    /// 本番環境では、シード値を指定しないオーバーロードを使用してください。
    /// </summary>
    public static class RandomProviderServiceCollectionExtensions
    {
        /// <summary>
        /// スレッドセーフな <see cref="IRandomProvider"/> サービスをシングルトンとして DI コンテナに登録します。
        /// 内部で <see cref="Random.Shared"/> を使用するため、高性能でロックが不要です。
        /// 本番環境ではこのメソッドを使用してください。
        /// </summary>
        /// <param name="services">サービスコレクション。</param>
        /// <returns>チェイン用のサービスコレクション。</returns>
        public static IServiceCollection AddSystemRandomProvider(this IServiceCollection services)
        {
            services.AddSingleton<IRandomProvider, SystemRandomProvider>();
            return services;
        }

        /// <summary>
        /// 指定されたシードを使用して、スレッドセーフな <see cref="IRandomProvider"/> サービスをシングルトンとして DI コンテナに登録します。
        /// 警告: このメソッドはテスト専用です。本番環境では絶対に使用しないでください。
        /// シード付きシングルトンは、アプリケーション全体が毎回同じ「ランダム」な動作を示し、
        /// 正しい動作が損なわれます。また、ロックによるパフォーマンスの低下も発生します。
        /// テストでは、各テストケース内で個別にインスタンスを作成することを推奨します。
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
