using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitCommandsTests.Git_Commands;
public class GitCommandsHelperTest
{
    [Test]
    public void CanGetRelativeDateString()
    {
        AppSettings.CurrentTranslation = "English";

        DateTime now = DateTime.Now;

        LocalizationHelpers.GetRelativeDateString(now, now).Should().Be("0 seconds ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-1)).Should().Be("1 second ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-1)).Should().Be("1 minute ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-45)).Should().Be("1 hour ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-1)).Should().Be("1 hour ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-1)).Should().Be("1 day ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-7)).Should().Be("1 week ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-30)).Should().Be("1 month ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-364)).Should().Be("12 months ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-365)).Should().Be("1 year ago");

        LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-2)).Should().Be("2 seconds ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-2)).Should().Be("2 minutes ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-2)).Should().Be("2 hours ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-2)).Should().Be("2 days ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-14)).Should().Be("2 weeks ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-60)).Should().Be("2 months ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-730)).Should().Be("2 years ago");
    }

    [Test]
    public void CanGetRelativeNegativeDateString()
    {
        AppSettings.CurrentTranslation = "English";

        DateTime now = DateTime.Now;

        LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(1)).Should().Be("-1 second ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(1)).Should().Be("-1 minute ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(45)).Should().Be("-1 hour ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddHours(1)).Should().Be("-1 hour ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(1)).Should().Be("-1 day ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(7)).Should().Be("-1 week ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(30)).Should().Be("-1 month ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(364)).Should().Be("-12 months ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(365)).Should().Be("-1 year ago");

        LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(2)).Should().Be("-2 seconds ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(2)).Should().Be("-2 minutes ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddHours(2)).Should().Be("-2 hours ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(2)).Should().Be("-2 days ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(14)).Should().Be("-2 weeks ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(60)).Should().Be("-2 months ago");
        LocalizationHelpers.GetRelativeDateString(now, now.AddDays(730)).Should().Be("-2 years ago");
    }

    [Test]
    public void TestFetchArguments()
    {
        using ReferenceRepository referenceRepository = new(createCommit: false);
        IGitModule module = referenceRepository.Module;
        module.UnsetSetting("fetch.parallel");
        module.UnsetSetting("submodule.fetchjobs");
        {
            // Specifying a remote and a local branch creates a local branch
            string? fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags");
        }

        {
            string? fetchCmd = module.FetchCmd("origin", "some-branch", "local", true).Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --tags");
        }

        {
            // Using a URL as remote and passing a local branch creates the branch
            string? fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", "local").Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"https://host.com/repo\" +some-branch:refs/heads/local --no-tags");
        }

        {
            // Using a URL as remote and not passing a local branch
            string? fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", null).Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"https://host.com/repo\" +some-branch --no-tags");
        }

        {
            // No remote branch -> No local branch
            string? fetchCmd = module.FetchCmd("origin", "", "local").Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" --no-tags");
        }

        {
            // Pull doesn't accept a local branch ever
            string? fetchCmd = module.PullCmd("origin", "some-branch", rebase: false).Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --progress \"origin\" +some-branch --no-tags");
        }

        {
            // Not even for URL remote
            string? fetchCmd = module.PullCmd("https://host.com/repo", "some-branch", rebase: false).Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --progress \"https://host.com/repo\" +some-branch --no-tags");
        }

        {
            // Pull with rebase
            string? fetchCmd = module.PullCmd("origin", "some-branch", rebase: true).Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --rebase --progress \"origin\" +some-branch --no-tags");
        }

        {
            // Config test fetch.parallel
            module.SetSetting("fetch.parallel", "1");
            string? fetchCmd = module.FetchCmd("fetch.parallel", "some-branch", "local").Arguments;
            fetchCmd.Should().Be("-c submodule.fetchjobs=0 fetch --progress \"fetch.parallel\" +some-branch:refs/heads/local --no-tags");
            module.UnsetSetting("fetch.parallel");
        }

        {
            // Config test submodule.fetchjobs
            module.SetSetting("submodule.fetchjobs", "0");
            string? fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            fetchCmd.Should().Be("-c fetch.parallel=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags");
            module.UnsetSetting("submodule.fetchjobs");
        }

        {
            // Config test fetch.parallel and submodule.fetchjobs
            module.SetSetting("fetch.parallel", "8");
            module.SetSetting("submodule.fetchjobs", "99");
            string? fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            fetchCmd.Should().Be("fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags");
            module.UnsetSetting("fetch.parallel");
            module.UnsetSetting("submodule.fetchjobs");
        }
    }

    [Test]
    public void TestUnsetStagedStatus()
    {
        GitItemStatus item = new("name");
        StagedStatus.Unset.Should().Be(item.Staged);
    }
}
