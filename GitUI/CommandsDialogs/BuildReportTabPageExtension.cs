using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public class BuildReportTabPageExtension
    {
        private readonly TabControl _tabControl;
        private readonly string _caption;
        private readonly Func<IGitModule> _getModule;

        private TabPage _buildReportTabPage;
        private WebBrowserControl _buildReportWebBrowser;
        private GitRevision _selectedGitRevision;
        private string _url;

        public Control Control { get => _buildReportWebBrowser; } // for focusing

        public BuildReportTabPageExtension(Func<IGitModule> getModule, TabControl tabControl, string caption)
        {
            _getModule = getModule;
            _tabControl = tabControl;
            _caption = caption;
        }

        public void FillBuildReport([CanBeNull] GitRevision revision)
        {
            if (_selectedGitRevision != null)
            {
                _selectedGitRevision.PropertyChanged -= RevisionPropertyChanged;
            }

            _selectedGitRevision = revision;

            if (_selectedGitRevision != null)
            {
                _selectedGitRevision.PropertyChanged += RevisionPropertyChanged;
            }

            _tabControl.SuspendLayout();

            try
            {
                var buildResultPageEnabled = IsBuildResultPageEnabled();
                var buildInfoIsAvailable = !string.IsNullOrEmpty(revision?.BuildStatus?.Url);

                if (buildResultPageEnabled && buildInfoIsAvailable)
                {
                    if (_buildReportTabPage == null)
                    {
                        CreateBuildReportTabPage(_tabControl);
                    }

                    var isFavIconMissing = _buildReportTabPage.ImageIndex < 0;

                    if (isFavIconMissing || _tabControl.SelectedTab == _buildReportTabPage)
                    {
                        try
                        {
                            if (revision.BuildStatus.ShowInBuildReportTab)
                            {
                                _url = null;
                                _buildReportWebBrowser.Navigate(revision.BuildStatus.Url);
                            }
                            else
                            {
                                _url = revision.BuildStatus.Url;
                                _buildReportWebBrowser.Navigate("about:blank");
                            }

                            if (isFavIconMissing)
                            {
                                _buildReportWebBrowser.Navigated += BuildReportWebBrowserOnNavigated;
                            }
                        }
                        catch
                        {
                            // No propagation to the user if the report fails
                        }
                    }

                    if (!_tabControl.Controls.Contains(_buildReportTabPage))
                    {
                        _tabControl.Controls.Add(_buildReportTabPage);
                    }
                }
                else
                {
                    if (_buildReportTabPage != null && _tabControl.Controls.Contains(_buildReportTabPage))
                    {
                        _buildReportWebBrowser.Stop();
                        _buildReportWebBrowser.Document.Write(string.Empty);
                        _tabControl.Controls.Remove(_buildReportTabPage);
                    }
                }
            }
            finally
            {
                _tabControl.ResumeLayout();
            }
        }

        private void RevisionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GitRevision.BuildStatus))
            {
                // Refresh the selected Git revision
                FillBuildReport(_selectedGitRevision);
            }
        }

        private void CreateBuildReportTabPage(TabControl tabControl)
        {
            _buildReportTabPage = new TabPage
            {
                Padding = new Padding(3),
                TabIndex = tabControl.Controls.Count,
                Text = _caption,
                UseVisualStyleBackColor = true
            };
            _buildReportWebBrowser = new WebBrowserControl
            {
                Dock = DockStyle.Fill
            };
            _buildReportTabPage.Controls.Add(_buildReportWebBrowser);
        }

        private void BuildReportWebBrowserOnNavigated(object sender,
                                                      WebBrowserNavigatedEventArgs webBrowserNavigatedEventArgs)
        {
            _buildReportWebBrowser.Navigated -= BuildReportWebBrowserOnNavigated;

            var favIconUrl = DetermineFavIconUrl(_buildReportWebBrowser.Document);

            if (favIconUrl != null)
            {
                ThreadHelper.JoinableTaskFactory.RunAsync(
                    async () =>
                    {
                        using (var imageStream = await DownloadRemoteImageFileAsync(favIconUrl))
                        {
                            if (imageStream != null)
                            {
                                await _tabControl.SwitchToMainThreadAsync();

                                var favIconImage = Image.FromStream(imageStream)
                                                        .GetThumbnailImage(16, 16, null, IntPtr.Zero);
                                var imageCollection = _tabControl.ImageList.Images;
                                var imageIndex = _buildReportTabPage.ImageIndex;

                                if (imageIndex < 0)
                                {
                                    _buildReportTabPage.ImageIndex = imageCollection.Count;
                                    imageCollection.Add(favIconImage);
                                }
                                else
                                {
                                    imageCollection[imageIndex] = favIconImage;
                                }

                                _tabControl.Invalidate(false);
                            }
                        }
                    });
            }

            if (_url != null)
            {
                _buildReportWebBrowser.Document.Write("<HTML><a href=\"" + _url + "\" target=\"_blank\">Open report</a></HTML>");
            }
        }

        private bool IsBuildResultPageEnabled()
        {
            var settings = GetModule().GetEffectiveSettings() as RepoDistSettings;
            return settings?.BuildServer.ShowBuildResultPage.ValueOrDefault ?? false;
        }

        private IGitModule GetModule()
        {
            var module = _getModule();

            if (module == null)
            {
                throw new ArgumentException($"Require a valid instance of {nameof(IGitModule)}");
            }

            return module;
        }

        [CanBeNull]
        private static string DetermineFavIconUrl(HtmlDocument htmlDocument)
        {
            var links = htmlDocument.GetElementsByTagName("link");
            var favIconLink =
                links.Cast<HtmlElement>()
                     .SingleOrDefault(x => x.GetAttribute("rel").ToLowerInvariant() == "shortcut icon");

            if (favIconLink == null || htmlDocument.Url == null)
            {
                return null;
            }

            var href = favIconLink.GetAttribute("href");

            if (htmlDocument.Url.PathAndQuery == "/")
            {
                // Szenario: http://test.test/teamcity/....
                return htmlDocument.Url.AbsoluteUri.Replace(htmlDocument.Url.PathAndQuery, href);
            }
            else
            {
                // Szenario: http://teamcity.domain.test/
                return new Uri(new Uri(htmlDocument.Url.AbsoluteUri), href).ToString();
            }
        }

        [ItemCanBeNull]
        private static async Task<Stream> DownloadRemoteImageFileAsync(string uri)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            var response = await GetWebResponseAsync(request).ConfigureAwait(false);

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.Moved ||
                    response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                // if the remote file was found, download it
                return response.GetResponseStream();
            }

            return null;
        }

        private static Task<HttpWebResponse> GetWebResponseAsync(HttpWebRequest webRequest)
        {
            return Task<HttpWebResponse>.Factory.FromAsync(
                webRequest.BeginGetResponse,
                ar => (HttpWebResponse)webRequest.EndGetResponse(ar),
                null);
        }
    }
}