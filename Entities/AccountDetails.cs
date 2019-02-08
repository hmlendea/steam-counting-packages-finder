using System;

namespace SteamAccountCreator.Entities
{
    public sealed class AccountDetails
    {
        string username;
        string displayName;

        public string Username
        {
            get { return username.ToLower(); }
            set { username = value; }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    return username;
                }

                return displayName;
            }
            set { displayName = value; }
        }

        public string Password { get; set; }

        public string SteamProfileUrl => $"https://steamcommunity.com/id/{Username}";

        public AccountDetails()
        {
            
        }

        public AccountDetails(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
