namespace GitExtensions.Extensibility.Git;

public readonly struct Remote
{
    public string Name { get; }
    public string FetchUrl { get; }
    public List<string> PushUrls { get; }

    public Remote(string name, string fetchUrl, string? firstPushUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FetchUrl = fetchUrl ?? throw new ArgumentNullException(nameof(fetchUrl));

        // At least one push URL must be added
        PushUrls = firstPushUrl is not null ? [firstPushUrl] : throw new ArgumentNullException(nameof(firstPushUrl));
    }
}
