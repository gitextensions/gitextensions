﻿using Newtonsoft.Json.Linq;
using RestSharp;

namespace Stash
{
    class Commit
    {
        public static Commit Parse(JObject json)
        {
            return new Commit
            {
                Hash = json["id"].ToString(),
                Message = json["message"].ToString(),
                AuthorName = json["author"]["name"].ToString(),
                IsMerge = ((JArray)json["parents"]).Count > 1
            };
        }
        public string Hash { get; set; }
        public string Message { get; set; }
        public string AuthorName { get; set; }
        public bool IsMerge { get; set; }
    }
    class GetHeadCommitRequest : StashRequestBase<Commit>
    {
        private readonly Repository _repo;
        private readonly string _branch;

        public GetHeadCommitRequest(Repository repository, string branchName, Settings settings)
            : base(settings)
        {
            _repo = repository;
            _branch = branchName;
        }

        protected override object RequestBody
        {
            get { return null; }
        }

        protected override Method RequestMethod
        {
            get { return Method.GET; }
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("/projects/{0}/repos/{1}/commits/refs/heads/{2}",
                                     _repo.ProjectKey, _repo.RepoName, _branch);
            }
        }

        protected override Commit ParseResponse(JObject json)
        {
            return Commit.Parse(json);
        }
    }
}
