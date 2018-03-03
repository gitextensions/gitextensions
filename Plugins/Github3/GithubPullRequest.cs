using System;
using System.IO;
using System.Net;
using System.Text;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    class GithubPullRequest : IPullRequestInformation
    {
        private PullRequest _pullrequest;

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
                    using (var respStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        _diffData = respStream.ReadToEnd();
                    }
                }
                return _diffData;
            }
        }

        private IHostedRepository _BaseRepo;
        public IHostedRepository BaseRepo
        {
            get
            {
                if (_BaseRepo == null)
                    _BaseRepo = new GithubRepo(_pullrequest.Base.Repo);

                return _BaseRepo;
            }
        }

        private IHostedRepository _HeadRepo;
        public IHostedRepository HeadRepo
        {
            get
            {
                if (_HeadRepo == null)
                    _HeadRepo = new GithubRepo(_pullrequest.Head.Repo);

                return _HeadRepo;
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

        private IPullRequestDiscussion _Discussion;
        public IPullRequestDiscussion Discussion
        {
            get
            {
                if (_Discussion == null)
                    _Discussion = new GithubPullRequestDiscussion(_pullrequest);

                return _Discussion;
            }
        }
    }
}
