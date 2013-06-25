using System.Net;

namespace Stash
{
    public class StashUser
    {
        public string Slug { get; set; }
    }

    class GetUserRequest : StashRequestBase
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


    }
}
