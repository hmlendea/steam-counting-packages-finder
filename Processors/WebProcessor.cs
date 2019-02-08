using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SteamAccountCreator.Processors
{
    public class WebProcessor : IProcessor
    {
        public string Name => GetType().Name.Replace("Processor", string.Empty);

        public List<string> Tabs { get; private set; }

        public List<string> DriverWindowTabs => driver.WindowHandles.ToList();

        public string CurrentTab { get; private set; }

        protected Random Random { get; set; }

        static readonly TimeSpan DefaultWaitDuration = TimeSpan.FromMilliseconds(333);

        static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(20);

        const int DefaultHttpAttemptsAmount = 3;

        readonly IWebDriver driver;

        public WebProcessor(IWebDriver driver)
        {
            this.driver = driver;

            Random = new Random();

            Tabs = new List<string>();
        }

        public void Close()
        {
            foreach(string tab in Tabs.ToList())
            {
                CloseTab(tab);
            }

            driver.SwitchTo().Window(driver.WindowHandles[0]);
        }

        protected void SwitchToTab(int index) => SwitchToTab(Tabs[index]);
        protected void SwitchToTab(string tab)
        {
            if (tab == driver.CurrentWindowHandle)
            {
                return;
            }

            if (!Tabs.Contains(tab))
            {
                throw new ArgumentOutOfRangeException("The specified tab does not belong to this processor");
            }
            
            CurrentTab = tab;
            driver.SwitchTo().Window(tab);
        }

        protected string NewTab() => NewTab("about:blank");
        protected string NewTab(string url)
        {
            driver.SwitchTo().Window(driver.WindowHandles[0]);

            // TODO: This is not covered by the retry mechanism
            string newTabScript =
                "var d=document,a=d.createElement('a');" +
                "a.target='_blank';a.href='" + url + "';" +
                "a.innerHTML='new tab';" +
                "d.body.appendChild(a);" +
                "a.click();" +
                "a.parentNode.removeChild(a);";

            List<string> oldWindowTabs = driver.WindowHandles.ToList();

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(newTabScript);

            List<string> newWindowTabs = driver.WindowHandles.ToList();
            string openedWindowTabs = newWindowTabs.Except(oldWindowTabs).Single();

            Tabs.Add(openedWindowTabs);

            SwitchToTab(openedWindowTabs);

            return openedWindowTabs;
        }

        protected void CloseTab(string tab)
        {
            if (!Tabs.Contains(tab))
            {
                throw new ArgumentOutOfRangeException("The specified tab does not belong to this processor");
            }
            
            driver.SwitchTo().Window(tab).Close();
            Tabs.Remove(tab);
        }

        protected void GoToUrl(string url) => GoToUrl(url, DefaultHttpAttemptsAmount);
        protected void GoToUrl(string url, int httpRetries) => GoToUrl(url, httpRetries, DefaultWaitDuration);
        protected void GoToUrl(string url, TimeSpan retryDelay) => GoToUrl(url, DefaultHttpAttemptsAmount, retryDelay);
        protected void GoToUrl(string url, int httpRetries, TimeSpan retryDelay)
        {
            if (string.IsNullOrWhiteSpace(CurrentTab))
            {
                NewTab();
            }
            else
            {
                SwitchToTab(CurrentTab);
            }

            if (driver.Url == url)
            {
                return;
            }

            By errorSelectorChrome = By.ClassName("error-code");
            By anythingSelector = By.XPath(@"/html/body/*");
            
            for (int attempt = 0; attempt < httpRetries; attempt++)
            {
                driver.Navigate().GoToUrl(url);

                for (int i = 0; i < 3; i++)
                {
                    if (IsElementVisible(anythingSelector))
                    {
                        break;
                    }

                    driver.Navigate().GoToUrl(url);
                }

                if (!IsAnyElementVisible(errorSelectorChrome))
                {
                    return;
                }

                GoToUrl("about:blank");
                Wait(retryDelay);
            }

            throw new Exception($"Failed to load the requested URL after {httpRetries} attempts");
        }

        protected void GoToIframe(By selector)
        {
            string iframeSource = GetSource(selector);
            GoToUrl(iframeSource);
        }

        protected void SwitchToIframe(int index)
        {
            SwitchToTab(CurrentTab);
            driver.SwitchTo().Frame(index);
        }
        protected void SwitchToIframe(By selector) => SwitchToIframe(selector, DefaultTimeout);
        protected void SwitchToIframe(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IWebElement iframe = GetElement(selector, timeout);
                    driver.SwitchTo().Frame(iframe);
                }
                finally
                {
                    Wait();
                }
            }
        }

        protected void ExecuteScript(string script)
        {
            SwitchToTab(CurrentTab);

            IJavaScriptExecutor scriptExecutor = (IJavaScriptExecutor)driver;
            scriptExecutor.ExecuteScript(script);
        }

        protected void AcceptAlert() => AcceptAlert(DefaultTimeout);
        protected void AcceptAlert(TimeSpan timeout)
        {
            IAlert alert = GetAlert(timeout);
            alert.Accept();
        }

        protected void DismissAlert() => DismissAlert(DefaultTimeout);
        protected void DismissAlert(TimeSpan timeout)
        {
            IAlert alert = GetAlert(timeout);
            alert.Dismiss();
        }

        protected List<IWebElement> GetElements(By selector) => GetElements(selector, DefaultTimeout);

        protected string GetAttribute(By selector, string attribute) => GetAttribute(selector, attribute, DefaultTimeout);
        protected string GetAttribute(By selector, string attribute, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.GetAttribute(attribute);
        }

        protected string GetClass(By selector) => GetClass(selector, DefaultTimeout);
        protected string GetClass(By selector, TimeSpan timeout) => GetAttribute(selector, "class", timeout);

        protected IEnumerable<string> GetClasses(By selector) => GetClasses(selector, DefaultTimeout);
        protected IEnumerable<string> GetClasses(By selector, TimeSpan timeout) => GetAttribute(selector, "class", timeout).Split(' ');

        protected string GetHyperlink(By selector) => GetHyperlink(selector, DefaultTimeout);
        protected string GetHyperlink(By selector, TimeSpan timeout) => GetAttribute(selector, "href", timeout);

        protected string GetSource(By selector) => GetSource(selector, DefaultTimeout);
        protected string GetSource(By selector, TimeSpan timeout) => GetAttribute(selector, "src", timeout);

        protected string GetValue(By selector) => GetValue(selector, DefaultTimeout);
        protected string GetValue(By selector, TimeSpan timeout) => GetAttribute(selector, "value", timeout);

        protected string GetText(By selector) => GetText(selector, DefaultTimeout);
        protected string GetText(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.Text;
        }

        protected string GetSelectedText(By selector) => GetSelectedText(selector, DefaultTimeout);
        protected string GetSelectedText(By selector, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);
            return element.SelectedOption.Text;
        }

        protected void SetText(By selector, string text) => SetText(selector, text, DefaultTimeout);
        protected void SetText(By selector, string text, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            element.Clear();
            element.SendKeys(text);
        }

        protected void AppendText(By selector, string text) => AppendText(selector, text, DefaultTimeout);
        protected void AppendText(By selector, string text, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.SendKeys(text);
        }

        protected void ClearText(By selector) => ClearText(selector, DefaultTimeout);
        protected void ClearText(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.Clear();
        }

        protected bool HasClass(By selector, string className) => HasClass(selector, className, DefaultTimeout);
        protected bool HasClass(By selector, string className, TimeSpan timeout)
        {
            IEnumerable<string> classes = GetClasses(selector, timeout);
            return classes.Contains(className);
        }

        protected bool IsSelected(By selector) => IsSelected(selector, DefaultTimeout);
        protected bool IsSelected(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            return element.Selected;
        }

        protected void Wait() => Wait(DefaultWaitDuration);
        protected void Wait(int milliseconds) => Wait(TimeSpan.FromMilliseconds(milliseconds));
        protected void Wait(DateTime targetTime) => Wait(targetTime - DateTime.Now);
        protected void Wait(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMilliseconds <= 0)
            {
                return;
            }

            DateTime now = DateTime.Now;
            WebDriverWait wait = new WebDriverWait(driver, timeSpan);
            wait.PollingInterval = TimeSpan.FromMilliseconds(10);
            wait.Until(wd=> (DateTime.Now - now) - timeSpan > TimeSpan.Zero);
        }

        protected void WaitForTextLength(By selector, int length) => WaitForTextLength(selector, length, DefaultTimeout);
        protected void WaitForTextLength(By selector, int length, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForTextLength(selector, length, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForTextLength(selector, length, DefaultTimeout);
            }
        }
        protected void WaitForTextLength(By selector, int length, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                bool conditionMet =
                    GetValue(selector, timeout).Length == length ||
                    GetText(selector, timeout).Length == length;

                if (conditionMet)
                {
                    break;
                }

                Wait();
            }
        }

        protected void WaitForAnyElementToExist(params By[] selectors) => WaitForAnyElementToExist(DefaultTimeout, selectors);
        protected void WaitForAnyElementToExist(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToExist(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAnyElementToExist(DefaultTimeout, selectors);
            }
        }
        protected void WaitForAnyElementToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesAnyElementExist(selectors))
            {
                Wait();
            }
        }

        protected void WaitForAllElementsToExist(params By[] selectors) => WaitForAllElementsToExist(DefaultTimeout, selectors);
        protected void WaitForAllElementsToExist(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToExist(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAllElementsToExist(DefaultTimeout, selectors);
            }
        }
        protected void WaitForAllElementsToExist(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoAllElementsExist(selectors))
            {
                Wait();
            }
        }

        protected void WaitForAnyElementToBeVisible(params By[] selectors) => WaitForAnyElementToBeVisible(DefaultTimeout, selectors);
        protected void WaitForAnyElementToBeVisible(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAnyElementToBeVisible(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAnyElementToBeVisible(DefaultTimeout, selectors);
            }
        }
        protected void WaitForAnyElementToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsAnyElementVisible(selectors))
            {
                Wait();
            }
        }

        protected void WaitForAllElementsToBeVisible(params By[] selectors) => WaitForAllElementsToBeVisible(DefaultTimeout, selectors);
        protected void WaitForAllElementsToBeVisible(bool waitIndefinetely, params By[] selectors)
        {
            if (waitIndefinetely)
            {
                WaitForAllElementsToBeVisible(TimeSpan.FromDays(873), selectors);
            }
            else
            {
                WaitForAllElementsToBeVisible(DefaultTimeout, selectors);
            }
        }
        protected void WaitForAllElementsToBeVisible(TimeSpan timeout, params By[] selectors)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !AreAllElementsVisible(selectors))
            {
                Wait();
            }
        }

        protected void WaitForElementToExist(By selector) => WaitForElementToExist(selector, DefaultTimeout);
        protected void WaitForElementToExist(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToExist(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToExist(selector, DefaultTimeout);
            }
        }
        protected void WaitForElementToExist(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !DoesElementExist(selector))
            {
                Wait();
            }
        }

        protected void WaitForElementToBeVisible(By selector) => WaitForElementToBeVisible(selector, DefaultTimeout);
        protected void WaitForElementToBeVisible(By selector, bool waitIndefinetely)
        {
            if (waitIndefinetely)
            {
                WaitForElementToBeVisible(selector, TimeSpan.FromDays(873));
            }
            else
            {
                WaitForElementToBeVisible(selector, DefaultTimeout);
            }
        }
        protected void WaitForElementToBeVisible(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout && !IsElementVisible(selector))
            {
                Wait();
            }
        }

        protected bool DoAllElementsExist(params By[] selectors) => selectors.All(DoesElementExist);
        protected bool DoesAnyElementExist(params By[] selectors) => selectors.Any(DoesElementExist);
        protected bool DoesElementExist(By selector)
        {
            SwitchToTab(CurrentTab);

            try
            {
                IWebElement element = driver.FindElement(selector);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected bool AreAllElementsVisible(params By[] selectors) => selectors.All(IsElementVisible);
        protected bool IsAnyElementVisible(params By[] selectors) => selectors.Any(IsElementVisible);
        protected bool IsElementVisible(By selector)
        {
            SwitchToTab(CurrentTab);

            try
            {
                IWebElement element = driver.FindElement(selector);
                return element.Displayed;
            }
            catch
            {
                return false;
            }
        }

        protected void ClickAny(params By[] selectors)
        {
            bool clicked = false;

            foreach (By selector in selectors)
            {
                if (IsElementVisible(selector))
                {
                    Click(selector);

                    clicked = true;
                    break;
                }
            }

            if (!clicked)
            {
                // TODO: Use a proper message
                throw new ElementNotVisibleException("No element to click");
            }
        }

        protected void Click(By selector) => Click(selector, DefaultTimeout);
        protected void Click(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            element.Click();
        }

        protected void UpdateCheckbox(By selector, bool status) => UpdateCheckbox(selector, status, DefaultTimeout);
        protected void UpdateCheckbox(By selector, bool status, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);

            if (element.Selected != status)
            {
                Click(selector, timeout);
            }
        }

        protected void SelectByIndex(By selector, int index) => SelectByIndex(selector, index, DefaultTimeout);
        protected void SelectByIndex(By selector, int index, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);
            element.SelectByIndex(index);
        }

        protected void SelectByValue(By selector, object value) => SelectByValue(selector, value, DefaultTimeout);
        protected void SelectByValue(By selector, object value, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            string stringValue;

            if (value is string)
            {
                stringValue = (string)value;
            }
            else
            {
                stringValue = value.ToString();
            }

            element.SelectByValue(stringValue);
        }

        protected void SelectRandom(By selector) => SelectRandom(selector, DefaultTimeout);
        protected void SelectRandom(By selector, TimeSpan timeout)
        {
            SelectElement element = GetSelectElement(selector, timeout);

            int option = Random.Next(0, element.Options.Count);
            element.SelectByIndex(option);
        }
        
        private IWebElement GetElement(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IWebElement element = driver.FindElement(selector);
                    
                    if (element != null && element.Displayed)
                    {
                        return element;
                    }
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
        
        private SelectElement GetSelectElement(By selector, TimeSpan timeout)
        {
            IWebElement element = GetElement(selector, timeout);
            SelectElement selectElement = new SelectElement(element);

            return selectElement;
        }
        
        private List<IWebElement> GetElements(By selector, TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    List<IWebElement> elements = driver.FindElements(selector).ToList();

                    if (elements != null && elements.Count > 0)
                    {
                        return elements;
                    }
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
 
        private IAlert GetAlert(TimeSpan timeout)
        {
            SwitchToTab(CurrentTab);

            DateTime beginTime = DateTime.Now;

            while (DateTime.Now - beginTime < timeout)
            {
                try
                {
                    IAlert alert = driver.SwitchTo().Alert();
                    return alert;
                }
                catch {  }
                finally
                {
                    Wait();
                }
            }

            return null;
        }
    }
}
