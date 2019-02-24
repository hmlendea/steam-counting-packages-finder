using System;
using System.IO;
using System.Reflection;

namespace SteamAccountCreator.Utils
{
    public sealed class ApplicationPaths
    {
        static string rootDirectory;

        /// <summary>
        /// The application directory.
        /// </summary>
        public static string ApplicationDirectory
        {
            get
            {
                if (rootDirectory == null)
                {
                    rootDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                return rootDirectory;
            }
        }

        public static string DataDirectory => Path.Combine(ApplicationDirectory, "Data");

        public static string SteamDbExtensionFilePath => Path.Combine(DataDirectory, "Steam Database.crx");
        
        public static string NonCountingPackagesListFilePath => Path.Combine(DataDirectory, "noncounting-packages.lst");

        public static string CountingPackagesListFilePath => Path.Combine(ApplicationDirectory, "counting-packages.lst");
        
        public static string FailedPackagesListFilePath => Path.Combine(ApplicationDirectory, "failed-packages.lst");

        public static string AccountsFilePath => Path.Combine(ApplicationDirectory, "accounts.txt");
    }
}
