using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace GitHub3
{
    internal class GitHubPullRequest : IPullRequestInformation
    {
        private readonly PullRequest _pullRequest;

        public GitHubPullRequest(PullRequest pullRequest)
        {
            _pullRequest = pullRequest;
        }

        public string Title => _pullRequest.Title;

        public string Body => _pullRequest.Body;

        public string Owner => _pullRequest.User.Login;

        public DateTime Created => _pullRequest.CreatedAt;

        private string _diffData;

        public async Task<string> GetDiffDataAsync()
        {
            if (_diffData == null)
            {
                var request = (HttpWebRequest)WebRequest.Create(_pullRequest.DiffUrl);
                using (var response = await request.GetResponseAsync())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    _diffData = await reader.ReadToEndAsync();
                }
            }

            return _diffData;
        }

        private IHostedRepository _baseRepo;
        public IHostedRepository BaseRepo
        {
            get
            {
                if (_baseRepo == null)
                {
                    _baseRepo = new GitHubRepo(_pullRequest.Base.Repo);
                }

                return _baseRepo;
            }
        }

        private IHostedRepository _headRepo;
        public IHostedRepository HeadRepo
        {
            get
            {
                if (_headRepo == null)
                {
                    _headRepo = new GitHubRepo(_pullRequest.Head.Repo);
                }

                return _headRepo;
            }
        }

        public string BaseSha => _pullRequest.Base.Sha;

        public string HeadSha => _pullRequest.Head.Sha;

        public string BaseRef => _pullRequest.Base.Ref;

        public string HeadRef => _pullRequest.Head.Ref;

        public string Id => _pullRequest.Number.ToString();

        public string DetailedInfo => string.Format("Base repo owner: {0}\nHead repo owner: {1}", BaseRepo.Owner, HeadRepo.Owner);

        public void Close()
        {
            _pullRequest.Close();
        }

        private IPullRequestDiscussion _discussion;

        public IPullRequestDiscussion GetDiscussion()
        {
            if (_discussion == null)
            {
                _discussion = new GitHubPullRequestDiscussion(_pullRequest);
            }

            return _discussion;
        }
    }
}
