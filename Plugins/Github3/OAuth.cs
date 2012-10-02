using System;
using System.Web;
using System.Windows.Forms;
using Git.hub;

namespace Github3
{
    public partial class OAuth : Form
    {
        public OAuth()
        {
            InitializeComponent();
        }

        protected override void OnLoad(System.EventArgs e)
        {
            webBrowser1.ScriptErrorsSuppressed = true;
            webBrowser1.CausesValidation = false;
            string url = "https://github.com/login/oauth/authorize?client_id=" + GithubAPIInfo.client_id + "&scope=repo,public_repo";
            this.webBrowser1.Navigate(url);
        }

        private bool gotToken = false;

        public void web_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            checkAuth(e.Url.ToString());
        }
        public void web_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            checkAuth(e.Url.ToString());
        }

        public void checkAuth(string url)
        {
            if (gotToken)
                return;

            if (url.Contains("?code="))
            {
                Uri uri = new Uri(url);
                string code = HttpUtility.ParseQueryString(uri.Query).Get("code");
                if (!code.IsNullOrEmpty())
                {
                    this.Hide();
                    this.Close();
                    string token = OAuth2Helper.requestToken(GithubAPIInfo.client_id, GithubAPIInfo.client_secret, code);
                    if (token == null)
                        return;
                    gotToken = true;

                    GithubLoginInfo.OAuthToken = token;

                    MessageBox.Show(this.Owner as IWin32Window, "Successfully retrieved OAuth token.", "Github Authorization");
                }
            }
        }
    }
}
