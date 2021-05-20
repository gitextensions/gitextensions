using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace GitExtensions.Plugins.Bitbucket
{
    internal class BitbucketResponse<T>
    {
        public bool Success { get; set; }
        public IEnumerable<string>? Messages { get; set; }
        public T? Result { get; set; }
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

            Validates.NotNull(Settings.BitbucketUrl);
            Validates.NotNull(Settings.Username);
            Validates.NotNull(Settings.Password);

            RestClient client = new()
            {
                BaseUrl = new System.Uri(Settings.BitbucketUrl),
                Authenticator = new HttpBasicAuthenticator(Settings.Username, Settings.Password)
            };

            RestRequest request = new(ApiUrl, RequestMethod);
            if (RequestBody is not null)
            {
                request.AddJsonBody(RequestBody);
            }

            // XSRF check fails when approving/creating
            request.AddHeader("X-Atlassian-Token", "no-check");

            var response = await client.ExecuteAsync(request).ConfigureAwait(false);
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

        protected abstract object? RequestBody { get; }
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
                BitbucketResponse<T> errorResponse = new() { Success = false };
                return errorResponse;
            }

            if (json["errors"] is not null)
            {
                List<string> messages = new();
                BitbucketResponse<T> errorResponse = new() { Success = false };
                foreach (var error in json["errors"])
                {
                    StringBuilder sb = new();
                    sb.AppendLine(error["message"].ToString());
                    if (error["reviewerErrors"] is not null)
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

            if (json["message"] is not null)
            {
                return new BitbucketResponse<T> { Success = false, Messages = new[] { json["message"].ToString() } };
            }

            return new BitbucketResponse<T> { Success = false, Messages = new[] { "Unknown error." } };
        }
    }
}
