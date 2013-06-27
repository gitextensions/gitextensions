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
                return new StashResponse<T> { Success = false, Messages = new[] { ex.Message } };
            }
        }

        protected abstract void WriteRequestBody(HttpWebRequest request);
        protected abstract string ApiUrl { get; }
        protected abstract string RequestMethod { get; }
        protected abstract T ParseResponse(JObject json);

        protected virtual StashResponse<T> ReadWebResponse(HttpWebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    return new StashResponse<T> { Success = false, Messages = new[] { "Unknown error." } };
                using (var reader = new StreamReader(stream))
                {
                    return new StashResponse<T>
                               {
                                   Success = true,
                                   Result = ParseResponse((JObject)JsonConvert.DeserializeObject(reader.ReadToEnd()))
                               };
                }
            }
        }

        private static StashResponse<T> ReadErrorWebResponse(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null)
                    return new StashResponse<T> { Success = false, Messages = new[] { "Unknown error." } };
                using (var reader = new StreamReader(stream))
                {
                    return ParseErrorResponse(reader);
                }
            }
        }

        private static StashResponse<T> ParseErrorResponse(StreamReader reader)
        {
            var jsonString = reader.ReadToEnd();
            var json = (JObject)JsonConvert.DeserializeObject(jsonString);
            if (json["errors"] != null)
            {
                var messages = new List<string>();
                var errorResponse = new StashResponse<T> { Success = false };
                foreach(var error in json["errors"])
                {
                    var sb = new StringBuilder();
                    sb.AppendLine(error["message"].ToString());
                    if (error["reviewerErrors"] != null)
                    {
                        foreach(var reviewerError in error["reviewerErrors"])
                        {
                            sb.Append("\t").Append(reviewerError["message"]).AppendLine();
                        }
                    }
                    messages.Add(sb.ToString());
                }
                
                errorResponse.Messages = messages;
                return errorResponse;
            }
            if (json["message"] != null)
            {
                return new StashResponse<T> { Success = false, Messages = new[] { json["message"].ToString() } };
            }
            return new StashResponse<T> { Success = false, Messages = new[] { "Unknown error." } };
        }

        private static void SetBasicAuthHeader(WebRequest request, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

    }
}
