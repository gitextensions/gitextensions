using System.Windows.Forms;
using System;
using Git.hub;
namespace Github3
{
    partial class OAuth
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.AllowWebBrowserDrop = false;
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(980, 600);
            this.webBrowser1.TabIndex = 0;
            this.webBrowser1.Navigated += web_Navigated;
            this.webBrowser1.Navigating += web_Navigating;
            // 
            // OAuth
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 600);
            this.Controls.Add(this.webBrowser1);
            this.Name = "OAuth";
            this.ShowIcon = false;
            this.Text = "Github Authorization";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;

        protected override void OnLoad(System.EventArgs e)
        {
            this.webBrowser1.Navigate("https://github.com/login/oauth/authorize?client_id=" + GithubAPIInfo.client_id + "&scope=repo,public_repo");
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

            if(url.Contains("?code="))
            {
                string[] splits = url.Split(new string[]{"?code="}, StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length == 2)
                {
                    this.Hide();
                    this.Close();
                    string code = splits[1];
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