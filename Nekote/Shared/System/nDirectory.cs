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
