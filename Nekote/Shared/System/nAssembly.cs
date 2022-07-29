using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Nekote
{
    public static class nAssembly
    {
        public static string? GetNameString (Assembly assembly)
        {
            string? xName = assembly.GetName ().Name;

            if (string.IsNullOrEmpty (xName) == false)
                return xName;

            else return null;
        }

        // 次のページにあるもののうち、外部パッケージを必要としないものを、ページで現れた順に
        // https://docs.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#assembly-attribute-properties

        public static string? GetCompanyString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyCompanyAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Company) == false)
                return xAttribute.Company;

            else return null;
        }

        public static string? GetConfigurationString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyConfigurationAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Configuration) == false)
                return xAttribute.Configuration;

            else return null;
        }

        public static string? GetCopyrightString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyCopyrightAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Copyright) == false)
                return xAttribute.Copyright;

            else return null;
        }

        public static string? GetDescriptionString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyDescriptionAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Description) == false)
                return xAttribute.Description;

            else return null;
        }

        public static string? GetFileVersionOriginalString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyFileVersionAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Version) == false)
                return xAttribute.Version;

            else return null;
        }

        public static Version? GetFileVersion (Assembly assembly)
        {
            if (Version.TryParse (GetFileVersionOriginalString (assembly), out Version? xResult))
                return xResult;

            else return null;
        }

        public static string? GetFileVersionString (Assembly assembly, int fieldCount)
        {
            Version? xVersion = GetFileVersion (assembly);

            if (xVersion != null)
                return nConvert.VersionToString (xVersion, fieldCount);

            else return null;
        }

        public static string? GetInformationalVersionOriginalString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyInformationalVersionAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.InformationalVersion) == false)
                return xAttribute.InformationalVersion;

            else return null;
        }

        public static Version? GetInformationalVersion (Assembly assembly)
        {
            if (Version.TryParse (GetInformationalVersionOriginalString (assembly), out Version? xResult))
                return xResult;

            else return null;
        }

        public static string? GetInformationalVersionString (Assembly assembly, int fieldCount)
        {
            Version? xVersion = GetInformationalVersion (assembly);

            if (xVersion != null)
                return nConvert.VersionToString (xVersion, fieldCount);

            else return null;
        }

        public static string? GetProductString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyProductAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Product) == false)
                return xAttribute.Product;

            else return null;
        }

        /// <summary>
        /// 画面に表示するなら GetTitleOrNameString を。
        /// </summary>
        public static string? GetTitleString (Assembly assembly)
        {
            var xAttribute = assembly.GetCustomAttribute <AssemblyTitleAttribute> ();

            if (xAttribute != null && string.IsNullOrEmpty (xAttribute.Title) == false)
                return xAttribute.Title;

            else return null;
        }

        public static string? GetTitleOrNameString (Assembly assembly)
        {
            string? xTitle = GetTitleString (assembly);

            if (string.IsNullOrEmpty (xTitle) == false)
                return xTitle;

            else
            {
                string? xName = GetNameString (assembly);

                if (string.IsNullOrEmpty (xName) == false)
                    return xName;

                else return null;
            }
        }

        public static Version? GetVersion (Assembly assembly)
        {
            return assembly.GetName ().Version;
        }

        public static string? GetVersionString (Assembly assembly, int fieldCount)
        {
            Version? xVersion = GetVersion (assembly);

            if (xVersion != null)
                return nConvert.VersionToString (xVersion, fieldCount);

            else return null;
        }
    }
}
