using System;
using System.Collections.Generic;
using System.Linq;

using OpenQA.Selenium;

using SteamAccountCreator.Utils;

namespace SteamAccountCreator.Processors
{
    public sealed class SteamDbProcessor : WebProcessor
    {
        public string HomePageUrl => "https://steamdb.info";
        public string FreePackagesListUrl => $"{HomePageUrl}/freepackages";
        
        public SteamDbProcessor(IWebDriver driver)
            : base(driver)
        {
        }

        public IEnumerable<string> GetFreePackagesList()
        {
            GoToUrl(FreePackagesListUrl);

            By freePackagesBoxSelector = By.Id("freepackages");
            By packageSelector = By.ClassName("package");

            WaitForElementToBeVisible(freePackagesBoxSelector);
            WaitForElementToExist(packageSelector);

            Wait(TimeSpan.FromSeconds(3));
            
            string pageSource = GetPageSource();
            IEnumerable<string> subids = GetSubidsFromSource(pageSource);
            
            return subids;
        }

        IEnumerable<string> GetSubidsFromSource(string pageSource)
        {
            List<string> lines = pageSource.Split('\n').ToList();
            List<string> subids = new List<string>();

            foreach (string line in lines)
            {
                if (!line.Contains("data-appid"))
                {
                    continue;
                }

                string[] lineSplit = line.Split(' ');
                string subidField = lineSplit[2];

                string[] subidFieldSplit = subidField.Split('=');
                string subid = subidFieldSplit[1].Replace("\"", "");

                subids.Add(subid);
            }

            return subids;
        }
    }
}
