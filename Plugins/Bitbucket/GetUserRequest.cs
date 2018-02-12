using Newtonsoft.Json.Linq;
using RestSharp;

namespace Bitbucket
{
    public class BitbucketUser
    {
        public string Slug { get; set; }
    }

    class GetUserRequest : BitbucketRequestBase<JObject>
    {
        public GetUserRequest(Settings settings)
            : base(settings)
        {
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
            get { return "/rest/api/1.0/users?limit=1000"; }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}
