using System;
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
            try
            {
                // AppHost を使用して、サービスが構成済みのホストを生成します。
                var host = AppHost.Create();

                // DI コンテナから TimeTester のインスタンスを取得します。
                var timeTester = host.Services.GetRequiredService<TimeTester>();

                // 現在時刻の取得と表示のテストを実行します。
                timeTester.DisplayCurrentTime();

                // DI コンテナから TextTester のインスタンスを取得します。
                var textTester = host.Services.GetRequiredService<TextTester>();

                // StringHelper.Reformat の速度テストを実行します（3000ミリ秒間）。
                textTester.SpeedTestReformat(3000);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("An unexpected error occurred:");
                System.Console.WriteLine(ex.ToString());
            }
        }
    }
}
