namespace GitExtensions.Plugins.GitlabIntegration.ApiClient.Models
{
    public class PagedResponse<TItem>
    {
        public int? Total { get; set; }
        public int? PageSize { get; set; }
        public int? PageNumber { get; set; }
        public int? NextPage { get; set; }

        public IEnumerable<TItem> Items { get; set; }
        public int? TotalPages { get; set; }
    }
}
