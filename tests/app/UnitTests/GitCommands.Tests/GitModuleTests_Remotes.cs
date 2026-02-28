using System.Collections.Frozen;
using CommonTestUtils;
using FluentAssertions;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitUI;

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
            using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
            using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", "");
            using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", "");
            GitVersion.ResetVersion();

            ClassicAssert.AreEqual(output, _gitModule.AddRemote(name, path));

            _gitModule.GetEffectiveSetting("reload now");
            _gitModule.GetSettings("reload local settings, too");
        }

        ClassicAssert.AreEqual("Please enter a name.", _gitModule.AddRemote("", path));
        ClassicAssert.AreEqual("Please enter a name.", _gitModule.AddRemote(null, path));
    }

    [Test]
    public void RemoveRemote()
    {
        const string remoteName = "foo";
        const string output = "bar";

        using IDisposable remoteRemove = _executable.StageOutput($"remote rm \"{remoteName}\"", output);
        using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
        using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", "");
        using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", "");
        GitVersion.ResetVersion();

        ClassicAssert.AreEqual(output, _gitModule.RemoveRemote(remoteName));

        _gitModule.GetEffectiveSetting("reload now");
        _gitModule.GetSettings("reload local settings, too");
    }

    [Test]
    public void RenameRemote()
    {
        const string oldName = "foo";
        const string newName = "far";
        const string output = "bar";

        using IDisposable remoteRename = _executable.StageOutput($"remote rename \"{oldName}\" \"{newName}\"", output);
        using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
        using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", "");
        using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", "");
        GitVersion.ResetVersion();

        ClassicAssert.AreEqual(output, _gitModule.RenameRemote(oldName, newName));

        _gitModule.GetEffectiveSetting("reload now");
        _gitModule.GetSettings("reload local settings, too");
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

                ClassicAssert.AreEqual(7, remotes.Count);

                ClassicAssert.AreEqual("RussKie", remotes[0].Name);
                ClassicAssert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[0].PushUrls.Count);
                ClassicAssert.AreEqual("git://github.com/RussKie/gitextensions.git", remotes[0].PushUrls[0]);

                ClassicAssert.AreEqual("origin", remotes[1].Name);
                ClassicAssert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[1].PushUrls.Count);
                ClassicAssert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[1].PushUrls[0]);

                ClassicAssert.AreEqual("upstream", remotes[2].Name);
                ClassicAssert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[2].PushUrls.Count);
                ClassicAssert.AreEqual("git@github.com:gitextensions/gitextensions.git", remotes[2].PushUrls[0]);

                ClassicAssert.AreEqual("asymmetrical", remotes[3].Name);
                ClassicAssert.AreEqual("https://github.com/gitextensions/fetch.git", remotes[3].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[3].PushUrls.Count);
                ClassicAssert.AreEqual("https://github.com/gitextensions/push.git", remotes[3].PushUrls[0]);

                ClassicAssert.AreEqual("with-space", remotes[4].Name);
                ClassicAssert.AreEqual("c:/Bare Repo", remotes[4].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[4].PushUrls.Count);
                ClassicAssert.AreEqual("c:/Bare Repo", remotes[4].PushUrls[0]);

                ClassicAssert.AreEqual("multi", remotes[5].Name);
                ClassicAssert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].FetchUrl);
                ClassicAssert.AreEqual(2, remotes[5].PushUrls.Count);
                ClassicAssert.AreEqual("git@github.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[0]);
                ClassicAssert.AreEqual("git@gitlab.com:drewnoakes/gitextensions.git", remotes[5].PushUrls[1]);

                ClassicAssert.AreEqual("with_option", remotes[6].Name);
                ClassicAssert.AreEqual("https://github.com/flannelhead/jsmn-stream.git", remotes[6].FetchUrl);
                ClassicAssert.AreEqual(1, remotes[6].PushUrls.Count);
                ClassicAssert.AreEqual("https://github.com/flannelhead/jsmn-stream.git", remotes[6].PushUrls[0]);
            }
        });
    }

    [Test]
    public void GetRemoteNames()
    {
        string[] lines = ["RussKie", "origin", "upstream", "asymmetrical", "with-space"];

        using (_executable.StageOutput("remote", string.Join("\n", lines)))
        {
            IReadOnlyList<string> remotes = _gitModule.GetRemoteNames();

            ClassicAssert.AreEqual(lines, remotes);
        }
    }

    [Test]
    public void GetRemoteColors_should_return_cached_colors()
    {
        const string origin = nameof(origin);
        const string fork = nameof(fork);
        const string remotes = $"{origin}\n{fork}";
        const string settings1 = $"remote.{fork}.color\n#cccccc";
        const string settings2 = $"{settings1}\0remote.{origin}.color\n#efefef";

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", remotes))
        {
            using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
            using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", settings1);
            using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", settings1);
            GitVersion.ResetVersion();

            FrozenDictionary<string, Color> colors = _gitModule.GetRemoteColors();

            colors.Count.Should().Be(1);
            colors[fork].Should().Be(Color.FromArgb(204, 204, 204));

            // "Color" another remote
            using IDisposable configListEffective2 = _executable.StageOutput("config list --includes --null", settings2);
            using IDisposable configListLocal2 = _executable.StageOutput("config list --local --includes --null", settings2);
            _gitModule.InvalidateGitSettings();
            _gitModule.GetEffectiveSetting("reload now");
            _gitModule.GetSettings("reload local settings, too");

            // Call again to check if cached colors are returned
            FrozenDictionary<string, Color> colors1 = _gitModule.GetRemoteColors();

            colors1.Count.Should().Be(1);
            colors1[fork].Should().Be(Color.FromArgb(204, 204, 204));
        }
    }

    [Test]
    public void GetRemoteColors_should_reload_colors_after_reset()
    {
        const string origin = nameof(origin);
        const string fork = nameof(fork);
        const string remotes = $"{origin}\n{fork}";
        const string settings1 = $"remote.{fork}.color\n#cccccc";
        const string settings2 = $"{settings1}\0remote.{origin}.color\n#efefef";

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", remotes))
        {
            using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
            using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", settings1);
            using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", settings1);
            GitVersion.ResetVersion();

            FrozenDictionary<string, Color> colors = _gitModule.GetRemoteColors();

            colors.Count.Should().Be(1);
            colors[fork].Should().Be(Color.FromArgb(204, 204, 204));

            // "Color" another remote
            using IDisposable configListEffective2 = _executable.StageOutput("config list --includes --null", settings2);
            using IDisposable configListLocal2 = _executable.StageOutput("config list --local --includes --null", settings2);
            _gitModule.InvalidateGitSettings();
            _gitModule.GetEffectiveSetting("reload now");
            _gitModule.GetSettings("reload local settings, too");

            // Call again to check if cached colors are returned
            FrozenDictionary<string, Color> colors1 = _gitModule.GetRemoteColors();

            colors1.Count.Should().Be(1);

            // Reset the colors
            _gitModule.ResetRemoteColors();

            _gitModule.GetTestAccessor().RemoteColors.Should().BeNull();
        }

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", remotes))
        {
            // Reload the colors
            FrozenDictionary<string, Color> colors2 = _gitModule.GetRemoteColors();

            colors2.Count.Should().Be(2);
            colors2[fork].Should().Be(Color.FromArgb(204, 204, 204)); // "#cccccc"
            colors2[origin].Should().Be(Color.FromArgb(239, 239, 239)); // "#efefef"
        }
    }

    [Test]
    public void ResetRemoteColors_should_clear_cached_colors()
    {
        const string origin = nameof(origin);
        const string fork = nameof(fork);
        const string remotes = $"{origin}\n{fork}";
        const string settings1 = $"remote.{fork}.color\n#cccccc";

        // Mock IGitModule.GetRemoteNames() call
        using (_executable.StageOutput("remote", remotes))
        {
            using IDisposable gitVersion = _executable.StageOutput("--version", "git version 2.46.0");
            using IDisposable configListEffective = _executable.StageOutput("config list --includes --null", settings1);
            using IDisposable configListLocal = _executable.StageOutput("config list --local --includes --null", settings1);
            GitVersion.ResetVersion();

            FrozenDictionary<string, Color> colors = _gitModule.GetRemoteColors();

            colors.Count.Should().Be(1);
            _gitModule.GetTestAccessor().RemoteColors.Count.Should().Be(1);

            _gitModule.ResetRemoteColors();

            _gitModule.GetTestAccessor().RemoteColors.Should().BeNull();
        }
    }
}
