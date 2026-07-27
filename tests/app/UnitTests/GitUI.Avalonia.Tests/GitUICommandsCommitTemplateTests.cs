using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using NSubstitute;

namespace GitExtensionsTests;

[NonParallelizable]
public sealed class GitUICommandsCommitTemplateTests
{
    [Test]
    public void GitUICommands_should_register_and_remove_plugin_commit_templates()
    {
        string key = $"Avalonia test {Guid.NewGuid():N}";
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);
        CommitTemplateManager observer = new(() => module);

        try
        {
            commands.AddCommitTemplate(key, () => "Template body", icon: null, isRegex: true);

            CommitTemplateItem template = observer.RegisteredTemplates.Single(item => item.Name == key);
            template.Text.Should().Be("Template body");
            template.IsRegex.Should().BeTrue();
        }
        finally
        {
            commands.RemoveCommitTemplate(key);
        }

        observer.RegisteredTemplates.Should().NotContain(item => item.Name == key);
    }
}
