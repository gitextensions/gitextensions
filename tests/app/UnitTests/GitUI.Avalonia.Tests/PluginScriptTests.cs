using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Plugins;
using GitUI;
using GitUI.ScriptsEngine;
using NSubstitute;

namespace GitExtensionsTests;

[NonParallelizable]
public class PluginScriptTests
{
    [Test]
    public void Plugin_script_should_execute_loaded_plugin_and_notify_repository_change()
    {
        IGitPlugin plugin = Substitute.For<IGitPlugin>();
        plugin.Name.Returns("Portable Plugin");
        plugin.Execute(Arg.Any<GitUIEventArgs>()).Returns(true);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(Substitute.For<IGitModule>());
        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        commands.RepoChangedNotifier.Returns(notifier);
        ScriptInfo script = new()
        {
            Name = "Run portable plugin",
            Command = "{plugin:portable plugin}",
        };

        lock (PluginRegistry.Plugins)
        {
            PluginRegistry.Plugins.Add(plugin);
        }

        try
        {
            bool result = ScriptsManager.ScriptRunner.RunScript(
                script,
                owner: null!,
                commands,
                Substitute.For<IScriptOptionsProvider>());

            result.Should().BeTrue();
            plugin.Received(1).Execute(Arg.Is<GitUIEventArgs>(args => args.GitUICommands == commands));
            notifier.Received(1).Notify();
        }
        finally
        {
            lock (PluginRegistry.Plugins)
            {
                PluginRegistry.Plugins.Remove(plugin);
            }
        }
    }
}
