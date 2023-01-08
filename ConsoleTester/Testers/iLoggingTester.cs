using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleTester
{
    internal static class iLoggingTester
    {
        // Nekote にロギングの機能を追加する
        // 車輪の再開発を避けたく、まずは既存の機能について学ぶ
        // 学びながらコードとコメントを書くためグダグダになるだろうが、テストコード扱いなので問題視しない

        // Nekote では、どうせアセンブリーには影響しないコメントとして、学んだことを積極的に書くことを基本的な方針としている
        // そうすることで他者が「こいつ、こんなことも知らんのか」と思うことに実害はない
        // 一方、誤った知識や理解も明示されることで、未来の自分を含む他者が誤りを訂正できる可能性がある

        // ということに重きを置くなら、学んだことを書いていくブログを別に用意するのが一つの方法
        // しかし、自分は、その時点における検証・確認ができたことは確定していきたいため、
        //     学習のワークフローに、VSC でのステージ・コミットを残したい

        // となると、Nekote のソリューションでなく、たとえば Docs レポジトリーのテキストファイルにメモを書いていくことが考えられる
        // コードとドキュメントの分離は、チームでの開発においては当たり前だ
        // しかし、単独開発においては、動くコードの隣にメモを書けないことの不便が大きい
        // コードとドキュメントを行き来する構成において矛盾を生じさせないことには、それなりに手間が掛かる

        // 妥協案として、Nekote のソリューション内での学習メモの書き方を策定する
        // ブワーッと広がらないように #region を作り、コメント行の境界線で日ごとに区切りながら、各部に日付と何日目かくらいは書く
        // そのうち後者の理由は、日付より「○日目」の方が頭の中でインデックス的に機能しやすそうだから
        // 基本的には、過去の日付の部分の修正や追記を避ける
        // その方が分かりやすいならその限りでないが、上から下へ時系列的に書き殴った方がメモの管理コストは低い

        #region .NET のロギングの機能や ASP.NET Core についての学習

        // 2023年1月8日（1日目）

        // まずは、次のページの全てのコードを TestDotNetLoggingFeatures 内で動かす

        // Logging - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/logging

        // ILogger <TCategoryName> などを使うには、Microsoft.Extensions.Logging パッケージを入れるのが良さそう

        // ILogger<TCategoryName> Interface (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger-1

        // NuGet Gallery | Microsoft.Extensions.Logging 7.0.0
        // https://www.nuget.org/packages/Microsoft.Extensions.Logging/

        // ILogger という、ジェネリックでないものもある
        // ドキュメントで使われているのは、その上位の ILogger <TCategoryName> の方

        // ILogger Interface (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.ilogger

        // appsettings.{Environment}.json を作る必要があるようなので、次のページをあとで読む

        // Configuration in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/

        // LogLevel という enum では、None == 6, Trace == 0 となっている
        // default で得られる0を None とするのがドキュメントでも推奨されているので違和感を覚えるが、
        //     To suppress all logs, specify LogLevel.None
        //     LogLevel.None has a value of 6, which is higher than LogLevel.Critical (5) とのことなので仕様に理由はある
        // 「○○以上をログ」という指定なので、None を最大値にしておけば、None 以上のものがなくてログを抑制できる
        // なお、推奨されているのは、DO provide a value of zero on simple enums
        //     Consider calling the value something like "None"
        //     If such a value is not appropriate for this particular enum, the most common default value for the enum should be assigned the underlying value of zero ということ

        // LogLevel Enum (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel

        // Enum Design - Framework Design Guidelines | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/enum

        // The following appsettings.json file contains settings for all of the built-in providers というのをコピペ
        // 設定ファイルの更新時に参照

        // {
        //     "Logging": {
        //         "LogLevel": {
        //             "Default": "Error",
        //             "Microsoft": "Warning",
        //             "Microsoft.Hosting.Lifetime": "Warning"
        //         },
        //         "Debug": {
        //             "LogLevel": {
        //                 "Default": "Information"
        //             }
        //         },
        //         "Console": {
        //             "IncludeScopes": true,
        //             "LogLevel": {
        //                 "Microsoft.Extensions.Hosting": "Warning",
        //                 "Default": "Information"
        //             }
        //         },
        //         "EventSource": {
        //             "LogLevel": {
        //                 "Microsoft": "Information"
        //             }
        //         },
        //         "EventLog": {
        //             "LogLevel": {
        //                 "Microsoft": "Information"
        //             }
        //         },
        //         "AzureAppServicesFile": {
        //             "IncludeScopes": true,
        //             "LogLevel": {
        //                 "Default": "Warning"
        //             }
        //         },
        //         "AzureAppServicesBlob": {
        //             "IncludeScopes": true,
        //             "LogLevel": {
        //                 "Microsoft": "Information"
        //             }
        //         },
        //         "ApplicationInsights": {
        //             "LogLevel": {
        //                 "Default": "Information"
        //             }
        //         }
        //     }
        // }

        // Log level can be set by any of the configuration providers
        //     For example, you can create a persisted environment variable named Logging:LogLevel:Microsoft with a value of Information とのこと
        // 今さら環境変数かと思ったが、Azure App Service でも設定できるようで、そちらでは環境全体に適用されるグローバル設定のようになっているのだろう

        // 次のページもあとで読む

        // Configuration providers - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers

        // ILoggerFactory Interface (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.iloggerfactory

        // LoggingBuilderExtensions.SetMinimumLevel(ILoggingBuilder, LogLevel) Method (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggingbuilderextensions.setminimumlevel

        // ILogger <TCategoryName> と ILogger の使い分けについては、
        //     ILogger <DefaultService> _logger = ...
        //     ILogger _logger = loggerFactory.CreateLogger ("CustomCategory") というコードがあった
        // 2行目については、Calling CreateLogger with a fixed name can be useful when used in multiple classes/types so the events can be organized by category とのこと
        // TCategoryName の型には where 制約があり、少なくともログの出力の abstract メソッドくらいは override していなければならないイメージだったが、
        //     ILogger<T> is equivalent to calling CreateLogger with the fully qualified type name of T とのことで、
        //     CreateLogger が Func や Action を取るわけでないため、「このへんで発生したログですよ」の「このへん」を引っ張ってくるためだけに TCategoryName を使っているか

        // LoggerExtensions.LogInformation などには、
        //     public static void LogInformation (this ILogger logger, EventId eventId, Exception? exception, string? message, params object? [] args) など、
        //     EventId という構造体や Exception を取るものがある

        // EventId について、あとで学ぶ
        // Exception をシリアライズするコードがあるかもしれないので、あとで探す

        // LoggerExtensions.LogInformation Method (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.loginformation

        // EventId Struct (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.eventid

        // EventId.Implicit(Int32 to EventId) Operator (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.eventid.op_implicit

        // Microsoft.Extensions.Logging 名前空間の内容について、あとでザッと学ぶ

        // Microsoft.Extensions.Logging Namespace | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging

        // _logger.LogInformation ("Getting item {Id} at {RunTime}", id, DateTime.Now) のような書き方により、
        //     最終の文字列だけでなく、それぞれの引数の値をログのフィールドとして保存できるようだ

        // How to use structured logging · NLog/NLog Wiki
        // https://github.com/NLog/NLog/wiki/How-to-use-structured-logging

        // まだ読んでいる最初の Logging - .NET | Microsoft Learn が ASP.NET Core 寄りになってきた
        // 知らないものについてページを特定しておき、あとでまとめて学ぶ

        // IHost Interface (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihost

        // Host.CreateDefaultBuilder Method (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.host.createdefaultbuilder

        // WebHostBuilderExtensions.ConfigureLogging Method (Microsoft.AspNetCore.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.webhostbuilderextensions.configurelogging

        // IWebHostBuilder.Build Method (Microsoft.AspNetCore.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.hosting.iwebhostbuilder.build

        // HostingAbstractionsHostExtensions.RunAsync(IHost, CancellationToken) Method (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostingabstractionshostextensions.runasync

        // .NET Generic Host in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host

        // .NET Generic Host - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host

        // FilterLoggingBuilderExtensions.AddFilter Method (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.filterloggingbuilderextensions.addfilter

        // Filters in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters

        // ロギングについては、次のページもあとで読む

        // Logging in .NET Core and ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/

        // ASP.NET Core Blazor logging | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/logging

        // log scopes については、A scope can group a set of logical operations
        //     This grouping can be used to attach the same data to each log that's created as part of a set
        //     For example, every log created as part of processing a transaction can include the transaction ID とのこと

        // Non-host console app のところには、Logging code for apps without a Generic Host differs in the way providers are added and loggers are created とある
        // コンソールアプリや WPF などは、これに該当するのだろう
        // あとで、ここのコードを参考に、TestDotNetLoggingFeatures で何か出力してみる

        // logging providers についても、あとで

        // Logging providers - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/logging-providers

        // FileConfigurationProvider Class (Microsoft.Extensions.Configuration) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.fileconfigurationprovider

        // WPF で目にした dependency injection が、Microsoft.Extensions.DependencyInjection 名前空間でガッツリと実装されている
        // 異なる体系のようなので、あとで学ぶ

        // Dependency injection - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection

        // Microsoft.Extensions.DependencyInjection Namespace | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection

        // サンプルコードで呼ばれている ServiceProviderServiceExtensions.GetRequiredService も、Microsoft.Extensions.DependencyInjection に含まれる

        // ServiceProviderServiceExtensions.GetRequiredService Method (Microsoft.Extensions.DependencyInjection) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.serviceproviderserviceextensions.getrequiredservice

        // 最初の Logging - .NET | Microsoft Learn を読み終えた
        // 17 minutes to read と書かれているが、アホなので2時間半も掛かった
        // 次回以降、今回の部分に含めたページをチェックしていく

        #endregion

        public static void TestDotNetLoggingFeatures ()
        {
        }
    }
}
