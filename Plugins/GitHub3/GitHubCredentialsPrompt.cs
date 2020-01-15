using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands.Utils;
using Microsoft.VisualStudio.Threading;
using ResourceManager;

namespace GitHub3
{
    public partial class GitHubCredentialsPrompt : Form
    {
        private readonly string _authorizationApiUrl;
        private bool _require2ndFactorCode = false;
        private readonly JoinableTaskFactory _joinableTaskFactory;
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly TranslationString _generationFailed = new TranslationString("Fail to generate token due to error:");
        private readonly TranslationString _ask2ndFactorCode = new TranslationString("Please enter your GitHub validation code (from {0})");
        private readonly TranslationString _generationSucceed = new TranslationString("Successfully retrieved OAuth token.");

        public GitHubCredentialsPrompt(string authorizationApiUrl)
        {
            InitializeComponent();

            _authorizationApiUrl = authorizationApiUrl;
            lblError.Text = string.Empty;
            _joinableTaskFactory = new JoinableTaskContext().Factory;
        }

        private void btn_OK_Click(object sender, EventArgs e)
        {
            var token = _joinableTaskFactory.Run(async () =>
                await GenerateOAuthTokenAsync(txtGitHubLogin.Text, txtGitHubPassword.Text, txtSecondFactor.Text));
            if (!string.IsNullOrWhiteSpace(token))
            {
                GitHubLoginInfo.OAuthToken = token;

                MessageBox.Show(this, _generationSucceed.Text, Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void GitHubCredentialsPrompt_Shown(object sender, EventArgs e)
        {
            txtGitHubLogin.Focus();
        }

        private void TxtGitHubLogin_TextChanged(object sender, EventArgs e)
        {
            UpdateOkButtonState();
        }

        private void UpdateOkButtonState()
        {
            btn_OK.Enabled = !string.IsNullOrWhiteSpace(txtGitHubLogin.Text) &&
                             !string.IsNullOrWhiteSpace(txtGitHubPassword.Text) &&
                             (!_require2ndFactorCode || !string.IsNullOrWhiteSpace(txtSecondFactor.Text));
        }

        public class GitHubToken
        {
            public string token { get; set; }
        }

        private async Task<string> GenerateOAuthTokenAsync(string login, string password, string secondFactorOtp)
        {
            // https://developer.github.com/v3/auth/#using-the-oauth-authorizations-api-with-two-factor-authentication
            const string otpHeaderKey = "X-GitHub-OTP";
            string note = $"Token for Git Extensions on {Environment.MachineName} at {DateTime.Now:g}";
            string githubScopesJson = "{\"scopes\":[\"repo\", \"public_repo\"],\"note\":\"" + note + "\"}";

            using (var request = new HttpRequestMessage(new HttpMethod("POST"), _authorizationApiUrl))
            {
                var base64Authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{login}:{password}"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Authorization);
                request.Headers.Add("User-Agent", "GitExtensions");

                if (!string.IsNullOrWhiteSpace(secondFactorOtp))
                {
                    request.Headers.TryAddWithoutValidation(otpHeaderKey, secondFactorOtp);
                }

                request.Content = new StringContent(githubScopesJson, Encoding.UTF8, "application/json");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var gitHubToken = JsonSerializer.Deserialize<GitHubToken>(await response.Content.ReadAsStringAsync());
                    return gitHubToken?.token;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized && response.Headers.Contains(otpHeaderKey))
                {
                    txtGitHubLogin.Enabled = false;
                    txtGitHubPassword.Enabled = false;
                    _require2ndFactorCode = true;
                    Show2ndFactorControls();
                    UpdateOkButtonState();

                    var otpTypeHeader = response.Headers.GetValues(otpHeaderKey).First();
                    var otpType = otpTypeHeader.Substring(otpTypeHeader.Length - 3);

                    lblError.Text = string.Format(_ask2ndFactorCode.Text, otpType);
                }
                else
                {
                    txtGitHubLogin.Enabled = true;
                    txtGitHubPassword.Enabled = true;
                    string message;
                    if ((int)response.StatusCode == 422)
                    {
                        message = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        message = response.ReasonPhrase;
                    }

                    lblError.Text = _generationFailed.Text + Environment.NewLine + message;
                }

                return null;
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Show2ndFactorControls();
        }

        private void Show2ndFactorControls()
        {
            linkLabel1.Visible = false;
            lbl2ndFactor.Visible = true;
            txtSecondFactor.Visible = true;
        }

        private void GitHubCredentialsPrompt_FormClosing(object sender, FormClosingEventArgs e)
        {
            _httpClient.Dispose();
        }
    }
}
