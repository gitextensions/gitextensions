﻿using System;
using System.IO;
using System.Net;
using System.Text;
using Git.hub;
using GitUIPluginInterfaces.RepositoryHosts;

namespace Github3
{
    internal class GithubPullRequest : IPullRequestInformation
    {
        private readonly PullRequest pullrequest;

        public GithubPullRequest(PullRequest pullrequest)
        {
            this.pullrequest = pullrequest;
        }

        public string Title => pullrequest.Title;

        public string Body => pullrequest.Body;

        public string Owner => pullrequest.User.Login;

        public DateTime Created => pullrequest.CreatedAt;


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

        public string BaseSha => pullrequest.Base.Sha;

        public string HeadSha => pullrequest.Head.Sha;

        public string BaseRef => pullrequest.Base.Ref;

        public string HeadRef => pullrequest.Head.Ref;

        public string Id => pullrequest.Number.ToString();

        public string DetailedInfo => string.Format("Base repo owner: {0}\nHead repo owner: {1}", BaseRepo.Owner, HeadRepo.Owner);

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
