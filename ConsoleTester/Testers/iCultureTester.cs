using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iCultureTester
    {
        // 全てのカルチャーを取得し、テキストファイルとしてデスクトップに保存
        // 前半に「親 > 子」を並べ、後半に、表示されなかった残り物を

        // 日本語版の Windows での出力結果（2022年12月21日）
        // Windows 11 の方が少しだけ情報が多い

        // Windows 11 → Cultures-20221221T081021Z.txt
        // Windows 10 → Cultures-20221221T081106Z.txt

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
                    StringComparer.InvariantCultureIgnoreCase).Count () != CultureInfo.GetCultures (CultureTypes.AllCultures).Count ())
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

            var xLeftOutCultures = xAllCultures.Except (xDisplayedCultures,
                new nEqualityComparer <CultureInfo> ((x, y) => string.Equals (x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase)));

            // InvariantCulture が必ず入るが、作法として

            if (xLeftOutCultures.Count () > 0)
            {
                iAppend (new string ('=', 80) + Environment.NewLine);

                foreach (CultureInfo xCulture in xLeftOutCultures)
                    iAppendCulture (false, xCulture);
            }

            // 項目が "" または null のところが残されていないのを確認

            if (xBuilder.ToString ().IndexOf (": \r") >= 0 || xBuilder.ToString ().IndexOf (": \n") >= 0)
                xErrorCount ++;

            string xFilePath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory),
                $"Cultures-{DateTime.UtcNow.ToString ("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture)}.txt");

            if (nFile.CanCreate (xFilePath) == false)
                xErrorCount ++;

            else nFile.WriteAllText (xFilePath, xBuilder.ToString ());

            Console.WriteLine ();
            Console.WriteLine ($"Error Count: {xErrorCount}");
        }
    }
}
