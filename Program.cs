using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using SteamAccountCreator.Entities;
using SteamAccountCreator.Extensions;
using SteamAccountCreator.Processors;
using SteamAccountCreator.Utils;

namespace SteamAccountCreator
{
    public sealed class Program
    {
        const int PackagesCountPerAccount = 50;

        static void Main(string[] args)
        {
            FindCountablePackages();
        }

        static void FindCountablePackages()
        {
            // TODO: Handle already-activated packages

            DateTime beginDate = DateTime.Now;

            Log.Info($"Setting up the driver");
            IWebDriver driver = SetupDriver();

            List<string> allPackages = File.ReadAllLines(ApplicationPaths.FreePackagesListPath).ToList();
            List<AccountDetails> accounts = File
                .ReadAllLines(ApplicationPaths.AccountsFilePath)
                .Select(x => new AccountDetails(x.Split(':')[0], x.Split(':')[1]))
                .ToList();

            List<List<string>> splitPackages = SplitList(allPackages, PackagesCountPerAccount).ToList();

            if (splitPackages.Count > accounts.Count)
            {
                // TODO: Better exception
                throw new Exception($"Not enough accounts ({accounts.Count}/{splitPackages.Count})");
            }
            
            for (int i = 0; i < splitPackages.Count; i++)
            {
                AccountDetails account = accounts[i];
                List<string> packages = splitPackages[i];

                SteamProcessor steamProcessor = new SteamProcessor(driver, account);

                Log.Info($"Activating packages for {account.Username}");
                //Log.Info($"Logging in {account.Username}");
                steamProcessor.LogIn();

                int rangeBegin = i * PackagesCountPerAccount;
                int rangeEnd = (i + 1) * PackagesCountPerAccount;

                //Log.Info($"Activating packages {rangeBegin}-{rangeEnd}");
                int initialGamesCount = steamProcessor.GetGamesCount();

                foreach (string package in packages)
                {
                    //int initialGamesCount = steamProcessor.GetGamesCount();
                    steamProcessor.ActivatePackage(package);
                    //int finalGamesCount = steamProcessor.GetGamesCount();

                    //if (finalGamesCount > initialGamesCount)
                    //{
                    //    Log.Info($"Found countable package: {package}!");
                    //}
                }

                int finalGamesCount = steamProcessor.GetGamesCount();

                if (finalGamesCount > initialGamesCount)
                {
                    Log.Info($"Found countable packages in range {rangeBegin}-{rangeEnd}!");
                }

                //Log.Info($"Logging out {account.Username}");
                steamProcessor.LogOut();
                steamProcessor.Close();
            }
        }

        static IWebDriver SetupDriver()
        {
            ChromeOptions options = new ChromeOptions();
            options.PageLoadStrategy = PageLoadStrategy.None;
            options.AddArgument("--silent");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-translate");
            options.AddArgument("--disable-infobars");
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--start-maximized");
            options.AddArgument("--blink-settings=imagesEnabled=false");

            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;

            IWebDriver driver = new ChromeDriver(service, options);
            driver.Manage().Window.Maximize();

            return driver;
        }

        static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize=30)  
        {        
            for (int i=0; i < locations.Count; i+= nSize) 
            { 
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i)); 
            }  
        } 
    }
}
