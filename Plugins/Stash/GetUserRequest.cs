using System.Net;
using Newtonsoft.Json.Linq;

namespace Stash
{
    public class StashUser
    {
        public string Slug { get; set; }
    }

    class GetUserRequest : StashRequestBase<JObject>
    {
        public GetUserRequest(Settings settings)
            : base(settings)
        {
        }

        protected override void WriteRequestBody(HttpWebRequest request)
        {
            //do nothing
        }

        protected override string ApiUrl
        {
            get { return "/rest/api/1.0/users?limit=1000"; }
        }

        protected override string RequestMethod
        {
            get { return "GET"; }
        }

        protected override JObject ParseResponse(JObject json)
        {
            return json;
        }
    }
}
