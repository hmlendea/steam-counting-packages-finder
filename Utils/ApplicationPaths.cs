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

        public static string FreePackagesListPath => Path.Combine(DataDirectory, "free-packages.lst");

        public static string AccountsFilePath => Path.Combine(ApplicationDirectory, "accounts.txt");
        
        public static string AlreadyCheckedPackagesListFilePath => Path.Combine(ApplicationDirectory, "checked-packages.lst");

        public static string CountingPackagesListFilePath => Path.Combine(ApplicationDirectory, "counting-packages.lst");
    }
}
