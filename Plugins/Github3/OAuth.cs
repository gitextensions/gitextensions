using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Git.hub;

namespace GitHub3
{
    public partial class OAuth : Form
    {
        public OAuth()
        {
            InitializeComponent();
            webBrowser1.ScriptErrorsSuppressed = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                webBrowser1.ScriptErrorsSuppressed = true;
                webBrowser1.CausesValidation = false;
                string url = "https://github.com/login/oauth/authorize?client_id=" + GitHubApiInfo.client_id + "&scope=repo,public_repo";
                webBrowser1.Navigate(url);
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(this, "Failure starting WebBrowser.");
            }
        }

        private bool _gotToken;

        public void web_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            CheckAuth(e.Url.ToString());
        }

        public void web_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            CheckAuth(e.Url.ToString());
        }

        private static Dictionary<string, string> GetParams(string uri)
        {
            var matches = Regex.Matches(uri, @"[\?&](([^&=]+)=([^&=#]*))", RegexOptions.Compiled);
            return matches.Cast<Match>().ToDictionary(
                m => Uri.UnescapeDataString(m.Groups[2].Value),
                m => Uri.UnescapeDataString(m.Groups[3].Value));
        }

        public void CheckAuth(string url)
        {
            if (_gotToken)
            {
                return;
            }

            if (url.Contains("?code="))
            {
                var uri = new Uri(url);
                var queryParams = GetParams(uri.Query);
                if (queryParams.TryGetValue("code", out var code))
                {
                    Hide();
                    Close();
                    string token = OAuth2Helper.requestToken(GitHubApiInfo.client_id, GitHubApiInfo.client_secret, code);
                    if (token == null)
                    {
                        return;
                    }

                    _gotToken = true;

                    GitHubLoginInfo.OAuthToken = token;

                    MessageBox.Show(Owner, "Successfully retrieved OAuth token.", "GitHub Authorization");
                }
            }
        }
    }
}
