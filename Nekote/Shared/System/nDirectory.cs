using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nDirectory
    {
        public static bool CanCreate (string path)
        {
            return Directory.Exists (path) == false && File.Exists (path) == false;
        }

        /// <summary>
        /// 元々あってもエラーにならない。
        /// </summary>
        public static DirectoryInfo Create (string path)
        {
            // 既存でも新たに作られても DirectoryInfo を返すので、存在チェックを省略

            // Directory.CreateDirectory Method (System.IO) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.createdirectory

            // Windows で試した限り、Hoge 内に Moge を作るにおいて Hoge が読み取り専用でもエラーにならない
            // 属性のリセットは不要

            return Directory.CreateDirectory (path);
        }

        // まずは、存在するディレクトリーを、存在しないパスのところに移動またはコピーするのが前提の簡単なメソッドを用意
        // 扱わないパスの（正規表現での）指定、処理中にディレクトリーの内容が変わっている場合への対処などは、いずれ専用クラスで

        private static void iMoveOrCopy (bool isMoving, DirectoryInfo sourceDirectory, DirectoryInfo destDirectory, bool overwrites, bool resetsAttributes)
        {
            // sourceDirectory がないなら destDirectory を作るより先に落ちてほしい
            DirectoryInfo [] xSubdirectories = sourceDirectory.GetDirectories ();

            // 移動でもコピーでも、dest* 側が存在しなかったなら、source* 側と同じ作成日時および更新日時を持つべき
            // しかし、存在したなら「併合」になるため、作成日時が変わらず、更新日時が併合により更新されるべき
            // いずれの場合も、アクセス日時については OS などの環境に任せる

            bool xIsDestDirectoryCreated = false;

            if (destDirectory.Exists == false)
            {
                destDirectory.Create ();
                xIsDestDirectoryCreated = true;
            }

            foreach (DirectoryInfo xSourceSubdirectory in xSubdirectories)
            {
                DirectoryInfo xDestSubdirectory = new DirectoryInfo (nPath.Join (destDirectory.FullName, xSourceSubdirectory.Name));
                iMoveOrCopy (isMoving, xSourceSubdirectory, xDestSubdirectory, overwrites, resetsAttributes);
            }

            foreach (FileInfo xFile in sourceDirectory.GetFiles ())
            {
                if (resetsAttributes)
                    xFile.Attributes = FileAttributes.Normal;

                string xDestFilePath = nPath.Join (destDirectory.FullName, xFile.Name);

                if (isMoving)
                    xFile.MoveTo (xDestFilePath, overwrites);

                else xFile.CopyTo (xDestFilePath, overwrites);
            }

            if (xIsDestDirectoryCreated)
            {
                destDirectory.CreationTimeUtc = sourceDirectory.CreationTimeUtc;
                destDirectory.LastWriteTimeUtc = sourceDirectory.LastWriteTimeUtc;
            }

            // 「移動」なら、上記コードがうまく動けばディレクトリーが空になっているはず
            // そうでないなら、DirectoryInfo.Delete により例外が飛ぶべき

            // DirectoryInfo.Delete Method (System.IO) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.io.directoryinfo.delete

            if (isMoving /* && sourceDirectory.GetFileSystemInfos ().Length == 0 */)
            {
                if (resetsAttributes)
                    sourceDirectory.Attributes = FileAttributes.Directory;

                sourceDirectory.Delete ();
            }
        }

        public static void Move (string sourcePath, string destPath, bool overwrites, bool resetsAttributes = true)
        {
            iMoveOrCopy (isMoving: true, new DirectoryInfo (sourcePath), new DirectoryInfo (destPath), overwrites, resetsAttributes);
        }

        public static void Copy (string sourcePath, string destPath, bool overwrites, bool resetsAttributes = true)
        {
            iMoveOrCopy (isMoving: false, new DirectoryInfo (sourcePath), new DirectoryInfo (destPath), overwrites, resetsAttributes);
        }

        private static void iDelete (DirectoryInfo directory, bool isRecursive, bool resetsAttributes)
        {
            if (isRecursive)
            {
                foreach (DirectoryInfo xSubdirectory in directory.GetDirectories ())
                    iDelete (xSubdirectory, isRecursive, resetsAttributes);

                foreach (FileInfo xFile in directory.GetFiles ())
                {
                    if (resetsAttributes)
                        xFile.Attributes = FileAttributes.Normal;

                    xFile.Delete ();
                }
            }

            if (resetsAttributes)
                directory.Attributes = FileAttributes.Directory;

            directory.Delete ();
        }

        /// <summary>
        /// 元々なくてもエラーにならない。
        /// </summary>
        public static void Delete (string path, bool isRecursive, bool resetsAttributes = true)
        {
            DirectoryInfo xDirectory = new DirectoryInfo (path);

            if (xDirectory.Exists)
                iDelete (xDirectory, isRecursive, resetsAttributes);
        }

        private static void iDeleteIfEmpty (DirectoryInfo directory, bool isRecursive, bool deletesEmptyFiles, bool resetsAttributes)
        {
            if (isRecursive)
            {
                foreach (DirectoryInfo xSubdirectory in directory.GetDirectories ())
                    iDeleteIfEmpty (xSubdirectory, isRecursive, deletesEmptyFiles, resetsAttributes);

                if (deletesEmptyFiles)
                {
                    foreach (FileInfo xFile in directory.GetFiles ())
                    {
                        if (xFile.Length == 0)
                        {
                            if (resetsAttributes)
                                xFile.Attributes = FileAttributes.Normal;

                            xFile.Delete ();
                        }
                    }
                }
            }

            if (directory.GetFileSystemInfos ().Length == 0)
            {
                if (resetsAttributes)
                    directory.Attributes = FileAttributes.Directory;

                directory.Delete ();
            }
        }

        /// <summary>
        /// 元々なくてもエラーにならない。
        /// </summary>
        public static void DeleteIfEmpty (string path, bool isRecursive = true, bool deletesEmptyFiles = true, bool resetsAttributes = true)
        {
            DirectoryInfo xDirectory = new DirectoryInfo (path);

            if (xDirectory.Exists)
                iDeleteIfEmpty (xDirectory, isRecursive, deletesEmptyFiles, resetsAttributes);
        }
    }
}
