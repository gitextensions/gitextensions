using System;
using System.IO;
using System.Net;
using System.Text;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    internal class GithubPullRequest : IPullRequestInformation
    {
        private readonly PullRequest _pullrequest;

        public GithubPullRequest(PullRequest pullrequest)
        {
            _pullrequest = pullrequest;
        }

        public string Title => _pullrequest.Title;

        public string Body => _pullrequest.Body;

        public string Owner => _pullrequest.User.Login;

        public DateTime Created => _pullrequest.CreatedAt;

        private string _diffData;
        public string DiffData
        {
            get
            {
                if (_diffData == null)
                {
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(_pullrequest.DiffUrl);
                    using (var response = wr.GetResponse())
                    {
                        using (var respStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        _diffData = respStream.ReadToEnd();
                    }
                    }
                }

                return _diffData;
            }
        }

        private IHostedRepository _baseRepo;
        public IHostedRepository BaseRepo
        {
            get
            {
                if (_baseRepo == null)
                {
                    _baseRepo = new GithubRepo(_pullrequest.Base.Repo);
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
                    _headRepo = new GithubRepo(_pullrequest.Head.Repo);
                }

                return _headRepo;
            }
        }

        public string BaseSha => _pullrequest.Base.Sha;

        public string HeadSha => _pullrequest.Head.Sha;

        public string BaseRef => _pullrequest.Base.Ref;

        public string HeadRef => _pullrequest.Head.Ref;

        public string Id => _pullrequest.Number.ToString();

        public string DetailedInfo => string.Format("Base repo owner: {0}\nHead repo owner: {1}", BaseRepo.Owner, HeadRepo.Owner);

        public void Close()
        {
            _pullrequest.Close();
        }

        private IPullRequestDiscussion _discussion;
        public IPullRequestDiscussion Discussion
        {
            get
            {
                if (_discussion == null)
                {
                    _discussion = new GithubPullRequestDiscussion(_pullrequest);
                }

                return _discussion;
            }
        }
    }
}
