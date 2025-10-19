using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nekote.Lab.Console.Hosting;
using Nekote.Lab.Console.Testers;

namespace Nekote.Lab.Console
{
    /// <summary>
    /// アプリケーションのエントリーポイントを定義します。
    /// </summary>
    public class Program
    {
        /// <summary>
        /// アプリケーションのメインエントリーポイント。
        /// </summary>
        /// <param name="args">コマンドライン引数。</param>
        public static void Main(string[] args)
        {
            // AppHost を使用して、サービスが構成済みのホストを生成します。
            var host = AppHost.Create();

            // DI コンテナから TimeTester のインスタンスを取得します。
            var timeTester = host.Services.GetRequiredService<TimeTester>();

            // テストを実行します。
            timeTester.DisplayCurrentTime();
        }
    }
}
