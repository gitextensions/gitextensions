using CommonTestUtils;
using GitCommands;
using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitCommandsTests.Git_Commands;

[TestFixture]
public class GitCommandsHelperTest
{
    [Test]
    public void CanGetRelativeDateString()
    {
        AppSettings.CurrentTranslation = "English";

        DateTime now = DateTime.Now;

        ClassicAssert.AreEqual("0 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now));
        ClassicAssert.AreEqual("1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-1)));
        ClassicAssert.AreEqual("1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-1)));
        ClassicAssert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-45)));
        ClassicAssert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-1)));
        ClassicAssert.AreEqual("1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-1)));
        ClassicAssert.AreEqual("1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-7)));
        ClassicAssert.AreEqual("1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-30)));
        ClassicAssert.AreEqual("12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-364)));
        ClassicAssert.AreEqual("1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-365)));

        ClassicAssert.AreEqual("2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-2)));
        ClassicAssert.AreEqual("2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-2)));
        ClassicAssert.AreEqual("2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-2)));
        ClassicAssert.AreEqual("2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-2)));
        ClassicAssert.AreEqual("2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-14)));
        ClassicAssert.AreEqual("2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-60)));
        ClassicAssert.AreEqual("2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-730)));
    }

    [Test]
    public void CanGetRelativeNegativeDateString()
    {
        AppSettings.CurrentTranslation = "English";

        DateTime now = DateTime.Now;

        ClassicAssert.AreEqual("-1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(1)));
        ClassicAssert.AreEqual("-1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(1)));
        ClassicAssert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(45)));
        ClassicAssert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(1)));
        ClassicAssert.AreEqual("-1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(1)));
        ClassicAssert.AreEqual("-1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(7)));
        ClassicAssert.AreEqual("-1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(30)));
        ClassicAssert.AreEqual("-12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(364)));
        ClassicAssert.AreEqual("-1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(365)));

        ClassicAssert.AreEqual("-2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(2)));
        ClassicAssert.AreEqual("-2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(2)));
        ClassicAssert.AreEqual("-2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(2)));
        ClassicAssert.AreEqual("-2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(2)));
        ClassicAssert.AreEqual("-2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(14)));
        ClassicAssert.AreEqual("-2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(60)));
        ClassicAssert.AreEqual("-2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(730)));
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
            string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
        }

        {
            string fetchCmd = module.FetchCmd("origin", "some-branch", "local", true).Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --tags", fetchCmd);
        }

        {
            // Using a URL as remote and passing a local branch creates the branch
            string fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", "local").Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"https://host.com/repo\" +some-branch:refs/heads/local --no-tags", fetchCmd);
        }

        {
            // Using a URL as remote and not passing a local branch
            string fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", null).Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
        }

        {
            // No remote branch -> No local branch
            string fetchCmd = module.FetchCmd("origin", "", "local").Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 fetch --progress \"origin\" --no-tags", fetchCmd);
        }

        {
            // Pull doesn't accept a local branch ever
            string fetchCmd = module.PullCmd("origin", "some-branch", rebase: false).Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --progress \"origin\" +some-branch --no-tags", fetchCmd);
        }

        {
            // Not even for URL remote
            string fetchCmd = module.PullCmd("https://host.com/repo", "some-branch", rebase: false).Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
        }

        {
            // Pull with rebase
            string fetchCmd = module.PullCmd("origin", "some-branch", rebase: true).Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 -c submodule.fetchjobs=0 pull --rebase --progress \"origin\" +some-branch --no-tags", fetchCmd);
        }

        {
            // Config test fetch.parallel
            module.SetSetting("fetch.parallel", "1");
            string fetchCmd = module.FetchCmd("fetch.parallel", "some-branch", "local").Arguments;
            ClassicAssert.AreEqual("-c submodule.fetchjobs=0 fetch --progress \"fetch.parallel\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            module.UnsetSetting("fetch.parallel");
        }

        {
            // Config test submodule.fetchjobs
            module.SetSetting("submodule.fetchjobs", "0");
            string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            ClassicAssert.AreEqual("-c fetch.parallel=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            module.UnsetSetting("submodule.fetchjobs");
        }

        {
            // Config test fetch.parallel and submodule.fetchjobs
            module.SetSetting("fetch.parallel", "8");
            module.SetSetting("submodule.fetchjobs", "99");
            string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
            ClassicAssert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            module.UnsetSetting("fetch.parallel");
            module.UnsetSetting("submodule.fetchjobs");
        }
    }

    [Test]
    public void TestUnsetStagedStatus()
    {
        GitItemStatus item = new("name");
        ClassicAssert.AreEqual(item.Staged, StagedStatus.Unset);
    }
}
