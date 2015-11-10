using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;

namespace GitUI.CommandsDialogs
{
    public class BuildReportTabPageExtension
    {
        private readonly TabControl tabControl;

        private TabPage buildReportTabPage;
        private WebBrowser buildReportWebBrowser;
        private GitRevision selectedGitRevision;

        public BuildReportTabPageExtension(TabControl tabControl)
        {
            this.tabControl = tabControl;
        }

        public void FillBuildReport(GitRevision revision)
        {
            if (EnvUtils.IsMonoRuntime())
                return;

            if (selectedGitRevision != null) selectedGitRevision.PropertyChanged -= RevisionPropertyChanged;
            selectedGitRevision = revision;
            if (selectedGitRevision != null) selectedGitRevision.PropertyChanged += RevisionPropertyChanged;

            var buildInfoIsAvailable =
                !(revision == null || revision.BuildStatus == null || string.IsNullOrEmpty(revision.BuildStatus.Url));

            tabControl.SuspendLayout();

            try
            {
                if (buildInfoIsAvailable)
                {
                    if (buildReportTabPage == null)
                    {
                        CreateBuildReportTabPage(tabControl);
                    }

                    var isFavIconMissing = buildReportTabPage.ImageIndex < 0;

                    if (isFavIconMissing || tabControl.SelectedTab == buildReportTabPage)
                    {
                        buildReportWebBrowser.Navigate(revision.BuildStatus.Url);

                        if (isFavIconMissing)
                        {
                            buildReportWebBrowser.Navigated += BuildReportWebBrowserOnNavigated;
                        }
                    }

                    if (!tabControl.Controls.Contains(buildReportTabPage))
                    {
                        tabControl.Controls.Add(buildReportTabPage);
                    }
                }
                else
                {
                    if (buildReportTabPage != null && tabControl.Controls.Contains(buildReportTabPage))
                    {
                        buildReportWebBrowser.Stop();
                        buildReportWebBrowser.Document.Write(string.Empty);
                        tabControl.Controls.Remove(buildReportTabPage);
                    }
                }
            }
            finally
            {
                tabControl.ResumeLayout();
            }
        }

        private void RevisionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BuildStatus")
            {
                // Refresh the selected Git revision
                this.FillBuildReport(this.selectedGitRevision);    
            }
        }

        private void CreateBuildReportTabPage(TabControl tabControl)
        {
            this.buildReportTabPage = new TabPage
                {
                    Padding = new Padding(3),
                    TabIndex = tabControl.Controls.Count,
                    Text = "Build Report",
                    UseVisualStyleBackColor = true
                };
            this.buildReportWebBrowser = new WebBrowser
                {
                    Dock = DockStyle.Fill
                };
            this.buildReportTabPage.Controls.Add(this.buildReportWebBrowser);
        }

        private void BuildReportWebBrowserOnNavigated(object sender,
                                                      WebBrowserNavigatedEventArgs webBrowserNavigatedEventArgs)
        {
            buildReportWebBrowser.Navigated -= BuildReportWebBrowserOnNavigated;

            var favIconUrl = DetermineFavIconUrl(buildReportWebBrowser.Document);

            if (favIconUrl != null)
            {
                DownloadRemoteImageFileAsync(favIconUrl).ContinueWith(
                    task =>
                        {
                            using (var imageStream = task.Result)
                            {
                                if (imageStream != null)
                                {
                                    var favIconImage = Image.FromStream(imageStream)
                                                            .GetThumbnailImage(16, 16, null, IntPtr.Zero);
                                    var imageCollection = tabControl.ImageList.Images;
                                    var imageIndex = buildReportTabPage.ImageIndex;

                                    if (imageIndex < 0)
                                    {
                                        buildReportTabPage.ImageIndex = imageCollection.Count;
                                        imageCollection.Add(favIconImage);
                                    }
                                    else
                                    {
                                        imageCollection[imageIndex] = favIconImage;
                                    }

                                    tabControl.Invalidate(false);
                                }
                            }
                        },
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private string DetermineFavIconUrl(HtmlDocument htmlDocument)
        {
            var links = htmlDocument.GetElementsByTagName("link");
            var favIconLink =
                links.Cast<HtmlElement>()
                     .SingleOrDefault(x => x.GetAttribute("rel").ToLowerInvariant() == "shortcut icon");

            if (favIconLink != null)
            {
                var href = favIconLink.GetAttribute("href");
                var favIconUrl = htmlDocument.Url.AbsoluteUri.Replace(htmlDocument.Url.PathAndQuery, href);

                return favIconUrl;
            }

            return null;
        }

        private static Task<Stream> DownloadRemoteImageFileAsync(string uri)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);

            return GetWebResponseAsync(request).ContinueWith(
                task =>
                    {
                        var response = task.Result;

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
                    },
                TaskContinuationOptions.ExecuteSynchronously);
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