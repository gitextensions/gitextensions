using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
public sealed class NavigationDialogTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        WinFormsShims.ShimHost.Clipboard = new StubClipboard();
    }

    [AvaloniaTest]
    public void Navigation_views_should_construct_through_their_designer_boundaries()
    {
        FormGoToCommit goToCommit = new();
        FormCheckoutRevision checkoutRevision = new();
        SearchControl searchControl = new();
        SearchWindow searchWindow = new();

        goToCommit.FindControl<TextBox>("textboxCommitExpression").Should().NotBeNull();
        goToCommit.FindControl<ComboBox>("comboBoxTags").Should().NotBeNull();
        checkoutRevision.FindControl<GitUI.UserControls.CommitPickerSmallControl>("commitPickerSmallControl1").Should().NotBeNull();
        checkoutRevision.FindControl<CheckBox>("Force").Should().NotBeNull();
        searchControl.FindControl<TextBox>("txtSearchBox").Should().NotBeNull();
        searchControl.FindControl<ListBox>("listBoxSearchResult").Should().NotBeNull();
        searchWindow.FindControl<TextBlock>("lblEnterFileName")!.Text.Should().Be("Enter File Name");
    }

    [AvaloniaTest]
    public void Navigation_dialogs_should_render_their_complete_minimum_layouts()
    {
        (IGitUICommands commands, _) = CreateCommands();
        Window[] forms =
        [
            new FormGoToCommit(commands) { Width = 620, Height = 340 },
            new FormCheckoutRevision(commands) { Width = 460, Height = 125 },
        ];

        foreach (Window form in forms)
        {
            form.Show();
            try
            {
                Dispatcher.UIThread.RunJobs();
                form.CaptureRenderedFrame().Should().NotBeNull();
                form.GetVisualDescendants().OfType<Button>().Should().Contain(button => button.Bounds.Width > 0);
            }
            finally
            {
                form.Close();
            }
        }
    }

    [AvaloniaTest]
    public void SearchControl_should_limit_candidates_select_the_first_and_report_its_size()
    {
        Avalonia.Size reportedSize = default;
        SearchControl<string> search = new(_ => [], size => reportedSize = size);
        IEnumerable<string> candidates = Enumerable.Range(1, 25).Select(index => $"src/File{index:00}.cs");

        Invoke(search, "SearchForCandidates", candidates);

        ListBox results = search.FindControl<ListBox>("listBoxSearchResult")!;
        results.ItemCount.Should().Be(20);
        results.SelectedIndex.Should().Be(0);
        results.IsVisible.Should().BeTrue();
        reportedSize.Width.Should().BeGreaterThanOrEqualTo(300);
    }

    [AvaloniaTest]
    public void SearchControl_should_commit_the_selected_item_and_raise_the_original_event()
    {
        SearchControl<string> search = new(_ => [], _ => { });
        Invoke(search, "SearchForCandidates", (object)new[] { "src/App.cs", "src/Program.cs" });
        bool entered = false;
        search.OnTextEntered += () => entered = true;

        Invoke(search, "ItemSelectedFromList");

        search.Text.Should().Be("src/App.cs");
        entered.Should().BeTrue();
        search.FindControl<ListBox>("listBoxSearchResult")!.IsVisible.Should().BeFalse();
    }

    [TestCase("app.cs", "src/App.cs", true)]
    [TestCase("APP.CS", "src/App.cs", true)]
    [TestCase("missing", "src/App.cs", false)]
    public void FindFilePredicate_should_match_portable_paths_case_insensitively(string pattern, string candidate, bool expected)
    {
        FindFilePredicateProvider provider = new();

        provider.Get(pattern, "/work/repository")(candidate).Should().Be(expected);
    }

    [Test]
    public void FindFilePredicate_should_strip_an_absolute_working_directory_prefix()
    {
        FindFilePredicateProvider provider = new();

        provider.Get("/work/repository/src", "/work/repository")("src/App.cs").Should().BeTrue();
        provider.Get("/work/repository/src", "/work/repository")("tests/AppTests.cs").Should().BeFalse();
    }

    private static void Invoke(object target, string methodName, params object[] arguments)
    {
        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"{target.GetType().Name}.{methodName} was not found.");
        method.Invoke(target, arguments);
    }

    private static (IGitUICommands Commands, IGitModule Module) CreateCommands()
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.GetRefs(Arg.Any<RefsFilter>()).Returns([]);

        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        return (commands, module);
    }

    private sealed class StubClipboard : WinFormsShims.IClipboard
    {
        public bool ContainsText() => false;

        public string GetText() => string.Empty;

        public void SetText(string value)
        {
        }
    }
}
