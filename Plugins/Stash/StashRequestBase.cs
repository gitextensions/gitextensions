using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Stash
{
    class StashResponse<T>
    {
        public bool Success { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public T Result { get; set; }
    }

    abstract class StashRequestBase<T>
    {
        protected Settings Settings { get; private set; }

        protected StashRequestBase(Settings settings)
        {
            Settings = settings;
        }

        public StashResponse<T> Send()
        {
            var client = new RestClient();
            client.BaseUrl = "http://" + Settings.StashUrl;
            client.Authenticator = new HttpBasicAuthenticator(Settings.Username, Settings.Password);

            var request = new RestRequest(ApiUrl, RequestMethod);
            if (RequestBody != null)
            {
                if (RequestBody is string)
                    request.AddParameter("application/json", RequestBody, ParameterType.RequestBody);
                else
                    request.AddBody(RequestBody);
            }

            var response = client.Execute(request);
            if (response.ResponseStatus != ResponseStatus.Completed)
                return new StashResponse<T>
                    {
                        Success = false,
                        Messages = new[] {response.ErrorMessage}
                    };

            if ((int)response.StatusCode >= 300)
            {
                return ParseErrorResponse(response.Content);
            }

            return new StashResponse<T>
                {
                    Success = true,
                    Result = ParseResponse(JObject.Parse(response.Content))
                };
        }

        protected abstract object RequestBody { get; }
        protected abstract Method RequestMethod { get; }
        protected abstract string ApiUrl { get; }
        protected abstract T ParseResponse(JObject json);

        private static StashResponse<T> ParseErrorResponse(string jsonString)
        {
            var json = (JObject) JsonConvert.DeserializeObject(jsonString);
            if (json["errors"] != null)
            {
                var messages = new List<string>();
                var errorResponse = new StashResponse<T> {Success = false};
                foreach (var error in json["errors"])
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(error["message"].ToString());
                    if (error["reviewerErrors"] != null)
                    {
                        sb.AppendLine();
                        foreach (var reviewerError in error["reviewerErrors"])
                        {
                            sb.Append(reviewerError["message"]).AppendLine();
                        }
                    }
                    messages.Add(sb.ToString());
                }

                errorResponse.Messages = messages;
                return errorResponse;
            }
            if (json["message"] != null)
            {
                return new StashResponse<T> {Success = false, Messages = new[] {json["message"].ToString()}};
            }
            return new StashResponse<T> {Success = false, Messages = new[] {"Unknown error."}};
        }

    }
}
