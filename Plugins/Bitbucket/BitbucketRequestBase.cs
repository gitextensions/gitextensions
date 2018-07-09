using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace Bitbucket
{
    internal class BitbucketResponse<T>
    {
        public bool Success { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public T Result { get; set; }
    }

    internal abstract class BitbucketRequestBase<T>
    {
        protected Settings Settings { get; }

        protected BitbucketRequestBase(Settings settings)
        {
            Settings = settings;
        }

        public async Task<BitbucketResponse<T>> SendAsync()
        {
            if (Settings.DisableSSL)
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            }

            var client = new RestClient
            {
                BaseUrl = new System.Uri(Settings.BitbucketUrl),
                Authenticator = new HttpBasicAuthenticator(Settings.Username, Settings.Password)
            };

            var request = new RestRequest(ApiUrl, RequestMethod);
            if (RequestBody != null)
            {
                if (RequestBody is string)
                {
                    request.AddParameter("application/json", RequestBody, ParameterType.RequestBody);
                }
                else
                {
                    request.AddBody(RequestBody);
                }
            }

            // XSRF check fails when approving/creating
            request.AddHeader("X-Atlassian-Token", "no-check");

            var response = await client.ExecuteTaskAsync(request).ConfigureAwait(false);
            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                return new BitbucketResponse<T>
                {
                    Success = false,
                    Messages = new[] { response.ErrorMessage }
                };
            }

            if ((int)response.StatusCode >= 300)
            {
                return ParseErrorResponse(response.Content);
            }

            return new BitbucketResponse<T>
            {
                Success = true,
                Result = ParseResponse(JObject.Parse(response.Content))
            };
        }

        [CanBeNull]
        protected abstract object RequestBody { get; }
        protected abstract Method RequestMethod { get; }
        protected abstract string ApiUrl { get; }
        protected abstract T ParseResponse(JObject json);

        private static BitbucketResponse<T> ParseErrorResponse(string jsonString)
        {
            JObject json;
            try
            {
                System.Console.WriteLine(jsonString);
                json = (JObject)JsonConvert.DeserializeObject(jsonString);
            }
            catch (JsonReaderException)
            {
                MessageBox.Show(jsonString, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                var errorResponse = new BitbucketResponse<T> { Success = false };
                return errorResponse;
            }

            if (json["errors"] != null)
            {
                var messages = new List<string>();
                var errorResponse = new BitbucketResponse<T> { Success = false };
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
                return new BitbucketResponse<T> { Success = false, Messages = new[] { json["message"].ToString() } };
            }

            return new BitbucketResponse<T> { Success = false, Messages = new[] { "Unknown error." } };
        }
    }
}
