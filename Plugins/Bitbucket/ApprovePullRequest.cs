using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    class ApprovePullRequest : BitbucketRequestBase<JObject>
    {
        private readonly MergeRequestInfo _info;

        public ApprovePullRequest(Settings settings, MergeRequestInfo info)
            : base(settings)
        {
            _info = info;
        }

        protected override object RequestBody
        {
            get { return ""; }
        }

        protected override Method RequestMethod
        {
            get { return Method.POST; }
        }

        protected override string ApiUrl
        {
            get
            {
                return string.Format("rest/api/1.0/projects/{0}/repos/{1}/pull-requests/{2}/approve",
                                     _info.ProjectKey, _info.TargetRepo, _info.Id);
            }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }

    }
}