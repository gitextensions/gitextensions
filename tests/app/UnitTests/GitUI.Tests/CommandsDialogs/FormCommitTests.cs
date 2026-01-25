using System.ComponentModel.Design;
using FluentAssertions;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using NSubstitute;

namespace GitUITests.CommandsDialogs;

[TestFixture]
[Apartment(ApartmentState.STA)]
public class FormCommitTests
{
    private IGitModule _gitModule = Substitute.For<IGitModule>();
    private FormCommit _formCommit;
    [SetUp]
    public void Setup()
    {
        ServiceContainer serviceContainer = new();
        serviceContainer.AddService(Substitute.For<GitUI.Hotkey.IHotkeySettingsManager>());
        serviceContainer.AddService(Substitute.For<ResourceManager.IHotkeySettingsLoader>());
        serviceContainer.AddService(Substitute.For<GitUI.ScriptsEngine.IScriptsRunner>());
        GitUICommands commands = new(serviceContainer, _gitModule);
        _formCommit = new FormCommit(commands);
    }

    [TestCase("Commit message")]
    [TestCase("Commit message ? () {} []")]
    [TestCase("Commit message ?? (()) {{}} [[]]")]
    [TestCase("Name: {{()-(.*)}}")]
    [TestCase("Name: {{([A-Z]+-\\d+)-(.*)}}[2]")]
    public void RegexReplaceDisabledTest(string msgToReplace)
    {
        _formCommit.GetTestAccessor().ReplaceMessage(msgToReplace, regexEnabled: false);
        _formCommit.GetTestAccessor().Message.Text.Should().Be(msgToReplace);
    }

    [TestCase("Commit message")]
    [TestCase("Commit message begin ? () {} [] end")]
    [TestCase("Commit message begin {}[] end")]
    [TestCase("Commit message ?? (()) [[]] end")]
    [TestCase("Commit message {{ } end")]
    [TestCase("Commit message { }} end")]
    [TestCase("{ } end")]
    public void RegexReplaceEnabledShouldNotChange(string msgToReplace)
    {
        _formCommit.GetTestAccessor().ReplaceMessage(msgToReplace, regexEnabled: true);
        _formCommit.GetTestAccessor().Message.Text.Should().Be(msgToReplace);
    }

    [TestCase("Commit message {{}}", "Commit message ")]
    [TestCase("Commit message {{}} end", "Commit message  end")]
    [TestCase("Commit message{{}}end", "Commit messageend")]
    [TestCase("Commit message {{.*}}", "Commit message ")]
    public void RegexReplaceEnabledShouldChangeEmptyBranchName(string msgToReplace, string expectedValue)
    {
        _formCommit.GetTestAccessor().ReplaceMessage(msgToReplace, regexEnabled: true);
        _formCommit.GetTestAccessor().Message.Text.Should().Be(expectedValue);
    }

    [TestCase("master", "Message:{{(.*)}} end", "Message:master end")]
    [TestCase("master", "Message:{{.*}} end", "Message: end")] // no group
    [TestCase("master", @"Name: {{([A-Z]+-\d+)-(.*)}}", "Name: ")] // no matching
    [TestCase("feature/ABC-4587-commitMessageRegex", @"{{([A-Z]+-\d+)}}: My message, issue:{{[A-Z]+-(\d+)}}", "ABC-4587: My message, issue:4587")] // multiple regex
    [TestCase("feature/ABC-4587-commitMessageRegex", @"Name: {{([A-Z]+-\d+)-(.*)}}[2], issue: {{([A-Z]+-\d+)-(.*)}}[1]", "Name: commitMessageRegex, issue: ABC-4587")] // regex indexing
    [TestCase("feature/ABC-4587-commitMessageRegex", @"{{([A-Z]+-\d+)-(.*)}}: My message", "ABC-4587: My message")] // default index is 1
    [TestCase("feature/ABC-4587-commitMessageRegex", @"Commit from:{{^feature/(.*)$}}[2] branch", "Commit from: branch")] // overindexing
    [TestCase("feature/ABC-4587-commitMessageRegex", @"Commit from:{{^feature/(.*)$}} branch", "Commit from:ABC-4587-commitMessageRegex branch")]
    public void RegexReplaceEnabledShouldChangeBasedOnBranchName(string branch, string msgToReplace, string expectedValue)
    {
        _gitModule.GetSelectedBranch().Returns(branch);
        _formCommit.GetTestAccessor().ReplaceMessage(msgToReplace, regexEnabled: true);
        _formCommit.GetTestAccessor().Message.Text.Should().Be(expectedValue);
    }
}
