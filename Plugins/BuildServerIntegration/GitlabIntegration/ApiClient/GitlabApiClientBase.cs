using System.Net;
using GitExtensions.Plugins.GitlabIntegration.ApiClient.Models;
using Microsoft;
using Newtonsoft.Json;

namespace GitExtensions.Plugins.GitlabIntegration.ApiClient
{
    public class GitlabApiClientBase : IDisposable
    {
        private readonly HttpClient _httpClient;

        public GitlabApiClientBase(string instanceUrl, string apiToken)
        {
            InstanceUrl = instanceUrl;
            _httpClient = InitClient(instanceUrl, apiToken);
        }

        public string InstanceUrl { get; }

        private HttpClient InitClient(string instanceUrl, string apiToken)
        {
            HttpClient? client = new()
            {
                BaseAddress = new Uri(instanceUrl, UriKind.RelativeOrAbsolute)
            };
            client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", apiToken);

            return client;
        }

        private async Task<HttpResponseMessage> HttpGetAsync(Uri url, CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
            switch (response.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.NotFound:
                    throw new UnauthorizedAccessException();
                default:
                    response.EnsureSuccessStatusCode();
                    return response;
            }
        }

        private static int? GetIntHeader(HttpResponseMessage response, string key)
        {
            if (response.Headers.TryGetValues(key, out IEnumerable<string>? values))
            {
                if (int.TryParse(values.First(), out int result))
                {
                    return result;
                }
            }

            return null;
        }

        protected async Task<PagedResponse<TItem>> LoadListAsync<TItem>(Uri url, CancellationToken cancellationToken)
        {
            using HttpResponseMessage response = await HttpGetAsync(url, cancellationToken);
            Validates.NotNull(response);

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            IEnumerable<TItem>? list = JsonConvert.DeserializeObject<IEnumerable<TItem>>(json);

            PagedResponse<TItem> result = new()
            {
                Total = GetIntHeader(response, "X-Total"),
                TotalPages = GetIntHeader(response, "X-Total-Pages"),
                PageNumber = GetIntHeader(response, "X-Page"),
                PageSize = GetIntHeader(response, "X-Per-Page"),
                NextPage = GetIntHeader(response, "X-Next-Page"),
                Items = list
            };

            return result;
        }

        protected async Task<TItem?> LoadItemAsync<TItem>(Uri url)
        {
            using HttpResponseMessage response = await HttpGetAsync(url, CancellationToken.None);
            Validates.NotNull(response);

            string json = await response.Content.ReadAsStringAsync();

            TItem? item = JsonConvert.DeserializeObject<TItem>(json);

            return item;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
