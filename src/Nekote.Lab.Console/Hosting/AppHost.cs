using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nekote.Core.Time.DependencyInjection;
using Nekote.Lab.Console.Testers;

namespace Nekote.Lab.Console.Hosting
{
    /// <summary>
    /// アプリケーションのホストを構築および構成するためのファクトリクラス。
    /// </summary>
    public static class AppHost
    {
        /// <summary>
        /// サービスが登録された .NET ホストを作成します。
        /// </summary>
        /// <returns>構成済みの IHost。</returns>
        public static IHost Create()
        {
            var builder = Host.CreateDefaultBuilder();

            // サービスの構成を行います。
            builder.ConfigureServices((hostContext, services) =>
            {
                // Nekote.Core で定義した拡張メソッドを呼び出し、IClock サービスを登録します。
                services.AddSystemClock();

                // テスト用の TimeTester クラスを一時的なサービスとして登録します。
                services.AddTransient<TimeTester>();
            });

            // ホストをビルドして返します。
            return builder.Build();
        }
    }
}
