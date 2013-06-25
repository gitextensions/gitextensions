using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Stash
{
    class RequestResponse
    {
        public bool Success { get; set; }
        public IEnumerable<string> Messages { get; set; }
        public JObject Result { get; set; }
    }

    abstract class StashRequestBase
    {
        protected Settings Settings { get; private set; }

        protected StashRequestBase(Settings settings)
        {
            Settings = settings;
        }

        public RequestResponse Send()
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://" + Settings.StashUrl + ApiUrl);
                request.Method = RequestMethod;
                request.ContentType = "application/json";
                request.Accept = "application/json";
                SetBasicAuthHeader(request, Settings.Username, Settings.Password);

                WriteRequestBody(request);

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    return ReadWebResponse(response);
                }
                catch (WebException wex)
                {
                    if (wex.Status == WebExceptionStatus.ProtocolError && wex.Response != null)
                    {
                        return ReadErrorWebResponse(wex.Response);
                    }
                    throw;
                }

            }
            catch (Exception ex)
            {
                return new RequestResponse { Success = false, Messages = new[] { ex.Message } };
            }
        }

        protected abstract void WriteRequestBody(HttpWebRequest request);
        protected abstract string ApiUrl { get; }
        protected abstract string RequestMethod { get; }

        protected virtual RequestResponse ReadWebResponse(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    return new RequestResponse { Success = false, Messages = new[] { "Unknown error." } };
                using (var reader = new StreamReader(stream))
                {
                    return new RequestResponse
                               {
                                   Success = true,
                                   Result = (JObject)JsonConvert.DeserializeObject(reader.ReadToEnd())
                               };
                }
            }
        }

        private static RequestResponse ReadErrorWebResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    return new RequestResponse { Success = false, Messages = new[] { "Unknown error." } };
                using (var reader = new StreamReader(stream))
                {
                    return ParseErrorResponse(reader);
                }
            }
        }

        private static RequestResponse ParseErrorResponse(StreamReader reader)
        {
            var jsonString = reader.ReadToEnd();
            var json = (JObject)JsonConvert.DeserializeObject(jsonString);
            if (json["errors"] != null)
            {
                var errorResponse = new RequestResponse { Success = false };
                var messages = json["errors"].Select(error => error["message"].ToString()).ToList();

                if (json["reviewerErrors"] != null)
                {
                    var reviewerErrors = json["reviewerErrors"].Select(e => e["message"].ToString());
                    messages.AddRange(reviewerErrors);
                }
                errorResponse.Messages = messages;
                return errorResponse;
            }
            if (json["message"] != null)
            {
                return new RequestResponse { Success = false, Messages = new[] { json["message"].ToString() } };
            }
            return new RequestResponse { Success = false, Messages = new[] { "Unknown error." } };
        }

        private static void SetBasicAuthHeader(WebRequest request, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

    }
}
