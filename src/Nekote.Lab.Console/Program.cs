using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nekote.Lab.Console.Hosting;
using Nekote.Lab.Console.Testers;

namespace Nekote.Lab.Console
{
    public class Program
    {
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
