using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public abstract class nIniLikeFileBasedCrudDataProvider <KeyType>: nCrudDataProvider <KeyType, nStringDictionary>
        where KeyType: notnull
    {
        // マルチスレッドでも、単一プロセスなら、lock (Locker) の徹底により良好な結果を得られるはず
        // 複数プロセスでも、ゆっくりなアプリなら、ファイルの存在チェックにより、それなりに動くかもしれない

        /// <summary>
        /// マルチスレッドなら、これで lock。
        /// </summary>
        public readonly object Locker = new object ();

        public readonly string DirectoryPath;

        public nIniLikeFileBasedCrudDataProvider (IEqualityComparer <KeyType> comparer, string directoryPath): base (comparer)
        {
            if (Path.IsPathFullyQualified (directoryPath) == false)
                throw new nArgumentException ();

            DirectoryPath = directoryPath;
        }

        public abstract KeyType GenerateKey ();

        public abstract string KeyToFilePath (KeyType key);

        public abstract void SetKeyToEntry (nStringDictionary entry, KeyType key);

        public abstract bool TryGetKeyFromEntry (nStringDictionary entry, out KeyType key);

        public abstract bool TryParseFileName (string name, out KeyType key);

        public override KeyType CreateEntry (nStringDictionary entry)
        {
            while (true)
            {
                KeyType xKey = GenerateKey ();

                if (ContainsKey (xKey) == false)
                {
                    string xFilePath = KeyToFilePath (xKey);

                    if (nFile.CanCreate (xFilePath))
                    {
                        SetKeyToEntry (entry, xKey);

                        string xFileContents = entry.ToIniLikeString ();
                        nFile.WriteAllText (xFilePath, xFileContents);

                        // TryCreateEntry のコメントを参照
                        // Try* でない方でも、できるだけデータの整合性を保つように

                        try
                        {
                            Add (xKey, entry);
                        }

                        catch
                        {
                            nFile.Delete (xFilePath);

                            throw;
                        }

                        return xKey;
                    }
                }
            }
        }

        public override bool TryCreateEntry (nStringDictionary entry, out KeyType key)
        {
            // このメソッドの実装において、落ちる可能性を無視できないのは、nFile.WriteAllText と Add の二つ
            // 前者は、たいていパーミッション関連のこと、後者は、確率的には低いが、ContainsKey 直後に別スレッドから同一キーを入れられての衝突
            // ContainsKey から Add までは一瞬だし、キーの生成がランダムなので、悪意があっても故意の衝突は難しいが、
            //     たまには GUID もぶつかるわけで、起こりえないことでないため、可能性を無視できない

            // nFile.WriteAllText が落ちれば、ファイルへの書き込みも、辞書への要素の追加も起こらない
            // Add が落ちれば、ファイルシステムと辞書の不整合を可能な限り回避するため、ファイルの削除を試みる

            try
            {
                while (true)
                {
                    KeyType xKey = GenerateKey ();

                    if (ContainsKey (xKey) == false)
                    {
                        string xFilePath = KeyToFilePath (xKey);

                        if (nFile.CanCreate (xFilePath))
                        {
                            SetKeyToEntry (entry, xKey);

                            string xFileContents = entry.ToIniLikeString ();
                            nFile.WriteAllText (xFilePath, xFileContents);

                            try
                            {
                                Add (xKey, entry);
                            }

                            catch
                            {
                                nFile.Delete (xFilePath);
                            }

                            key = xKey;
                            return true;
                        }
                    }
                }
            }

            catch
            {
                // Nullable でないものに null を入れてしまうが、
                //     結果が bool なら key を見ないため問題でない

                key = default!;
                return false;
            }
        }

        public override nStringDictionary ReadEntry (KeyType key)
        {
            // パフォーマンスを優先し、辞書内にデータがあればそれを返す
            // なければ、ファイルにあるか調べ、あれば辞書に入れてから返す

            // ? を付けないと、null かもしれないものを Nullable でない型に、と叱られる
            // nCrudDataProvider に where EntryType: notnull を入れても同じ
            // 不正な操作を行わない限り値側に null が入ることはないため、? を付けて様子見

            if (TryGetValue (key, out nStringDictionary? xResult))
                return xResult;

            string xFilePath = KeyToFilePath (key);

            if (File.Exists (xFilePath))
            {
                string xFileContents = nFile.ReadAllText (xFilePath);
                nStringDictionary xEntry = nStringDictionary.ParseIniLikeString (xFileContents);

                // 最初はここまでしていなかったが、TryLoadAllEntries の方で厳密化したので、こちらでも
                // キーからファイル名を生成しているので、ファイル名には間違いがない
                // ファイルのすり替えを警戒し、ファイルの内容がキーと整合する場合のみ処理を続行

                if (TryGetKeyFromEntry (xEntry, out KeyType xResultAlt) &&
                    Comparer.Equals (xResultAlt, key))
                {
                    // TryGetValue が bool を返した直後なので、たいてい大丈夫
                    Add (key, xEntry);

                    return xEntry;
                }
            }

            throw new nArgumentException ();
        }

        public override bool TryReadEntry (KeyType key, out nStringDictionary entry)
        {
            // 落ちる可能性を無視できないのは、nFile.ReadAllText, nStringDictionary.ParseIniLikeString, Add あたり
            // 最初の二つのいずれかで落ちても、ファイルシステムや辞書にダメージは発生しない
            // これら三つでは Add が最後なので、Add で落ちても、ごみデータは残らない

            try
            {
                if (TryGetValue (key, out nStringDictionary? xResult))
                {
                    entry = xResult;
                    return true;
                }

                string xFilePath = KeyToFilePath (key);

                if (File.Exists (xFilePath))
                {
                    string xFileContents = nFile.ReadAllText (xFilePath);
                    nStringDictionary xEntry = nStringDictionary.ParseIniLikeString (xFileContents);

                    if (TryGetKeyFromEntry (xEntry, out KeyType xResultAlt) &&
                        Comparer.Equals (xResultAlt, key))
                    {
                        Add (key, xEntry);

                        entry = xEntry;
                        return true;
                    }
                }
            }

            catch
            {
            }

            entry = default!;
            return false;
        }

        public override void UpdateEntry (KeyType key, nStringDictionary entry)
        {
            // このあたりで、辞書のキー、辞書の値の中のキー、ファイルの中のキーの整合性について考えた
            // *CreateEntry は、SetKey により同期を取り、それをファイルに書き込む
            // *ReadEntry は、read-only なので、上記三つが初めから整合していれば、それを乱さない
            // *UpdateEntry は、*CreateEntry により整合している上記三つを乱さないためのチェックを必要とする
            // *DeleteEntry は、キー → 値の関係性が辞書により、ファイルパス → ファイルの関係性が OS により保証されるため、
            //     辞書の値の中のキーやファイルの中のキーをチェックしなくてよい

            // キーの異なるエントリーへのすり替えが試みられていないかチェック

            if (TryGetKeyFromEntry (entry, out KeyType xResult) == false ||
                    Comparer.Equals (xResult, key) == false)
                throw new nArgumentException ();

            // ファイルへの書き込みに失敗すれば、辞書のデータを元に戻して整合性を保つ
            // ContainsKey のみ見てファイルを更新し、最後に base [key] = entry も選択肢だが、
            //     万が一そこで落ちると、もう上書きしてしまったファイルを元に戻せない
            // 戻すために、まずは移動のみ行うとか、現行の内容を読み込むとかは、
            //     base [key] = entry がほぼ確実に成功することを考えるとコストが大きい

            if (TryGetValue (key, out nStringDictionary? xResultAlt))
            {
                base [key] = entry;

                try
                {
                    string xFilePath = KeyToFilePath (key),
                        xFileContents = entry.ToIniLikeString ();

                    nFile.WriteAllText (xFilePath, xFileContents);
                }

                catch
                {
                    base [key] = xResultAlt;

                    throw;
                }
            }

            else
            {
                // *CreateEntry は、辞書になく、ファイルシステムにもないことを確認する
                // *ReadEntry は、辞書から読めなければ、ファイルシステムからも探す
                // *DeleteEntry は、「ない状態にする」のメソッドなので、辞書からもファイルシステムからも消す
                // これらは、複数プロセスから同じディレクトリーに同時期に行われても、ある程度は動作する

                // *UpdateEntry もそうであってほしい
                // そのため、辞書になくても、ファイルシステムにはあれば、
                //     *DeleteEntry と同程度に乱暴だが、エラー扱いせず、更新の処理を続行する

                // これは、*DeleteEntry に仕様を寄せてのことでもある
                // *DeleteEntry は、辞書にないエントリーでも、
                //     ファイルシステムにあれば、その内容を見ることなく削除する
                // というのは乱暴にも思えるが、偶然にはまず一致しないキーを得ているのだから、
                //     更新や削除を行おうとしているプロセスは、たいてい、元々そのエントリーを扱っていた側
                // *UpdateEntry で設定しようとしている新しいデータは、たいてい、古いものを把握した上でのもの
                // *DeleteEntry で削除しようとしているエントリーは、そうする理由が呼び出し側にあるもの

                // 「キーを得ている」ということを「内容を把握している」と解釈する
                // 実際、別プロセスにより追加されたエントリーを、それが何かすら分からないまま、
                //     キーだけ手元に得て更新したり削除したりという実装は考えにくい

                string xFilePath = KeyToFilePath (key);

                if (File.Exists (xFilePath))
                {
                    base [key] = entry;

                    try
                    {
                        string xFileContents = entry.ToIniLikeString ();
                        nFile.WriteAllText (xFilePath, xFileContents);
                    }

                    catch
                    {
                        // 不整合の状態に戻す
                        // ここでは「元に戻す」を忠実に
                        Remove (key);

                        throw;
                    }
                }

                else throw new nArgumentException ();
            }
        }

        public override bool TryUpdateEntry (KeyType key, nStringDictionary entry)
        {
            try
            {
                if (TryGetKeyFromEntry (entry, out KeyType xResult) == false ||
                        Comparer.Equals (xResult, key) == false)
                    return false;

                if (TryGetValue (key, out nStringDictionary? xResultAlt))
                {
                    base [key] = entry;

                    try
                    {
                        string xFilePath = KeyToFilePath (key),
                            xFileContents = entry.ToIniLikeString ();

                        nFile.WriteAllText (xFilePath, xFileContents);

                        return true;
                    }

                    catch
                    {
                        base [key] = xResultAlt;
                    }
                }

                else
                {
                    string xFilePath = KeyToFilePath (key);

                    if (File.Exists (xFilePath))
                    {
                        base [key] = entry;

                        try
                        {
                            string xFileContents = entry.ToIniLikeString ();
                            nFile.WriteAllText (xFilePath, xFileContents);

                            return true;
                        }

                        catch
                        {
                            Remove (key);
                        }
                    }
                }
            }

            catch
            {
            }

            return false;
        }

        public override void DeleteEntry (KeyType key)
        {
            bool xResult = TryGetValue (key, out nStringDictionary? xResultAlt);

            if (xResult)
                Remove (key);

            try
            {
                string xFilePath = KeyToFilePath (key);

                if (File.Exists (xFilePath))
                    nFile.Delete (xFilePath);
            }

            catch
            {
                if (xResult)
                    Add (key, xResultAlt!); // ! を付けないと叱られる

                throw;
            }
        }

        public override bool TryDeleteEntry (KeyType key)
        {
            try
            {
                bool xResult = TryGetValue (key, out nStringDictionary? xResultAlt);

                if (xResult)
                    Remove (key);

                try
                {
                    string xFilePath = KeyToFilePath (key);

                    if (File.Exists (xFilePath))
                        nFile.Delete (xFilePath);

                    return true;
                }

                catch
                {
                    if (xResult)
                        Add (key, xResultAlt!);
                }
            }

            catch
            {
            }

            return false;
        }

        // ログなど、出力しっぱなしのものもある
        // それの古いデータまで毎回アプリに読み込むことはないため、デフォルトでは読み込まれない
        // 古いデータが必要なら、このメソッドで読み込む
        // 一つのファイルに問題があるだけで落とすわけにはいかないため、Try* のみ用意

        public bool TryLoadAllEntries (out List <string> goodFilePaths, out List <string> badFilePaths, bool reloadsLoadedOnes = false)
        {
            goodFilePaths = new List <string> ();
            badFilePaths = new List <string> ();

            try
            {
                // bad の定義は、「読み込みに失敗したファイルが一つ以上あった」
                // ディレクトリーがないのが初期状態のため、ここで false を返す理由はない

                if (Directory.Exists (DirectoryPath) == false)
                    return true;

                // ディレクトリーには余計なファイルがないのが望ましいため、全ての拡張子のファイルについて out に情報を詰める
                // 通常の CRUD ではサブディレクトリーのファイルが扱われないため、ここでも無視
                // オプション一つで読んでしまえるが、生半可なコードで扱ってはセキュリティーリスクにつながる
                // 読み込まれる順序は不定

                foreach (string xFilePath in Directory.GetFiles (DirectoryPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        string xFileName = Path.GetFileName (xFilePath);

                        if (TryParseFileName (xFileName, out KeyType xResult))
                        {
                            // 追加分のみ定期的に速く読み込みたいことも考えられるため、
                            //     辞書にキーが含まれていれば、ファイルの内容を全くチェックしない
                            // *UpdateEntry でもノーチェックで更新される

                            if (reloadsLoadedOnes == false &&
                                ContainsKey (xResult))
                            {
                                goodFilePaths.Add (xFilePath);
                                continue;
                            }

                            string xFileContents = nFile.ReadAllText (xFilePath);
                            nStringDictionary xEntry = nStringDictionary.ParseIniLikeString (xFileContents);

                            // 同じファイル名のまま内容をすり替えられることで不正なキーが辞書などに戻ってくるのを回避
                            // 辞書のキー、辞書の値の中のキー、ファイルの中のキーの整合性が常に問われるデータ構造

                            if (TryGetKeyFromEntry (xEntry, out KeyType xResultAlt) &&
                                Comparer.Equals (xResultAlt, xResult))
                            {
                                Add (xResult, xEntry);
                                goodFilePaths.Add (xFilePath);
                                continue;
                            }
                        }
                    }

                    catch
                    {
                    }

                    badFilePaths.Add (xFilePath);
                }
            }

            catch
            {
            }

            return badFilePaths.Count > 0;
        }
    }
}
