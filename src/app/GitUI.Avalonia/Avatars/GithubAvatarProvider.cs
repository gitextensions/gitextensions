using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using JetBrains.Annotations;

namespace GitUI.Avatars;

public sealed partial class GithubAvatarProvider : IAvatarProvider
{
    private static readonly HttpClient _client = CreateClient();

    private readonly IAvatarDownloader _downloader;
    private readonly bool _onlySupplyNoReply;

    [GeneratedRegex(@"^(\d+\+)?(?<username>[^@]+)@users\.noreply\.github\.com$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex GitHubEmailRegex { get; }

    public GithubAvatarProvider([NotNull] IAvatarDownloader downloader, bool onlySupplyNoReply = false)
    {
        _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        _onlySupplyNoReply = onlySupplyNoReply;
    }

    public bool PerformsIo => true;

    public async Task<byte[]?> GetAvatarAsync(string email, string? name, int imageSize)
    {
        Uri? uri = await BuildAvatarUriAsync(email, imageSize);

        if (uri is null)
        {
            return null;
        }

        byte[]? image = await _downloader.DownloadImageAsync(uri);
        bool isIdenticon = imageSize != 420 && AvatarImage.GetPixelSize(image)?.Width is 420;
        return isIdenticon ? null : image;
    }

    private async Task<Uri?> BuildAvatarUriAsync(string email, int imageSize)
    {
        Match match = GitHubEmailRegex.Match(email);

        if (!match.Success)
        {
            if (_onlySupplyNoReply)
            {
                return null;
            }

            string encodedEmail = HttpUtility.UrlEncode(email);
            return new Uri($"https://avatars.githubusercontent.com/u/e?email={encodedEmail}&s={imageSize}");
        }

        string username = match.Groups["username"].Value;
        if (username.Contains('['))
        {
            using HttpResponseMessage response = await _client.GetAsync($"https://api.github.com/users/{HttpUtility.UrlEncode(username)}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using Stream profileStream = await response.Content.ReadAsStreamAsync();
            using JsonDocument profile = await JsonDocument.ParseAsync(profileStream);
            string? avatarUrl = null;
            if (!profile.RootElement.TryGetProperty("avatar_url", out JsonElement avatarUrlElement)
                || string.IsNullOrWhiteSpace(avatarUrl = avatarUrlElement.GetString()))
            {
                return null;
            }

            UriBuilder builder = new(avatarUrl);
            StringBuilder query = new(builder.Query.TrimStart('?'));
            query.Append(query.Length == 0 ? '?' : '&');
            query.Append("s=").Append(imageSize);
            builder.Query = query.ToString();
            return builder.Uri;
        }

        string encodedUsername = HttpUtility.UrlEncode(username);
        return new Uri($"https://avatars.githubusercontent.com/{encodedUsername}?s={imageSize}");
    }

    private static HttpClient CreateClient()
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("GitExtensions");
        return client;
    }
}
