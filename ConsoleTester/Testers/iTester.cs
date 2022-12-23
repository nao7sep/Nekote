﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace ConsoleTester
{
    internal static class iTester
    {
        // 1次元目に各テスト、2次元目に所要時間が入っている nMultiArray を想定
        // 決め打ちの仕様になるが、今後も同様のテストを行うだろうから、コードを共通化

        public static string FormatLabelsAndElapsedTimes (string [] labels, nMultiArray <TimeSpan> elapsed)
        {
            // <TimeSpan> なので、y.Value は Nullable でない
            // ElementType? Value としての宣言だが、値型だとうまく処理される
            return string.Join (Environment.NewLine, Enumerable.Range (0, labels.Length).Select (x => $"{labels [x]}: {nTimeSpan.Average (elapsed.GetSubarray (x).Subarrays.Select (y => y.Value)).TotalMilliseconds}ms"));
        }

        // 指定されたディレクトリーから、名前で指定されたファイルまたはディレクトリーのうち一つ目を探す

        // ignoredSubdirectoryPaths に sourceDirectory が含まれているから何も行われないというのは不毛なので、
        //     ignoredSubdirectoryPaths は、名前通り、サブディレクトリーに対してのみ適用される

        // 二つ目の FindFileOrDirectory がメインだが、internal クラスなので、
        //     何かに役立つかもしれない一つ目も public にしておく

        public static string? FindFileOrDirectory (DirectoryInfo sourceDirectory, string targetName, List <string> ignoredSubdirectoryPaths)
        {
            foreach (DirectoryInfo xSubdirectory in sourceDirectory.GetDirectories ())
            {
                if (ignoredSubdirectoryPaths.Contains (xSubdirectory.FullName, StringComparer.OrdinalIgnoreCase))
                    continue;

                if (string.Equals (xSubdirectory.Name, targetName, StringComparison.OrdinalIgnoreCase))
                    return xSubdirectory.FullName;

                string? xPath = FindFileOrDirectory (xSubdirectory, targetName, ignoredSubdirectoryPaths);

                if (xPath != null)
                    return xPath;
            }

            foreach (FileInfo xFile in sourceDirectory.GetFiles ())
            {
                if (string.Equals (xFile.Name, targetName, StringComparison.OrdinalIgnoreCase))
                    return xFile.FullName;
            }

            return null;
        }

        public static string? FindFileOrDirectory (string currentDirectoryPath, string targetName)
        {
            if (Path.IsPathFullyQualified (currentDirectoryPath) == false)
                throw new nArgumentException ();

            DirectoryInfo? xCurrentDirectory = new DirectoryInfo (currentDirectoryPath);

            List <string> xIgnoredSubdirectoryPaths = new List <string> ();

            while (xCurrentDirectory != null)
            {
                string? xPath = FindFileOrDirectory (xCurrentDirectory, targetName, xIgnoredSubdirectoryPaths);

                if (xPath != null)
                    return xPath;

                xIgnoredSubdirectoryPaths.Add (xCurrentDirectory.FullName);
                xCurrentDirectory = xCurrentDirectory.Parent;
            }

            return null;
        }
    }
}
