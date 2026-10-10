using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class UpstreamDialogSyncTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    [TestCase("escape")]
    [TestCase("close")]
    public void TaskDialog_AllowCancel_should_return_Cancel_without_turning_a_checked_verification_into_No(string dismissal)
    {
        TaskDialogPage page = new()
        {
            Caption = "Cancelable source confirmation",
            AllowCancel = true,
            DefaultButton = TaskDialogButton.Yes,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
            Verification = new TaskDialogVerificationCheckBox { Text = "Do not ask again" },
        };
        using UpstreamTaskDialogObserver observer = new(window =>
        {
            Button[] buttons = window.GetVisualDescendants().OfType<Button>().ToArray();
            buttons.Single(button => button.IsDefault).Content.Should().BeOfType<AccessText>()
                .Which.Text.Should().Be(TaskDialogButton.Yes.Text);
            buttons.Should().OnlyContain(button => !button.IsCancel);
            window.GetVisualDescendants().OfType<CheckBox>().Single().IsChecked = true;
            if (dismissal == "escape")
            {
                window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null);
            }
            else
            {
                window.Close();
            }
        });

        TaskDialog.ShowDialog(owner: null, page).Should().BeSameAs(TaskDialogButton.Cancel);
        observer.Count.Should().Be(1);
        observer.Failure.Should().BeNull();
        page.Verification.Checked.Should().BeTrue();
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public void TaskDialog_should_preserve_explicit_Cancel_and_the_nonopted_No_escape_route(bool explicitCancel)
    {
        TaskDialogPage page = new() { Caption = "Existing source confirmation", AllowCancel = false };
        page.Buttons.Add(TaskDialogButton.Yes);
        page.Buttons.Add(explicitCancel ? TaskDialogButton.Cancel : TaskDialogButton.No);
        using UpstreamTaskDialogObserver observer = new(window =>
            window.KeyPress(Key.Escape, RawInputModifiers.None, PhysicalKey.Escape, keySymbol: null));

        TaskDialog.ShowDialog(owner: null, page).Should().BeSameAs(explicitCancel ? TaskDialogButton.Cancel : TaskDialogButton.No);
        observer.Count.Should().Be(1);
        observer.Failure.Should().BeNull();
    }

    [AvaloniaTest]
    [TestCase(true)]
    [TestCase(false)]
    public void TaskDialog_AllowCancel_should_keep_the_actual_Yes_or_No_button_identity(bool answerYes)
    {
        TaskDialogPage page = new()
        {
            Caption = "Source yes/no confirmation",
            AllowCancel = true,
            Buttons = { TaskDialogButton.Yes, TaskDialogButton.No },
        };
        using UpstreamTaskDialogObserver observer = new(window => UpstreamTaskDialogObserver.Click(window, answerYes ? "Yes" : "No"));

        TaskDialog.ShowDialog(owner: null, page).Should().BeSameAs(answerYes ? TaskDialogButton.Yes : TaskDialogButton.No);
        observer.Count.Should().Be(1);
        observer.Failure.Should().BeNull();
    }

    [AvaloniaTest]
    public void WorktreeDelete_should_refuse_a_main_worktree_before_scripts_notifications_or_directory_deletion()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.MainWorktreeProtection-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(directory, ".git"));
        string marker = Path.Combine(directory, "repository-sentinel.txt");
        File.WriteAllText(marker, "must survive");
        try
        {
            IGitModule module = Substitute.For<IGitModule>();
            GitUICommands commands = new(Substitute.For<IServiceProvider>(), module);
            int notifications = 0;
            commands.PostRepositoryChanged += (_, _) => notifications++;
            using UpstreamTaskDialogObserver observer = new(window =>
            {
                window.Title.Should().Be(TranslatedStrings.DeleteWorktreeCaption);
                window.GetVisualDescendants().OfType<SelectableTextBlock>().Should()
                    .Contain(block => block.Text == string.Format(TranslatedStrings.DeleteMainWorktreeRefused, directory));
                window.Close();
            });

            commands.WorktreeDelete(owner: null, directory).Should().BeFalse();

            observer.Count.Should().Be(1);
            observer.Failure.Should().BeNull();
            Directory.Exists(Path.Combine(directory, ".git")).Should().BeTrue();
            File.ReadAllText(marker).Should().Be("must survive");
            notifications.Should().Be(0);
            module.ReceivedCalls().Should().BeEmpty();
        }
        finally
        {
            TestDirectory.Delete(directory);
        }
    }

    [AvaloniaTest]
    [TestCase("comboBoxTags", "_selectedTag")]
    [TestCase("comboBoxBranches", "_selectedBranch")]
    public void FormGoToCommit_should_use_the_entering_revision_supplier_before_framework_focus_is_published(string controlName, string selectionField)
    {
        const string revisionId = "1111111111111111111111111111111111111111";
        IGitModule module = Substitute.For<IGitModule>();
        module.RevParse(revisionId).Returns(ObjectId.Parse(revisionId));
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        IGitRef selectedRef = Substitute.For<IGitRef>();
        selectedRef.Guid.Returns(revisionId);
        FormGoToCommit form = new(commands);
        try
        {
            FieldInfo field = typeof(FormGoToCommit).GetField(selectionField, BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("The source selected-ref field is absent.");
            field.SetValue(form, selectedRef);
            ComboBox control = form.FindControl<ComboBox>(controlName)
                ?? throw new InvalidOperationException("The source revision selector is absent.");
            control.IsKeyboardFocusWithin.Should().BeFalse();

            control.RaiseEvent(new FocusChangedEventArgs(InputElement.GotFocusEvent));

            form.ValidateAndGetSelectedObjectId().Should().Be(ObjectId.Parse(revisionId));
            module.Received(1).RevParse(revisionId);
        }
        finally
        {
            form.Close();
        }
    }
}

internal sealed class UpstreamTaskDialogObserver : IDisposable
{
    private readonly IDisposable _subscription;

    public UpstreamTaskDialogObserver(Action<Window> onOpened)
    {
        _subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) =>
        {
            if (!window.Classes.Contains("gitextensions-task-dialog"))
            {
                return;
            }

            Count++;
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    onOpened(window);
                }
                catch (Exception ex)
                {
                    Failure = ex;
                    window.Close();
                }
            });
        });
    }

    public int Count { get; private set; }

    public Exception? Failure { get; private set; }

    public static void Click(Window window, string caption)
    {
        Button button = window.GetVisualDescendants().OfType<Button>()
            .Single(button => button.Content is AccessText accessText && accessText.Text == caption);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    public void Dispose() => _subscription.Dispose();
}
