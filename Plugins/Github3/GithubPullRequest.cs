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
        private PullRequest pullrequest;

        public GithubPullRequest(PullRequest pullrequest)
        {
            this.pullrequest = pullrequest;
        }

        public string Title
        {
            get { return pullrequest.Title; }
        }

        public string Body
        {
            get { return pullrequest.Body; }
        }

        public string Owner
        {
            get { return pullrequest.User.Login; }
        }

        public DateTime Created
        {
            get { return pullrequest.CreatedAt; }
        }



        private string _diffData;
        public string DiffData 
        { 
            get
            {
                if (_diffData == null)
                {
                    HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(pullrequest.DiffUrl);
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
                    _BaseRepo = new GithubRepo(pullrequest.Base.Repo);

                return _BaseRepo;
            }
        }

        private IHostedRepository _HeadRepo;
        public IHostedRepository HeadRepo
        {
            get
            {
                if(_HeadRepo == null)
                    _HeadRepo = new GithubRepo(pullrequest.Head.Repo);

                return _HeadRepo;
            }
        }

        public string BaseSha
        {
            get { return pullrequest.Base.Sha; }
        }

        public string HeadSha
        {
            get { return pullrequest.Head.Sha; }
        }

        public string BaseRef
        {
            get { return pullrequest.Base.Ref; }
        }

        public string HeadRef
        {
            get { return pullrequest.Head.Ref; }
        }

        public string Id
        {
            get { return pullrequest.Number.ToString(); }
        }

        public string DetailedInfo
        {
            get { return string.Format("Base repo owner: {0}\nHead repo owner: {1}", BaseRepo.Owner, HeadRepo.Owner); }
        }

        public void Close()
        {
            pullrequest.Close();
        }

        private IPullRequestDiscussion _Discussion;
        public IPullRequestDiscussion Discussion
        {
            get
            {
                if(_Discussion == null)
                    _Discussion = new GithubPullRequestDiscussion(pullrequest);

                return _Discussion;
            }
        }
    }
}
