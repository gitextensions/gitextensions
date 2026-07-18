using System.ComponentModel.Design;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using GitUI.ConsoleEmulation;
using GitUI.Models;
using GitUI.UserControls;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class ConsoleAndOutputTests
{
    private int _originalOutputHistoryDepth;
    private bool _originalOutputHistoryPanelVisible;
    private bool _originalShowConsoleTab;
    private bool _originalShowOutputHistoryAsTab;
    private string? _originalSerializedHotkeys;

    [SetUp]
    public void SetUp()
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();

        _originalOutputHistoryDepth = AppSettings.OutputHistoryDepth.Value;
        _originalOutputHistoryPanelVisible = AppSettings.OutputHistoryPanelVisible.Value;
        _originalShowConsoleTab = AppSettings.ShowConEmuTab.Value;
        _originalShowOutputHistoryAsTab = AppSettings.ShowOutputHistoryAsTab.Value;
        _originalSerializedHotkeys = AppSettings.SerializedHotkeys;

        AppSettings.OutputHistoryDepth.Value = 3;
        AppSettings.OutputHistoryPanelVisible.Value = false;
        AppSettings.ShowConEmuTab.Value = true;
        AppSettings.ShowOutputHistoryAsTab.Value = true;
        AppSettings.SerializedHotkeys = string.Empty;
    }

    [TearDown]
    public void TearDown()
    {
        AppSettings.OutputHistoryDepth.Value = _originalOutputHistoryDepth;
        AppSettings.OutputHistoryPanelVisible.Value = _originalOutputHistoryPanelVisible;
        AppSettings.ShowConEmuTab.Value = _originalShowConsoleTab;
        AppSettings.ShowOutputHistoryAsTab.Value = _originalShowOutputHistoryAsTab;
        AppSettings.SerializedHotkeys = _originalSerializedHotkeys!;
    }

    [AvaloniaTest]
    public async Task Plain_text_console_should_run_the_platform_shell_and_accept_input()
    {
        using ServiceContainer serviceContainer = CreateServiceContainer();
        IConsoleEmulatorsRegistry registry = serviceContainer.GetRequiredService<IConsoleEmulatorsRegistry>();
        registry.AvailableConsoleEmulators.Should().ContainSingle();

        IConsoleShellRunner shell = registry.CreateShellRunner()
            ?? throw new InvalidOperationException("The platform shell was not available.");
        Window host = new() { Content = shell.Control };
        try
        {
            host.Show();
            Dispatcher.UIThread.RunJobs();
            shell.StartShell(Path.GetTempPath());
            TextBox input = shell.Control.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "ConsoleInput");
            TextBox output = shell.Control.GetVisualDescendants().OfType<TextBox>().Single(control => control.Name == "ConsoleOutput");
            input.Text = "echo gitextensions-console-ready";
            input.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.Enter,
            });

            await WaitUntilAsync(() => output.Text?.Contains("gitextensions-console-ready", StringComparison.Ordinal) is true);
            shell.IsShellRunning.Should().BeTrue();
            input.Text.Should().BeEmpty();
        }
        finally
        {
            host.Close();
            (shell as IDisposable)?.Dispose();
        }
    }

    [AvaloniaTest]
    public void Output_history_service_should_format_notify_and_clear_history()
    {
        using ServiceContainer serviceContainer = CreateServiceContainer();
        IOutputHistoryProvider provider = serviceContainer.GetRequiredService<IOutputHistoryProvider>();
        IOutputHistoryRecorder recorder = serviceContainer.GetRequiredService<IOutputHistoryRecorder>();
        int notifications = 0;
        provider.HistoryChanged += (_, _) => notifications++;

        recorder.RecordHistory(new RunProcessInfo(
            "git",
            "status",
            "\u001b[31mfailed\u001b[0m",
            new DateTime(2026, 7, 18, 12, 34, 0)));

        provider.Enabled.Should().BeTrue();
        provider.History.Should().Contain("git status").And.Contain("failed").And.NotContain("\u001b[");
        notifications.Should().Be(1);

        provider.ClearHistory();
        provider.History.Should().Be($"###{Environment.NewLine}");
        notifications.Should().Be(2);
    }

    [AvaloniaTest]
    public void Output_history_control_should_reuse_the_existing_translation_keys()
    {
        OutputHistoryControl control = new();
        ITranslation translation = Substitute.For<ITranslation>();
        translation.TranslateItem(
                nameof(OutputHistoryControl),
                "tsmiCopy",
                "Text",
                Arg.Any<Func<string?>>())
            .Returns("&Copy translated");

        control.AddTranslationItems(translation);
        control.TranslateItems(translation);

        translation.Received(1).AddTranslationItem(nameof(OutputHistoryControl), "tsmiCopy", "Text", "&Copy");
        translation.Received(1).AddTranslationItem(nameof(OutputHistoryControl), "tsmiClear", "Text", "C&lear");
        control.tsmiCopy.Header.Should().Be("_Copy translated");
    }

    [AvaloniaTest]
    public void FormBrowse_should_host_console_and_output_tabs_and_refresh_output()
    {
        using ServiceContainer serviceContainer = CreateServiceContainer();
        FormBrowse form = CreateForm(serviceContainer);
        try
        {
            form.Show();
            TabItem consoleTab = form.CommitInfoTabControl.Items
                .OfType<TabItem>()
                .Single(tab => Equals(tab.Header, "Console"));
            TabItem outputTab = form.CommitInfoTabControl.Items
                .OfType<TabItem>()
                .Single(tab => tab.Name == "OutputHistoryTab");
            OutputHistoryControl outputControl = (OutputHistoryControl)outputTab.Content!;

            consoleTab.Content.Should().BeNull("the shell is created only when its tab is selected");
            outputTab.Header.Should().Be("Output");

            serviceContainer.GetRequiredService<IOutputHistoryRecorder>().RecordHistory("recorded output");
            Dispatcher.UIThread.RunJobs();
            outputControl.TextBox.Text.Should().Contain("recorded output");

            form.ProcessHotkey(WinFormsShims.Keys.Control | WinFormsShims.Keys.D9).Should().BeTrue();
            Dispatcher.UIThread.RunJobs();
            form.CommitInfoTabControl.SelectedItem.Should().BeSameAs(outputTab);
            outputControl.TextBox.IsKeyboardFocusWithin.Should().BeTrue();
        }
        finally
        {
            form.Close();
        }
    }

    [AvaloniaTest]
    public void FormBrowse_should_toggle_output_panel_when_tab_mode_is_disabled()
    {
        AppSettings.ShowOutputHistoryAsTab.Value = false;
        using ServiceContainer serviceContainer = CreateServiceContainer();
        FormBrowse form = CreateForm(serviceContainer);
        try
        {
            form.Show();
            form.CommitInfoTabControl.Items
                .OfType<TabItem>()
                .Should().NotContain(tab => tab.Name == "OutputHistoryTab");
            form.outputHistoryPanelHost.IsVisible.Should().BeFalse();

            form.ProcessHotkey(WinFormsShims.Keys.Control | WinFormsShims.Keys.D9).Should().BeTrue();
            AppSettings.OutputHistoryPanelVisible.Value.Should().BeTrue();
            form.outputHistoryPanelHost.IsVisible.Should().BeTrue();
            form.mainContentGrid.RowDefinitions[2].Height.Value.Should().BeGreaterThan(0);

            form.ProcessHotkey(WinFormsShims.Keys.Control | WinFormsShims.Keys.D9).Should().BeTrue();
            AppSettings.OutputHistoryPanelVisible.Value.Should().BeFalse();
            form.outputHistoryPanelHost.IsVisible.Should().BeFalse();
            form.mainContentGrid.RowDefinitions[2].Height.Value.Should().Be(0);
        }
        finally
        {
            form.Close();
        }
    }

    private static ServiceContainer CreateServiceContainer()
    {
        ServiceContainer serviceContainer = new();
        GitExtUtils.ServiceContainerRegistry.RegisterServices(serviceContainer);
        GitUI.ServiceContainerRegistry.RegisterServices(serviceContainer);
        return serviceContainer;
    }

    private static FormBrowse CreateForm(ServiceContainer serviceContainer)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(Path.GetTempPath());
        module.IsValidGitWorkingDir().Returns(false);

        IAppTitleGenerator appTitleGenerator = Substitute.For<IAppTitleGenerator>();
        appTitleGenerator.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        serviceContainer.AddService<IAppTitleGenerator>(appTitleGenerator);

        ILockableNotifier notifier = Substitute.For<ILockableNotifier>();
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(notifier);
        commands.GetService(Arg.Any<Type>()).Returns(call => serviceContainer.GetService(call.Arg<Type>()));
        return new FormBrowse(commands);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (!condition() && stopwatch.Elapsed < TimeSpan.FromSeconds(10))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        condition().Should().BeTrue("the shell output should arrive before the timeout");
    }
}
