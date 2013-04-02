using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public class BuildReportTabPageExtension
    {
        private readonly TabControl tabControl;

        private TabPage BuildReportTabPage;
        private WebBrowser BuildReportWebBrowser;

        public BuildReportTabPageExtension(TabControl tabControl)
        {
            this.tabControl = tabControl;
        }

        public void FillBuildReport(GitRevision revision)
        {
            if (Settings.IsMonoRuntime())
                return;

            var buildInfoIsAvailable =
                !(revision == null || revision.BuildStatus == null || string.IsNullOrEmpty(revision.BuildStatus.Url));

            tabControl.SuspendLayout();

            try
            {
                if (buildInfoIsAvailable)
                {
                    if (this.BuildReportTabPage == null)
                    {
                        CreateBuildReportTabPage(tabControl);
                    }

                    var isFavIconMissing = BuildReportTabPage.ImageIndex < 0;

                    if (isFavIconMissing || tabControl.SelectedTab == BuildReportTabPage)
                    {
                        BuildReportWebBrowser.Navigate(revision.BuildStatus.Url);

                        if (isFavIconMissing)
                        {
                            BuildReportWebBrowser.Navigated += BuildReportWebBrowserOnNavigated;
                        }
                    }

                    if (!tabControl.Controls.Contains(BuildReportTabPage))
                    {
                        tabControl.Controls.Add(BuildReportTabPage);
                    }
                }
                else
                {
                    if (BuildReportTabPage != null && tabControl.Controls.Contains(BuildReportTabPage))
                    {
                        BuildReportWebBrowser.Stop();
                        BuildReportWebBrowser.Document.Write(string.Empty);
                        tabControl.Controls.Remove(BuildReportTabPage);
                    }
                }
            }
            finally
            {
                tabControl.ResumeLayout();
            }
        }

        private void CreateBuildReportTabPage(TabControl tabControl)
        {
            this.BuildReportTabPage = new TabPage
                {
                    Padding = new Padding(3),
                    TabIndex = tabControl.Controls.Count,
                    Text = "Build Report",
                    UseVisualStyleBackColor = true
                };
            this.BuildReportWebBrowser = new WebBrowser
                {
                    Dock = DockStyle.Fill
                };
            this.BuildReportTabPage.Controls.Add(this.BuildReportWebBrowser);
        }

        private void BuildReportWebBrowserOnNavigated(object sender,
                                                      WebBrowserNavigatedEventArgs webBrowserNavigatedEventArgs)
        {
            BuildReportWebBrowser.Navigated -= BuildReportWebBrowserOnNavigated;

            var favIconUrl = DetermineFavIconUrl(BuildReportWebBrowser.Document);

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
                                    var imageIndex = BuildReportTabPage.ImageIndex;

                                    if (imageIndex < 0)
                                    {
                                        BuildReportTabPage.ImageIndex = imageCollection.Count;
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