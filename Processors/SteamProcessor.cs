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
using SteamAccountCreator.Utils;

namespace SteamAccountCreator.Processors
{
    public sealed class SteamProcessor : WebProcessor
    {
        public string HomePageUrl => "https://store.steampowered.com";
        public string LoginUrl => $"{HomePageUrl}/login/?redir=&redir_ssl=1";
        public string LicensesUrl => $"{HomePageUrl}/account/licenses/";
        
        AccountDetails account;

        string EditProfileUrl => $"{account.SteamProfileUrl}/edit";
        string PrivacySettingsUrl => $"{account.SteamProfileUrl}/edit/settings";
        string BadgesUrl => $"{account.SteamProfileUrl}/badges";

        public SteamProcessor(IWebDriver driver, AccountDetails account)
            : base(driver)
        {
            this.account = account;
        }

        public void LogIn()
        {
            GoToUrl(LoginUrl);

            By usernameSelector = By.Id("input_username");
            By passwordSelector = By.Id("input_password");
            By avatarSelector = By.XPath(@"//a[contains(@class,'playerAvatar')]");
            By loginButtonSelector = By.XPath(@"//*[@id='login_btn_signin']/button");

            SetText(usernameSelector, account.Username);
            SetText(passwordSelector, account.Password);
            
            Click(loginButtonSelector);
            
            WaitForElementToExist(avatarSelector);
        }

        public void LogOut()
        {
            By accountPulldownSelector = By.Id("account_pulldown");
            By logoutLinkSelector = By.XPath(@"//*[@id='account_dropdown']/div/a[1]");
            By globalMenuSelector = By.Id("global_action_menu");

            WaitForElementToExist(accountPulldownSelector);

            Click(accountPulldownSelector);
            Click(logoutLinkSelector);

            WaitForElementToExist(globalMenuSelector);
        }

        public int GetGamesCount()
        {
            GoToUrl(account.SteamProfileUrl);

            By profileHeaderSelector = By.ClassName("profile_header");
            By gamesCountSelector = By.XPath(@"//a[contains(@href,'/games')]/span[@class='profile_count_link_total']");
            
            WaitForElementToBeVisible(profileHeaderSelector);

            if (!IsElementVisible(gamesCountSelector))
            {
                return 0;
            }
            
            string gamesCountText = GetText(gamesCountSelector).Trim();

            int gamesCount = int.Parse(gamesCountText);

            return gamesCount;
        }

        public void ActivatePackage(string package)
        {
            GoToUrl(LicensesUrl);

            By pageContentSelector = By.ClassName("page_content");
            WaitForElementToExist(pageContentSelector);

            string activationScript = $"jQuery.post('//store.steampowered.com/checkout/addfreelicense',{{action:'add_to_cart',sessionid:g_sessionID,subid:{package}}})";
            ExecuteScript(activationScript);
        }
    }
}