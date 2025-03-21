using System.Collections.Frozen;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Remotes;
using GitCommands.Settings;
using GitUI;
using NSubstitute;

namespace GitCommandsTests;

partial class GitModuleTests
{
    [Test]
    public void AddRemote()
    {
        const string name = "foo";
        const string path = "a\\b\\c";
        const string output = "bar";

        using (_executable.StageOutput($"remote add \"{name}\" \"{path.ToPosixPath()}\"", output))
        {
            Assert.AreEqual(output, _gitModule.AddRemote(name, path));
        }

        Assert.AreEqual("Please enter a name.", _gitModule.AddRemote("", path));
        Assert.AreEqual("Please enter a name.", _gitModule.AddRemote(null, path));
    }

    [Test]
    public void RemoveRemote()
    {
        const string remoteName = "foo";
        const string output = "bar";

        using (_executable.StageOutput($"remote rm \"{remoteName}\"", output))
        {
            Assert.AreEqual(output, _gitModule.RemoveRemote(remoteName));
        }
    }

    [Test]
    public void RenameRemote()
    {
        const string oldName = "foo";
        const string newName = "far";
        const string output = "bar";

        using (_executable.StageOutput($"remote rename \"{oldName}\" \"{newName}\"", output))
        {
            Assert.AreEqual(output, _gitModule.RenameRemote(oldName, newName));
        }
    }

    [TestCase("ignorenopush\tgit@github.com:drewnoakes/gitextensions.git (fetch)")]
    [TestCase("ignorenopull\tgit@github.com:drewnoakes/gitextensions.git (push)")]
    public async Task GetRemotes_should_throw_if_not_pairs(string line)
    {
        using (_executable.StageOutput("remote -v", line))
        {
            await AssertEx.ThrowsAsync<Exception>(async () => await _gitModule.GetRemotesAsync());
        }
    }

    [Test]
    public void GetRemotes_should_parse_correctly_configured_remotes()
    {
        ThreadHelper.JoinableTaskFactory.Run(async () =>
        {
            string[] lines =
            [
                "RussKie\tgit://github.com/RussKie/gitextensions.git (fetch)",
                "RussKie\tgit://github.com/RussKie/gitextensions.git (push)",
                "origin\tgit@github.com:drewnoakes/gitextensions.git (fetch)",
                "origin\tgit@github.com:drewnoakes/gitextensions.git (push)",
                "upstream\tgit@github.com:gitextensions/gitextensions.git (fetch)",
                "upstream\tgit@github.com:gitextensions/gitextensions.git (push)",
                "asymmetrical\thttps://github.com/gitextensions/fetch.git (fetch)",
                "asymmetrical\thttps://github.com/gitextensions/push.git (push)",
                "with-space\tc:\\Bare Repo (fetch)",
                "with-space\tc:\\Bare Repo (push)",

                // A remote may have multiple push URLs, but only a single fetch URL
                "multi\tgit@github.com:drewnoakes/gitextensions.git (fetch)",
                "multi\tgit@github.com:drewnoakes/gitextensions.git (push)",
                "multi\tgit@gitlab.com:drewnoakes/gitextensions.git (push)",

                "ignoreunknown\tgit@github.com:drewnoakes/gitextensions.git (unknownType)",
                "ignorenotab git@github.com:drewnoakes/gitextensions.git (fetch)",
                "ignoremissingtype\tgit@gitlab.com:drewnoakes/gitextensions.git",
                "git@gitlab.com:drewnoakes/gitextensions.git",

                "with_option\thttps://github.com/flannelhead/jsmn-stream.git (fetch) [blob:none]",
                "with_option\thttps://github.com/flannelhead/jsmn-stream.git (push) [ignored]"
            ];

            using (_executable.StageOutput("remote -v", string.Join("\n", lines)))
            {
                IReadOnlyList<GitExtensions.Extensibility.Git.Remote> remotes = await _gitModule.GetRemotesAsync();

                Assert.AreEqual(7, remotes.Count);

                Assert.AreEqual("RussKie", remotes[0].Name);
                Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].FetchUrl);
                Assert.AreEqual(1, remotes[0].PushUrls.Count);
                Assert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].PushUrls[0]);

                Assert.AreEqual("origin", remotes[1].Name);
                Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].FetchUrl);
                Assert.AreEqual(1, remotes[1].PushUrls.Count);
                Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].PushUrls[0]);

                Assert.AreEqual("upstream", remotes[2].Name);
                Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].FetchUrl);
                Assert.AreEqual(1, remotes[2].PushUrls.Count);
                Assert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].PushUrls[0]);

                Assert.AreEqual("asymmetrical", remotes[3].Name);
                Assert.AreEqual("https://github.com/gitextensions/fetch.git", remotes[3].FetchUrl);
                Assert.AreEqual(1, remotes[3].PushUrls.Count);
                Assert.AreEqual("https://github.com/gitextensions/push.git", remotes[3].PushUrls[0]);

                Assert.AreEqual("with-space", remotes[4].Name);
                Assert.AreEqual("c:/Bare Repo", remotes[4].FetchUrl);
                Assert.AreEqual(1, remotes[4].PushUrls.Count);
                Assert.AreEqual("c:/Bare Repo", remotes[4].PushUrls[0]);

                Assert.AreEqual("multi", remotes[5].Name);
                Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].FetchUrl);
                Assert.AreEqual(2, remotes[5].PushUrls.Count);
                Assert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[0]);
                Assert.AreEqual("git@gitlab.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[1]);

                Assert.AreEqual("with_option", remotes[6].Name);
                Assert.AreEqual("https://github.com/flannelhead/jsmn-stream.git", remotes[6].FetchUrl);
                Assert.AreEqual(1, remotes[6].PushUrls.Count);
                Assert.AreEqual("https://github.com/flannelhead/jsmn-stream.git", remotes[6].PushUrls[0]);
            }
        });
    }

    [Test]
    public void GetRemoteNames()
    {
        string[] lines = new[] { "RussKie", "origin", "upstream", "asymmetrical", "with-space" };

        using (_executable.StageOutput("remote", string.Join("\n", lines)))
        {
            IReadOnlyList<string> remotes = _gitModule.GetRemoteNames();

            Assert.AreEqual(lines, remotes);
        }
    }

    [Test]
    public void GetRemoteColors_should_return_saved_colors()
    {
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

        string remote = "fork";
        gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remote), "#cccccc");

        static string GetSettingKey(string settingKey, string remoteName) => string.Format(settingKey, remoteName);

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", "origin\nfork"))
        {
            FrozenDictionary<string, Color> colors = gitModule.GetRemoteColors();

            colors.Should().NotBeNull();
            colors.Count.Should().Be(1);
            colors[remote].Should().Be(Color.FromArgb(204, 204, 204));
        }
    }

    [Test]
    public void GetRemoteColors_should_return_cached_colors()
    {
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

        string remote = "fork";
        gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remote), "#cccccc");

        static string GetSettingKey(string settingKey, string remoteName) => string.Format(settingKey, remoteName);

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", "origin\nfork"))
        {
            FrozenDictionary<string, Color> colors = gitModule.GetRemoteColors();

            colors.Should().NotBeNull();
            colors.Count.Should().Be(1);
            colors.Should().ContainKey(remote);
            colors[remote].Should().Be(Color.FromArgb(204, 204, 204));

            // "Color" another remote
            gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remoteName: "origin"), "#efefef");

            // Call again to check if cached colors are returned
            FrozenDictionary<string, Color> colors1 = gitModule.GetRemoteColors();

            colors1.Should().NotBeNull();
            colors1.Count.Should().Be(1);
            colors1[remote].Should().Be(Color.FromArgb(204, 204, 204));
        }
    }

    [Test]
    public void GetRemoteColors_should_reload_colors_after_reset()
    {
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

        string remote = "fork";
        gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remote), "#cccccc");

        static string GetSettingKey(string settingKey, string remoteName) => string.Format(settingKey, remoteName);

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", "origin\nfork"))
        {
            FrozenDictionary<string, Color> colors = gitModule.GetRemoteColors();

            colors.Should().NotBeNull();
            colors.Count.Should().Be(1);
            colors[remote].Should().Be(Color.FromArgb(204, 204, 204));

            // "Color" another remote
            gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remoteName: "origin"), "#efefef");

            // Call again to check if cached colors are returned
            FrozenDictionary<string, Color> colors1 = gitModule.GetRemoteColors();
            colors1.Should().NotBeNull();
            colors1.Count.Should().Be(1);

            // Reset the colors
            gitModule.ResetRemoteColors();
            gitModule.GetTestAccessor().RemoteColors.Should().BeNull();
        }

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", "origin\nfork"))
        {
            // Reload the colors
            FrozenDictionary<string, Color> colors2 = gitModule.GetRemoteColors();

            colors2.Should().NotBeNull();
            colors2.Count.Should().Be(2);
            colors2[remote].Should().Be(Color.FromArgb(204, 204, 204)); // "#cccccc"
            colors2["origin"].Should().Be(Color.FromArgb(239, 239, 239)); // "#efefef"
        }
    }

    [Test]
    public void ResetRemoteColors_should_clear_cached_colors()
    {
        using GitModuleTestHelper moduleTestHelper = new();
        GitModule gitModule = GetGitModuleWithExecutable(_executable, module: moduleTestHelper.Module);

        string remote = "fork";
        gitModule.LocalConfigFile.SetValue(GetSettingKey(SettingKeyString.RemoteColor, remote), "#cccccc");

        static string GetSettingKey(string settingKey, string remoteName) => string.Format(settingKey, remoteName);

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", "origin\nfork"))
        {
            FrozenDictionary<string, Color> colors = gitModule.GetRemoteColors();

            colors.Should().NotBeNull();
            colors.Count.Should().Be(1);
            gitModule.GetTestAccessor().RemoteColors.Count.Should().Be(1);

            gitModule.ResetRemoteColors();

            gitModule.GetTestAccessor().RemoteColors.Should().BeNull();
        }
    }
}
