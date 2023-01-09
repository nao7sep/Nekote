using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleTester
{
    internal static class iLoggingTester
    {
        public static void TestDotNetLoggingFeatures ()
        {
            // todo
            // 知識が増えてから全体を再チェック
            // デフォルト引数に頼るべきでないところを探す

            using ILoggerFactory xFactory = LoggerFactory.Create (builder =>
            {
                builder.SetMinimumLevel (LogLevel.Information)
                    .AddFilter ("ConsoleTester.Program", LogLevel.Warning)
                    .AddConsole ();
            });

            ILogger xLogger = xFactory.CreateLogger <Program> ();
            xLogger.LogInformation ("information");
            xLogger.LogWarning ("warning");
            xLogger.LogError ("error");

            // =============================================================================

            IHost xHost = Host.CreateDefaultBuilder ().ConfigureLogging (logging =>
            {
                logging.SetMinimumLevel (LogLevel.Information)
                    .AddFilter ("ConsoleTester.Program", LogLevel.Warning);
            })
            .Build ();

            ILogger xLoggerAlt = xHost.Services.GetRequiredService <ILogger <Program>> ();
            xLoggerAlt.LogInformation ("information");
            xLoggerAlt.LogWarning ("warning");
            xLoggerAlt.LogError ("error");
        }
    }
}
