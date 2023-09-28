using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiffMatchPatch;
using Nekote;

namespace NekoteConsole
{
    internal static class iCultureTester
    {
        // 全てのカルチャーを取得し、テキストファイルとしてデスクトップに保存
        // 前半に「親 > 子」を並べ、後半に、表示されなかった残り物を

        // 日本語版の Windows での出力結果（2022年12月21日）
        // Windows 11 の方が少しだけ情報が多い

        // Windows 11 → Cultures-20221221T081021Z.txt
        // Windows 10 → Cultures-20221221T081106Z.txt

        // Mac (Ventura 13.1) での出力結果（2022年12月23日）

        // Cultures-20221223T012020Z.txt

        // Nekote を Mac でコンパイル・実行するのは今回が初めてで、そのことも関係してか、初回は DisplayName などが英語で出た
        // Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo ("ja-JP") の実行により、日本語になった
        // 以来、CurrentCulture を設定しなくても日本語で出ている

        // Windows で出力したものには、CultureTypes のところに InstalledWin32Cultures が入る
        // それ以外に違いのないエントリーがとても多い
        // .NET におけるカルチャー情報は、International Components for Unicode (ICU) というライブラリーに依存しているとのこと
        // 以前は National Language Support (NLS) を使っていたが、Windows 10 May 2019 Update から ICU になった
        // Unix 系 OS は元々 ICU だそうで、だから Windows 11/10, Mac での出力結果が概ね一致した

        // Globalization and ICU - .NET | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/globalization-icu

        public static void GetCultures ()
        {
            List <CultureInfo> xDisplayedCultures = new List <CultureInfo> ();

            StringBuilder xBuilder = new StringBuilder ();

            // 0かどうかのみ見るので、数え方は適当
            int xErrorCount = 0;

            void iAppend (string value)
            {
                if (xBuilder.Length > 0)
                {
                    xBuilder.AppendLine ();
                    Console.WriteLine ();
                }

                xBuilder.Append (value);
                Console.Write (value);
            }

            void iAppendCulture (bool isIndented, CultureInfo culture)
            {
                string xIndentationString = isIndented ? "\x20\x20\x20\x20" : string.Empty;

                if (xDisplayedCultures.Any (x => string.Equals (x.Name, culture.Name, StringComparison.InvariantCultureIgnoreCase)))
                    xErrorCount ++;

                else xDisplayedCultures.Add (culture);

                // 4桁表示にするので桁数をチェック

                // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Min (x => x.KeyboardLayoutId).ToString ("X")); // 1
                // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Max (x => x.KeyboardLayoutId).ToString ("X")); // 7C67
                // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Min (x => x.LCID).ToString ("X")); // 1
                // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Max (x => x.LCID).ToString ("X")); // 7C67

                if (culture.KeyboardLayoutId > 0xFFFF || culture.LCID > 0xFFFF)
                    xErrorCount ++;

                StringBuilder xBuilderAlt = new StringBuilder ();

                // CultureInfo の主キー的なものは、LCID と Name のようだ
                // コンストラクターの説明に、A predefined CultureInfo identifier, LCID property of an existing CultureInfo object, or Windows-only culture identifier
                //     A predefined CultureInfo name, Name of an existing CultureInfo, or Windows-only culture name. name is not case-sensitive とある
                // Parent を $"{...}" で文字列化すると Name らしきものが得られる

                // CultureInfo Constructor (System.Globalization) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.-ctor

                // InvariantCulture の Name は null でなく "" とのこと
                // 一度でも "" の得られたものを nString.GetLiteralIfNullOrEmpty に
                // 後半で ": " + 改行の検索も行う

                // CultureInfo.InvariantCulture Property (System.Globalization) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture

                xBuilderAlt.AppendLine ($"{xIndentationString}CultureTypes: {culture.CultureTypes}");
                xBuilderAlt.AppendLine ($"{xIndentationString}DisplayName: {culture.DisplayName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}EnglishName: {culture.EnglishName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}IetfLanguageTag: {nString.GetLiteralIfNullOrEmpty (culture.IetfLanguageTag)}");
                xBuilderAlt.AppendLine ($"{xIndentationString}IsNeutralCulture: {culture.IsNeutralCulture}");
                xBuilderAlt.AppendLine ($"{xIndentationString}IsReadOnly: {culture.IsReadOnly}");
                xBuilderAlt.AppendLine (FormattableString.Invariant ($"{xIndentationString}KeyboardLayoutId: 0x{culture.KeyboardLayoutId:X4} ({culture.KeyboardLayoutId})"));
                xBuilderAlt.AppendLine (FormattableString.Invariant ($"{xIndentationString}LCID: 0x{culture.LCID:X4}, ({culture.LCID})"));
                xBuilderAlt.AppendLine ($"{xIndentationString}Name: {nString.GetLiteralIfNullOrEmpty (culture.Name)}");
                xBuilderAlt.AppendLine ($"{xIndentationString}NativeName: {culture.NativeName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}Parent: {nString.GetLiteralIfNullOrEmpty (culture.Parent.Name)}");
                xBuilderAlt.AppendLine ($"{xIndentationString}ThreeLetterISOLanguageName: {culture.ThreeLetterISOLanguageName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}ThreeLetterWindowsLanguageName: {culture.ThreeLetterWindowsLanguageName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}TwoLetterISOLanguageName: {culture.TwoLetterISOLanguageName}");
                xBuilderAlt.AppendLine ($"{xIndentationString}UseUserOverride: {culture.UseUserOverride}");

                iAppend (xBuilderAlt.ToString ());
            }

            // LCID と Name の両方が一意なのか確認したところ、LCID はそうでなかった

            // if (CultureInfo.GetCultures (CultureTypes.AllCultures).DistinctBy (x => x.LCID).Count () != CultureInfo.GetCultures (CultureTypes.AllCultures).Count ())
            //     xErrorCount ++;

            if (CultureInfo.GetCultures (CultureTypes.AllCultures).DistinctBy (x => x.Name,
                    StringComparer.InvariantCultureIgnoreCase).Count () != CultureInfo.GetCultures (CultureTypes.AllCultures).Length)
                xErrorCount ++;

            // LCID の共通する二つ以上のカルチャーのグループは一つだけだった

            // foreach (var xGroup in CultureInfo.GetCultures (CultureTypes.AllCultures).GroupBy (x => x.LCID))
            // {
            //     if (xGroup.Count () >= 2)
            //     {
            //         Console.WriteLine (xGroup.First ().LCID); // 4096
            //         Console.WriteLine (xGroup.Count ()); // 475
            //     }
            // }

            // LCID の4096について、ドキュメントには、Starting with Windows 10, it is assigned to any culture
            //     that does not have a unique locale identifier and does not have complete system-provided data とある

            // CultureInfo.LCID Property (System.Globalization) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.lcid

            // LCID が4096のところは CultureTypes.UserCustomCulture フラグが立っているようだったので、それぞれを数え、一応、完全一致も確認
            // このフラグについては、This member is deprecated. Custom cultures created by the user とあるが、まだ残っているようだ

            // CultureTypes Enum (System.Globalization) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.culturetypes

            // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Count (x => x.LCID == 4096)); // 475
            // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Count (x => x.CultureTypes.HasFlag (CultureTypes.UserCustomCulture))); // 475

            if (Enumerable.SequenceEqual (CultureInfo.GetCultures (CultureTypes.AllCultures).Where (x => x.LCID == 4096),
                    CultureInfo.GetCultures (CultureTypes.AllCultures).Where (x => x.CultureTypes.HasFlag (CultureTypes.UserCustomCulture))) == false)
                xErrorCount ++;

            // CultureTypes.NeutralCultures フラグが立っていて、かつ LCID が4096のものもある
            // 数え方を変えても、結果は同じ

            // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.NeutralCultures).Count (x => x.LCID == 4096)); // 117
            // Console.WriteLine (CultureInfo.GetCultures (CultureTypes.AllCultures).Count (x => x.IsNeutralCulture && x.LCID == 4096)); // 117

            // 数え方を変えてみたのは、CultureTypes.NeutralCultures で取得しても IsNeutralCulture == false のものが混じるため
            // InvariantCulture のみ、便宜上、そういうことになっているようだ

            if (CultureInfo.GetCultures (CultureTypes.NeutralCultures).Count (x => x.IsNeutralCulture == false) != 1)
                xErrorCount ++;

            // まず、LCID == 4096 のものだけ除外
            // OrderBy には、SortName というものがあるようなので使いたいが、internal なので不可能
            // テストを実行するパソコンによって順序が異ならないよう、一応、Name でソート
            // Name が一意なのは既に確認できている

            // CultureInfo.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Globalization/CultureInfo.cs

            var xNeutralCultures = CultureInfo.GetCultures (CultureTypes.NeutralCultures).Where (x => x.LCID != 4096).OrderBy (y => y.Name, StringComparer.InvariantCultureIgnoreCase);
            var xAllCultures = CultureInfo.GetCultures (CultureTypes.AllCultures).Where (x => x.LCID != 4096).OrderBy (y => y.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (CultureInfo xNeutralCulture in xNeutralCultures.Where (x => x.IsNeutralCulture)) // InvariantCulture を除外
            {
                iAppendCulture (false, xNeutralCulture);

                foreach (CultureInfo xCulture in xAllCultures.Where (x => x.IsNeutralCulture == false &&
                        string.Equals (x.Parent.Name, xNeutralCulture.Name, StringComparison.InvariantCultureIgnoreCase)))
                    iAppendCulture (true, xCulture);
            }

            // ここで x, y が null になることはない
            // 通常は、StringComparer のコードを一例として、それぞれが null の場合の条件分岐を行う

            // StringComparer.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/StringComparer.cs

            var xLeftOutCultures = xAllCultures.Except (xDisplayedCultures,
                new nEqualityComparer <CultureInfo> ((x, y) => string.Equals (x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase)));

            // InvariantCulture が必ず入るが、作法として

            if (xLeftOutCultures.Any ())
            {
                iAppend (new string ('=', 80) + Environment.NewLine);

                foreach (CultureInfo xCulture in xLeftOutCultures)
                    iAppendCulture (false, xCulture);
            }

            // 項目が "" または null のところが残されていないのを確認

            if (xBuilder.ToString ().Contains (": \r") || xBuilder.ToString ().Contains (": \n"))
                xErrorCount ++;

            string xFilePath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory),
                $"Cultures-{DateTime.UtcNow.ToString ("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture)}.txt");

            if (nFile.CanCreate (xFilePath) == false)
                xErrorCount ++;

            else nFile.WriteAllText (xFilePath, xBuilder.ToString ());

            Console.WriteLine ();
            Console.WriteLine ($"Error Count: {xErrorCount}");
        }

        // Windows 11/10, Mac の三つで出力した Cultures-*.txt を探し、差分を HTML ファイルとしてデスクトップに保存する
        // WinMerge などでも diff を取れるが、Windows でのみ入る文字列や、OS による記号の違いによる影響を排除

        // 既存の Cultures-*.txt の diff を取ったものを入れておく

        // Cultures-diff-20221223T080704Z.htm → Windows 11 と Windows 10 の差分
        // Cultures-diff-20221223T080705Z.htm → Windows 11 と Mac の差分
        // Cultures-diff-20221223T080706Z.htm → Windows 10 と Mac の差分

        public static void CompareCultureInfoFiles ()
        {
            string? [] xFilePaths =
            {
                iTesterShared.FindFileOrDirectory ("Cultures-20221221T081021Z.txt"),
                iTesterShared.FindFileOrDirectory ("Cultures-20221221T081106Z.txt"),
                iTesterShared.FindFileOrDirectory ("Cultures-20221223T012020Z.txt")
            };

            // ここでは、引数に問題があるのでなく、「データ」の取得に必要なファイルが見付からない

            if (xFilePaths.Any (x => x == null))
                throw new nDataException ();

            // Windows は半角、Mac は全角
            // 半角 → 全角にすると、LCID の列などにも全角が入る
            // DisplayName 欄は ja-JP ロケールでの出力なので全体が全角でもよいが、
            //     キーを見てから置換するほどの作り込みに利益がない

            var xFileContents = xFilePaths.Select (x => nFile.ReadAllText (x!)
                .Replace (", InstalledWin32Cultures", string.Empty, StringComparison.Ordinal)
                .Replace ("（", " (", StringComparison.Ordinal)
                .Replace ("）", ")", StringComparison.Ordinal));

            string iCompare (string value1, string value2)
            {
                diff_match_patch xDmp = new diff_match_patch ();

                // diff_main には、bool checklines を持つものもある
                // コードのコメントに、Speedup flag
                //     If false, then don't run a line-level diff first to identify the changed areas
                //     If true, then run a faster slightly less optimal diff とある

                // checklines を指定しなければ、デフォルトで true になる
                // 20221227-003100.png がゴチャゴチャなので false も試したが、少なくともこの部分では全く変化がなかった
                // その上、他の部分で、元々キーなどが共通しやすい差分データがエラいことになり、さらに数バイト分、<span> がちりばめられた
                // 一方、日本語のメールやブログ記事などでは、「まずは行単位で比較されてゴソッと」がなくなり、誤字・脱字が文字単位でより良く分かるかもしれない
                // 今後、diff-match-patch を使うときにはここのコードを見るだろうから、メモを添えて引数を明示的に指定しておく

                var xDiffs = xDmp.diff_main (value1, value2, checklines: true);

                // diff_cleanupSemantic により、1文字ずつ「削除」「挿入」「削除」「挿入」となるようなことが緩和されるか
                // 実装まで詳しく理解したわけでないが、実際にやってみたところ、何となく慌ただしさが落ち着いていた

                // API · google/diff-match-patch Wiki
                // https://github.com/google/diff-match-patch/wiki/API

                xDmp.diff_cleanupSemantic (xDiffs);

                StringBuilder xBuilder = new StringBuilder (Math.Max (value1.Length, value2.Length));

                foreach (Diff xDiff in xDiffs)
                {
                    // まず DELETE から得られるので、その順で見ている

                    // ちゃんと作るなら、DELETE または INSERT の text 中の改行を目に見える記号にするべき
                    // そうでないと、<span ...>\r\n</span> では文字の増減が視覚的に分からない

                    if (xDiff.operation == Operation.DELETE)
                        xBuilder.Append ($"<span class=\"deleted\">{nHtml.Encode (xDiff.text)}</span>");

                    else if (xDiff.operation == Operation.INSERT)
                        xBuilder.Append ($"<span class=\"inserted\">{nHtml.Encode (xDiff.text)}</span>");

                    else xBuilder.Append (nHtml.Encode (xDiff.text));
                }

                // 各 diff の位置や長さが予測不能なので、親カルチャー用の div と子カルチャー用の div を作って後者に左側 margin を設定するなどが難しい
                // 子カルチャーの途中から空行を経て次の親カルチャーの途中までが <span> に入るなどがあるため
                // 作り込むようなところでないため、&nbsp; と <br/> でサラッと

                nStringOptimizationResult xResult = nStringOptimizer.Default.Optimize (xBuilder.ToString ());

                string xIndentationString = "&nbsp;&nbsp;&nbsp;&nbsp;";

                string xString = string.Join ("<br/>" + Environment.NewLine, xResult.Lines.Select (x =>
                {
                    if (string.IsNullOrEmpty (x.IndentationString) == false && string.IsNullOrEmpty (x.VisibleString) == false)
                        return xIndentationString + x.VisibleString;

                    else return x.VisibleString;
                }));

                xBuilder.Clear ();

                xBuilder.AppendLine ("<html>");
                xBuilder.AppendLine ("<head>");
                xBuilder.AppendLine ("    <style>");
                xBuilder.AppendLine ("        span.deleted { background: red; color: white }");
                xBuilder.AppendLine ("        span.inserted { background: blue; color: white }");
                xBuilder.AppendLine ("    </style>");
                xBuilder.AppendLine ("</head>");
                xBuilder.AppendLine ("<body>");
                xBuilder.AppendLine ();
                xBuilder.AppendLine (xString);
                xBuilder.AppendLine ();
                xBuilder.AppendLine ("</body>");
                xBuilder.AppendLine ("</html>");

                return xBuilder.ToString ();
            }

            void iSave (string contents)
            {
                string xFileName = $"Cultures-diff-{DateTime.UtcNow.ToString ("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture)}.htm",
                    xFilePath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), xFileName);

                // 引数にもデータにも問題がなく、やってみての失敗
                // 「操作」「作戦」といったニュアンスで、ランタイムの問題を

                if (nFile.CanCreate (xFilePath) == false)
                    throw new nOperationException ();

                nFile.WriteAllText (xFilePath, contents);
            }

            // 左から右へカニが歩いて行くイメージ
            // 0, 1 の次が 1, 2 では、また左に戻ることになる

            // 「秒」でファイル名がぶつかるので Sleep で手抜き
            // 何度も実行するコードでない

            iSave (iCompare (xFileContents.ElementAt (0), xFileContents.ElementAt (1)));
            Thread.Sleep (1000);
            iSave (iCompare (xFileContents.ElementAt (0), xFileContents.ElementAt (2)));
            Thread.Sleep (1000);
            iSave (iCompare (xFileContents.ElementAt (1), xFileContents.ElementAt (2)));
        }
    }
}
