using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DiffMatchPatch;
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

            // ここで x, y が null になることはない
            // 通常は、StringComparer のコードを一例として、それぞれが null の場合の条件分岐を行う

            // StringComparer.cs
            // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/StringComparer.cs

            var xLeftOutCultures = xAllCultures.Except (xDisplayedCultures,
                new nEqualityComparer <CultureInfo> ((x, y) => string.Equals (x!.Name, y!.Name, StringComparison.InvariantCultureIgnoreCase)));

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
                iTester.FindFileOrDirectory ("Cultures-20221221T081021Z.txt"),
                iTester.FindFileOrDirectory ("Cultures-20221221T081106Z.txt"),
                iTester.FindFileOrDirectory ("Cultures-20221223T012020Z.txt")
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

                var xDiffs = xDmp.diff_main (value1, value2);

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

        // nJaJpCulture の漢字のリストをチェック
        // まだなければ生成し、あれば、それが正しいか調べる

        public static void TestKanjiLists ()
        {
            // 複数のページから常用漢字のリストや学年ごとのリストを拝借
            // ミスを減らすため、できるだけ元のままコピペし、コードで処理

            #region 第1グループ → 後半が不正確
            // IMABI - Joyo Kanji List
            // https://www.imabi.net/joyokanjilist.htm

            // 1st Grade から最後の「訃」までをエディターに貼り付け、空行を残したまま、見える文字だけを引用符に
            // 決め打ちで処理できるため、改行が入らないのは問題でない

            // 追記: Secondary School のところが二つに分かれているのが気になっていたが、含まれる文字も不正確のようだ
            // 1～6年生までは大丈夫そう

            string xString1 =
                "1st Grade" +

                "一 九 七 二 人 入 八 力 十 下 三 千 上 口 土 夕 大 女 子 小 山 川 五 天 中 六 円 手 文 日 月 木 水 火 犬 王 正 出 本 右 四 左 玉 生 田 白 目 石 立 百 年 休 先 名 字 早 気 竹 糸 耳 虫 村 男 町 花 見 貝 赤 足 車 学 林 空 金 雨 青 草 音 校 森" +

                "2nd Grade" +

                "刀 万 丸 才 工 弓 内 午 少 元 今 公 分 切 友 太 引 心 戸 方 止 毛 父 牛 半 市 北 古 台 兄 冬 外 広 母 用 矢 交 会 合 同 回 寺 地 多 光 当 毎 池 米 羽 考 肉 自 色 行 西 来 何 作 体 弟 図 声 売 形 汽 社 角 言 谷 走 近 里 麦 画 東 京 夜 直 国 姉 妹 岩 店 明 歩 知 長 門 昼 前 南 点 室 後 春 星 海 活 思 科 秋 茶 計 風 食 首 夏 弱 原 家 帰 時 紙 書 記 通 馬 高 強 教 理 細 組 船 週 野 雪 魚 鳥 黄 黒 場 晴 答 絵 買 朝 道 番 間 雲 園 数 新 楽 話 遠 電 鳴 歌 算 語 読 聞 線 親 頭 曜 顔" +

                "3rd Grade" +

                "丁 予 化 区 反 央 平 申 世 由 氷 主 仕 他 代 写 号 去 打 皮 皿 礼 両 曲 向 州 全 次 安 守 式 死 列 羊 有 血 住 助 医 君 坂 局 役 投 対 決 究 豆 身 返 表 事 育 使 命 味 幸 始 実 定 岸 所 放 昔 板 泳 注 波 油 受 物 具 委 和 者 取 服 苦 重 乗 係 品 客 県 屋 炭 度 待 急 指 持 拾 昭 相 柱 洋 畑 界 発 研 神 秒 級 美 負 送 追 面 島 勉 倍 真 員 宮 庫 庭 旅 根 酒 消 流 病 息 荷 起 速 配 院 悪 商 動 宿 帳 族 深 球 祭 第 笛 終 習 転 進 都 部 問 章 寒 暑 植 温 湖 港 湯 登 短 童 等 筆 着 期 勝 葉 落 軽 運 遊 開 階 陽 集 悲 飲 歯 業 感 想 暗 漢 福 詩 路 農 鉄 意 様 緑 練 銀 駅 鼻 横 箱 談 調 橋 整 薬 館 題" +

                "4th Grade" +

                "士 不 夫 欠 氏 民 史 必 失 包 末 未 以 付 令 加 司 功 札 辺 印 争 仲 伝 共 兆 各 好 成 灯 老 衣 求 束 兵 位 低 児 冷 別 努 労 告 囲 完 改 希 折 材 利 臣 良 芸 初 果 刷 卒 念 例 典 周 協 参 固 官 底 府 径 松 毒 泣 治 法 牧 的 季 英 芽 単 省 変 信 便 軍 勇 型 建 昨 栄 浅 胃 祝 紀 約 要 飛 候 借 倉 孫 案 害 帯 席 徒 挙 梅 残 殺 浴 特 笑 粉 料 差 脈 航 訓 連 郡 巣 健 側 停 副 唱 堂 康 得 救 械 清 望 産 菜 票 貨 敗 陸 博 喜 順 街 散 景 最 量 満 焼 然 無 給 結 覚 象 貯 費 達 隊 飯 働 塩 戦 極 照 愛 節 続 置 腸 辞 試 歴 察 旗 漁 種 管 説 関 静 億 器 賞 標 熱 養 課 輪 選 機 積 録 観 類 験 願 鏡 競 議" +

                "5th Grade" +

                "久 仏 支 比 可 旧 永 句 圧 弁 布 刊 犯 示 再 仮 件 任 因 団 在 舌 似 余 判 均 志 条 災 応 序 快 技 状 防 武 承 価 舎 券 制 効 妻 居 往 性 招 易 枝 河 版 肥 述 非 保 厚 故 政 査 独 祖 則 逆 退 迷 限 師 個 修 俵 益 能 容 恩 格 桜 留 破 素 耕 財 造 率 貧 基 婦 寄 常 張 術 情 採 授 接 断 液 混 現 略 眼 務 移 経 規 許 設 責 険 備 営 報 富 属 復 提 検 減 測 税 程 絶 統 証 評 賀 貸 貿 過 勢 幹 準 損 禁 罪 義 群 墓 夢 解 豊 資 鉱 預 飼 像 境 増 徳 慣 態 構 演 精 総 綿 製 複 適 酸 銭 銅 際 雑 領 導 敵 暴 潔 確 編 賛 質 興 衛 燃 築 輸 績 講 謝 織 職 額 識 護" +

                "6th Grade" +

                "亡 寸 己 干 仁 尺 片 冊 収 処 幼 庁 穴 危 后 灰 吸 存 宇 宅 机 至 否 我 系 卵 忘 孝 困 批 私 乱 垂 乳 供 並 刻 呼 宗 宙 宝 届 延 忠 拡 担 拝 枚 沿 若 看 城 奏 姿 宣 専 巻 律 映 染 段 洗 派 皇 泉 砂 紅 背 肺 革 蚕 値 俳 党 展 座 従 株 将 班 秘 純 納 胸 朗 討 射 針 降 除 陛 骨 域 密 捨 推 探 済 異 盛 視 窓 翌 脳 著 訪 訳 欲 郷 郵 閉 頂 就 善 尊 割 創 勤 裁 揮 敬 晩 棒 痛 筋 策 衆 装 補 詞 貴 裏 傷 暖 源 聖 盟 絹 署 腹 蒸 幕 誠 賃 疑 層 模 穀 磁 暮 誤 誌 認 閣 障 劇 権 潮 熟 蔵 諸 誕 論 遺 奮 憲 操 樹 激 糖 縦 鋼 厳 優 縮 覧 簡 臨 難 臓 警" +

                "Secondary School" +

                "乙 了 又 与 及 丈 刃 凡 互 弔 井 升 丹 乏 屯 介 冗 凶 刈 匹 厄 双 孔 幻 斗 斤 且 丙 甲 凸 丘 斥 仙 凹 召 巨 占 囚 奴 尼 巧 払 汁 玄 甘 矛 込 弐 朱 吏 劣 充 妄 企 仰 伐 伏 刑 旬 旨 匠 叫 吐 吉 如 妃 尽 帆 忙 扱 朽 朴 汚 汗 江 壮 缶 肌 舟 芋 芝 巡 迅 亜 更 寿 励 含 佐 伺 伸 但 伯 伴 呉 克 却 吟 吹 呈 壱 坑 坊 妊 妨 妙 肖 尿 尾 岐 攻 忌 床 廷 忍 戒 戻 抗 抄 択 把 抜 扶 抑 杉 沖 沢 沈 没 妥 狂 秀 肝 即 芳 辛 迎 邦 岳 奉 享 盲 依 佳 侍 侮 併 免 刺 劾 卓 叔 坪 奇 奔 姓 宜 尚 屈 岬 弦 征 彼 怪 怖 肩 房 押 拐 拒 拠 拘 拙 拓 抽 抵 拍 披 抱 抹 昆 昇 枢 析 杯 枠 欧 肯 殴 況 沼 泥 泊 泌 沸 泡 炎 炊 炉 邪 祈 祉 突 肢 肪 到 茎 苗 茂 迭 迫 邸 阻 附 斉 甚 帥 衷 幽 為 盾 卑 哀 亭 帝 侯 俊 侵 促 俗 盆 冠 削 勅 貞 卸 厘 怠 叙 咲 垣 契 姻 孤 封 峡 峠 弧 悔 恒 恨 怒 威 括 挟 拷 挑 施 是 冒 架 枯 柄 柳 皆 洪 浄 津 洞 牲 狭 狩 珍 某 疫 柔 砕 窃 糾 耐 胎 胆 胞 臭 荒 荘 虐 訂 赴 軌 逃 郊 郎 香 剛 衰 畝 恋 倹 倒 倣 俸 倫 翁 兼 准 凍 剣 剖 脅 匿 栽 索 桑 唆 哲 埋 娯 娠 姫 娘 宴 宰 宵 峰 貢 唐 徐 悦 恐 恭 恵 悟 悩 扇 振 捜 挿 捕 敏 核 桟 栓 桃 殊 殉 浦 浸 泰 浜 浮 涙 浪 烈 畜 珠 畔 疾 症 疲 眠 砲 祥 称 租 秩 粋 紛 紡 紋 耗 恥 脂 朕 胴 致 般 既 華 蚊 被 託 軒 辱 唇 逝 逐 逓 途 透 酌 陥 陣 隻 飢 鬼 剤 竜 粛 尉 彫 偽 偶 偵 偏 剰 勘 乾 喝 啓 唯 執 培 堀 婚 婆 寂 崎 崇 崩 庶 庸 彩 患 惨 惜 悼 悠 掛 掘 掲 控 据 措 掃 排 描 斜 旋 曹 殻 貫 涯 渇 渓 渋 淑 渉 淡 添 涼 猫 猛 猟 瓶 累 盗 眺 窒 符 粗 粘 粒 紺 紹 紳 脚 脱 豚 舶 菓 菊 菌 虚 蛍 蛇 袋 訟 販 赦 軟 逸 逮 郭 酔 釈 釣 陰 陳 陶 陪 隆 陵 麻 斎 喪 奥 蛮 偉 傘 傍 普 喚 喫 圏 堪 堅 堕 塚 堤 塔 塀 媒 婿 掌 項 幅 帽 幾 廃 廊 弾 尋 御 循 慌 惰 愉 惑 雇 扉 握 援 換 搭 揚 揺 敢 暁 晶 替 棺 棋 棚 棟 款 欺 殖 渦 滋 湿 渡 湾 煮 猶 琴 畳 塁 疎 痘 痢 硬 硝 硫 筒 粧 絞 紫 絡 腕 葬 募 裕 裂 詠 詐 詔 診 訴 越 超 距 軸 遇 遂 遅 遍 酢 鈍 閑 隅 随 焦 雄 雰 殿 棄 傾 傑 債 催 僧 慈 勧 載 嗣 嘆 塊 塑 塗 奨 嫁 嫌 寛 寝 廉 微 慨 愚 愁 慎 携 搾 摂 搬 暇 楼 歳 滑 溝 滞 滝 漠 滅 溶 煙 煩 雅 猿 献 痴 睡 督 碁 禍 禅 稚 継 腰 艇 蓄 虞 虜 褐 裸 触 該 詰 誇 詳 誉 賊 賄 跡 践 跳 較 違 遣 酬 酪 鉛 鉢 鈴 隔 雷 零 靴 頑 頒 飾 飽 鼓 豪 僕 僚 暦 塾 奪 嫡 寡 寧 腐 彰 徴 憎 慢 摘 概 雌 漆 漸 漬 滴 漂 漫 漏 獄 碑 稲 端 箇 維 綱 緒 網 罰 膜 慕 誓 誘 踊 遮 遭 酵 酷 銃 銘 閥 隠 需 駆 駄 髪 魂 錬 緯 韻 影 鋭 謁 閲 縁 憶 穏 稼 餓 壊 懐 嚇 獲 穫 潟 轄 憾 歓 環 監 緩 艦 還 鑑 輝 騎 儀 戯 擬 犠 窮 矯 響 驚 凝 緊 襟 謹 繰 勲 薫 慶 憩 鶏 鯨 撃 懸 謙 賢 顕 顧 稿 衡 購 墾 懇 鎖 錯 撮 擦 暫 諮 賜 璽 爵 趣 儒 襲 醜 獣 瞬 潤 遵 償 礁 衝 鐘 壌 嬢 譲 醸 錠 嘱 審 薪 震 髄 澄 瀬 請 籍 潜 繊 薦 遷 鮮 繕 礎 槽 燥 藻 霜 騒 贈 濯 濁 諾 鍛 壇 鋳 駐 懲 聴 鎮 墜 締 徹 撤 謄 踏 騰 闘 篤 曇 縄 濃 覇 輩 賠 薄 爆 縛 繁 藩 範 盤 罷 避 賓 頻 敷 膚 譜 賦 舞 覆 噴 墳 憤 幣 弊 壁 癖 舗 穂 簿 縫 褒 膨 謀 墨 撲 翻 摩 磨 魔 繭 魅 霧 黙 躍 癒 諭 憂 融 慰 窯 謡 翼 羅 頼 欄 濫 履 離 慮 寮 療 糧 隣 隷 霊 麗 齢 擁 露 藤 誰 俺 岡 頃 奈 阪 韓 弥 那 鹿 斬 虎 狙 脇 熊 尻 旦 闇 籠 呂 亀 頬 膝 鶴 匂 沙 須 椅 股 眉 挨 拶 鎌 凄 謎 稽 曾 喉 拭 貌 塞 蹴 鍵 膳 袖 潰 駒 剥 鍋 湧 葛 梨 貼 拉 枕 顎 苛 蓋 裾 腫 爪 嵐 鬱 妖 藍 捉 宛 崖 叱 瓦 拳 乞 呪 汰 勃 昧 唾 艶 痕 諦 餅 瞳 唄 隙 淫 錦 箸 戚 妬 蔑 嗅 蜜 戴 痩 怨 醒 詣 窟 巾 蜂 骸 弄 嫉 罵 璧 阜 埼 伎 曖 餌 爽 詮 芯 綻 肘 麓 憧 頓" +

                "牙 咽 嘲 臆 挫 溺 侶 丼 瘍 僅 諜 柵 腎 梗 瑠 羨 酎 畿 畏 瞭 踪 栃 蔽 茨 慄 傲 虹 捻 臼 喩 萎 腺 桁 玩 冶 羞 惧 舷 貪 采 堆 煎 斑 冥 遜 旺 麺 璃 串 填 箋 脊 緻 辣 摯 汎 憚 哨 氾 諧 媛 彙 恣 聘 沃 憬 捗 訃";

            xString1 = xString1
                .Replace ("\x20", "")
                .Replace ("SecondarySchool", ",")
                .Replace ("頓牙", "頓,牙");

            xString1 = Regex.Replace (xString1, "[1-6][a-z]+", ",", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var xKanji1 = xString1.Split (',', StringSplitOptions.RemoveEmptyEntries).Select (x => x.ToCharArray ());

            // 追記: 1110文字でないといけない
            // Console.WriteLine (xKanji1.Skip (6).SelectMany (x => x).Count ()); // → 1121
            #endregion

            #region 第2グループ → 正確
            // 別表　学年別漢字配当表：文部科学省
            // https://www.mext.go.jp/a_menu/shotou/new-cs/youryou/syo/koku/001.htm

            // 「第一学年」から「（181字）」までをコピペし、各行に引用符などを付けた

            // 追記: .go.jp だし、第1グループの1～6年生と一致したため、このグループは正確だろう

            string xString2 =
                "第一学年	一　右　雨　円　王　音　下　火　花　貝　学　気　九　休　玉　金　空　月　犬　見　五　口　校　左　三　山　子　四　糸　字　耳　七　車　手　十　出　女　小　上　森　人　水　正　生　青　夕　石　赤　千　川　先　早　草　足　村　大　男　竹　中　虫　町　天　田　土　二　日　入　年　白　八　百　文　木　本　名　目　立　力　林　六（80字）" +
                "第二学年	引　羽　雲　園　遠　何　科　夏　家　歌　画　回　会　海　絵　外　角　楽　活　間　丸　岩　顔　汽　記　帰　弓　牛　魚　京　強　教　近　兄　形　計　元　言　原　戸　古　午　後　語　工　公　広　交　光　考　行　高　黄　合　谷　国　黒　今　才　細　作　算　止　市　矢　姉　思　紙　寺　自　時　室　社　弱　首　秋　週　春　書　少　場　色　食　心　新　親　図　数　西　声　星　晴　切　雪　船　線　前　組　走　多　太　体　台　地　池　知　茶　昼　長　鳥　朝　直　通　弟　店　点　電　刀　冬　当　東　答　頭　同　道　読　内　南　肉　馬　売　買　麦　半　番　父　風　分　聞　米　歩　母　方　北　毎　妹　万　明　鳴　毛　門　夜　野　友　用　曜　来　里　理　話（160字）" +
                "第三学年	悪　安　暗　医　委　意　育　員　院　飲　運　泳　駅　央　横　屋　温　化　荷　界　開　階　寒　感　漢　館　岸　起　期　客　究　急　級　宮　球　去　橋　業　曲　局　銀　区　苦　具　君　係　軽　血　決　研　県　庫　湖　向　幸　港　号　根　祭　皿　仕　死　使　始　指　歯　詩　次　事　持　式　実　写　者　主　守　取　酒　受　州　拾　終　習　集　住　重　宿　所　暑　助　昭　消　商　章　勝　乗　植　申　身　神　真　深　進　世　整　昔　全　相　送　想　息　速　族　他　打　対　待　代　第　題　炭　短　談　着　注　柱　丁　帳　調　追　定　庭　笛　鉄　転　都　度　投　豆　島　湯　登　等　動　童　農　波　配　倍　箱　畑　発　反　坂　板　皮　悲　美　鼻　筆　氷　表　秒　病　品　負　部　服　福　物　平　返　勉　放　味　命　面　問　役　薬　由　油　有　遊　予　羊　洋　葉　陽　様　落　流　旅　両　緑　礼　列　練　路　和（200字）" +
                "第四学年	愛　案　以　衣　位　囲　胃　印　英　栄　塩　億　加　果　貨　課　芽　改　械　害　街　各　覚　完　官　管　関　観　願　希　季　紀　喜　旗　器　機　議　求　泣　救　給　挙　漁　共　協　鏡　競　極　訓　軍　郡　径　型　景　芸　欠　結　建　健　験　固　功　好　候　航　康　告　差　菜　最　材　昨　札　刷　殺　察　参　産　散　残　士　氏　史　司　試　児　治　辞　失　借　種　周　祝　順　初　松　笑　唱　焼　象　照　賞　臣　信　成　省　清　静　席　積　折　節　説　浅　戦　選　然　争　倉　巣　束　側　続　卒　孫　帯　隊　達　単　置　仲　貯　兆　腸　低　底　停　的　典　伝　徒　努　灯　堂　働　特　得　毒　熱　念　敗　梅　博　飯　飛　費　必　票　標　不　夫　付　府　副　粉　兵　別　辺　変　便　包　法　望　牧　末　満　未　脈　民　無　約　勇　要　養　浴　利　陸　良　料　量　輪　類　令　冷　例　歴　連　老　労　録（200字）" +
                "第五学年	圧　移　因　永　営　衛　易　益　液　演　応　往　桜　恩　可　仮　価　河　過　賀　快　解　格　確　額　刊　幹　慣　眼　基　寄　規　技　義　逆　久　旧　居　許　境　均　禁　句　群　経　潔　件　券　険　検　限　現　減　故　個　護　効　厚　耕　鉱　構　興　講　混　査　再　災　妻　採　際　在　財　罪　雑　酸　賛　支　志　枝　師　資　飼　示　似　識　質　舎　謝　授　修　述　術　準　序　招　承　証　条　状　常　情　織　職　制　性　政　勢　精　製　税　責　績　接　設　舌　絶　銭　祖　素　総　造　像　増　則　測　属　率　損　退　貸　態　団　断　築　張　提　程　適　敵　統　銅　導　徳　独　任　燃　能　破　犯　判　版　比　肥　非　備　俵　評　貧　布　婦　富　武　復　複　仏　編　弁　保　墓　報　豊　防　貿　暴　務　夢　迷　綿　輸　余　預　容　略　留　領　（185字）" +
                "第六学年	異　遺　域　宇　映　延　沿　我　灰　拡　革　閣　割　株　干　巻　看　簡　危　机　揮　貴　疑　吸　供　胸　郷　勤　筋　系　敬　警　劇　激　穴　絹　権　憲　源　厳　己　呼　誤　后　孝　皇　紅　降　鋼　刻　穀　骨　困　砂　座　済　裁　策　冊　蚕　至　私　姿　視　詞　誌　磁　射　捨　尺　若　樹　収　宗　就　衆　従　縦　縮　熟　純　処　署　諸　除　将　傷　障　城　蒸　針　仁　垂　推　寸　盛　聖　誠　宣　専　泉　洗　染　善　奏　窓　創　装　層　操　蔵　臓　存　尊　宅　担　探　誕　段　暖　値　宙　忠　著　庁　頂　潮　賃　痛　展　討　党　糖　届　難　乳　認　納　脳　派　拝　背　肺　俳　班　晩　否　批　秘　腹　奮　並　陛　閉　片　補　暮　宝　訪　亡　忘　棒　枚　幕　密　盟　模　訳　郵　優　幼　欲　翌　乱　卵　覧　裏　律　臨　朗　論　（181字）";

            xString2 = xString2
                .Replace ("　", "") // 全角空白
                .Replace ("\t", "");

            xString2 = Regex.Replace (xString2, "第.学年", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);
            xString2 = Regex.Replace (xString2, "（[0-9]+字）", ",", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var xKanji2 = xString2.Split (',', StringSplitOptions.RemoveEmptyEntries).Select (x => x.ToCharArray ());
            #endregion

            #region 第1グループと第2グループの照合 → 1～6年生が一致
            // 1～6年生の漢字を char として簡単にソートしたものが全て一致しなければ、データの問題として例外が飛ぶ
            // Enumerable.Range でも書けそうだが、ここはシンプルに

            for (int temp = 0; temp < 6; temp ++)
            {
                if (Enumerable.SequenceEqual (xKanji1.ElementAt (temp).OrderBy (x => x), xKanji2.ElementAt (temp).OrderBy (x => x)) == false)
                    throw new nDataException ();

                // 追記: 念のため、数もチェック
                // Console.WriteLine (xKanji1.ElementAt (temp).Count ()); // → ページの内容と一致
            }
            #endregion

            #region 第3グループ → 正確
            // 付録:常用漢字の一覧 - ウィクショナリー日本語版
            // https://ja.wiktionary.org/wiki/%E4%BB%98%E9%8C%B2:%E5%B8%B8%E7%94%A8%E6%BC%A2%E5%AD%97%E3%81%AE%E4%B8%80%E8%A6%A7

            // 「あ」から「腕」までをコピペ
            // 「編集」がコピー対象から外れてくれた
            // これまで同様、見える文字のある行に引用符を

            // 追記: 第4グループと条件付きで一致したので正しいはず
            // その条件については、第4グループの方へ

            string xString3 =
                "あ" +
                "亜 哀 挨 愛 曖 悪 握 圧 扱 宛 嵐 安 案 暗" +

                "い" +
                "以 衣 位 囲 医 依 委 威 為 畏 胃 尉 異 移 萎 偉 椅 彙 意 違 維 慰 遺 緯 域 育 一 壱 逸 茨 芋 引 印 因 咽 姻 員 院 淫 陰 飲 隠 韻" +

                "う" +
                "右 宇 羽 雨 唄 鬱 畝 浦 運 雲" +

                "え" +
                "永 泳 英 映 栄 営 詠 影 鋭 衛 易 疫 益 液 駅 悦 越 謁 閲 円 延 沿 炎 怨 宴 媛 援 園 煙 猿 遠 鉛 塩 演 縁 艶" +

                "お" +
                "汚 王 凹 央 応 往 押 旺 欧 殴 桜 翁 奥 横 岡 屋 億 憶 臆 虞 乙 俺 卸 音 恩 温 穏" +

                "か" +
                "下 化 火 加 可 仮 何 花 佳 価 果 河 苛 科 架 夏 家 荷 華 菓 貨 渦 過 嫁 暇 禍 靴 寡 歌 箇 稼 課 蚊 牙 瓦 我 画 芽 賀 雅 餓 介 回 灰 会 快 戒 改 怪 拐 悔 海 界 皆 械 絵 開 階 塊 楷 解 潰 壊 懐 諧 貝 外 劾 害 崖 涯 街 慨 蓋 該 概 骸 垣 柿 各 角 拡 革 格 核 殻 郭 覚 較 隔 閣 確 獲 嚇 穫 学 岳 楽 額 顎 掛 潟 括 活 喝 渇 割 葛 滑 褐 轄 且 株 釜 鎌 刈 干 刊 甘 汗 缶 完 肝 官 冠 巻 看 陥 乾 勘 患 貫 寒 喚 堪 換 敢 棺 款 間 閑 勧 寛 幹 感 漢 慣 管 関 歓 監 緩 憾 還 館 環 簡 観 韓 艦 鑑 丸 含 岸 岩 玩 眼 頑 顔 願" +

                "き" +
                "企 伎 危 机 気 岐 希 忌 汽 奇 祈 季 紀 軌 既 記 起 飢 鬼 帰 基 寄 規 亀 喜 幾 揮 期 棋 貴 棄 毀 旗 器 畿 輝 機 騎 技 宜 偽 欺 義 疑 儀 戯 擬 犠 議 菊 吉 喫 詰 却 客 脚 逆 虐 九 久 及 弓 丘 旧 休 吸 朽 臼 求 究 泣 急 級 糾 宮 救 球 給 嗅 窮 牛 去 巨 居 拒 拠 挙 虚 許 距 魚 御 漁 凶 共 叫 狂 京 享 供 協 況 峡 挟 狭 恐 恭 胸 脅 強 教 郷 境 橋 矯 鏡 競 響 驚 仰 暁 業 凝 曲 局 極 玉 巾 斤 均 近 金 菌 勤 琴 筋 僅 禁 緊 錦 謹 襟 吟 銀" +

                "く" +
                "区 句 苦 駆 具 惧 愚 空 偶 遇 隅 串 屈 掘 窟 熊 繰 君 訓 勲 薫 軍 郡 群" +

                "け" +
                "兄 刑 形 系 径 茎 係 型 契 計 恵 啓 掲 渓 経 蛍 敬 景 軽 傾 携 継 詣 慶 憬 稽 憩 警 鶏 芸 迎 鯨 隙 劇 撃 激 桁 欠 穴 血 決 結 傑 潔 月 犬 件 見 券 肩 建 研 県 倹 兼 剣 拳 軒 健 険 圏 堅 検 嫌 献 絹 遣 権 憲 賢 謙 鍵 繭 顕 験 懸 元 幻 玄 言 弦 限 原 現 舷 減 源 厳" +

                "こ" +
                "己 戸 古 呼 固 股 虎 孤 弧 故 枯 個 庫 湖 雇 誇 鼓 錮 顧 五 互 午 呉 後 娯 悟 碁 語 誤 護 口 工 公 勾 孔 功 巧 広 甲 交 光 向 后 好 江 考 行 坑 孝 抗 攻 更 効 幸 拘 肯 侯 厚 恒 洪 皇 紅 荒 郊 香 候 校 耕 航 貢 降 高 康 控 梗 黄 喉 慌 港 硬 絞 項 溝 鉱 構 綱 酵 稿 興 衡 鋼 講 購 乞 号 合 拷 剛 傲 豪 克 告 谷 刻 国 黒 穀 酷 獄 骨 駒 込 頃 今 困 昆 恨 根 婚 混 痕 紺 魂 墾 懇" +

                "さ" +
                "左 佐 沙 査 砂 唆 差 詐 鎖 座 挫 才 再 災 妻 采 砕 宰 栽 彩 採 済 祭 斎 細 菜 最 裁 債 催 塞 歳 載 際 埼 在 材 剤 財 罪 崎 作 削 昨 柵 索 策 酢 搾 錯 咲 冊 札 刷 刹 拶 殺 察 撮 擦 雑 皿 三 山 参 桟 蚕 惨 産 傘 散 算 酸 賛 残 斬 暫" +

                "し" +
                "士 子 支 止 氏 仕 史 司 四 市 矢 旨 死 糸 至 伺 志 私 使 刺 始 姉 枝 祉 肢 姿 思 指 施 師 恣 紙 脂 視 紫 詞 歯 嗣 試 詩 資 飼 誌 雌 摯 賜 諮 示 字 寺 次 耳 自 似 児 事 侍 治 持 時 滋 慈 辞 磁 餌 璽 鹿 式 識 軸 七 叱 失 室 疾 執 湿 嫉 漆 質 実 芝 写 社 車 舎 者 射 捨 赦 斜 煮 遮 謝 邪 蛇 尺 借 酌 釈 爵 若 弱 寂 手 主 守 朱 取 狩 首 殊 珠 酒 腫 種 趣 寿 受 呪 授 需 儒 樹 収 囚 州 舟 秀 周 宗 拾 秋 臭 修 袖 終 羞 習 週 就 衆 集 愁 酬 醜 蹴 襲 十 汁 充 住 柔 重 従 渋 銃 獣 縦 叔 祝 宿 淑 粛 縮 塾 熟 出 述 術 俊 春 瞬 旬 巡 盾 准 殉 純 循 順 準 潤 遵 処 初 所 書 庶 暑 署 緒 諸 女 如 助 序 叙 徐 除 小 升 少 召 匠 床 抄 肖 尚 招 承 昇 松 沼 昭 宵 将 消 症 祥 称 笑 唱 商 渉 章 紹 訟 勝 掌 晶 焼 焦 硝 粧 詔 証 象 傷 奨 照 詳 彰 障 憧 衝 賞 償 礁 鐘 上 丈 冗 条 状 乗 城 浄 剰 常 情 場 畳 蒸 縄 壌 嬢 錠 譲 醸 色 拭 食 植 殖 飾 触 嘱 織 職 辱 尻 心 申 伸 臣 芯 身 辛 侵 信 津 神 唇 娠 振 浸 真 針 深 紳 進 森 診 寝 慎 新 審 震 薪 親 人 刃 仁 尽 迅 甚 陣 尋 腎" +

                "す" +
                "須 図 水 吹 垂 炊 帥 粋 衰 推 酔 遂 睡 穂 随 髄 枢 崇 数 据 杉 裾 寸" +

                "せ" +
                "瀬 是 井 世 正 生 成 西 声 制 姓 征 性 青 斉 政 星 牲 省 凄 逝 清 盛 婿 晴 勢 聖 誠 精 製 誓 静 請 整 醒 税 夕 斥 石 赤 昔 析 席 脊 隻 惜 戚 責 跡 積 績 籍 切 折 拙 窃 接 設 雪 摂 節 説 舌 絶 千 川 仙 占 先 宣 専 泉 浅 洗 染 扇 栓 旋 船 戦 煎 羨 腺 詮 践 箋 銭 潜 線 遷 選 薦 繊 鮮 全 前 善 然 禅 漸 膳 繕" +

                "そ" +
                "狙 阻 祖 租 素 措 粗 組 疎 訴 塑 遡 礎 双 壮 早 争 走 奏 相 荘 草 送 倉 捜 挿 桑 巣 掃 曹 曽 爽 窓 創 喪 痩 葬 装 僧 想 層 総 遭 槽 踪 操 燥 霜 騒 藻 造 像 増 憎 蔵 贈 臓 即 束 足 促 則 息 捉 速 側 測 俗 族 属 賊 続 卒 率 存 村 孫 尊 損 遜" +

                "た" +
                "他 多 汰 打 妥 唾 堕 惰 駄 太 対 体 耐 待 怠 胎 退 帯 泰 堆 袋 逮 替 貸 隊 滞 態 戴 大 代 台 第 題 滝 宅 択 沢 卓 拓 託 濯 諾 濁 但 達 脱 奪 棚 誰 丹 旦 担 単 炭 胆 探 淡 短 嘆 端 綻 誕 鍛 団 男 段 断 弾 暖 談 壇" +

                "ち" +
                "地 池 知 値 恥 致 遅 痴 稚 置 緻 竹 畜 逐 蓄 築 秩 窒 茶 着 嫡 中 仲 虫 沖 宙 忠 抽 注 昼 柱 衷 酎 鋳 駐 著 貯 丁 弔 庁 兆 町 長 挑 帳 張 彫 眺 釣 頂 鳥 朝 貼 超 腸 跳 徴 嘲 潮 澄 調 聴 懲 直 勅 捗 沈 珍 朕 陳 賃 鎮" +

                "つ" +
                "追 椎 墜 通 痛 塚 漬 坪 爪 鶴" +

                "て" +
                "低 呈 廷 弟 定 底 抵 邸 亭 貞 帝 訂 庭 逓 停 偵 堤 提 程 艇 締 諦 泥 的 笛 摘 滴 適 敵 溺 迭 哲 鉄 徹 撤 天 典 店 点 展 添 転 填 田 伝 殿 電" +

                "と" +
                "斗 吐 妬 徒 途 都 渡 塗 賭 土 奴 努 度 怒 刀 冬 灯 当 投 豆 東 到 逃 倒 凍 唐 島 桃 討 透 党 悼 盗 陶 塔 搭 棟 湯 痘 登 答 等 筒 統 稲 踏 糖 頭 謄 藤 闘 騰 同 洞 胴 動 堂 童 道 働 銅 導 瞳 峠 匿 特 得 督 徳 篤 毒 独 読 栃 凸 突 届 屯 豚 頓 貪 鈍 曇 丼" +

                "な" +
                "那 奈 内 梨 謎 鍋 南 軟 難" +

                "に" +
                "二 尼 弐 匂 肉 虹 日 入 乳 尿 任 妊 忍 認" +

                "ぬ" +
                "ね" +
                "寧 熱 年 念 捻 粘 燃" +

                "の" +
                "悩 納 能 脳 農 濃" +

                "は" +
                "把 波 派 破 覇 馬 婆 罵 拝 杯 背 肺 俳 配 排 敗 廃 輩 売 倍 梅 培 陪 媒 買 賠 白 伯 拍 泊 迫 剥 舶 博 薄 麦 漠 縛 爆 箱 箸 畑 肌 八 鉢 発 髪 伐 抜 罰 閥 反 半 氾 犯 帆 汎 伴 判 坂 阪 板 版 班 畔 般 販 斑 飯 搬 煩 頒 範 繁 藩 晩 番 蛮 盤" +

                "ひ" +
                "比 皮 妃 否 批 彼 披 肥 非 卑 飛 疲 秘 被 悲 扉 費 碑 罷 避 尾 眉 美 備 微 鼻 膝 肘 匹 必 泌 筆 姫 百 氷 表 俵 票 評 漂 標 苗 秒 病 描 猫 品 浜 貧 賓 頻 敏 瓶" +

                "ふ" +
                "不 夫 父 付 布 扶 府 怖 阜 附 訃 負 赴 浮 婦 符 富 普 腐 敷 膚 賦 譜 侮 武 部 舞 封 風 伏 服 副 幅 復 福 腹 複 覆 払 沸 仏 物 粉 紛 雰 噴 墳 憤 奮 分 文 聞" +

                "へ" +
                "丙 平 兵 併 並 柄 陛 閉 塀 幣 弊 蔽 餅 米 壁 璧 癖 別 蔑 片 辺 返 変 偏 遍 編 弁 便 勉" +

                "ほ" +
                "歩 保 哺 捕 補 舗 母 募 墓 慕 暮 簿 方 包 芳 邦 奉 宝 抱 放 法 泡 胞 俸 倣 峰 砲 崩 訪 報 蜂 豊 飽 褒 縫 亡 乏 忙 坊 妨 忘 防 房 肪 某 冒 剖 紡 望 傍 帽 棒 貿 貌 暴 膨 謀 頬 北 木 朴 牧 睦 僕 墨 撲 没 勃 堀 本 奔 翻 凡 盆" +

                "ま" +
                "麻 摩 磨 魔 毎 妹 枚 昧 埋 幕 膜 枕 又 末 抹 万 満 慢 漫" +

                "み" +
                "未 味 魅 岬 密 蜜 脈 妙 民 眠" +

                "む" +
                "矛 務 無 夢 霧 娘" +

                "め" +
                "名 命 明 迷 冥 盟 銘 鳴 滅 免 面 綿 麺" +

                "も" +
                "茂 模 毛 妄 盲 耗 猛 網 目 黙 門 紋 問" +

                "や" +
                "冶 夜 野 弥 厄 役 約 訳 薬 躍 闇" +

                "ゆ" +
                "由 油 喩 愉 諭 輸 癒 唯 友 有 勇 幽 悠 郵 湧 猶 裕 遊 雄 誘 憂 融 優" +

                "よ" +
                "与 予 余 誉 預 幼 用 羊 妖 洋 要 容 庸 揚 揺 葉 陽 溶 腰 様 瘍 踊 窯 養 擁 謡 曜 抑 沃 浴 欲 翌 翼" +

                "ら" +
                "拉 裸 羅 来 雷 頼 絡 落 酪 辣 乱 卵 覧 濫 藍 欄" +

                "り" +
                "吏 利 里 理 痢 裏 履 璃 離 陸 立 律 慄 略 柳 流 留 竜 粒 隆 硫 侶 旅 虜 慮 了 両 良 料 涼 猟 陵 量 僚 領 寮 療 瞭 糧 力 緑 林 厘 倫 輪 隣 臨" +

                "る" +
                "瑠 涙 累 塁 類" +

                "れ" +
                "令 礼 冷 励 戻 例 鈴 零 霊 隷 齢 麗 暦 歴 列 劣 烈 裂 恋 連 廉 練 錬" +

                "ろ" +
                "呂 炉 賂 路 露 老 労 弄 郎 朗 浪 廊 楼 漏 籠 六 録 麓 論" +

                "わ" +
                "和 話 賄 脇 惑 枠 湾 腕";

            xString3 = xString3.Replace ("\x20", "");

            // Hiragana (Unicode block) - Wikipedia
            // https://en.wikipedia.org/wiki/Hiragana_(Unicode_block)

            var xKanji3 = xString3.ToCharArray ().Where (x => (x >= 0x3040 && x <= 0x309F) == false);

            // Console.WriteLine (xKanji3.Count ()); // → 2136
            #endregion

            #region 第1グループの後半をチェック → 不正確
            // 第1グループの後半を統合・ソートしたものが、第3グループ（全文字・ウィクショナリー）から第2グループ（1～6年生・文部科学省）の分を引いてソートしたものと一致しなかった

            var xHoge = xKanji1.Skip (6).SelectMany (x => x).OrderBy (y => y);
            var xMoge = xKanji3.Except (xKanji2.SelectMany (x => x)).OrderBy (y => y);

            // Console.WriteLine (string.Join (", ", xHoge.Except (xMoge))); // → 哨, 憚, 曾, 聘, 諜
            // Console.WriteLine (string.Join (", ", xMoge.Except (xHoge))); // → 刹, 勾, 哺, 曽, 柿, 椎, 楷, 毀, 睦, 賂, 賭, 遡, 釜, 錮

            // 差分として得られた全ての文字を文化庁の「常用漢字表」と照合した
            // 曾 → 「曽」の「康熙字典体」とのこと
            // 1行目の他の4文字は、常用漢字表に含まれていなかった
            // 2行目の文字は、いずれも含まれていた

            // 文化庁 | 国語施策・日本語教育 | 国語施策情報 | 内閣告示・内閣訓令 | 常用漢字表（平成22年内閣告示第2号）
            // https://www.bunka.go.jp/kokugo_nihongo/sisaku/joho/joho/kijun/naikaku/kanji/index.html

            // joyokanjihyo_20101130.pdf
            // https://www.bunka.go.jp/kokugo_nihongo/sisaku/joho/joho/kijun/naikaku/pdf/joyokanjihyo_20101130.pdf

            // 康熙字典 - Wikipedia
            // https://ja.wikipedia.org/wiki/%E5%BA%B7%E7%86%99%E5%AD%97%E5%85%B8

            // Hoge は、1文字も常用漢字のリストのメインの文字（つまり康熙字典体でないもの）と一致しなかった
            // Moge は、全てが一致した
            // Hoge は、文字数が正しくないことからも不正確なのが確定
            // Moge は、この時点では特に問題なし
            #endregion

            #region 第4グループ → 置換後は正確
            // 【みんなの知識 ちょっと便利帳】常用漢字一覧（2,136字）= 「常用漢字表」（2010年・平成22年11月30日内閣告示）」より
            // https://www.benricho.org/kanji/kyoikukanji/check-jyoyo-kanji-ichiran.html

            // 「あ」から「腕」をそのままコピペ

            // 追記: 一部の文字の置換により第3グループと一致したため、第3グループおよび置換後の第4グループの両方を正確と見なせる

            string xString4 = "あ亜哀挨愛曖悪握圧扱宛嵐安案暗い以衣位囲医依委威為畏胃尉異移萎偉椅彙意違維慰遺緯域育一壱逸茨芋引印因咽姻員院淫陰飲隠韻う右宇羽雨唄鬱畝浦運雲え永泳英映栄営詠影鋭衛易疫益液駅悦越謁閲円延沿炎怨宴媛援園煙猿遠鉛塩演縁艶お汚王凹央応往押旺欧殴桜翁奥横岡屋億憶臆虞乙俺卸音恩温穏か下化火加可仮何花佳価果河苛科架夏家荷華菓貨渦過嫁暇禍靴寡歌箇稼課蚊牙瓦我画芽賀雅餓介回灰会快戒改怪拐悔海界皆械絵開階塊楷解潰壊懐諧貝外劾害崖涯街慨蓋該概骸垣柿各角拡革格核殻郭覚較隔閣確獲嚇穫学岳楽額顎掛潟括活喝渇割葛滑褐轄且株釜鎌刈干刊甘汗缶完肝官冠巻看陥乾勘患貫寒喚堪換敢棺款間閑勧寛幹感漢慣管関歓監緩憾還館環簡観韓艦鑑丸含岸岩玩眼頑顔願き企伎危机気岐希忌汽奇祈季紀軌既記起飢鬼帰基寄規亀喜幾揮期棋貴棄毀旗器畿輝機騎技宜偽欺義疑儀戯擬犠議菊吉喫詰却客脚逆虐九久及弓丘旧休吸朽臼求究泣急級糾宮救球給嗅窮牛去巨居拒拠挙虚許距魚御漁凶共叫狂京享供協況峡挟狭恐恭胸脅強教郷境橋矯鏡競響驚仰暁業凝曲局極玉巾斤均近金菌勤琴筋僅禁緊錦謹襟吟銀く区句苦駆具惧愚空偶遇隅串屈掘窟熊繰君訓勲薫軍郡群け兄刑形系径茎係型契計恵啓掲渓経蛍敬景軽傾携継詣慶憬稽憩警鶏芸迎鯨隙劇撃激桁欠穴血決結傑潔月犬件見券肩建研県倹兼剣拳軒健険圏堅検嫌献絹遣権憲賢謙鍵繭顕験懸元幻玄言弦限原現舷減源厳こ己戸古呼固股虎孤弧故枯個庫湖雇誇鼓錮顧五互午呉後娯悟碁語誤護口工公勾孔功巧広甲交光向后好江考行坑孝抗攻更効幸拘肯侯厚恒洪皇紅荒郊香候校耕航貢降高康控梗黄喉慌港硬絞項溝鉱構綱酵稿興衡鋼講購乞号合拷剛傲豪克告谷刻国黒穀酷獄骨駒込頃今困昆恨根婚混痕紺魂墾懇さ左佐沙査砂唆差詐鎖座挫才再災妻采砕宰栽彩採済祭斎細菜最裁債催塞歳載際埼在材剤財罪崎作削昨柵索策酢搾錯咲冊札刷刹拶殺察撮擦雑皿三山参桟蚕惨産傘散算酸賛残斬暫し士子支止氏仕史司四市矢旨死糸至伺志私使刺始姉枝祉肢姿思指施師恣紙脂視紫詞歯嗣試詩資飼誌雌摯賜諮示字寺次耳自似児事侍治持時滋慈辞磁餌璽鹿式識軸七𠮟失室疾執湿嫉漆質実芝写社車舎者射捨赦斜煮遮謝邪蛇尺借酌釈爵若弱寂手主守朱取狩首殊珠酒腫種趣寿受呪授需儒樹収囚州舟秀周宗拾秋臭修袖終羞習週就衆集愁酬醜蹴襲十汁充住柔重従渋銃獣縦叔祝宿淑粛縮塾熟出述術俊春瞬旬巡盾准殉純循順準潤遵処初所書庶暑署緒諸女如助序叙徐除小升少召匠床抄肖尚招承昇松沼昭宵将消症祥称笑唱商渉章紹訟勝掌晶焼焦硝粧詔証象傷奨照詳彰障憧衝賞償礁鐘上丈冗条状乗城浄剰常情場畳蒸縄壌嬢錠譲醸色拭食植殖飾触嘱織職辱尻心申伸臣芯身辛侵信津神唇娠振浸真針深紳進森診寝慎新審震薪親人刃仁尽迅甚陣尋腎す須図水吹垂炊帥粋衰推酔遂睡穂随髄枢崇数据杉裾寸せ瀬是井世正生成西声制姓征性青斉政星牲省凄逝清盛婿晴勢聖誠精製誓静請整醒税夕斥石赤昔析席脊隻惜戚責跡積績籍切折拙窃接設雪摂節説舌絶千川仙占先宣専泉浅洗染扇栓旋船戦煎羨腺詮践箋銭潜線遷選薦繊鮮全前善然禅漸膳繕そ狙阻祖租素措粗組疎訴塑遡礎双壮早争走奏相荘草送倉捜挿桑巣掃曹曽爽窓創喪痩葬装僧想層総遭槽踪操燥霜騒藻造像増憎蔵贈臓即束足促則息捉速側測俗族属賊続卒率存村孫尊損遜た他多汰打妥唾堕惰駄太対体耐待怠胎退帯泰堆袋逮替貸隊滞態戴大代台第題滝宅択沢卓拓託濯諾濁但達脱奪棚誰丹旦担単炭胆探淡短嘆端綻誕鍛団男段断弾暖談壇ち地池知値恥致遅痴稚置緻竹畜逐蓄築秩窒茶着嫡中仲虫沖宙忠抽注昼柱衷酎鋳駐著貯丁弔庁兆町長挑帳張彫眺釣頂鳥朝貼超腸跳徴嘲潮澄調聴懲直勅捗沈珍朕陳賃鎮つ追椎墜通痛塚漬坪爪鶴て低呈廷弟定底抵邸亭貞帝訂庭逓停偵堤提程艇締諦泥的笛摘滴適敵溺迭哲鉄徹撤天典店点展添転塡田伝殿電と斗吐妬徒途都渡塗賭土奴努度怒刀冬灯当投豆東到逃倒凍唐島桃討透党悼盗陶塔搭棟湯痘登答等筒統稲踏糖頭謄藤闘騰同洞胴動堂童道働銅導瞳峠匿特得督徳篤毒独読栃凸突届屯豚頓貪鈍曇丼な那奈内梨謎鍋南軟難に二尼弐匂肉虹日入乳尿任妊忍認ね寧熱年念捻粘燃の悩納能脳農濃は把波派破覇馬婆罵拝杯背肺俳配排敗廃輩売倍梅培陪媒買賠白伯拍泊迫剝舶博薄麦漠縛爆箱箸畑肌八鉢発髪伐抜罰閥反半氾犯帆汎伴判坂阪板版班畔般販斑飯搬煩頒範繁藩晩番蛮盤ひ比皮妃否批彼披肥非卑飛疲秘被悲扉費碑罷避尾眉美備微鼻膝肘匹必泌筆姫百氷表俵票評漂標苗秒病描猫品浜貧賓頻敏瓶ふ不夫父付布扶府怖阜附訃負赴浮婦符富普腐敷膚賦譜侮武部舞封風伏服副幅復福腹複覆払沸仏物粉紛雰噴墳憤奮分文聞へ丙平兵併並柄陛閉塀幣弊蔽餅米壁璧癖別蔑片辺返変偏遍編弁便勉ほ歩保哺捕補舗母募墓慕暮簿方包芳邦奉宝抱放法泡胞俸倣峰砲崩訪報蜂豊飽褒縫亡乏忙坊妨忘防房肪某冒剖紡望傍帽棒貿貌暴膨謀頰北木朴牧睦僕墨撲没勃堀本奔翻凡盆ま麻摩磨魔毎妹枚昧埋幕膜枕又末抹万満慢漫み未味魅岬密蜜脈妙民眠む矛務無夢霧娘め名命明迷冥盟銘鳴滅免面綿麺も茂模毛妄盲耗猛網目黙門紋問や冶夜野弥厄役約訳薬躍闇ゆ由油喩愉諭輸癒唯友有勇幽悠郵湧猶裕遊雄誘憂融優よ与予余誉預幼用羊妖洋要容庸揚揺葉陽溶腰様瘍踊窯養擁謡曜抑沃浴欲翌翼ら拉裸羅来雷頼絡落酪辣乱卵覧濫藍欄り吏利里理痢裏履璃離陸立律慄略柳流留竜粒隆硫侶旅虜慮了両良料涼猟陵量僚領寮療瞭糧力緑林厘倫輪隣臨る瑠涙累塁類れ令礼冷励戻例鈴零霊隷齢麗暦歴列劣烈裂恋連廉練錬ろ呂炉賂路露老労弄郎朗浪廊楼漏籠六録麓論わ和話賄脇惑枠湾腕";

            // Visual Studio では「𠮟」が他の文字と異なる色で表示されていた
            // 文字コードは、U+20B9F になっていた
            // これは、Unicode の BMP に収まらず、char 一つで表現できず、Unicode においてもサロゲートペアとして2文字分の領域を必要とするもの

            // これを含む文字については、次のページに、The "New" column attempts to reflect the official glyph shapes as closely as possible
            //     This requires using the characters 𠮟, 塡, 剝, 頰 which are outside of Japan's basic character set, JIS X 0208 (one of them is also outside the Unicode BMP)
            //     In practice, these characters are usually replaced by the characters 叱, 填, 剥, 頬, which are present in JIS X 0208 とある

            // List of jōyō kanji - Wikipedia
            // https://en.wikipedia.org/wiki/List_of_j%C5%8Dy%C5%8D_kanji

            // これらの組み合わせが「どちらであっても同じ文字と見なされるべき」ということなら、置換せず、「どちらでも一致する comparer」を用意しなければならない
            // しかし、今、nJaJpCulture に与えたいのは、「この文章は○年生でも読めるのか」の判別や、漢字ベースのテストデータの生成などの簡単な機能のみ
            // ここにコードとコメントを残しておくことで、いずれ、もうちょっと作り込むことになったとしても思い出せる

            xString4 = xString4
                .Replace ("𠮟", "叱")
                .Replace ('塡', '填')
                .Replace ('剝', '剥')
                .Replace ('頰', '頬');

            var xKanji4 = xString4.ToCharArray ().Where (x => (x >= 0x3040 && x <= 0x309F) == false);
            #endregion

            #region 第3グループと第4グループの照合 → 一致
            // 第3グループはそのままで、第4グループでは JIS X 0208 に含まれない文字の置換により、これらのグループの文字が一致
            // 常用漢字に関する英語版の Wikipedia のページの情報と整合するため、これらのグループはいずれも正確と見なせる

            if (Enumerable.SequenceEqual (xKanji3.OrderBy (x => x), xKanji4.OrderBy (x => x)) == false)
                throw new nDataException ();
            #endregion

            #region 第5グループ → 正確
            // 人名用漢字 - Wikipedia
            // https://ja.wikipedia.org/wiki/%E4%BA%BA%E5%90%8D%E7%94%A8%E6%BC%A2%E5%AD%97

            // いずれも自動折り返しの文字列なのでそのままコピペ

            // 追記: 第6グループとの照合などにより正確と見なせる

            string xString5a = "丑 丞 乃 之 乎 也 云 亘‐亙 些 亦 亥 亨 亮 仔 伊 伍 伽 佃 佑 伶 侃 侑 俄 俠 俣 俐 倭 俱 倦 倖 偲 傭 儲 允 兎 兜 其 冴 凌 凜‐凛 凧 凪 凰 凱 函 劉 劫 勁 勺 勿 匁 匡 廿 卜 卯 卿 厨 厩 叉 叡 叢 叶 只 吾 吞 吻 哉 哨 啄 哩 喬 喧 喰 喋 嘩 嘉 嘗 噌 噂 圃 圭 坐 尭‐堯 坦 埴 堰 堺 堵 塙 壕 壬 夷 奄 奎 套 娃 姪 姥 娩 嬉 孟 宏 宋 宕 宥 寅 寓 寵 尖 尤 屑 峨 峻 崚 嵯 嵩 嶺 巌‐巖 巫 已 巳 巴 巷 巽 帖 幌 幡 庄 庇 庚 庵 廟 廻 弘 弛 彗 彦 彪 彬 徠 忽 怜 恢 恰 恕 悌 惟 惚 悉 惇 惹 惺 惣 慧 憐 戊 或 戟 托 按 挺 挽 掬 捲 捷 捺 捧 掠 揃 摑 摺 撒 撰 撞 播 撫 擢 孜 敦 斐 斡 斧 斯 於 旭 昂 昊 昏 昌 昴 晏 晃‐晄 晒 晋 晟 晦 晨 智 暉 暢 曙 曝 曳 朋 朔 杏 杖 杜 李 杭 杵 杷 枇 柑 柴 柘 柊 柏 柾 柚 桧‐檜 栞 桔 桂 栖 桐 栗 梧 梓 梢 梛 梯 桶 梶 椛 梁 棲 椋 椀 楯 楚 楕 椿 楠 楓 椰 楢 楊 榎 樺 榊 榛 槙‐槇 槍 槌 樫 槻 樟 樋 橘 樽 橙 檎 檀 櫂 櫛 櫓 欣 欽 歎 此 殆 毅 毘 毬 汀 汝 汐 汲 沌 沓 沫 洸 洲 洵 洛 浩 浬 淵 淳 渚‐渚 淀 淋 渥 渾 湘 湊 湛 溢 滉 溜 漱 漕 漣 澪 濡 瀕 灘 灸 灼 烏 焰 焚 煌 煤 煉 熙 燕 燎 燦 燭 燿 爾 牒 牟 牡 牽 犀 狼 猪‐猪 獅 玖 珂 珈 珊 珀 玲 琢‐琢 琉 瑛 琥 琶 琵 琳 瑚 瑞 瑶 瑳 瓜 瓢 甥 甫 畠 畢 疋 疏 皐 皓 眸 瞥 矩 砦 砥 砧 硯 碓 碗 碩 碧 磐 磯 祇 祢‐禰 祐‐祐 祷‐禱 禄‐祿 禎‐禎 禽 禾 秦 秤 稀 稔 稟 稜 穣‐穰 穹 穿 窄 窪 窺 竣 竪 竺 竿 笈 笹 笙 笠 筈 筑 箕 箔 篇 篠 簞 簾 籾 粥 粟 糊 紘 紗 紐 絃 紬 絆 絢 綺 綜 綴 緋 綾 綸 縞 徽 繫 繡 纂 纏 羚 翔 翠 耀 而 耶 耽 聡 肇 肋 肴 胤 胡 脩 腔 脹 膏 臥 舜 舵 芥 芹 芭 芙 芦 苑 茄 苔 苺 茅 茉 茸 茜 莞 荻 莫 莉 菅 菫 菖 萄 菩 萌‐萠 萊 菱 葦 葵 萱 葺 萩 董 葡 蓑 蒔 蒐 蒼 蒲 蒙 蓉 蓮 蔭 蔣 蔦 蓬 蔓 蕎 蕨 蕉 蕃 蕪 薙 蕾 蕗 藁 薩 蘇 蘭 蝦 蝶 螺 蟬 蟹 蠟 衿 袈 袴 裡 裟 裳 襖 訊 訣 註 詢 詫 誼 諏 諄 諒 謂 諺 讃 豹 貰 賑 赳 跨 蹄 蹟 輔 輯 輿 轟 辰 辻 迂 迄 辿 迪 迦 這 逞 逗 逢 遥‐遙 遁 遼 邑 祁 郁 鄭 酉 醇 醐 醍 醬 釉 釘 釧 銑 鋒 鋸 錘 錐 錆 錫 鍬 鎧 閃 閏 閤 阿 陀 隈 隼 雀 雁 雛 雫 霞 靖 鞄 鞍 鞘 鞠 鞭 頁 頌 頗 顚 颯 饗 馨 馴 馳 駕 駿 驍 魁 魯 鮎 鯉 鯛 鰯 鱒 鱗 鳩 鳶 鳳 鴨 鴻 鵜 鵬 鷗 鷲 鷺 鷹 麒 麟 麿 黎 黛 鼎";

            // ASCII のハイフンでない
            xString5a = Regex.Replace (xString5a, @"[\x20‐]", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var xKanji5a = xString5a.ToCharArray ();

            // Console.WriteLine (xKanji5a.Count ()); // → 651

            string xString5b = "亞（亜） 惡（悪） 爲（為） 逸（逸） 榮（栄） 衞（衛） 謁（謁） 圓（円） 緣（縁） 薗（園） 應（応） 櫻（桜） 奧（奥） 橫（横） 溫（温） 價（価） 禍（禍） 悔（悔） 海（海） 壞（壊） 懷（懐） 樂（楽） 渴（渇） 卷（巻） 陷（陥） 寬（寛） 漢（漢） 氣（気） 祈（祈） 器（器） 僞（偽） 戲（戯） 虛（虚） 峽（峡） 狹（狭） 響（響） 曉（暁） 勤（勤） 謹（謹） 駈（駆） 勳（勲） 薰（薫） 惠（恵） 揭（掲） 鷄（鶏） 藝（芸） 擊（撃） 縣（県） 儉（倹） 劍（剣） 險（険） 圈（圏） 檢（検） 顯（顕） 驗（験） 嚴（厳） 廣（広） 恆（恒） 黃（黄） 國（国） 黑（黒） 穀（穀） 碎（砕） 雜（雑） 祉（祉） 視（視） 兒（児） 濕（湿） 實（実） 社（社） 者（者） 煮（煮） 壽（寿） 收（収） 臭（臭） 從（従） 澁（渋） 獸（獣） 縱（縦） 祝（祝） 暑（暑） 署（署） 緖（緒） 諸（諸） 敍（叙） 將（将） 祥（祥） 涉（渉） 燒（焼） 奬（奨） 條（条） 狀（状） 乘（乗） 淨（浄） 剩（剰） 疊（畳） 孃（嬢） 讓（譲） 釀（醸） 神（神） 眞（真） 寢（寝） 愼（慎） 盡（尽） 粹（粋） 醉（酔） 穗（穂） 瀨（瀬） 齊（斉） 靜（静） 攝（摂） 節（節） 專（専） 戰（戦） 纖（繊） 禪（禅） 祖（祖） 壯（壮） 爭（争） 莊（荘） 搜（捜） 巢（巣） 曾（曽） 裝（装） 僧（僧） 層（層） 瘦（痩） 騷（騒） 增（増） 憎（憎） 藏（蔵） 贈（贈） 臟（臓） 卽（即） 帶（帯） 滯（滞） 瀧（滝） 單（単） 嘆（嘆） 團（団） 彈（弾） 晝（昼） 鑄（鋳） 著（著） 廳（庁） 徵（徴） 聽（聴） 懲（懲） 鎭（鎮） 轉（転） 傳（伝） 都（都） 嶋（島） 燈（灯） 盜（盗） 稻（稲） 德（徳） 突（突） 難（難） 拜（拝） 盃（杯） 賣（売） 梅（梅） 髮（髪） 拔（抜） 繁（繁） 晚（晩） 卑（卑） 祕（秘） 碑（碑） 賓（賓） 敏（敏） 冨（富） 侮（侮） 福（福） 拂（払） 佛（仏） 勉（勉） 步（歩） 峯（峰） 墨（墨） 飜（翻） 每（毎） 萬（万） 默（黙） 埜（野） 彌（弥） 藥（薬） 與（与） 搖（揺） 樣（様） 謠（謡） 來（来） 賴（頼） 覽（覧） 欄（欄） 龍（竜） 虜（虜） 凉（涼） 綠（緑） 淚（涙） 壘（塁） 類（類） 禮（礼） 曆（暦） 歷（歴） 練（練） 鍊（錬） 郞（郎） 朗（朗） 廊（廊） 錄（録）";

            xString5b = Regex.Replace (xString5b, @"（.）|\x20", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var xKanji5b = xString5b.ToCharArray ();

            // Console.WriteLine (xKanji5b.Count ()); // → 212
            #endregion

            #region 第6グループ → 不正確
            // Jinmeiyō kanji - Wikipedia
            // https://en.wikipedia.org/wiki/Jinmeiy%C5%8D_kanji

            // いずれも自動折り返しの文字列なのでそのままコピペ
            // Jinmeiyō kanji not part of the jōyō kanji の後半の18文字は、全て前半にも入っている

            // 追記: 第5グループとの照合により、一部の文字に転写ミスがあるのが確認された
            // 文字数が一致し、文字も概ね一致したことなどから、第5グループの信頼性の補強と考える

            string xString6a = "丑⁠　丞⁠　乃⁠　之⁠　乎⁠　也⁠　云⁠　亘（亙）些⁠　亦⁠　亥⁠　亨⁠　亮⁠　仔⁠　伊⁠　伍⁠　伽⁠　佃⁠　佑⁠　伶⁠　侃⁠　侑⁠　俄⁠　俠⁠　俣⁠　俐⁠　倭⁠　俱⁠　倦⁠　倖⁠　偲⁠　傭⁠　儲⁠　允⁠　兎⁠　兜⁠　其⁠　冴⁠　凌⁠　凜（凛）凧⁠　凪⁠　凰⁠　凱⁠　函⁠　劉⁠　劫⁠　勁⁠　勺⁠　勿⁠　匁⁠　匡⁠　廿⁠　卜⁠　卯⁠　卿⁠　厨⁠　厩⁠　叉⁠　叡⁠　叢⁠　叶⁠　只⁠　吾⁠　吞⁠　吻⁠　哉⁠　哨⁠　啄⁠　哩⁠　喬⁠　喧⁠　喰⁠　喋⁠　嘩⁠　嘉⁠　嘗⁠　噌⁠　噂⁠　圃⁠　圭⁠　坐⁠　尭（堯）坦⁠　埴⁠　堰⁠　堺⁠　堵⁠　塙⁠　壕⁠　壬⁠　夷⁠　奄⁠　奎⁠　套⁠　娃⁠　姪⁠　姥⁠　娩⁠　嬉⁠　孟⁠　宏⁠　宋⁠　宕⁠　宥⁠　寅⁠　寓⁠　寵⁠　尖⁠　尤⁠　屑⁠　峨⁠　峻⁠　崚⁠　嵯⁠　嵩⁠　嶺⁠　巌（巖）巫⁠　已⁠　巳⁠　巴⁠　巷⁠　巽⁠　帖⁠　幌⁠　幡⁠　庄⁠　庇⁠　庚⁠　庵⁠　廟⁠　廻⁠　弘⁠　弛⁠　彗⁠　彦⁠　彪⁠　彬⁠　徠⁠　忽⁠　怜⁠　恢⁠　恰⁠　恕⁠　悌⁠　惟⁠　惚⁠　悉⁠　惇⁠　惹⁠　惺⁠　惣⁠　慧⁠　憐⁠　戊⁠　或⁠　戟⁠　托⁠　按⁠　挺⁠　挽⁠　掬⁠　捲⁠　捷⁠　捺⁠　捧⁠　掠⁠　揃⁠　摑⁠　摺⁠　撒⁠　撰⁠　撞⁠　播⁠　撫⁠　擢⁠　孜⁠　敦⁠　斐⁠　斡⁠　斧⁠　斯⁠　於⁠　旭⁠　昂⁠　昊⁠　昏⁠　昌⁠　昴⁠　晏⁠　晃（晄）晒⁠　晋⁠　晟⁠　晦⁠　晨⁠　智⁠　暉⁠　暢⁠　曙⁠　曝⁠　曳⁠　朋⁠　朔⁠　杏⁠　杖⁠　杜⁠　李⁠　杭⁠　杵⁠　杷⁠　枇⁠　柑⁠　柴⁠　柘⁠　柊⁠　柏⁠　柾⁠　柚⁠　桧（檜）栞⁠　桔⁠　桂⁠　栖⁠　桐⁠　栗⁠　梧⁠　梓⁠　梢⁠　梛⁠　梯⁠　桶⁠　梶⁠　椛⁠　梁⁠　棲⁠　椋⁠　椀⁠　楯⁠　楚⁠　楕⁠　椿⁠　楠⁠　楓⁠　椰⁠　楢⁠　楊⁠　榎⁠　樺⁠　榊⁠　榛⁠　槙（槇）槍⁠　槌⁠　樫⁠　槻⁠　樟⁠　樋⁠　橘⁠　樽⁠　橙⁠　檎⁠　檀⁠　櫂⁠　櫛⁠　櫓⁠　欣⁠　欽⁠　歎⁠　此⁠　殆⁠　毅⁠　毘⁠　毬⁠　汀⁠　汝⁠　汐⁠　汲⁠　沌⁠　沓⁠　沫⁠　洸⁠　洲⁠　洵⁠　洛⁠　浩⁠　浬⁠　淵⁠　淳⁠　渚（渚︀）淀⁠　淋⁠　渥⁠　渾⁠　湘⁠　湊⁠　湛⁠　溢⁠　滉⁠　溜⁠　漱⁠　漕⁠　漣⁠　澪⁠　濡⁠　瀕⁠　灘⁠　灸⁠　灼⁠　烏⁠　焰⁠　焚⁠　煌⁠　煤⁠　煉⁠　熙⁠　燕⁠　燎⁠　燦⁠　燭⁠　燿⁠　爾⁠　牒⁠　牟⁠　牡⁠　牽⁠　犀⁠　狼⁠　猪（猪︀）獅⁠　玖⁠　珂⁠　珈⁠　珊⁠　珀⁠　玲⁠　琢（琢︀）琉⁠　瑛⁠　琥⁠　琶⁠　琵⁠　琳⁠　瑚⁠　瑞⁠　瑶⁠　瑳⁠　瓜⁠　瓢⁠　甥⁠　甫⁠　畠⁠　畢⁠　疋⁠　疏⁠　皐⁠　皓⁠　眸⁠　瞥⁠　矩⁠　砦⁠　砥⁠　砧⁠　硯⁠　碓⁠　碗⁠　碩⁠　碧⁠　磐⁠　磯⁠　祇⁠　祢（禰）祐（祐︀）祷（禱）禄（祿）禎（禎︀）禽⁠　禾⁠　秦⁠　秤⁠　稀⁠　稔⁠　稟⁠　稜⁠　穣（穰）穹⁠　穿⁠　窄⁠　窪⁠　窺⁠　竣⁠　竪⁠　竺⁠　竿⁠　笈⁠　笹⁠　笙⁠　笠⁠　筈⁠　筑⁠　箕⁠　箔⁠　篇⁠　篠⁠　簞⁠　簾⁠　籾⁠　粥⁠　粟⁠　糊⁠　紘⁠　紗⁠　紐⁠　絃⁠　紬⁠　絆⁠　絢⁠　綺⁠　綜⁠　綴⁠　緋⁠　綾⁠　綸⁠　縞⁠　徽⁠　繫⁠　繡⁠　纂⁠　纏⁠　羚⁠　翔⁠　翠⁠　耀⁠　而⁠　耶⁠　耽⁠　聡⁠　肇⁠　肋⁠　肴⁠　胤⁠　胡⁠　脩⁠　腔⁠　脹⁠　膏⁠　臥⁠　舜⁠　舵⁠　芥⁠　芹⁠　芭⁠　芙⁠　芦⁠　苑⁠　茄⁠　苔⁠　苺⁠　茅⁠　茉⁠　茸⁠　茜⁠　莞⁠　荻⁠　莫⁠　莉⁠　菅⁠　菫⁠　菖⁠　萄⁠　菩⁠　萌（萠）萊⁠　菱⁠　葦⁠　葵⁠　萱⁠　葺⁠　萩⁠　董⁠　葡⁠　蓑⁠　蒔⁠　蒐⁠　蒼⁠　蒲⁠　蒙⁠　蓉⁠　蓮⁠　蔭⁠　蔣⁠　蔦⁠　蓬⁠　蔓⁠　蕎⁠　蕨⁠　蕉⁠　蕃⁠　蕪⁠　薙⁠　蕾⁠　蕗⁠　藁⁠　薩⁠　蘇⁠　蘭⁠　蝦⁠　蝶⁠　螺⁠　蟬⁠　蟹⁠　蠟⁠　衿⁠　袈⁠　袴⁠　裡⁠　裟⁠　裳⁠　襖⁠　訊⁠　訣⁠　註⁠　詢⁠　詫⁠　誼⁠　諏⁠　諄⁠　諒⁠　謂⁠　諺⁠　讃⁠　豹⁠　貰⁠　賑⁠　赳⁠　跨⁠　蹄⁠　蹟⁠　輔⁠　輯⁠　輿⁠　轟⁠　辰⁠　辻⁠　迂⁠　迄⁠　辿⁠　迪⁠　迦⁠　這⁠　逞⁠　逗⁠　逢⁠　遥（遙）遁⁠　遼⁠　邑⁠　祁⁠　郁⁠　鄭⁠　酉⁠　醇⁠　醐⁠　醍⁠　醬⁠　釉⁠　釘⁠　釧⁠　銑⁠　鋒⁠　鋸⁠　錘⁠　錐⁠　錆⁠　錫⁠　鍬⁠　鎧⁠　閃⁠　閏⁠　閤⁠　阿⁠　陀⁠　隈⁠　隼⁠　雀⁠　雁⁠　雛⁠　雫⁠　霞⁠　靖⁠　鞄⁠　鞍⁠　鞘⁠　鞠⁠　鞭⁠　頁⁠　頌⁠　頗⁠　顚⁠　颯⁠　饗⁠　馨⁠　馴⁠　馳⁠　駕⁠　駿⁠　驍⁠　魁⁠　魯⁠　鮎⁠　鯉⁠　鯛⁠　鰯⁠　鱒⁠　鱗⁠　鳩⁠　鳶⁠　鳳⁠　鴨⁠　鴻⁠　鵜⁠　鵬⁠　鷗⁠　鷲⁠　鷺⁠　鷹⁠　麒⁠　麟⁠　麿⁠　黎⁠　黛⁠　鼎";

            // 見えず、幅もない文字があるようだったのでページのソースを見たところ、&#x2060; というのが文字ごとに入っていた
            // Zero width no-break space とも呼ばれるようで、括弧とその直前の文字がページ右端での折り返しにより分断されないために使われている

            // Word joiner - Wikipedia
            // https://en.wikipedia.org/wiki/Word_joiner

            // 文字数が合わなかったのでさらに調べたところ、variation selectors というものも含まれていた
            // IT は開発が英語圏なので英語で一次情報を見ているが、「異体字セレクター」については、日本語ページの方が圧倒的に詳しい

            // “◌︀” U+FE00 Variation Selector-1 (VS1) Unicode Character
            // https://www.compart.com/en/unicode/U+FE00

            // “◌︁” U+FE01 Variation Selector-2 (VS2) Unicode Character
            // https://www.compart.com/en/unicode/U+FE01

            // Variation Selectors (Unicode block) - Wikipedia
            // https://en.wikipedia.org/wiki/Variation_Selectors_(Unicode_block)

            // 異体字セレクタ - Wikipedia
            // https://ja.wikipedia.org/wiki/%E7%95%B0%E4%BD%93%E5%AD%97%E3%82%BB%E3%83%AC%E3%82%AF%E3%82%BF

            // Unicode は、サロゲートペアだけでも（海外の個人プログラマーは知らず）トラブルになるのに、異体字セレクターは、なおのことめんどくさい
            // Nekote としては、暫定的に「存在しても気にしないが、検索などにおいて特に配慮することはなく、存在する場合の検索などの結果は不定」と考える

            // セキュリティーリスクとしては、既に取られているハンドル名を word joiner や異体字セレクターにより重複して取られるなどが考えられる
            // それなりの人数が利用し、互いに面識がなく、運用におけるチェックの弱いシステムにおいては、なりすましが可能となってくる
            // Unicode ブロックを見て、使える文字を制限していけば、東南アジアやアラブの文字を含むハンドル名を取れないなどの不都合が生じうる
            // 「ここからの文字列は右から左へ」という記号にも遭遇したことがある
            // グローバルなシステムを作るにおいては、日本人の限られた知識で文字を縛るのは難しい
            // ID を「英数字 + 少しの記号」で作ってもらい、一意性をそちらに頼り、ハンドル名などについては問題の報告を受けるのが現実的か

            // Right-to-left mark - Wikipedia
            // https://en.wikipedia.org/wiki/Right-to-left_mark

            xString6a = Regex.Replace (xString6a, @"[　\u2060\uFE00\uFE01（）]", "", RegexOptions.Compiled | RegexOptions.CultureInvariant); // 空白は全角

            var xKanji6a = xString6a.ToCharArray ();

            // Console.WriteLine (xKanji6a.Count ()); // → 651

            // 異体字セレクターを除き、第5グループと同じに見える
            // おそらくこれが共通のソースなのだろうというものが見付かった
            // しかし、アウトライン化されているのか、ここからの文字のコピーはできないようだ
            // 追記: おそらくそれが原因で、第6グループには転写のミスが多い

            // 法務省：子の名に使える漢字
            // https://www.moj.go.jp/MINJI/minji86.html

            // 加除_17_05_30　別表第二の表.pwd
            // https://www.moj.go.jp/content/001131003.pdf

            string xString6b = "亞（亜） 惡（悪） 爲（為） 逸︁（逸） 榮（栄） 衞（衛） 謁︀（謁） 圓（円） 緣（縁） 薗（園） 應（応） 櫻（桜） 奧（奥） 橫（横） 溫（温） 價（価） 禍︀（禍） 悔︀（悔） 海︀（海） 壞（壊） 懷（懐） 樂（楽） 渴（渇） 卷（巻） 陷（陥） 寬（寛） 漢︀（漢） 氣（気） 祈︀（祈） 器︀（器） 僞（偽） 戲（戯） 虛（虚） 峽（峡） 狹（狭） 響︀（響） 曉（暁） 勤︀（勤） 謹︀（謹） 駈（駆） 勳（勲） 薰（薫） 惠（恵） 揭（掲） 鷄（鶏） 藝（芸） 擊（撃） 縣（県） 儉（倹） 劍（剣） 險（険） 圈（圏） 檢（検） 顯（顕） 驗（験） 嚴（厳） 廣（広） 恆（恒） 黃（黄） 國（国） 黑（黒） 穀︀（穀） 碎（砕） 雜（雑） 祉︀（祉） 視︀（視） 兒（児） 濕（湿） 實（実） 社︀（社） 者︀（者） 煮︀（煮） 壽（寿） 收（収） 臭︀（臭） 從（従） 澁（渋） 獸（獣） 縱（縦） 祝︀（祝） 暑︀（暑） 署︀（署） 緖（緒） 諸︀（諸） 敍（叙） 將（将） 祥︀（祥） 涉（渉） 燒（焼） 奬（奨） 條（条） 狀（状） 乘（乗） 淨（浄） 剩（剰） 疊（畳） 孃（嬢） 讓（譲） 釀（醸） 神︀（神） 眞（真） 寢（寝） 愼（慎） 盡（尽） 粹（粋） 醉（酔） 穗（穂） 瀨（瀬） 齊（斉） 靜（静） 攝（摂） 節︀（節） 專（専） 戰（戦） 纖（繊） 禪（禅） 祖︀（祖） 壯（壮） 爭（争） 莊（荘） 搜（捜） 巢（巣） 曾（曽） 裝（装） 僧︀（僧） 層︀（層） 瘦（痩） 騷（騒） 增（増） 憎︀（憎） 藏（蔵） 贈︀（贈） 臟（臓） 卽（即） 帶（帯） 滯（滞） 瀧（滝） 單（単） 嘆︀（嘆） 團（団） 彈（弾） 晝（昼） 鑄（鋳） 著︀（著） 廳（庁） 徵（徴） 聽（聴） 懲︀（懲） 鎭（鎮） 轉（転） 傳（伝） 都︀（都） 嶋（島） 燈（灯） 盜（盗） 稻（稲） 德（徳） 突︀（突） 難︀（難） 拜（拝） 盃（杯） 賣（売） 梅︀（梅） 髮（髪） 拔（抜） 繁︀（繁） 晚（晩） 卑︀（卑） 祕（秘） 碑︀（碑） 賓︀（賓） 敏︀（敏） 冨（富） 侮︀（侮） 福︀（福） 拂（払） 佛（仏） 勉︀（勉） 步（歩） 峯（峰） 墨︀（墨） 飜（翻） 每（毎） 萬（万） 默（黙） 埜（野） 彌（弥） 藥（薬） 與（与） 搖（揺） 樣（様） 謠（謡） 來（来） 賴（頼） 覽（覧） 欄︀（欄） 龍（竜） 虜︀（虜） 凉（涼） 綠（緑） 淚（涙） 壘（塁） 類︀（類） 禮（礼） 曆（暦） 歷（歴） 練︁（練） 鍊（錬） 郞（郎） 朗︀（朗） 廊︀（廊） 錄（録）";

            xString6b = Regex.Replace (xString6b, @"（.）|[\x20\uFE00\uFE01]", "", RegexOptions.Compiled | RegexOptions.CultureInvariant);

            var xKanji6b = xString6b.ToCharArray ();

            // Console.WriteLine (xKanji6b.Count ()); // → 212
            #endregion

            #region 第5グループと第6グループの照合 → 第5グループは正確
            // まず B について判断を下してから、A についても
            // たまたまそういう流れの作業になっただけで、特に理由はない

            // 第5グループと第6グループの A が一致しなかったので、差分を調べた
            // 文字数が同じなので、第6グループの A には、グループ内の重複が考えられた

            // 「猪」以外、コンソールでは文字化けした
            // Console.WriteLine (string.Join (", ", xKanji5a.Except (xKanji6a))); // → 渚, 猪, 琢, 祐, 禎

            // Console.WriteLine (xKanji6a.Except (xKanji5a).Count ()); // → 0

            // A について判断する前に B についても調べてみた

            // 多くの文字がコンソールでは文字化けした
            // Console.WriteLine (string.Join (", ", xKanji5b.Except (xKanji6b))); // → 逸, 謁, 禍, 悔, 海, 漢, 祈, 器, 響, 勤, 謹, 穀, 祉, 視, 社, 者, 煮, 臭, 祝, 暑, 署, 諸, 祥, 神, 節, 祖, 僧, 層, 憎, 贈, 嘆, 著, 懲, 都, 突, 難, 梅, 繁, 卑, 碑, 賓, 敏, 侮, 福, 勉, 墨, 欄, 虜, 類, 練, 朗, 廊

            // 全ての文字がコンソールでも正しく表示された
            // Console.WriteLine (string.Join (", ", xKanji6b.Except (xKanji5b))); // → 逸, 謁, 禍, 悔, 海, 漢, 祈, 器, 響, 勤, 謹, 穀, 祉, 視, 社, 者, 煮, 臭, 祝, 暑, 署, 諸, 祥, 神, 節, 祖, 僧, 層, 憎, 贈, 嘆, 著, 懲, 都, 突, 難, 梅, 繁, 卑, 碑, 賓, 敏, 侮, 福, 勉, 墨, 欄, 虜, 類, 練, 朗, 廊

            // 改めて Traditional variants of jōyō kanji のところを見れば、「逸︁（逸）」など、括弧の内外の文字が一致するところが散見された
            // 第5グループの B には、第3グループ（常用漢字）に含まれる文字が一つもない
            // 第6グループの B には52文字あり、これは第5グループの B との差分の数と一致した

            if (xKanji5b.Count (x => xKanji3.Contains (x)) > 0)
                throw new nDataException ();

            // Console.WriteLine (xKanji6b.Count (x => xKanji3.Contains (x))); // → 52
            // Console.WriteLine (xKanji6b.Except (xKanji5b).Count ()); // → 52

            // 上記のコメント部分を見比べるにおいて、52の文字全てが同じ文字の異体字と普通（？）の字体の組み合わせになっていること、
            //     第5グループの B と第3グループに重複がないことの二つから、第5グループの B は正しいと見なせる

            // といったことから A についても見たところ、Jinmeiyō kanji not part of the jōyō kanji のところに「渚（渚︀）」など、同一文字の重複が認められた
            // B と同じ問題ということになる
            // 渚, 猪, 琢, 祐, 禎 → 日本語のページでは、いずれも、少し違った文字と隣同士になっていて、英語のページでは、括弧の内外で文字が重複していた
            // 文字数の一致、B で信頼性を高めた第5グループであること、5文字を検索しての結果が納得のいくものであったことから、第5グループの A も正しいと見なせる

            // A は、第5～6グループのいずれにおいても「常用漢字以外の文字とその異体字」なので、第3グループの常用漢字と重複しない
            // 第6グループでは、「常用漢字以外の文字」と「その異体字」での重複があったが、常用漢字との重複はないようだ

            if (xKanji5a.Count (x => xKanji3.Contains (x)) > 0)
                throw new nDataException ();

            if (xKanji6a.Count (x => xKanji3.Contains (x)) > 0)
                throw new nDataException ();
            #endregion

            // ここまでの結論

            // 常用漢字 → 第3～4グループのいずれか（4文字、便宜的に JIS X 0208 に寄せられているので留意）
            // 学年ごとの漢字 → 第2グループで1～6年生、それらを常用漢字から引いて中学（以降？）
            // 人名用漢字 → 第5グループ

            #region JIS X 0208 との関係 → 人名用漢字の多くは範囲外
            // それぞれのリストの文字が JIS X 0208 に含まれるか

            // Encoding.GetEncoding に指定する文字列を調べた
            // 最近の .NET では Encoding.RegisterProvider が必要とのこと

            // Encoding.GetEncodings Method (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencodings

            Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);

            // foreach (var xEncoding in Encoding.GetEncodings ())
            //     Console.WriteLine (xEncoding.Name);

            // Æ (U+00C6) が変換先のエンコーディングで表現できないから AE (U+0041 + U+0045) に置き換えるなどの仕組み
            // EncoderReplacementFallback については、replaces each byte sequence that cannot be decoded
            //     with a question mark character ("?", or U+003F) or a REPLACEMENT CHARACTER (U+FFFD) とのこと
            // そのコンストラクターに string.Empty を与えることで、「変換できなければ "" に」という挙動に
            // DecoderFallback.ReplacementFallback の指定は、例外を投げるなどの他の挙動もあっての、既存の挙動からの一つの選択

            // Encoding.GetEncoding Method (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencoding

            // EncoderFallback Class (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoderfallback

            // EncoderReplacementFallback Class (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.encoderreplacementfallback

            // DecoderFallback Class (System.Text) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.text.decoderfallback

            Encoding xShiftJis = Encoding.GetEncoding ("shift_jis", new EncoderReplacementFallback (string.Empty), DecoderFallback.ReplacementFallback);

            void iCheck (IEnumerable <char> values)
            {
                List <char> xChars = new List <char> ();

                foreach (char xValue in values)
                {
                    // ついでにサロゲートペアの文字でないかチェック

                    // Char.IsSurrogate Method (System) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.char.issurrogate

                    // Char.IsSurrogate の戻り値は、true if the character at position index in s is a either a high surrogate or a low surrogate; otherwise, false とのこと
                    // サロゲートペアは、high → low の順序になっていて、それぞれの範囲は U+D800 to U+DBFF および U+DC00 to U+DFFF とのこと
                    // これらの範囲は重複しないため、サロゲートペアの文字が見付かれば、それが前か後ろかは一発で分かる
                    // Char.IsSurrogate は、前後をまとめて U+D800 to U+DFFF の範囲内かどうかを調べる

                    // Char.IsHighSurrogate Method (System) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.char.ishighsurrogate

                    // Char.IsLowSurrogate Method (System) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.char.islowsurrogate

                    if (char.IsSurrogate (xValue))
                        throw new nDataException ();

                    // EncoderReplacementFallback の指定により、変換できれば "" が長さ0のバイト列になる

                    if (xShiftJis.GetByteCount (xValue.ToString ()) == 0)
                        xChars.Add (xValue);
                }

                // 一応ソートしてファイルに出力
                // コンソールだと文字化けするものが多い

                // nFile.AppendAllText (nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), "Hoge.txt"),
                //     string.Join (", ", xChars.OrderBy (x => x)) + Environment.NewLine);
            }

            iCheck (xKanji3); // → なし

            for (int temp = 0; temp < 6; temp ++)
                iCheck (xKanji2.ElementAt (temp)); // → なし

            iCheck (xKanji5a); // → 俠, 俱, 吞, 摑, 焰, 禱, 簞, 繡, 繫, 萊, 蔣, 蟬, 蠟, 醬, 顚, 鷗, 渚, 琢, 祐, 禎
            iCheck (xKanji5b); // → 卽, 巢, 徵, 揭, 擊, 晚, 曆, 步, 歷, 每, 涉, 淚, 渴, 溫, 狀, 瘦, 緣, 虛, 錄, 鍊, 黃, 欄, 廊, 虜, 類, 侮, 僧, 勉, 勤, 卑, 嘆, 器, 墨, 層, 悔, 憎, 懲, 敏, 暑, 梅, 海, 漢, 煮, 碑, 社, 祉, 祈, 祖, 祝, 禍, 穀, 突, 節, 練, 繁, 署, 者, 臭, 著, 視, 謁, 謹, 賓, 贈, 逸, 難, 響

            // 常用漢字でも JIS X 0208 外の文字が四つあり、人名用漢字には多数あると判明
            // となると、常用漢字の四つについても、フィールドを用意しなければならない
            #endregion

            #region nJaJpCulture.cs のチェック
            int xErrorCount = 0;

            StringBuilder xBuilder = new StringBuilder ();

            string? xFilePath = iTester.FindFileOrDirectory ("nJaJpCulture.cs");
            string xFileContents = nFile.ReadAllText (xFilePath!); // なければ例外が飛んでよい

            void iIsContained (IEnumerable <char> chars)
            {
                // 最初、コードポイント順に並び替えたが、さまざまなところで、あいうえお順のリストを目にする

                // 先ほども引用した「学年別漢字配当表」を、ja-JP によるソートの結果と比較すると、次のようになる

                // 一右雨円王音下火花貝学気九休玉金　空月犬見五口校左三山　子四糸字耳七車手十出女小上森人水正生青夕石赤　千川先早草足村大男竹中虫町天田土二日入年白八百文木本名　目　立力林六（学年別漢字配当表）
                // 一右雨円王音下火花貝学気　休玉金九空月犬見五口校左三山四子　糸字耳七車手十出女小上森人水正生青　石赤先千川　早草足村大男竹中虫町天田土二日入年白八百文　本名木目夕立力林六 (ja-JP)

                // 別表　学年別漢字配当表：文部科学省
                // https://www.mext.go.jp/a_menu/shotou/new-cs/youryou/syo/koku/001.htm

                // 人間が目視で漢字を探すにおいて、主たる読みをイメージして二分探索を行うなら、この程度の精度でも十分に役立つ
                // ということから、最終的な配列についてのみ、ICU による、あいうえお順 (ja-JP) でのソートを行う

                string xString = string.Join (", ", nEnumerable.OrderBy (chars, nJaJpCulture.Comparer).Select (x => $"'{x.Value}'"));

                if (xFileContents.Contains (xString, StringComparison.Ordinal) == false)
                {
                    xErrorCount ++;
                    xBuilder.Append ("Not Contained: ");
                }

                else xBuilder.Append ("Contained: ");

                xBuilder.AppendLine (xString);
            }

            iIsContained (xKanji3); // 常用漢字

            for (int temp = 0; temp < 6; temp ++)
                iIsContained (xKanji2.ElementAt (temp)); // 1～6年生

            // nJaJpCulture.cs の方に書いた理由により、出力をやめる
            // iIsContained (xKanji3.Except (xKanji2.SelectMany (x => x))); // 中学（以降？）

            iIsContained (xKanji5a); // 常用漢字以外の文字とその異体字
            iIsContained (xKanji5b); // 常用漢字の異体字

            if (xErrorCount > 0)
            {
                string xDestFilePath = nPath.Join (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), "Moge.txt");
                nFile.AppendAllText (xDestFilePath, xBuilder.ToString ());

                Console.WriteLine ("デスクトップに Moge.txt が出力されました。");
            }

            else Console.WriteLine ("OK");
            #endregion
        }
    }
}
