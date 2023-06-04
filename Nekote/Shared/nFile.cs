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

        // GB 単位のファイルも読むなら 4 KB ずつはループを回しすぎの気もする
        // しかし、その下でファイルシステムのキャッシュが利くことや、CPU の L1 キャッシュの大きさなどから、4 KB でよいとのこと

        // HDD 上の二つの大きなファイルの比較など、両方を同時進行で読み込むときには、
        //     大きなバッファに交互に一気読みすることでシークを大幅に減らせる可能性はある

        // c# - File I/O with streams - best memory buffer size - Stack Overflow
        // https://stackoverflow.com/questions/3033771/file-i-o-with-streams-best-memory-buffer-size

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

        // ファイルの先頭を少しだけ読み込み、エンコーディングを判別するメソッド
        // FileStream をそのまま使って後続の処理を行いたいが、それでは File.ReadAllLinesAsync などを実装し直すことになる
        // IO のパフォーマンスについては、ファイルシステムのキャッシュの恩恵があるだろうから、気にするほどでなさそう

        public static Encoding? GetEncoding (string path)
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
        public static string [] ReadAllLines (string path, Encoding? encoding = null)
        {
            return File.ReadAllLines (path, GetEncoding (path) ?? encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static Task <string []> ReadAllLinesAsync (string path, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return File.ReadAllLinesAsync (path, GetEncoding (path) ?? encoding ?? Encoding.UTF8, cancellationToken);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static string ReadAllText (string path, Encoding? encoding = null)
        {
            return File.ReadAllText (path, GetEncoding (path) ?? encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// BOM があれば、そちらが優先される。
        /// </summary>
        public static Task <string> ReadAllTextAsync (string path, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            return File.ReadAllTextAsync (path, GetEncoding (path) ?? encoding ?? Encoding.UTF8, cancellationToken);
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

#pragma warning disable CA1068
        public static Task WriteAllBytesAsync (string path, byte [] values, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllBytesAsync (path, values, cancellationToken);
        }

        public static void WriteAllLines (string path, IEnumerable <string> values, Encoding? encoding = null, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.WriteAllLines (path, values, encoding ?? Encoding.UTF8);
        }

#pragma warning disable CA1068
        public static Task WriteAllLinesAsync (string path, IEnumerable <string> values, Encoding? encoding = null, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllLinesAsync (path, values, encoding ?? Encoding.UTF8, cancellationToken);
        }

        public static void WriteAllText (string path, string? value, Encoding? encoding = null, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.WriteAllText (path, value, encoding ?? Encoding.UTF8);
        }

#pragma warning disable CA1068
        public static Task WriteAllTextAsync (string path, string? value, Encoding? encoding = null, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.WriteAllTextAsync (path, value, encoding ?? Encoding.UTF8, cancellationToken);
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

        // .NET に *Async として用意されているもののラッパーは、.NET の方に async が付いていないこともあって async なしでよい
        // 一方、AppendAllBytesAsync は、async/await でないと FileStream の WriteAsync 中に Dispose が呼ばれる

#pragma warning disable CA1068
        public static async Task AppendAllBytesAsync (string path, byte [] values, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);

            using (FileStream xStream = new FileStream (path, FileMode.Append, FileAccess.Write, FileShare.Read, iDefaultBufferSize, FileOptions.Asynchronous))
            {
                // xStream.Seek (0, SeekOrigin.End);
                await xStream.WriteAsync (values, cancellationToken); // ReadOnlyMemory <byte> として
            }
        }

        public static void AppendAllLines (string path, IEnumerable <string> values, Encoding? encoding = null, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.AppendAllLines (path, values, encoding ?? Encoding.UTF8);
        }

#pragma warning disable CA1068
        public static Task AppendAllLinesAsync (string path, IEnumerable <string> values, Encoding? encoding = null, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.AppendAllLinesAsync (path, values, encoding ?? Encoding.UTF8, cancellationToken);
        }

        public static void AppendAllText (string path, string? value, Encoding? encoding = null, bool createsParentDirectory = true, bool resetsAttributes = true)
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            File.AppendAllText (path, value, encoding ?? Encoding.UTF8);
        }

#pragma warning disable CA1068
        public static Task AppendAllTextAsync (string path, string? value, Encoding? encoding = null, CancellationToken cancellationToken = default, bool createsParentDirectory = true, bool resetsAttributes = true)
#pragma warning restore CA1068
        {
            FileInfo xFile = new FileInfo (path);
            iCreateParentDirectoryAndOrResetAttributesIfRequired (xFile, createsParentDirectory, resetsAttributes);
            return File.AppendAllTextAsync (path, value, encoding ?? Encoding.UTF8, cancellationToken);
        }

        // まずは、工夫を凝らさない、シンプルな比較を実装しておく
        // ファイルがないとか読めないとかなら例外が飛ぶ

        public static bool Equals (string path1, string path2)
        {
            FileInfo xFile1 = new FileInfo (path1),
                xFile2 = new FileInfo (path2);

            if (xFile1.Length != xFile2.Length)
                return false;

            using (FileStream xStream1 = new FileStream (path1, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (FileStream xStream2 = new FileStream (path2, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte [] xValues1 = new byte [iDefaultBufferSize],
                    xValues2 = new byte [iDefaultBufferSize];

                int xLength;

                while ((xLength = xStream1.Read (xValues1, 0, iDefaultBufferSize)) > 0)
                {
                    // 長さは「フォーマット」というより「データ」の問題
                    // 二つ目のファイルが「一つ目のファイルと同じ長さであるべき」というルールに反しているわけだが、
                    //     それは一つ目のファイルがあっての相対的なことで、二つ目のファイルに絶対的な問題があるわけでない

                    if (xStream2.Read (xValues2, 0, iDefaultBufferSize) != xLength)
                        throw new nDataException ();

                    if (nArray.Equals (xValues1, 0, xValues2, 0, xLength) == false)
                        return false;
                }

                return true;
            }
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
