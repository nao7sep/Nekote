using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nekote;

namespace NekoteConsole
{
    internal static class iEnvironmentTester
    {
        // マルチプラットフォームにおける各値を調べておく

        // 2022年12月27日の結果

        // SpecialFolders-20221227T073831Z.txt (Windows 11)
        // SpecialFolders-20221227T073927Z.txt (Windows 10)
        // SpecialFolders-20221227T074350Z.txt (Mac)

        // Windows 11/10 では、全く同一の結果が得られた
        // 作業ミスを疑い、同じことを繰り返しても一致

        public static void GetSpecialFolderPaths ()
        {
            string xFilePath = nPath.Map (Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory), $"SpecialFolders-{DateTime.UtcNow.ToMinimalUniversalDateTimeString ()}.txt");

            // Enum.GetValues からデータを取ると、MyDocuments と Personal は値が同じになっているため、Name のところに MyDocuments が二度入る

            // Environment.SpecialFolder Enum (System) | Microsoft Learn
            // https://learn.microsoft.com/en-us/dotnet/api/system.environment.specialfolder

            nFile.WriteAllLines (xFilePath, Enum.GetNames <Environment.SpecialFolder> ().Select (x => (Name: x, Path: Environment.GetFolderPath (Enum.Parse <Environment.SpecialFolder> (x))))
                .OrderBy (y => y.Name, StringComparer.OrdinalIgnoreCase).Select (z => $"{z.Name}: {nString.GetLiteralIfNullOrEmpty (z.Path)}"));
        }
    }
}
