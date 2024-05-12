using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility.Git;
using ResourceManager;

namespace GitCommandsTests.Git_Commands
{
    [TestFixture]
    public class GitCommandsHelperTest
    {
        [Test]
        public void CanGetRelativeDateString()
        {
            AppSettings.CurrentTranslation = "English";

            DateTime now = DateTime.Now;

            Assert.AreEqual("0 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now));
            Assert.AreEqual("1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-1)));
            Assert.AreEqual("1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-1)));
            Assert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-45)));
            Assert.AreEqual("1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-1)));
            Assert.AreEqual("1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-1)));
            Assert.AreEqual("1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-7)));
            Assert.AreEqual("1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-30)));
            Assert.AreEqual("12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-364)));
            Assert.AreEqual("1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-365)));

            Assert.AreEqual("2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(-2)));
            Assert.AreEqual("2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(-2)));
            Assert.AreEqual("2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(-2)));
            Assert.AreEqual("2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-2)));
            Assert.AreEqual("2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-14)));
            Assert.AreEqual("2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-60)));
            Assert.AreEqual("2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(-730)));
        }

        [Test]
        public void CanGetRelativeNegativeDateString()
        {
            AppSettings.CurrentTranslation = "English";

            DateTime now = DateTime.Now;

            Assert.AreEqual("-1 second ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(1)));
            Assert.AreEqual("-1 minute ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(1)));
            Assert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(45)));
            Assert.AreEqual("-1 hour ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(1)));
            Assert.AreEqual("-1 day ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(1)));
            Assert.AreEqual("-1 week ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(7)));
            Assert.AreEqual("-1 month ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(30)));
            Assert.AreEqual("-12 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(364)));
            Assert.AreEqual("-1 year ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(365)));

            Assert.AreEqual("-2 seconds ago", LocalizationHelpers.GetRelativeDateString(now, now.AddSeconds(2)));
            Assert.AreEqual("-2 minutes ago", LocalizationHelpers.GetRelativeDateString(now, now.AddMinutes(2)));
            Assert.AreEqual("-2 hours ago", LocalizationHelpers.GetRelativeDateString(now, now.AddHours(2)));
            Assert.AreEqual("-2 days ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(2)));
            Assert.AreEqual("-2 weeks ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(14)));
            Assert.AreEqual("-2 months ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(60)));
            Assert.AreEqual("-2 years ago", LocalizationHelpers.GetRelativeDateString(now, now.AddDays(730)));
        }

        [Test]
        public void TestFetchArguments()
        {
            // TODO produce a valid working directory
            GitModule module = new(Path.GetTempPath());
            ConfigFileSettings localConfigFile = (ConfigFileSettings)module.LocalConfigFile;
            localConfigFile.SetString("fetch.parallel", null);
            localConfigFile.SetString("submodule.fetchJobs", null);
            {
                // Specifying a remote and a local branch creates a local branch
                string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                string fetchCmd = module.FetchCmd("origin", "some-branch", "local", true).Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"origin\" +some-branch:refs/heads/local --tags", fetchCmd);
            }

            {
                // Using a URL as remote and passing a local branch creates the branch
                string fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", "local").Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"https://host.com/repo\" +some-branch:refs/heads/local --no-tags", fetchCmd);
            }

            {
                // Using a URL as remote and not passing a local branch
                string fetchCmd = module.FetchCmd("https://host.com/repo", "some-branch", null).Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // No remote branch -> No local branch
                string fetchCmd = module.FetchCmd("origin", "", "local").Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 fetch --progress \"origin\" --no-tags", fetchCmd);
            }

            {
                // Pull doesn't accept a local branch ever
                string fetchCmd = module.PullCmd("origin", "some-branch", false).Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 pull --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Not even for URL remote
                string fetchCmd = module.PullCmd("https://host.com/repo", "some-branch", false).Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 pull --progress \"https://host.com/repo\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Pull with rebase
                string fetchCmd = module.PullCmd("origin", "some-branch", true).Arguments;
                Assert.AreEqual("-c fetch.parallel=0 -c submodule.fetchJobs=0 pull --rebase --progress \"origin\" +some-branch --no-tags", fetchCmd);
            }

            {
                // Config test fetch.parallel
                localConfigFile.SetString("fetch.parallel", "1");
                string fetchCmd = module.FetchCmd("fetch.parallel", "some-branch", "local").Arguments;
                Assert.AreEqual("-c submodule.fetchJobs=0 fetch --progress \"fetch.parallel\" +some-branch:refs/heads/local --no-tags", fetchCmd);
                localConfigFile.SetString("fetch.parallel", null);
            }

            {
                // Config test submodule.fetchJobs
                localConfigFile.SetString("submodule.fetchJobs", "0");
                string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
                Assert.AreEqual("-c fetch.parallel=0 fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
                localConfigFile.SetString("submodule.fetchJobs", null);
            }

            {
                // Config test fetch.parallel and submodule.fetchJobs
                localConfigFile.SetString("fetch.parallel", "8");
                localConfigFile.SetString("submodule.fetchJobs", "99");
                string fetchCmd = module.FetchCmd("origin", "some-branch", "local").Arguments;
                Assert.AreEqual("fetch --progress \"origin\" +some-branch:refs/heads/local --no-tags", fetchCmd);
                localConfigFile.SetString("fetch.parallel", null);
                localConfigFile.SetString("submodule.fetchJobs", null);
            }
        }

        [Test]
        public void TestUnsetStagedStatus()
        {
            GitItemStatus item = new("name");
            Assert.AreEqual(item.Staged, StagedStatus.Unset);
        }
    }
}
