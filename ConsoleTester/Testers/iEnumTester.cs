using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iEnumTester
    {
        // なくてよい値もあるが、コードとコメントの修正がめんどくさい
        // あるべきものは全てある

        public enum Color
        {
            None = 0,
            Red = 1,
            Green = 2,
            Blue = 3,
            Black = 0b0100,
            White = 0b1000
        }

        [Flags]
        public enum Pets: ulong
        {
            None = 0b0000,
            Dog = 0b0001,
            Cat = 0b0010,
            Giraffe = 0b0100,
            Godzilla = 0b_10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000
        }

        public static void TestEverything ()
        {
            // ページへのリンクは、nEnum にあるので、こちらでは省略

            // Flags の有無による違いは、ToString ("G") によりカンマ区切りの文字列になるかどうか

            // 文字列化は、enum の値が、定義されている値のみの OR になっているときのみ行われるようだ
            // Flags の方では、Dog, Cat, 103から Dog, Cat の値を引いた整数、というのも想定したが、そうならなかった
            // 文字列化された enum は、その文字列に改変がなく、enum の値などにも仕様の変更がなければ、
            //     必ず、定義された項目またはそれらの OR とラウンドトリップするだろう

            // 文字列化には Format もあるが、これは内部的に ToString を呼ぶ
            // 特に理由がなければ、常に ToString を使ってよいだろう

            Console.WriteLine (Color.Red); // → Red
            Console.WriteLine (Color.Black | Color.White); // → 12
            Console.WriteLine ((Color.Black | Color.White).ToString ("G")); // → 12
            Console.WriteLine ((Color.Black | Color.White).ToString ("F")); // → Black, White
            Console.WriteLine ((Color.Black | Color.White).ToString ("D")); // → 12
            Console.WriteLine ((Color.Black | Color.White).ToString ("X")); // → 0000000C
            Console.WriteLine ((Color) 100); // → 100
            Console.WriteLine ((Color.Black | Color.White | (Color) 100).ToString ("G")); // → 108
            Console.WriteLine ((Color.Black | Color.White | (Color) 100).ToString ("F")); // → 108
            Console.WriteLine ((Color.Black | Color.White | (Color) 100).ToString ("D")); // → 108
            Console.WriteLine ((Color.Black | Color.White | (Color) 100).ToString ("X")); // → 0000006C
            Console.WriteLine ();

            Console.WriteLine (Pets.Dog); // → Dog
            Console.WriteLine (Pets.Dog | Pets.Cat); // → Dog, Cat
            Console.WriteLine ((Pets.Dog | Pets.Cat).ToString ("G")); // → Dog, Cat
            Console.WriteLine ((Pets.Dog | Pets.Cat).ToString ("F")); // → Dog, Cat
            Console.WriteLine ((Pets.Dog | Pets.Cat).ToString ("D")); // → 3
            Console.WriteLine ((Pets.Dog | Pets.Cat).ToString ("X")); // → 0000000000000003
            Console.WriteLine ((Pets) 100); // → 100
            Console.WriteLine ((Pets.Dog | Pets.Cat | (Pets) 100).ToString ("G")); // → 103
            Console.WriteLine ((Pets.Dog | Pets.Cat | (Pets) 100).ToString ("F")); // → 103
            Console.WriteLine ((Pets.Dog | Pets.Cat | (Pets) 100).ToString ("D")); // → 103
            Console.WriteLine ((Pets.Dog | Pets.Cat | (Pets) 100).ToString ("X")); // → 0000000000000067
            Console.WriteLine ();

            // IsDefined は、意外なことに、Flags 付きの enum であっても、複数の値の OR だと false を返す
            // そのことは、次のページの Notes to Callers のところにも書かれていた

            // Enum.IsDefined Method (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.enum.isdefined

            Console.WriteLine (Enum.IsDefined (Color.Red)); // → True
            Console.WriteLine (Enum.IsDefined (Color.Black | Color.White)); // → False
            Console.WriteLine (Enum.IsDefined ((Color) 100)); // → False
            Console.WriteLine (Enum.IsDefined (Color.Black | Color.White | (Color) 100)); // → False
            Console.WriteLine ();

            Console.WriteLine (Enum.IsDefined (Pets.Dog)); // → True
            Console.WriteLine (Enum.IsDefined (Pets.Dog | Pets.Cat)); // → False
            Console.WriteLine (Enum.IsDefined ((Pets) 100)); // → False
            Console.WriteLine (Enum.IsDefined (Pets.Dog | Pets.Cat | (Pets) 100)); // → False
            Console.WriteLine ();

            // IsDefined は、名前や値を順に見て、完全一致するものを探す
            // 一方、HasFlag は、ソースによると、シンプルに AND を取っての一致の確認

            Color xColors = Color.Black | Color.White;
            Console.WriteLine (xColors.HasFlag (Color.Black | Color.White)); // → True

            Pets xPets = Pets.Dog | Pets.Cat;
            Console.WriteLine (xPets.HasFlag (Pets.Dog | Pets.Cat)); // → True

            Console.WriteLine ();

            // =============================================================================

            // enum から文字列へ

            // 先述の通り、enum の値がその型の値のうち一つまたはそれらの OR と一致しなければ文字列にならない
            // それを事前にチェックできる必要がある

            Console.WriteLine (nEnum.ValidateValue (Color.Red)); // → True
            Console.WriteLine (nEnum.ValidateValue (Color.Black | Color.White)); // → False
            Console.WriteLine (nEnum.ValidateAllValues (Color.Black | Color.White)); // → True
            Console.WriteLine (nEnum.ValidateAllValues (Color.Black | Color.White | (Color) 100)); // → False
            Console.WriteLine ();

            // ulong で最上位ビットが1のゴジラがオーバーフローしないのを確認
            // ValidateAllValues 内で試験的に long として処理したところ、
            //     同じ64ビットでも System.OverflowException: Value was either too large or too small for an Int64 が飛んだ

            xPets = Pets.Dog | Pets.Cat | Pets.Godzilla;
            Console.WriteLine ("0x" + ((ulong) Pets.Godzilla).ToString ("X", CultureInfo.InvariantCulture)); // → 0x8000000000000000
            Console.WriteLine (nEnum.ValidateAllValues (xPets)); // → True
            Console.WriteLine ();

            // enum → 文字列は、.NET の実装に任せてよい
            // キーの順序を変更したければ、, で Split

            // この処理は、ローカライゼーションが関わってくるし、
            //     数行で書ける簡単な処理のため、今のところメソッド化しない
            // そのうち、UI への出力時には「ゴジラ, 猫, 犬」にしなければならない

            string [] xNames = xPets.ToString ("F").Split (',', StringSplitOptions.TrimEntries);
            Array.Reverse (xNames);
            Console.WriteLine (string.Join (", ", xNames)); // → Godzilla, Cat, Dog
            Console.WriteLine ();

            // =============================================================================

            // enum から値へ

            // nEnum.Validate* によりセキュリティーリスクを低減できる
            // Flags の有無に関わらず、underlying type への変換はシンプル

            // =============================================================================

            // 文字列から enum へ

            // ignoreCase を指定しなければ、デフォルトでは case-sensitive
            // 空白系文字には寛容のようで、4行目の各部は「半角空白、全角空白、半角空白」

            // 一つでも無効な名前が含まれていると構文解析に失敗する
            // 読める部分だけでも読むメソッドの実装を考えたが、enum は他の値型と同じくビット単位での完全な同一性が問われるもの
            // たとえば int のデータを転送していて数ビット飛べば、その値はその後の処理において無効とされる
            // enum は、A, B, C ... と、リストのように表記されるが、あくまで A | B | C であることを念頭に

            Console.WriteLine (Enum.TryParse ("Red", out Color xResult)); // → True
            Console.WriteLine (Enum.TryParse ("rED", out xResult)); // → False
            Console.WriteLine (Enum.TryParse ("赤", out xResult)); // → False
            Console.WriteLine (Enum.TryParse (" 　 Black 　 , 　 White 　 ", out xResult)); // → True
            Console.WriteLine (Enum.TryParse ("Black, White, 赤", out xResult)); // → False
            Console.WriteLine (Enum.TryParse (string.Empty, out xResult)); // → False
            Console.WriteLine (Enum.TryParse (null, out xResult)); // → False
            Console.WriteLine ();

            // =============================================================================

            // 値から enum へ

            // ここでも nEnum.Validate* を通しておけば、セキュリティーリスクを低減できる

            Console.WriteLine (nEnum.ValidateValue ((Color) 1 /* Red */)); // → True
            Console.WriteLine (nEnum.ValidateValue ((Color) 0b0100 /* Black */ | (Color) 0b1000 /* White */)); // → False
            Console.WriteLine (nEnum.ValidateAllValues ((Color) 0b0100 /* Black */ | (Color) 0b1000 /* White */)); // → True
            Console.WriteLine (nEnum.ValidateAllValues ((Color) 0b0100 /* Black */ | (Color) 0b1000 /* White */ | (Color) 100)); // → False
        }
    }
}
