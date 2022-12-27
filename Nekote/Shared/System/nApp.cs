using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nApp
    {
        private static Assembly? mAssembly = null;

        public static Assembly? Assembly
        {
            get
            {
                if (mAssembly == null)
                {
                    // null が返ってくることがある
                    // そのうち取り組む ASP.NET でもそうだったはず
                    // 例外は飛んでこないようだ

                    // Assembly.GetEntryAssembly Method (System.Reflection) | Microsoft Learn
                    // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.getentryassembly

                    mAssembly = Assembly.GetEntryAssembly ();
                }

                return mAssembly;
            }
        }

        private static string? mDirectoryPath = null;

        public static string? DirectoryPath
        {
            get
            {
                if (mDirectoryPath == null)
                {
                    if (Assembly != null)
                    {
                        try
                        {
                            // null にならないが、"" が得られることがある
                            // 例外が飛ぶこともあるとのこと

                            // Assembly.Location Property (System.Reflection) | Microsoft Learn
                            // https://learn.microsoft.com/en-us/dotnet/api/system.reflection.assembly.location

                            if (string.IsNullOrEmpty (Assembly.Location) == false && Path.IsPathFullyQualified (Assembly.Location))
                            {
                                mDirectoryPath = Path.GetDirectoryName (Assembly.Location);
                                goto End;
                            }
                        }

                        catch
                        {
                        }
                    }

                    try
                    {
                        // nLibrary.Assembly は null にならないようだが、Location は例外を投げる可能性がある

                        if (string.IsNullOrEmpty (nLibrary.Assembly.Location) == false && Path.IsPathFullyQualified (nLibrary.Assembly.Location))
                        {
                            mDirectoryPath = Path.GetDirectoryName (nLibrary.Assembly.Location);
                            goto End;
                        }
                    }

                    catch
                    {
                    }

                    try
                    {
                        // "" や（パスを持たない）ファイル名になることがあるらしい
                        // 例外が飛ぶこともあるとのこと

                        // Environment.GetCommandLineArgs Method (System) | Microsoft Learn
                        // https://learn.microsoft.com/en-us/dotnet/api/system.environment.getcommandlineargs

                        string [] xArgs = Environment.GetCommandLineArgs ();

                        if (xArgs.Length >= 1 && Path.IsPathFullyQualified (xArgs [0]))
                        {
                            mDirectoryPath = Path.GetDirectoryName (xArgs [0]);
                            goto End;
                        }
                    }

                    catch
                    {
                    }
                }

            End:
                return mDirectoryPath;
            }
        }

        /// <summary>
        /// ローカルパスなので、ディレクトリーの区切り文字は実行環境のものに。
        /// </summary>
        public static string MapPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (DirectoryPath))
                throw new nDataException ();

            return nPath.Map (DirectoryPath, paths);
        }
    }
}
