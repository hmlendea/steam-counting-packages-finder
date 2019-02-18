﻿using System;
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
            DateTime beginDate = DateTime.Now;

            Log.Info($"Setting up the driver");
            IWebDriver driver = SetupDriver();

            List<string> packagesToCheck = File
                .ReadAllLines(ApplicationPaths.FreePackagesListPath)
                .ToList();

            if (File.Exists(ApplicationPaths.FailedPackagesListFilePath))
            {
                List<string> failedPackages = File
                    .ReadAllLines(ApplicationPaths.CountingPackagesListFilePath)
                    .ToList();

                packagesToCheck = packagesToCheck.Except(failedPackages).ToList();
                packagesToCheck.AddRange(failedPackages);
            }

            if (File.Exists(ApplicationPaths.NonCountingPackagesListFilePath))
            {
                List<string> nonCountingPackages = File
                    .ReadAllLines(ApplicationPaths.NonCountingPackagesListFilePath)
                    .ToList();
                
                packagesToCheck = packagesToCheck
                    .Except(nonCountingPackages)
                    .ToList();
            }

            if (File.Exists(ApplicationPaths.CountingPackagesListFilePath))
            {
                List<string> countingPackages = File
                    .ReadAllLines(ApplicationPaths.CountingPackagesListFilePath)
                    .ToList();
                
                packagesToCheck = packagesToCheck
                    .Except(countingPackages)
                    .ToList();
            }

            List<AccountDetails> accounts = File
                .ReadAllLines(ApplicationPaths.AccountsFilePath)
                .Select(x => new AccountDetails(x.Split(':')[0], x.Split(':')[1]))
                .ToList();

            List<List<string>> splitPackages = SplitList(packagesToCheck, PackagesCountPerAccount).ToList();

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
                //int initialGamesCount = steamProcessor.GetGamesCount();

                foreach (string package in packages)
                {
                    int initialGamesCount = steamProcessor.GetGamesCount();
                    
                    bool success = steamProcessor.ActivatePackage(package);

                    if (!success)
                    {
                        Log.Info($"Failed to activate package: {package}!");
                        SaveToFile(ApplicationPaths.FailedPackagesListFilePath, package);

                        continue;
                    }
                    else
                    {
                        RemoveFromFile(ApplicationPaths.FailedPackagesListFilePath, package);
                    }

                    int finalGamesCount = steamProcessor.GetGamesCount();

                    if (finalGamesCount > initialGamesCount)
                    {
                        Log.Info($"Found countable package: {package}!");
                        SaveToFile(ApplicationPaths.CountingPackagesListFilePath, package);
                    }
                    else
                    {
                        SaveToFile(ApplicationPaths.NonCountingPackagesListFilePath, package);
                    }
                }

                //int finalGamesCount = steamProcessor.GetGamesCount();

                //if (finalGamesCount > initialGamesCount)
                //{
                //    Log.Info($"Found countable packages in range {rangeBegin}-{rangeEnd}!");
                //    SaveToResults($"range {rangeBegin}-{rangeEnd}");
                //}

                //Log.Info($"Logging out {account.Username}");
                steamProcessor.LogOut();
                steamProcessor.Close();
            }
        }

        static void SaveToFile(string path, string subid)
        {
            List<string> fileLines = new List<string>();
            
            if (File.Exists(path))
            {
                fileLines = File.ReadAllLines(path).ToList();

                if (fileLines.Contains(subid))
                {
                    return;
                }
            }
            
            fileLines.Add(subid);
            File.WriteAllLines(path, fileLines);
        }

        static void RemoveFromFile(string path, string subid)
        {
            List<string> fileLines = new List<string>();
            
            if (File.Exists(path))
            {
                fileLines = File.ReadAllLines(path).ToList();

                if (!fileLines.Contains(subid))
                {
                    return;
                }
            }
            
            fileLines.Remove(subid);
            File.WriteAllLines(path, fileLines);
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
            //options.AddArgument("--headless");
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
