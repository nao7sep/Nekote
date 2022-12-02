using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nFile
    {
        // FileStream に internal const int DefaultBufferSize = 4096 とある
        // ファイルアクセスに特化したクラスの追加時に移動の可能性があるため public にしない

        // FileStream.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/FileStream.cs

        internal const int iDefaultBufferSize = 4096;

        public static bool CanCreate (string path)
        {
            return File.Exists (path) == false && Directory.Exists (path) == false;
        }

        // Directory.CreateDirectory にならい、FileInfo を返す

        /// <summary>
        /// 元々あってもエラーにならない。
        /// </summary>
        public static FileInfo Create (string path, bool createsParentDirectory = true)
        {
            FileInfo xFile = new FileInfo (path);

            if (xFile.Exists == false)
            {
                if (createsParentDirectory && xFile.Directory != null && xFile.Directory.Exists == false)
                    xFile.Directory.Create ();

                // FileInfo.Create Method (System.IO) | Microsoft Learn
                // https://learn.microsoft.com/en-us/dotnet/api/system.io.fileinfo.create

                using (xFile.Create ())
                {
                }
            }

            return xFile;
        }

        // 「たぶん UTF-8 だが、他のエンコーディングの BOM があればそれを尊重する」という読み方をしたいときがある
        // そのためのメソッドを揃えるにおいて、バイト列のものだけがないと探す可能性があるのでラップしておく

        public static byte [] ReadAllBytes (string path)
        {
            return File.ReadAllBytes (path);
        }

        public static Task <byte []> ReadAllBytesAsync (string path, CancellationToken cancellationToken = default)
        {
            return File.ReadAllBytesAsync (path, cancellationToken);
        }

        private static Encoding? iGetEncoding (string path)
        {
            // ファイルがなければ、どうせどこかで落ちなければならない
            // ファイルがあれば、長さ0でも問題にならない

            using (FileStream xStream = new FileStream (path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte [] xValues = new byte [nBom.MaxLength];
                int xLength = xStream.Read (xValues, 0, nBom.MaxLength);
                return nBom.GetEncoding (xValues, 0, xLength);
            }
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static string [] ReadAllLines (string path, Encoding encoding)
        {
            return File.ReadAllLines (path, iGetEncoding (path) ?? encoding);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static Task <string []> ReadAllLinesAsync (string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return File.ReadAllLinesAsync (path, iGetEncoding (path) ?? encoding, cancellationToken);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static string ReadAllText (string path, Encoding encoding)
        {
            return File.ReadAllText (path, iGetEncoding (path) ?? encoding);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static Task <string> ReadAllTextAsync (string path, Encoding encoding, CancellationToken cancellationToken = default)
        {
            return File.ReadAllTextAsync (path, iGetEncoding (path) ?? encoding, cancellationToken);
        }

        private static void iCreateParentDirectoryAndOrResetAttributesIfRequired (FileInfo file, bool createsParentDirectory, bool resetsAttributes)
        {
            if (createsParentDirectory && file.Directory != null && file.Directory.Exists == false)
                file.Directory.Create ();

            if (resetsAttributes && file.Exists)
                file.Attributes = FileAttributes.Normal;
        }

        public static void WriteAllBytes (string path, byte [] values, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.WriteAllBytes (path, values);
        }

        public static Task WriteAllBytesAsync (string path, byte [] values, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllBytesAsync (path, values, cancellationToken);
        }

        public static void WriteAllLines (string path, IEnumerable <string> values, Encoding encoding, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.WriteAllLines (path, values, encoding);
        }

        public static Task WriteAllLinesAsync (string path, IEnumerable <string> values, Encoding encoding, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllLinesAsync (path, values, encoding, cancellationToken);
        }

        public static void WriteAllText (string path, string? value, Encoding encoding, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.WriteAllText (path, value, encoding);
        }

        public static Task WriteAllTextAsync (string path, string? value, Encoding encoding, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllTextAsync (path, value, encoding, cancellationToken);
        }

        // File.AppendAllBytes* がないようなので実装
        // .NET の実装に含まれる Validate を呼べないので path のチェックがすぐには行われないが、他のところで行われるため大丈夫
        // .NET の実装 では SafeFileHandle と RandomAccess の組み合わせをよく見るが、FileStream.WriteAsync が使いやすいので、そちらに統一
        // コンストラクターのパラメーターについては、WriteAllBytesAsync に OpenHandle (path, FileMode.Create, FileAccess.Write, FileShare.Read, FileOptions.Asynchronous) とある

        // File.cs
        // https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/IO/File.cs

        public static void AppendAllBytes (string path, byte [] values, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);

            using (FileStream xStream = new FileStream (path, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                // xStream.Seek (0, SeekOrigin.End);
                xStream.Write (values, 0, values.Length);
            }
        }

        public static Task AppendAllBytesAsync (string path, byte [] values, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);

            using (FileStream xStream = new FileStream (path, FileMode.Append, FileAccess.Write, FileShare.Read, iDefaultBufferSize, FileOptions.Asynchronous))
            {
                // xStream.Seek (0, SeekOrigin.End);
                return xStream.WriteAsync (values, 0, values.Length, cancellationToken);
            }
        }

        public static void AppendAllLines (string path, IEnumerable <string> values, Encoding encoding, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.AppendAllLines (path, values, encoding);
        }

        public static Task AppendAllLinesAsync (string path, IEnumerable <string> values, Encoding encoding, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.AppendAllLinesAsync (path, values, encoding, cancellationToken);
        }

        public static void AppendAllText (string path, string? value, Encoding encoding, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.AppendAllText (path, value, encoding);
        }

        public static Task AppendAllTextAsync (string path, string? value, Encoding encoding, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.AppendAllTextAsync (path, value, encoding, cancellationToken);
        }

        private static void iResetAttributesAndOrCreateParentDirectoryIfRequired (string sourcePath, string destPath, bool resetsAttributes, bool createsParentDirectory)
        {
            if (resetsAttributes)
            {
                FileInfo xSourceFile = new FileInfo (sourcePath);

                if (xSourceFile.Exists)
                    xSourceFile.Attributes = FileAttributes.Normal;
            }

            if (createsParentDirectory)
            {
                FileInfo xDestFile = new FileInfo (destPath);

                if (xDestFile.Directory != null && xDestFile.Directory.Exists == false)
                    xDestFile.Directory.Create ();
            }
        }

        public static void Move (string sourcePath, string destPath, bool overwrites, bool resetsAttributes = true, bool createsParentDirectory = true)
        {
            iResetAttributesAndOrCreateParentDirectoryIfRequired (sourcePath, destPath, resetsAttributes, createsParentDirectory);
            File.Move (sourcePath, destPath, overwrites);
        }

        public static void Copy (string sourcePath, string destPath, bool overwrites, bool resetsAttributes = true, bool createsParentDirectory = true)
        {
            iResetAttributesAndOrCreateParentDirectoryIfRequired (sourcePath, destPath, resetsAttributes, createsParentDirectory);
            File.Copy (sourcePath, destPath, overwrites);
        }

        /// <summary>
        /// 元々なくてもエラーにならない。
        /// </summary>
        public static void Delete (string path, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);

            if (xFile.Exists)
            {
                if (resetsAttributes)
                    xFile.Attributes = FileAttributes.Normal;

                xFile.Delete ();
            }
        }

        /// <summary>
        /// 元々なくてもエラーにならない。
        /// </summary>
        public static void DeleteIfEmpty (string path, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);

            if (xFile.Exists)
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
}
