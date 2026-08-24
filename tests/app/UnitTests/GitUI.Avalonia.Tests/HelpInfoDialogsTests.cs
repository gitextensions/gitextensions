using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Threading;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;

namespace GitExtensionsTests;

[TestFixture]
public sealed class HelpInfoDialogsTests
{
    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
    }

    [AvaloniaTest]
    public void EnvironmentInfo_should_construct_and_show_the_environment_report()
    {
        UserEnvironmentInformation.Initialise("9999999999999999999999999999999999abcdef", isDirty: true);

        EnvironmentInfo control = new();
        EnvironmentInfo.TestAccessor accessor = control.GetTestAccessor();

        accessor.CopyButton.Should().NotBeNull();
        accessor.EnvironmentIssueInfo.Text.Should().Contain("Git Extensions");
    }

    [AvaloniaTest]
    public void EnvironmentInfo_should_match_native_96_dpi_designer_geometry()
    {
        UserEnvironmentInformation.Initialise("9999999999999999999999999999999999abcdef", isDirty: true);
        EnvironmentInfo control = new();
        Window host = new()
        {
            Width = 165,
            Height = 78,
            Content = control,
            SizeToContent = SizeToContent.Manual,
        };

        try
        {
            host.Show();
            Dispatcher.UIThread.RunJobs();

            control.Bounds.Should().Be(new Rect(0, 0, 165, 78));
            AssertBoundsRelativeTo(control.FindControl<Border>("tableLayoutPanel1")!, control, 0, 4, 165, 70);
            AssertBoundsRelativeTo(control.FindControl<TextBlock>("environmentIssueInfo")!, control, 0, 12, 132, 60);
            AssertBoundsRelativeTo(control.FindControl<Button>("copyButton")!, control, 140, 12, 25, 26);
            AssertBoundsRelativeTo(control.FindControl<Border>("lblSeparatorTop")!, control, 0, 4, 165, 2);
            AssertBoundsRelativeTo(control.FindControl<Border>("lblSeparatorBottom")!, control, 0, 72, 165, 2);
        }
        finally
        {
            host.Close();
        }
    }

    [AvaloniaTest]
    public void FormCommandlineHelp_should_construct_and_list_the_commands()
    {
        FormCommandlineHelp form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormCommandlineHelp), "$this", "Text", "Commandline usage");
        translation.Received(1).AddTranslationItem(nameof(FormCommandlineHelp), "label1", "Text", "Supported commandline arguments for\ngitex.cmd / gitex (located in the same folder as GitExtensions.exe):");
    }

    [AvaloniaTest]
    public void FormCommandlineHelp_should_match_native_96_dpi_client_geometry()
    {
        FormCommandlineHelp form = new();

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.ClientSize.Should().Be(new Size(394, 662));
            form.FindControl<StackPanel>("flowLayoutPanel1")!.Bounds.Should().Be(new Rect(0, 0, 394, 662));
            form.FindControl<TextBlock>("label1")!.Bounds.Should().Be(new Rect(3, 0, 373, 52));
            form.FindControl<TextBlock>("_NO_TRANSLATE_commands")!.Bounds.Should().Be(new Rect(3, 52, 382, 649));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormDonate_should_construct_and_emit_its_translation_keys()
    {
        FormDonate form = new();
        ITranslation translation = Substitute.For<ITranslation>();

        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormDonate), "$this", "Text", "Donate");
        translation.Received(1).AddTranslationItem(nameof(FormDonate), "_donateText", "Text", Arg.Any<string>());
        FormDonate.DonationUrl.Should().Be("https://opencollective.com/gitextensions");
    }

    [AvaloniaTest]
    public void FormDonate_should_follow_the_original_auto_size_rows()
    {
        FormDonate form = new();

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.ClientSize.Should().Be(new Size(508, 237));
            Grid table = form.FindControl<Grid>("tableLayoutPanel")!;
            table.Bounds.Should().Be(new Rect(0, 0, 476, 205));
            table.TranslatePoint(default, form).Should().Be(new Point(16, 16));
            form.FindControl<TextBlock>("lblText")!.Bounds.Should().Be(new Rect(10, 10, 456, 105));
            form.FindControl<Border>("pbxDonate")!.Bounds.Should().Be(new Rect(12, 137, 452, 56));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_construct_with_the_original_controls_and_keys()
    {
        FormOpenDirectory form = new();
        FormOpenDirectory.TestAccessor accessor = form.GetTestAccessor();

        accessor.Directory.Should().NotBeNull();
        accessor.OpenButton.Should().NotBeNull();
        accessor.BrowseButton.Should().NotBeNull();
        accessor.GoUpButton.Should().NotBeNull();

        ITranslation translation = Substitute.For<ITranslation>();
        form.AddTranslationItems(translation);

        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "$this", "Text", "Open local repository");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "label1", "Text", "&Directory:");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "Load", "Text", "Open");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "folderBrowserButton", "Text", "&Browse...");
        translation.Received(1).AddTranslationItem(nameof(FormOpenDirectory), "folderGoUpButton", "toolTip1", "Go to parent directory...");
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_match_native_96_dpi_runtime_geometry()
    {
        FormOpenDirectory form = new();

        try
        {
            form.Show();
            Dispatcher.UIThread.RunJobs();

            form.ClientSize.Should().Be(new Size(615, 77));
            form.FindControl<Label>("label1")!.Bounds.Should().Be(new Rect(13, 13, 51, 15));
            form.FindControl<ComboBox>("_NO_TRANSLATE_Directory")!.Bounds.Should().Be(new Rect(85, 9, 360, 23));
            form.FindControl<Button>("folderGoUpButton")!.Bounds.Should().Be(new Rect(448, 7, 26, 25));
            form.FindControl<Button>("folderBrowserButton")!.Bounds.Should().Be(new Rect(477, 7, 135, 25));
            form.FindControl<Button>("Load")!.Bounds.Should().Be(new Rect(448, 39, 164, 25));
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_reject_a_non_repository_path()
    {
        IGitExecutorProvider executorProvider = Substitute.For<IGitExecutorProvider>();
        ILocalRepositoryManager localRepositoryManager = Substitute.For<ILocalRepositoryManager>();

        string missing = Path.Combine(Path.GetTempPath(), $"ge-open-{Guid.NewGuid():N}");

        FormOpenDirectory.TestAccessor.OpenGitRepository(executorProvider, missing, localRepositoryManager)
            .Should().BeNull();
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_preserve_directory_order_and_remove_duplicates()
    {
        string first = Path.Combine(Path.GetTempPath(), "ge-first");
        string second = Path.Combine(Path.GetTempPath(), "ge-second");
        Repository[] history = [new(first), new(first), new(second)];

        IReadOnlyList<string> directories = FormOpenDirectory.TestAccessor.GetDirectories(null, history);

        directories.Should().ContainInOrder(first, second);
        directories.Count(path => path == first).Should().Be(1);
    }

    [AvaloniaTest]
    public void FormOpenDirectory_should_navigate_to_the_parent_and_update_parent_availability()
    {
        FormOpenDirectory form = new();
        FormOpenDirectory.TestAccessor accessor = form.GetTestAccessor();
        string child = Path.Combine(Path.GetTempPath(), "GitExtensions", "nested");

        accessor.Directory.Text = child;
        accessor.GoUp();

        accessor.Directory.Text.Should().Be(new DirectoryInfo(child).Parent!.FullName.EnsureTrailingPathSeparator());

        accessor.Directory.Text = Path.GetTempPath();
        accessor.UpdateDirectoryState();
        accessor.GoUpButton.IsEnabled.Should().BeTrue();

        accessor.Directory.Text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        accessor.UpdateDirectoryState();
        accessor.GoUpButton.IsEnabled.Should().BeFalse();
    }

    private static void AssertBoundsRelativeTo(Control control, Visual relativeTo, double x, double y, double width, double height)
    {
        Point origin = control.TranslatePoint(default, relativeTo)
            ?? throw new InvalidOperationException($"Could not translate {control.Name} into {relativeTo}.");
        new Rect(origin, control.Bounds.Size).Should().Be(new Rect(x, y, width, height));
    }
}
