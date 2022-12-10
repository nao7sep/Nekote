using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iGenericTester
    {
        // nStopwatch* には、最初、where 句で class の指定を受ける DataType および struct の TagType を用意した
        // しかし、ジェネリックにおける nullability に関するところを調べたことで一つで足りると判明し、TagType Tag のみを残した
        // そのときのテストコードを少し冗長気味に整理しておく

        // エラーのうちいくつかについては、次のページが詳しい
        // 以下で # 入りの直リンを貼るにおいては、重複するページタイトルを省略

        // Resolve nullable warnings | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings

        private class iSomeClass <SomeType>
        {
            // CS8618 - Non-nullable variable must contain a non-null value when exiting constructor
            // Consider declaring it as nullable

            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings#nonnullable-reference-not-initialized

            // SomeType が値型なら自動的に0で初期化される
            // クラスなら、null 非許容のフィールドになるので、それをコンパイラーが勝手に null とするのもアレで怒ってくる
            // 参照の nullability は元々あるもので、コンパイラーによるただのエラーチェックなので、警告を無効にすれば勝手に null にされる

#pragma warning disable CS8618
            public SomeType Hoge;
#pragma warning restore CS8618

            // 次のコードについては誤解があった
            // 値型のとき、たとえば <int> での new なら、ここは int? Moge → Nullable <int> Moge となり、default により null が入るイメージだった
            // しかし、ジェネリッククラスの ? 付きのフィールド（および引数）については、便宜的に次のような区別が行われるようだ

            // * クラス → 元々 nullable であるものにおいて、コンパイラーにより nullability が明示的に認められる
            // * 値型 → ? が無視され、フィールド（および引数）は Nullable にならない

            // 少しググったが、なぜそういう仕様なのかをドンピシャで説明するページは見当たらなかった
            // 推測では、? がなかった時代、「初期値」といえば、参照なら null、値型なら0（または人によっては -1）で、
            //     string, int などをそのまま <...> に入れて new したなら、その初期値が適用されるようにしたかったからかもしれない
            // 実際、<int> で int? Moge になるなら、Moge を実体のフィールドにするには、SomeType Moge と、? を付けずに宣言するしかない
            // そうすると、Hoge と同様、今度は <string> からの string Hoge に勝手に null を入れてよいかとの問題になる
            // コードの表記と挙動が異なるのが気になるが、その方が設計の自由度が高くなっているため、慣れてしまうしかない

            public SomeType? Moge; // = default;

            public void Poge (SomeType value)
            {
                Hoge = value;
                Moge = value;
            }

            public void PogeAlt (SomeType? value)
            {
                // CS8601 - Possible null reference assignment

                // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings#possible-null-assigned-to-a-nonnullable-reference

                // 引数の型に ? が付いているので、SomeType がクラスなら null が飛んでくるかもしれないとの警告
                // 詳しくは後述するが、値型のときに Nullable <SomeType> になることはないようだ

#pragma warning disable CS8601
                Hoge = value;
#pragma warning restore CS8601

                Moge = value;
            }
        }

        public static void TestNullability ()
        {
            iSomeClass <string> xSomeInstance = new iSomeClass <string> ();
            Console.WriteLine (xSomeInstance.Hoge == xSomeInstance.Moge); // → True

            // CS8625 - Cannot convert null literal to non-nullable reference type

            // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/nullable-warnings#possible-null-assigned-to-a-nonnullable-reference

            // ? の付いていない string に null を通すのは、コンパイラーが怒ってくるが、できないことではない

#pragma warning disable CS8625
            xSomeInstance.Poge (null);
#pragma warning restore CS8625

            xSomeInstance.PogeAlt (null);
            Console.WriteLine (xSomeInstance.Moge == null); // → True

            // =============================================================================

            iSomeClass <string?> xSomeInstanceAlt = new iSomeClass <string?> ();

            // SomeType Hoge と SomeType? Moge なので、string? Hoge は分かっても、Nullable <string?> Moge にはなれないので……と思ったが、
            //     string?? はサクッと string? になるようだ
            // string?? と書くと文法ミスだが、ジェネリックの型と引数の型の組み合わせにより ? が複数になるのは問題でない
            Console.WriteLine (xSomeInstanceAlt.Hoge == xSomeInstanceAlt.Moge); // → True

            xSomeInstanceAlt.Poge (null);
            xSomeInstanceAlt.PogeAlt (null);
            Console.WriteLine (xSomeInstanceAlt.Moge == null); // → True

            // =============================================================================

            iSomeClass <int> xSomeInstanceAlt1 = new iSomeClass <int> ();

            // SomeType Hoge も SomeType? Moge も <int> だと int Hoge と int Moge になり、初期値0で一致
            Console.WriteLine (xSomeInstanceAlt1.Hoge == xSomeInstanceAlt1.Moge); // → True

            // 引数の型に ? が付いていても、<int> での new なら無視されて、引数は int value になる
            // そこに null を通せないのは、ただの警告でないので、コメントアウトしないとコンパイルできない
            // PogeAlt の方は、パッと見、Nullable <SomeType> を受け取るように見えるので、今後も注意が必要
            // 「ジェネリックで値型に nullability を与えたければ <...> に ? を入れる」と丸覚えする

            // xSomeInstanceAlt1.Poge (null);
            // xSomeInstanceAlt1.PogeAlt (null);

            // The result of the expression is always 'value1' since a value of type 'value2' is never equal to 'null' of type 'value3'

            // Compiler Warning (level 2) CS0472 | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs0472

            // == にカーソルを置くと、bool int.operator == (int left, int right) と表示される
            // 実体は Int32.IEqualityOperators <Int32, Int32, Boolean>.Equality だろう
            // 代入はコンパイルできないが、比較は、コンパイル時の最適化において False に確定されるのか、コンパイルは可能のようだ

            // Int32.IEqualityOperators<Int32,Int32,Boolean>.Equality(Int32, Int32) Operator (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.int32.system-numerics-iequalityoperators-system-int32-system-int32-system-boolean--op_equality

#pragma warning disable CS0472
            Console.WriteLine (xSomeInstanceAlt1.Moge == null); // → False
#pragma warning restore CS0472

            // 0で初期化されていることを一応確認
            Console.WriteLine (xSomeInstanceAlt1.Moge); // → 0

            // =============================================================================

            iSomeClass <int?> xSomeInstanceAlt2 = new iSomeClass <int?> ();

            // SomeType Hoge も SomeType? Moge も int? になり、null として一致
            Console.WriteLine (xSomeInstanceAlt2.Hoge == xSomeInstanceAlt2.Moge); // → True

            // 引数も、どちらのメソッドでも int? となり、Nullable <int> が渡される

            xSomeInstanceAlt2.Poge (null);
            xSomeInstanceAlt2.PogeAlt (null);

            Console.WriteLine (xSomeInstanceAlt2.Moge == null); // → True
        }

        // ジェネリックだけに関連しているわけでないが、Nullable とボックス化の速度を比較するメソッドを用意しておく
        // 結論としては、やっていることが同じなのか、これらに速度差はないと考えてよさそう

        // Boxing and Unboxing - C# Programming Guide | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing

        // 以下は、それよりも、Stopwatch の不可解な挙動を示すものとして価値を持つ

        public static void CompareSpeedsOfNullableAndBoxing ()
        {
            const int xTestCount = 10,
                xHandlingCount = 1000000000;

            Stopwatch xStopwatch = new Stopwatch ();

            string [] xLabels = { "Nullable", "Boxing" };
            var xResults = Enumerable.Range (0, xLabels.Length).Select (x => (Index: x, Elapsed: new List <TimeSpan> ())).ToArray ();

            for (int temp = 0; temp < xTestCount; temp ++)
            {
                int xLabelIndex = 0;

                // =============================================================================

                // Stopwatch.Stop と Stopwatch.Reset の両方をどこでも一度も呼ばなければ、なぜか Nullable の方が倍ほど遅いとの結果になる

                // Nullable: 546.169ms
                // Boxing: 271.4836ms

                // どこか有効なところで呼べば、これらの速度はほぼ同じになる
                // どこで呼べば効果があるかは、再現率が100％だった

                // Nullable: 277.9892ms
                // Boxing: 279.5886ms

                // 倍となると片方の計測データが残ったまま次の計測を行ってしまっている可能性を考えるが、
                //     Stop や Reset のシンプルな実装を見る限り、その可能性はない
                // Restart がいきなり呼ばれ、Elapsed がいきなり見られることも想定された実装になっている

                // Stopwatch.cs
                // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Diagnostics/Stopwatch.cs

                // 興味深いのは、計測の結果のみ倍ほど遅く見えるのでなく、テスト時の体感速度もはっきりと変わること
                // つまり、Stop や Reset を呼ばないことで、Restart から Elapsed までの何かが実際に遅くなっている

                // Stopwatch がそれぞれの for ループの速度に影響するとは考えにくく、Restart も Elapsed も分かりやすい実装なので、
                //     Elapsed → GetElapsedDateTimeTicks → GetRawElapsedTicks → GetTimestamp → QueryPerformanceCounter → Interop.Sys.GetTimestamp くらいしか疑いようがない
                // これには [LibraryImport (Libraries.SystemNative, EntryPoint = "SystemNative_GetTimestamp")] と [SuppressGCTransition] が付いている
                // 中身が見えないため100％これでないと言うことができないが、「倍」というのを考えると、これではない気がする

                // 以下、効果のあるなしについてコメントを書いておく
                // 正確な計測に必要なので、一つはコメントアウトせずに残す

                // xStopwatch.Stop (); 効果あり
                xStopwatch.Reset (); // 効果あり
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xHandlingCount; tempAlt ++)
                {
                    int? xValue = null;
                    xValue = 0;
                    int xValueAlt = xValue.Value;
                }

                // xStopwatch.Stop (); 効果なし
                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // =============================================================================

                // xStopwatch.Stop (); 効果あり
                // xStopwatch.Reset (); 効果あり
                xStopwatch.Restart ();

                for (int tempAlt = 0; tempAlt < xHandlingCount; tempAlt ++)
                {
                    object? xValue = null;
                    xValue = 0;
                    int xValueAlt = (int) xValue;
                }

                // xStopwatch.Stop (); 効果なし
                xResults [xLabelIndex ++].Elapsed.Add (xStopwatch.Elapsed);

                // さらに興味深いのは、for ループの次の回に進む直前での Stop/Reset に効果がないこと
                // xLabelIndex は使い捨てだし、マルチスレッドが関わるコードでもない
                // ここでは効果のないものが、ループの次の回だと効果がある

                // xStopwatch.Stop (); 効果なし
                // xStopwatch.Reset (); 効果なし

                // 可能性として考えているのは、ソースでは見えない最適化が、現在（2022年12月9日）、（一時的に）不具合を起こしているということ

                // Restart 直前の Stop/Reset に効果があるのは分かっている
                // 始めてもないものを Stop というのは違和感があるため、Reset → Restart をおまじないにして様子見
                // ソースを見る限り何の意味もない Reset だが、それでうまくいってしまうのだから仕方ない

                // =============================================================================

                nConsole.WriteProcessingMessage ("計測中");
            }

            Console.WriteLine ();
            Console.WriteLine (string.Join (Environment.NewLine, xResults.Select (x => $"{xLabels [x.Index]}: {nTimeSpan.Average (x.Elapsed).TotalMilliseconds}ms")));
        }
    }
}
