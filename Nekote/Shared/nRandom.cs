using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nRandom
    {
        // テストコードにランダムな文字列が必要になったので、生成する方法を探した
        // .NET 4.* までは、Membership.GeneratePassword があったようだ

        // c# - Generating Random Passwords - Stack Overflow
        // https://stackoverflow.com/questions/54991/generating-random-passwords

        // Membership.GeneratePassword(Int32, Int32) Method (System.Web.Security) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.web.security.membership.generatepassword

        // Membership.cs
        // https://referencesource.microsoft.com/#System.Web/Security/Membership.cs

        // 実装を見ると、punctuations として "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray () が使われていた

        // ` と ' が含まれないのは分かる
        // grave accent と呼ばれるらしい ` の方を自分は、たぶん一度も使ったことがない
        // これらは見分けが付きにくい
        // パスワードを手元のノートにメモ書きで残す人や、他者にメモ書きで渡す人もいる

        // Grave accent - Wikipedia
        // https://en.wikipedia.org/wiki/Grave_accent

        // しかし、それなら - と _ を省かないのが分からない
        // これらも手書きだと微妙な高低での区別になる
        // 人によっては見落とす . を入れるのに , を入れないのも分からない

        // 使える記号を少し減らすと、パスワードの強度が少し下がる
        // しかし、たとえば「分かりにくい文字を含む10文字」と、「分かりやすい文字だけの12文字」では、
        //     同程度の強度で、後者の方がシステム管理者の負担が軽減されるだろう

        // そもそも何が punctuation で、何が symbol なのか調べてみた

        // for (char temp = (char) 0x20; temp <= (char) 0x7E; temp ++) // 0x7F は制御文字の DEL
        // {
        //     if (char.IsLetterOrDigit (temp) == false)
        //         Console.WriteLine ($"0x{(int) temp:X2} {temp} {(char.IsPunctuation (temp) ? "P" : "\x20")} {(char.IsSeparator (temp) ? "Se" : "\x20\x20")} {(char.IsSymbol (temp) ? "Sy" : "\x20\x20")}");
        // }

        // 0x20     Se
        // 0x21 ! P
        // 0x22 " P
        // 0x23 # P
        // 0x24 $      Sy
        // 0x25 % P
        // 0x26 & P
        // 0x27 ' P
        // 0x28 ( P
        // 0x29 ) P
        // 0x2A * P
        // 0x2B +      Sy
        // 0x2C , P
        // 0x2D - P
        // 0x2E . P
        // 0x2F / P
        // 0x3A : P
        // 0x3B ; P
        // 0x3C <      Sy
        // 0x3D =      Sy
        // 0x3E >      Sy
        // 0x3F ? P
        // 0x40 @ P
        // 0x5B [ P
        // 0x5C \ P
        // 0x5D ] P
        // 0x5E ^      Sy
        // 0x5F _ P
        // 0x60 `      Sy
        // 0x7B { P
        // 0x7C |      Sy
        // 0x7D } P
        // 0x7E ~      Sy

        // IsSeparator というのは , や | など、区切りに使われることのある文字のことかと思い、そちらも試した
        // ドキュメントによると、SpaceSeparator, LineSeparator, ParagraphSeparator とのこと
        // ただし CR, LF などは該当しないそうで、何を目的とする分類なのか不詳

        // Char.IsSeparator Method (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.char.isseparator

        // punctuation と symbol については、「なぜそちらなのか」と思う文字が多々ある
        // ここに深入りしても仕方ない
        // とりあえず、GenerateAsciiPassword には、
        //     Membership.GeneratePassword の numberOfNonAlphanumericCharacters にならった引数名を採用

        public static readonly string AsciiDigits = "0123456789";

        public static readonly string AsciiUppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static readonly string AsciiLowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// 半角空白は、見えないので含まれない。
        /// </summary>
        public static readonly string AsciiNonAlphanumericLetters = "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";

        // max* を通常は exclusive にする
        // Random.Next がそうだし、.NET では他にもそういうところが散見される
        // 理由はおそらく、終了条件の判断に、より大きな型への一時的な変換が不要なこと
        // inclusive な int max* に int.MaxValue を指定されると、uint/long/ulong でないと処理できない

        // という認識により、自分は、max* を exclusive とし、inclusive なものには last* や *length を使ってきた
        // 配列の長さやコレクションの大きさが関わるところでは、悪意なしに *.MaxValue を受け取ることはない

        // しかし、ここで max* を、名前がそうだからと exclusive にすると、ずいぶんと使いにくい
        // かといって、inclusive にして既存のコードと整合する引数名も思い付かない

        /// <summary>
        /// min* も max* も inclusive。
        /// </summary>
        public static string GenerateAsciiPassword (int length = 16,
            int minDigitCount = 2, int? maxDigitCount = null,
            int minUpperCount = 2, int? maxUpperCount = null,
            int minLowerCount = 2, int? maxLowerCount = null,
            int minNonAlphanumericCount = 2, int? maxNonAlphanumericCount = null,
            Random? random = null)
        {
            // 一つの if 文にすると煩雑なので分割
            // パスワード生成の処理コストに比して微々たるもの

            if ((maxDigitCount != null && minDigitCount > maxDigitCount) ||
                    (maxUpperCount != null && minUpperCount > maxUpperCount) ||
                    (maxLowerCount != null && minLowerCount > maxLowerCount) ||
                    (maxNonAlphanumericCount != null && minNonAlphanumericCount > maxNonAlphanumericCount))
                throw new nArgumentException ();

            if (minDigitCount + minUpperCount + minLowerCount + minNonAlphanumericCount > length)
                throw new nArgumentException ();

            if (maxDigitCount != null && maxUpperCount != null && maxLowerCount != null && maxNonAlphanumericCount != null &&
                    maxDigitCount + maxUpperCount + maxLowerCount + maxNonAlphanumericCount < length)
                throw new nArgumentException ();

            int xDigitCount = minDigitCount,
                xUpperCount = minUpperCount,
                xLowerCount = minLowerCount,
                xNonAlphanumericCount = minNonAlphanumericCount,
                xLength = xDigitCount + xUpperCount + xLowerCount + xNonAlphanumericCount;

            Random xRandom = random ?? Random.Shared;

            while (xLength < length)
            {
                // さらに、それぞれの種類の文字の出現率を指定できるようにする考えがあったが、
                //     それをやるなら ` などを生成しないモードも実装したく、中途半端な作り込みになる
                // min* と max* の両方があるので、今回の実装でも、だいたいイメージ通りのパスワードを生成できる

                int xIndex = xRandom.Next (4);

                // while (true) において、そのインデックスのところに入らなければ次に進む実装も考えた
                // もう文字の入らないところが当たり続ける無駄を省くため

                // しかし、それでは、それぞれの種類の文字の当たる確率が大きく偏る
                // たとえば、記号はいらない引数指定だと、実質、数字の当たる確率が2倍になる

                // 速度の問われる実装なら、LinkedList から「もう入らないところ」を落としていくか

                // if 文を一つで済ませるため、xCounts と xMaxCounts を作る実装も試みた
                // コードはシンプルだったが、配列の初期化のコストが気になったためベタ書きに変更

                if (xIndex == 0)
                {
                    if (maxDigitCount == null || xDigitCount + 1 <= maxDigitCount)
                    {
                        xDigitCount ++;
                        xLength ++;
                    }
                }

                else if (xIndex == 1)
                {
                    if (maxUpperCount == null || xUpperCount + 1 <= maxUpperCount)
                    {
                        xUpperCount ++;
                        xLength ++;
                    }
                }

                else if (xIndex == 2)
                {
                    if (maxLowerCount == null || xLowerCount + 1 <= maxLowerCount)
                    {
                        xLowerCount ++;
                        xLength ++;
                    }
                }

                else
                {
                    if (maxNonAlphanumericCount == null || xNonAlphanumericCount + 1 <= maxNonAlphanumericCount)
                    {
                        xNonAlphanumericCount ++;
                        xLength ++;
                    }
                }
            }

            List <char> xChars = new List <char> ();

            void iAddChars (string charList, int count)
            {
                for (int temp = 0; temp < count; temp ++)
                    xChars.Add (charList [xRandom.Next (charList.Length)]);
            }

            iAddChars (AsciiDigits, xDigitCount);
            iAddChars (AsciiUppercaseLetters, xUpperCount);
            iAddChars (AsciiLowercaseLetters, xLowerCount);
            iAddChars (AsciiNonAlphanumericLetters, xNonAlphanumericCount);

            char [] xCharArray = xChars.ToArray ();
            nArray.Shuffle (xCharArray, 0, xCharArray.Length);

            return new string (xCharArray);
        }
    }
}
