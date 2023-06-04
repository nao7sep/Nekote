using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nEnvironment
    {
        // Mac で取得できるパスの一覧の含まれる SpecialFolders-20221227T074350Z.txt において値側が (Empty) でないものは多くない
        // それらの値は Windows でも取得できる
        // それらのうち、プログラムやユーザーがデータを日常的に出し入れするところについて、パスと Map* を用意しておく
        // ProgramFiles と System については、プログラムやユーザーがデータを出し入れするところでないため除外した
        // UserProfile は、Mac では MyDocuments や Personal と内容が同じだが、Windows では異なる
        // 次のページには Applications should not create files or folders at this level; they should put their data under the locations referred to by ApplicationData とあるが、
        //     ここにも隠しフォルダーとして .vscode を初めとするさまざまなものが作られるようになっているため、Nekote 側でパスや Map* を用意しない理由はない

        // Environment.SpecialFolder Enum (System) | Microsoft Learn
        // https://learn.microsoft.com/en-us/dotnet/api/system.environment.specialfolder

        private static string? mApplicationDataDirectoryPath = null;

        public static string ApplicationDataDirectoryPath
        {
            get
            {
                if (mApplicationDataDirectoryPath == null)
                    mApplicationDataDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);

                return mApplicationDataDirectoryPath;
            }
        }

        public static string MapApplicationDataDirectoryPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (ApplicationDataDirectoryPath))
                throw new nDataException ();

            return nPath.Map (ApplicationDataDirectoryPath, paths);
        }

        // =============================================================================

        private static string? mCommonApplicationDataDirectoryPath = null;

        public static string CommonApplicationDataDirectoryPath
        {
            get
            {
                if (mCommonApplicationDataDirectoryPath == null)
                    mCommonApplicationDataDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.CommonApplicationData);

                return mCommonApplicationDataDirectoryPath;
            }
        }

        public static string MapCommonApplicationDataDirectoryPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (CommonApplicationDataDirectoryPath))
                throw new nDataException ();

            return nPath.Map (CommonApplicationDataDirectoryPath, paths);
        }

        // =============================================================================

        private static string? mDesktopDirectoryPath = null;

        public static string DesktopDirectoryPath
        {
            get
            {
                if (mDesktopDirectoryPath == null)
                    mDesktopDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.DesktopDirectory);

                return mDesktopDirectoryPath;
            }
        }

        public static string MapDesktopDirectoryPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (DesktopDirectoryPath))
                throw new nDataException ();

            return nPath.Map (DesktopDirectoryPath, paths);
        }

        // =============================================================================

        private static string? mLocalApplicationDataDirectoryPath = null;

        public static string LocalApplicationDataDirectoryPath
        {
            get
            {
                if (mLocalApplicationDataDirectoryPath == null)
                    mLocalApplicationDataDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);

                return mLocalApplicationDataDirectoryPath;
            }
        }

        public static string MapLocalApplicationDataDirectoryPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (LocalApplicationDataDirectoryPath))
                throw new nDataException ();

            return nPath.Map (LocalApplicationDataDirectoryPath, paths);
        }

        // =============================================================================

        private static string? mUserProfileDirectoryPath = null;

        public static string UserProfileDirectoryPath
        {
            get
            {
                if (mUserProfileDirectoryPath == null)
                    mUserProfileDirectoryPath = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);

                return mUserProfileDirectoryPath;
            }
        }

        public static string MapUserProfileDirectoryPath (params string [] paths)
        {
            if (string.IsNullOrEmpty (UserProfileDirectoryPath))
                throw new nDataException ();

            return nPath.Map (UserProfileDirectoryPath, paths);
        }
    }
}
