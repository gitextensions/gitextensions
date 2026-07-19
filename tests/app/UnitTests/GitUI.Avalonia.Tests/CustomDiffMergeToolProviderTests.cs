using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class CustomDiffMergeToolProviderTests
{
    [AvaloniaTest]
    public async Task Provider_should_put_the_default_tool_first_and_route_selected_tools()
    {
        bool showAvailableDiffTools = AppSettings.ShowAvailableDiffTools;
        CustomDiffMergeToolProvider provider = new();
        await provider.ClearAsync(isDiff: true);

        try
        {
            AppSettings.ShowAvailableDiffTools = true;
            IGitModule module = Substitute.For<IGitModule>();
            module.GetEffectiveSetting(Arg.Any<string>(), Arg.Any<string>()).Returns("meld");
            module.GetCustomDiffMergeTools(isDiff: true, Arg.Any<CancellationToken>()).Returns(
                "'git difftool --tool=<tool>' may be set to one of the following:\n\t\tkdiff3\n\t\tmeld\n");

            MenuItem parent = new() { Header = "Open with difftool", InputGesture = new KeyGesture(Key.F3) };
            object? clickedSender = null;
            List<GitUI.CustomDiffMergeTool> menus =
            [
                new(parent, (sender, _) => clickedSender = sender),
            ];

            await provider.GetTestAccessor().LoadCustomDiffMergeToolsAsync(module, menus, isDiff: true);

            MenuItem[] items = parent.Items.OfType<MenuItem>().ToArray();
            items.Should().HaveCount(3, "two tools and the disable command are menu items");
            items[0].Tag.Should().Be("meld", "the configured default tool is first");
            items[0].FontWeight.Should().Be(FontWeight.Bold);
            items[0].InputGesture.Should().Be(parent.InputGesture);
            items[1].Tag.Should().Be("kdiff3");

            items[1].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            clickedSender.Should().BeSameAs(items[1]);

            items[2].RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            AppSettings.ShowAvailableDiffTools.Should().BeFalse();
            parent.Items.Should().BeEmpty();
        }
        finally
        {
            AppSettings.ShowAvailableDiffTools = showAvailableDiffTools;
            await provider.ClearAsync(isDiff: true);
        }
    }

    [Test]
    public void GitUICommands_should_translate_revision_selection_for_the_module_difftool()
    {
        IGitModule module = Substitute.For<IGitModule>();
        GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);
        GitRevision revision = new(ObjectId.Parse("2222222222222222222222222222222222222222"))
        {
            ParentIds = [ObjectId.Parse("1111111111111111111111111111111111111111")],
        };

        commands.OpenWithDifftool(
            owner: null,
            revisions: [revision],
            fileName: "src/file.txt",
            oldFileName: null,
            RevisionDiffKind.DiffAB,
            isTracked: true,
            customTool: "meld");

        module.Received(1).OpenWithDifftool(
            "src/file.txt",
            null,
            "1111111111111111111111111111111111111111",
            "2222222222222222222222222222222222222222",
            isTracked: true,
            customTool: "meld");
    }
}
