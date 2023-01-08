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

        // =============================================================================

        // 2023年1月8日（1日目、2回目）

        // AddConsole を呼べない
        // Microsoft.Extensions.Logging.Console パッケージが必要のようだ

        // ConsoleLoggerExtensions.AddConsole Method (Microsoft.Extensions.Logging) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.consoleloggerextensions.addconsole

        // NuGet Gallery | Microsoft.Extensions.Logging.Console 7.0.0
        // https://www.nuget.org/packages/Microsoft.Extensions.Logging.Console/

        // Host などを使うには、Microsoft.Extensions.Hosting パッケージも必要

        // NuGet Gallery | Microsoft.Extensions.Hosting 7.0.0
        // https://www.nuget.org/packages/Microsoft.Extensions.Hosting

        // LoggerFactory.Create と Host.CreateDefaultBuilder の二つのコースで Log* を呼べた
        // 色付きでコンソールに表示された
        // その書式を変更する方法や、渡したデータがどこに保存されたか、といったことは今のところ不詳

        // 「イベントビューアー > Windows ログ > Application」に、「ソース」を .NET Runtime として「警告」が入っていた
        // LogLevel.Information としたログは、AddFilter で除外したわけでないが、そこには見当たらなかった
        // どういう条件でそこに入るのかや、Mac ではどうなるのか、といったことを学ぶ必要がある

        // ASP.NET Core について学ぶのは今回が初めてで、何も知らない
        // 何をググっても、内容のほとんどを知らないページが現れる
        // 一度、次のページ以下の全てのページをきちんと読む必要がありそう

        // ASP.NET Core fundamentals overview | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/

        // Configuration in ASP.NET Core | Microsoft Learn を読み始めた
        // 以下、しばらくは、このページに関するメモが続く

        // 読まなければならないページについては、もう、なぜ読むかを書かない
        // ASP.NET 4 の頃とは全く異なる体系になっている
        // 自分は、ASP.NET Web Forms しか使ったことがないため、知識がとても古い
        // 差分だけ読めば足りるのでなく、とりあえず全て読まなければならない

        // Configuration - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration

        // ASP.NET Core において Host が中核的なものなのは分かった
        // その機能がコンソールアプリなどでどのくらい有効かについては、あとで調べる

        // WebApplicationBuilder Class (Microsoft.AspNetCore.Builder) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder

        // WebApplication.CreateBuilder Method (Microsoft.AspNetCore.Builder) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.webapplication.createbuilder

        // Safe storage of app secrets in development in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets

        // when the app runs in the Development environment という表現がある
        // 少なくとも appsettings.Production.json と appsettings.Development.json の二つがあるようだ
        // これらの指定方法や、具体的にどういったところが影響を受けるのかを調べる

        // EnvironmentVariablesConfigurationProvider Class (Microsoft.Extensions.Configuration.EnvironmentVariables) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.environmentvariables.environmentvariablesconfigurationprovider

        // .NET Generic Host in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host

        // ASP.NET Core Web Host | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host

        // Minimal APIs quick reference | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis#change-the-content-root-application-name-and-environment

        // Use multiple environments in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/environments

        // Use hosting startup assemblies in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/platform-specific-configuration

        // HostBuilderContext.Configuration Property (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.hostbuildercontext.configuration#microsoft-extensions-hosting-hostbuildercontext-configuration

        // IHostBuilder.ConfigureAppConfiguration Method (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostbuilder.configureappconfiguration

        // Code samples migrated to the new minimal hosting model in 6.0 | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/migration/50-to-60-samples

        // JsonConfigurationProvider Class (Microsoft.Extensions.Configuration.Json) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.json.jsonconfigurationprovider

        // Configuration ["Logging:LogLevel:Default"] のような使い方ができるようだ

        // IConfiguration Interface (Microsoft.Extensions.Configuration) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.iconfiguration

        // appsettings.Production.json と appsettings.Development.json の区別などは、このあたりか

        // IHostingEnvironment.EnvironmentName Property (Microsoft.Extensions.Hosting) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostingenvironment.environmentname

        // ConfigurationBinder.GetValue Method (Microsoft.Extensions.Configuration) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.configurationbinder.getvalue

        // 次のページのコードには、reloadOnChange: true というのが含まれる
        // デフォルトで、アプリ実行中の JSON ファイルへの変更がリロードされるようだ
        // https://source.dot.net/ の最新のソースには見当たらないため、あとで現状を調べる

        // extensions/Host.cs at release/3.1 · dotnet/extensions
        // https://github.com/dotnet/extensions/blob/release/3.1/src/Hosting/Hosting/src/Host.cs

        // Options pattern in ASP.NET Core | Microsoft Learn
        // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options

        // 次のメソッドは、ORM 的なことを行うか
        // ルール通りに作られたクラスへの設定データの流し込み

        // ConfigurationBinder.Bind Method (Microsoft.Extensions.Configuration) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.configurationbinder.bind

        // 名前から分かりにくいが、こちらも同様のことを行うようだ

        // ConfigurationBinder.Get Method (Microsoft.Extensions.Configuration) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.configurationbinder.get

        // OptionsConfigurationServiceCollectionExtensions.Configure Method (Microsoft.Extensions.DependencyInjection) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.optionsconfigurationservicecollectionextensions.configure

        // MvcServiceCollectionExtensions.AddRazorPages Method (Microsoft.Extensions.DependencyInjection) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.mvcservicecollectionextensions.addrazorpages

        // Configuration in ASP.NET Core | Microsoft Learn の Combining service collection の直前まで読んだ
        // 77 minutes to read とのことだが、1時間40分を掛けて1/6ほど

        // ページの左下に Download PDF というリンクがあったので開いたところ、9681ページあってビビった
        // 思っていた以上に体系が大きいため、学習コストについて考える必要がある

        // ドキュメントの質が高く、読んでいて詰まることはない
        // 情報が足りないところも、ググれば、同じく公式のドキュメントの別のページがすぐに出てきて、よく分かる

        // 量がエラいことになっているのは、1) 各ページが単体でもそれなりに理解できるように構成されていて、情報の重複がかなり多い、
        //     2) 抽象的な骨格に、あれもこれもプラグイン的に付く仕様になっていて、各部の情報量が掛け算になっている、の二つの理由による

        // そういった性質を勘案するなら、IDE にサンプルコードを吐かせ、分からないところを全てググるのも選択肢
        // とりあえず動くものを作れるようになるまでの学習コストは、間違いなくそれが最小

        // ただ、ASP.NET Web Forms の頃、それをやって逆に遠回りになったことがある
        // SQL/HTML/CSS の生成、データアクセス、多言語化、キャッシング、ロギング、その他、何から何まで自分で実装した
        // 自動生成されたコードのうち分からない部分のみググるアプローチだと、車輪を再開発しまくることになりうる

        // とりあえず、1週間を目安に、毎日3～6時間ほどドキュメントを読み、30時間ほどで、どのくらい理解できるか調べる
        // 今日は2回でトータル4時間だが、ドキュメントが素晴らしいので、既に何となく雰囲気はつかめてきている
        // あと30時間も読めば、どういう車輪があるか程度は全体的に理解できる可能性が高い

        #endregion

        public static void TestDotNetLoggingFeatures ()
        {
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
