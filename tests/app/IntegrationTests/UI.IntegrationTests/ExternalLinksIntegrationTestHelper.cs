using System.ComponentModel.Design;
using CommonTestUtils;
using GitCommands;
using GitCommands.Config;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Settings;
using GitExtUtils;
using GitUI;
using GitUI.Editor.RichTextBoxExtension;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.UITests;

internal static class ExternalLinksIntegrationTestHelper
{
    internal const string MergePullRequestSubject = "Merge pull request #3657";
    internal const string MergePullRequestBody = "Merge pull request #3657 from RussKie/tweak_FormRemotes_tooltips";

    internal const string RepoAUserRepoSlug = "userA/repoA";
    internal const string RepoBUserRepoSlug = "userB/repoB";

    private const string GitHubSchemeAndHost = "https://github.com/";

    internal static readonly DateTime SampleAuthorTime = new(2010, 3, 24, 13, 37, 12, DateTimeKind.Unspecified);

    internal static string OriginUrlForUserRepo(string userRepoSlug) => GitHubSchemeAndHost + userRepoSlug + ".git";

    public static ExternalLinkDefinition CreateGitHubIssuesLinkDefinition()
    {
        ExternalLinkDefinition definition = new()
        {
            Name = "GitHub - issues",
            Enabled = true,
            SearchPattern = @"(\s*(,|and)?\s*#\d+)+",
            NestedSearchPattern = @"(\d+)+",
            RemoteSearchPattern = @"github.com[:/](.+)\.git",
            UseRemotesPattern = "origin",
            UseOnlyFirstRemote = true,
        };
        definition.SearchInParts.Add(ExternalLinkDefinition.RevisionPart.Message);
        definition.RemoteSearchInParts.Add(ExternalLinkDefinition.RemotePart.URL);
        definition.LinkFormats.Add(new ExternalLinkFormat
        {
            Caption = "Issue {1}",
            Format = "https://github.com/{0}/issues/{1}",
        });
        return definition;
    }

    public static void ConfigureRepositoryForExternalLinks(GitModuleTestHelper helper, string originUrl, ExternalLinkDefinition linkDefinition)
    {
        GitModule module = helper.Module;
        module.SetSetting(string.Format(SettingKeyString.RemoteUrl, "origin"), originUrl);

        string settingsPath = helper.CreateRepoFile(".git", "GitExtensions.settings", "<dictionary />");
        using GitExtSettingsCache settingsCache = new(settingsPath);
        DistributedSettings settings = new(lowerPriority: null, settingsCache, SettingLevel.Unknown);
        new ExternalLinksStorage().Save(settings, [linkDefinition]);
        settings.Save();
    }

    public static GitRevision CreateMergePullRequestRevision(string authorFullIdentity, string authorEmail, DateTime authorTime)
    {
        return new GitRevision(ObjectId.Random())
        {
            Author = authorFullIdentity,
            AuthorUnixTime = DateTimeUtils.ToUnixTime(authorTime),
            AuthorEmail = authorEmail,
            Subject = MergePullRequestSubject,
            Body = MergePullRequestBody,
        };
    }

    public static string? FindFirstGitHubIssueLink(RichTextBox box)
    {
        for (int i = 0; i < box.TextLength; i++)
        {
            string? link = box.GetLink(i);
            if (link?.Contains("github.com", StringComparison.OrdinalIgnoreCase) is true
                && link.Contains("issues", StringComparison.OrdinalIgnoreCase))
            {
                return link;
            }
        }

        return null;
    }

    public static int FindLinkCharIndex(RichTextBox box, string uriPart)
    {
        for (int i = 0; i < box.TextLength; i++)
        {
            string? link = box.GetLink(i);
            if (link?.Contains(uriPart, StringComparison.Ordinal) is true)
            {
                return i;
            }
        }

        return -1;
    }
}

internal sealed class ExternalLinksTwoRepositoryFixture : IDisposable
{
    private readonly GitModuleTestHelper _repoA;
    private readonly GitModuleTestHelper _repoB;

    public GitUICommands CommandsA { get; }
    public GitUICommands CommandsB { get; }
    public TestGitUICommandsSource UiCommandsSource { get; }
    public GitRevision Revision { get; }

    public ExternalLinksTwoRepositoryFixture(
        ILinkFactory linkFactory,
        string repositoryNamePrefix,
        string authorFullIdentity,
        string authorEmail,
        DateTime authorTime)
    {
        ExternalLinkDefinition linkDefinition = ExternalLinksIntegrationTestHelper.CreateGitHubIssuesLinkDefinition();

        _repoA = new GitModuleTestHelper($"{repositoryNamePrefix}A");
        _repoB = new GitModuleTestHelper($"{repositoryNamePrefix}B");
        ExternalLinksIntegrationTestHelper.ConfigureRepositoryForExternalLinks(
            _repoA,
            ExternalLinksIntegrationTestHelper.OriginUrlForUserRepo(ExternalLinksIntegrationTestHelper.RepoAUserRepoSlug),
            linkDefinition);
        ExternalLinksIntegrationTestHelper.ConfigureRepositoryForExternalLinks(
            _repoB,
            ExternalLinksIntegrationTestHelper.OriginUrlForUserRepo(ExternalLinksIntegrationTestHelper.RepoBUserRepoSlug),
            linkDefinition);

        ServiceContainer serviceContainer = GlobalServiceContainer.CreateDefaultMockServiceContainer();
        serviceContainer.RemoveService<ILinkFactory>();
        serviceContainer.AddService<ILinkFactory>(linkFactory);

        CommandsA = new GitUICommands(serviceContainer, _repoA.Module);
        CommandsB = new GitUICommands(serviceContainer, _repoB.Module);
        UiCommandsSource = new TestGitUICommandsSource(CommandsA);
        Revision = ExternalLinksIntegrationTestHelper.CreateMergePullRequestRevision(authorFullIdentity, authorEmail, authorTime);
    }

    public void Dispose()
    {
        _repoB.Dispose();
        _repoA.Dispose();
    }
}

internal sealed class TestGitUICommandsSource : IGitUICommandsSource
{
    private IGitUICommands _commands;

    public TestGitUICommandsSource(IGitUICommands commands)
    {
        _commands = commands;
    }

    public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged;

    public IGitUICommands UICommands => _commands;

    public void SetCommands(IGitUICommands commands)
    {
        IGitUICommands oldCommands = _commands;
        _commands = commands;
        UICommandsChanged?.Invoke(this, new GitUICommandsChangedEventArgs(oldCommands));
    }
}
