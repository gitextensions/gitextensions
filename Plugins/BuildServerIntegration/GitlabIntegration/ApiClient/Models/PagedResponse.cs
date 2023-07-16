namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    internal class PagedResponse<TItem>
    {
        public int Total { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public IEnumerable<TItem> Items { get; set; }
        public int TotalPages { get; set; }
    }
}
