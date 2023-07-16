using System.Net;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using Microsoft;
using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient
{
    internal class GitlabApiClient
    {
        private readonly HttpClient _httpClient;

        public GitlabApiClient(string instanceUrl, string apiToken)
        {
            _httpClient = InitClient(instanceUrl, apiToken);
        }

        private HttpClient InitClient(string instanceUrl, string apiToken)
        {
            HttpClient? client = new()
            {
                BaseAddress = new Uri(instanceUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("PRIVATE-TOKEN", apiToken);

            return client;
        }

        private async Task<HttpResponseMessage> HttpGetAsync(Uri url)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            response.EnsureSuccessStatusCode();
            return response;
        }

        private int GetIntHeader(HttpResponseMessage response, string key)
        {
            if (response.Headers.TryGetValues(key, out IEnumerable<string>? values))
            {
                if (int.TryParse(values.First(), out int result))
                {
                    return result;
                }
            }

            throw new HttpRequestException($"Unable to extract header with key {key}");
        }

        protected async Task<PagedResponse<TItem>> LoadListAsync<TItem>(Uri url)
        {
            HttpResponseMessage response = await HttpGetAsync(url);
            Validates.NotNull(response);

            string json = await response.Content.ReadAsStringAsync();

            IEnumerable<TItem>? list = JsonConvert.DeserializeObject<IEnumerable<TItem>>(json);

            PagedResponse<TItem> result = new()
            {
                Total = GetIntHeader(response, "X-Total"),
                PageNumber = GetIntHeader(response, "X-Page"),
                PageSize = GetIntHeader(response, "X-Per-Page"),
                Items = list
            };

            return result;
        }
    }
}
