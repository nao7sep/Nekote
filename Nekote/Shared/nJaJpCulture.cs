using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    // Cultures ディレクトリーの最初のクラスなので、共通的なコメントをここに書いておく

    // Cultures ディレクトリーは、ローカライゼーションのうち、主に文字列に関することを扱うクラスを含む
    // 日時の文字列化もこのディレクトリーだが、時差は文字列が関わらないため他のところで

    // ローカルエンコーディングも、Cultures ディレクトリーでは扱われない
    // 国や地域とのゆるい相関があるが、たとえばヨーロッパの言語のローカルエンコーディングは多数の国で使えるなど、一対一の関係とはほど遠い
    // ローカルエンコーディングは、既に、Unicode を基本とする文字列処理からの派生的かつ代替的な表現方法という位置付けになっている
    // どちらかと言えば、「文字列処理」のカテゴリーに属するもの

    // Exceptions ディレクトリーと同様、それぞれのクラス名に接尾辞として Culture を付ける → ディレクトリーの階層構造をフラットにした
    // Localization ディレクトリーに *Locale も考えたが、
    //     クラス名の前半が第一に represent するのは CultureInfo なので、culture が主体のクラスと考えるべき

    // 日本語には、ja のみと ja-JP の二つがある
    // 日本語に JP 以外が今後生じる可能性は低いため、nJaCulture を作った方が英語も（まずは）nEnCulture で足りるかもしれないと考えたが、
    //     最初から .NET と全く同じように細分化させた上で各部にフォールバックの仕組みを用意する方が、あとで詰まりにくいだろう

    // .NET では、ja のみのようなカルチャーは neutral culture とされるようだ
    // neutral culture での comparer などの生成は非推奨
    // To display and sort data, specify both the language and region ともある

    // CultureInfo Class (System.Globalization) | Microsoft Learn
    // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo

    // CultureInfo.cs
    // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Globalization/CultureInfo.cs

    public static class nJaJpCulture
    {
        private static CultureInfo? mCulture;

        public static CultureInfo Culture
        {
            get
            {
                // CultureInfo のインスタンスを得る方法は二つある
                // GetCultureInfo については、
                //     The GetCultureInfo method retrieves a cached, read-only CultureInfo object
                //     It offers better performance than a corresponding call to the CultureInfo.CultureInfo(String) constructor とある
                // コンストラクターの方は、
                //     Initializes a new instance of the CultureInfo class based on the culture specified by name
                //     and on a value that specifies whether to use the user-selected culture settings from Windows とのことで、
                //     Windows にログインしている現在のユーザーが Windows の設定をいじっていれば、それを引き継ぐこともできるようだ
                // n*Culture は、習熟のあるメジャーなカルチャーについて、特有のコードを扱ったり、最善の comparer を取得したりのためのもの
                // システムに登録されているカルチャーをそのまま使いたいため、GetCultureInfo の使用が適する

                // CultureInfo.GetCultureInfo Method (System.Globalization) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.getcultureinfo

                // CultureInfo Constructor (System.Globalization) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.-ctor

                // Windows の設定に影響を受けるというのがピンとこなかったので、もう少し調べた
                // CultureInfo.UseUserOverride が false になっていれば、とりあえず大丈夫そう

                // c# - How to detect if CurrentCulture has been customised by the user? - Stack Overflow
                // https://stackoverflow.com/questions/21280754/how-to-detect-if-currentculture-has-been-customised-by-the-user

                // CultureInfo のプロパティーとして得られるものは、InvariantCulture 以外、null あるいは影響あり

                // Console.WriteLine (CultureInfo.CurrentCulture.UseUserOverride); // True
                // Console.WriteLine (CultureInfo.CurrentUICulture.UseUserOverride); // True
                // Console.WriteLine (CultureInfo.DefaultThreadCurrentCulture?.UseUserOverride == null); // True
                // Console.WriteLine (CultureInfo.DefaultThreadCurrentUICulture?.UseUserOverride == null); // True
                // Console.WriteLine (CultureInfo.InstalledUICulture.UseUserOverride); // True
                // Console.WriteLine (CultureInfo.InvariantCulture.UseUserOverride); // False

                // CultureInfo.GetCultureInfo で取得したものは大丈夫
                // Console.WriteLine (nJaJpCulture.Culture.UseUserOverride); // False

                // ja でなく ja-JP として一致していること、その上で UseUserOverride のみ異なることの確認

                // Console.WriteLine (CultureInfo.GetCultureInfo ("ja").LCID); // 17
                // Console.WriteLine (CultureInfo.CurrentCulture.LCID); // 1041
                // Console.WriteLine (nJaJpCulture.Culture.LCID); // 1041

                if (mCulture == null)
                    mCulture = CultureInfo.GetCultureInfo ("ja-JP", predefinedOnly: true);

                return mCulture;
            }
        }

        // CaseSensitiveComparer なども考えたが、.NET 的な命名に
        // In が入っているかどうかで見分けるより分かりやすい

        private static StringComparer? mComparer;

        public static StringComparer Comparer
        {
            get
            {
                if (mComparer == null)
                    mComparer = StringComparer.Create (Culture, ignoreCase: false);

                return mComparer;
            }
        }

        private static StringComparer? mComparerIgnoreCase;

        public static StringComparer ComparerIgnoreCase
        {
            get
            {
                if (mComparerIgnoreCase == null)
                    mComparerIgnoreCase = StringComparer.Create (Culture, ignoreCase: true);

                return mComparerIgnoreCase;
            }
        }

        // nAlphanumericComparer クラスの、カルチャー関係なしのインスタンスはプログラムの起動と同時に初期化されるが、こちらのものは JIT 的に
        // StringComparer.Create のコストが気になるし、特定のカルチャーの alphanumeric comparer は、よく使われるわけでもない

        private static nAlphanumericComparer? mComparerAlphanumeric;

        public static nAlphanumericComparer ComparerAlphanumeric
        {
            get
            {
                if (mComparerAlphanumeric == null)
                    mComparerAlphanumeric = new nAlphanumericComparer (Comparer);

                return mComparerAlphanumeric;
            }
        }

        private static nAlphanumericComparer? mComparerAlphanumericIgnoreCase;

        public static nAlphanumericComparer ComparerAlphanumericIgnoreCase
        {
            get
            {
                if (mComparerAlphanumericIgnoreCase == null)
                    mComparerAlphanumericIgnoreCase = new nAlphanumericComparer (ComparerIgnoreCase);

                return mComparerAlphanumericIgnoreCase;
            }
        }
    }
}
