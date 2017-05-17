using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Windows.Forms;
using RestSharp.Authenticators;

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
            if (Settings.DisableSSL)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback
                    = delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            }
            var client = new RestClient
            {
                BaseUrl = new System.Uri(Settings.StashUrl),
                Authenticator = new HttpBasicAuthenticator(Settings.Username, Settings.Password)
            };

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
            var json = new JObject();
            try
            {
                System.Console.WriteLine(jsonString);
                json = (JObject)JsonConvert.DeserializeObject(jsonString);
            }
            catch (JsonReaderException)
            {
                MessageBox.Show(jsonString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var errorResponse = new StashResponse<T> { Success = false };
                return errorResponse;
            }
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
