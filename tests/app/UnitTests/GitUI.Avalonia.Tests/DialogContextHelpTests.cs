using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.Compat;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
public sealed class DialogContextHelpTests
{
    private static IEnumerable<TestCaseData> ManualDialogs()
    {
        (string Name, string Section)[] dialogs =
        [
            ("FormCherryPick", "modify_history.html#cherry-pick-commit"),
            ("FormClone", "getting_started.html#clone-repository"),
            ("FormInit", "getting_started.html#create-new-repository"),
            ("FormCheckoutBranch", "branches.html#checkout-branch"),
            ("FormCreateBranch", "branches.html#create-branch"),
            ("FormDeleteBranch", "branches.html#delete-branch"),
            ("FormDeleteRemoteBranch", "branches.html#delete-branch"),
        ];
        foreach ((string name, string section) in dialogs)
        {
            yield return new TestCaseData(name, section, false);
            yield return new TestCaseData(name, section, true);
        }
    }

    [AvaloniaTest]
    [TestCaseSource(nameof(ManualDialogs))]
    public async Task Original_dialog_help_should_open_its_configured_section(string name, string section, bool keyboard)
    {
        await WithDialogAsync(name, configure: null, async (form, portal) =>
        {
            Button help = form.GetVisualDescendants().OfType<Button>().Should()
                .ContainSingle(button => button.Name == "HelpButton").Subject;
            help.IsVisible.Should().BeTrue();
            help.Bounds.Width.Should().BeGreaterThan(0);
            Control footer = form.FindControl<Control>("ControlsPanel")!;
            Rect helpBounds = new(help.TranslatePoint(default, form)!.Value, help.Bounds.Size);
            foreach (Button action in footer.GetVisualDescendants().OfType<Button>().Where(button => button != help && button.IsEffectivelyVisible))
            {
                Rect actionBounds = new(action.TranslatePoint(default, form)!.Value, action.Bounds.Size);
                helpBounds.Intersects(actionBounds).Should().BeFalse("context help must not cover an original action");
            }

            int helpEvents = 0;
            EventInfo helpEvent = form.GetType().GetEvent("HelpButtonClicked")!;
            helpEvent.Should().NotBeNull();
            helpEvent.AddEventHandler(form, (EventHandler<CancelEventArgs>)((_, _) => helpEvents++));
            bool eventBeforeLaunch = false;
            portal.When(instance => instance.TryLaunchAsync(Arg.Any<string>(), WinFormsShims.OsShellLaunchKind.OpenUri))
                .Do(_ => eventBeforeLaunch = helpEvents == 1);
            InvokeHelp(form, help, keyboard);
            Dispatcher.UIThread.RunJobs();
            helpEvents.Should().Be(1);
            eventBeforeLaunch.Should().BeTrue();
            await portal.Received(1).TryLaunchAsync(AppSettings.DocumentationBaseUrl + section, WinFormsShims.OsShellLaunchKind.OpenUri);
            form.IsVisible.Should().BeTrue();
        });
    }

    [AvaloniaTest]
    [TestCase(null, "modify_history", false)]
    [TestCase("cherry-pick-commit", " ", false)]
    [TestCase(null, "modify_history", true)]
    [TestCase("cherry-pick-commit", " ", true)]
    public async Task Invalid_documentation_section_should_hide_help_without_launching(string? anchor, string? folder, bool keyboard)
    {
        await WithDialogAsync("FormCherryPick", form =>
        {
            form.ManualSectionAnchorName = anchor;
            form.ManualSectionSubfolder = folder;
        }, async (form, portal) =>
        {
            Button help = form.GetVisualDescendants().OfType<Button>().Should()
                .ContainSingle(button => button.Name == "HelpButton").Subject;
            InvokeHelp(form, help, keyboard);
            Dispatcher.UIThread.RunJobs();
            help.IsVisible.Should().BeFalse();
            form.GetType().GetProperty("HelpButton")!.GetValue(form).Should().Be(false);
            await portal.DidNotReceiveWithAnyArgs().TryLaunchAsync(default!, default);
            form.IsVisible.Should().BeTrue();
        });
    }

    [AvaloniaTest]
    public async Task Footer_help_should_reuse_the_original_German_translation_key()
    {
        await WithDialogAsync("FormCherryPick", configure: null, (form, _) =>
        {
            Button help = form.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "HelpButton");
            AutomationProperties.GetName(help).Should().Be("Hilfe");
            ToolTip.GetTip(help).Should().Be("Hilfe (F1)");
            return Task.CompletedTask;
        }, translation: "German");
    }

    private static void InvokeHelp(Window form, Button help, bool keyboard)
    {
        if (keyboard)
        {
            form.KeyPress(Key.F1, RawInputModifiers.None, PhysicalKey.F1, null);
        }
        else
        {
            Point point = help.TranslatePoint(new Point(help.Bounds.Width / 2, help.Bounds.Height / 2), form)!.Value;
            form.MouseDown(point, MouseButton.Left);
            form.MouseUp(point, MouseButton.Left);
        }
    }

    private static async Task WithDialogAsync(string name, Action<GitExtensionsDialog>? configure,
        Func<GitExtensionsDialog, IXdgDesktopPortal, Task> check, string? translation = null)
    {
        AvaloniaSynchronizationContext.InstallIfNeeded();
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        using AppSettingsTestScope settings = new();
        using TranslationTestScope translationScope = translation is null ? new() : new(translation);

        using ServiceContainer services = new();
        GitExtensions.ServiceContainerRegistry.RegisterServices(services);
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.ContextHelp-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        FieldInfo[] fields = typeof(WinFormsShims.ShimHost).GetFields(BindingFlags.Static | BindingFlags.NonPublic);
        object?[] previousServices = fields.Select(field => field.GetValue(null)).ToArray();
        var previousExecutable = OsShellUtil.TestAccessor.MockExecutable;
        GitExtensionsDialog? form = null;
        try
        {
            OsShellUtil.TestAccessor.MockExecutable = null;
            GitModule module = new(services.GetRequiredService<IGitExecutorProvider>(), directory);
            module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "--initial-branch=main" }).Should().BeTrue();
            module.SetSetting("user.name", "Avalonia Test");
            module.SetSetting("user.email", "avalonia@example.com");
            File.WriteAllText(Path.Combine(directory, "file.txt"), "content\n");
            module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "file.txt" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", "initial" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", "origin", "https://example.invalid/repository.git" }).Should().BeTrue();
            module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { "refs/remotes/origin/main", "HEAD" }).Should().BeTrue();
            IXdgDesktopPortal portal = Substitute.For<IXdgDesktopPortal>();
            portal.TryLaunchAsync(Arg.Any<string>(), WinFormsShims.OsShellLaunchKind.OpenUri).Returns(true);
            ShimServices.Install(Substitute.For<IClassicDesktopStyleApplicationLifetime>(), portal, () => true);
            WinFormsShims.ShimHost.OsShell.Should().BeOfType<AvaloniaOsShell>();
            GitUICommands commands = new(services, module);
            var revision = module.GetRevision(module.GetCurrentCheckout(), shortFormat: true, loadRefs: false);
            form = name switch
            {
                "FormCherryPick" => new FormCherryPick(commands, revision),
                "FormClone" => new FormClone(commands, null, openedFromProtocolHandler: false, gitModuleChanged: null),
                "FormInit" => new FormInit(commands, Path.Combine(directory, "new"), gitModuleChanged: null),
                "FormCheckoutBranch" => new FormCheckoutBranch(commands, "main", remote: false),
                "FormCreateBranch" => new FormCreateBranch(commands, revision.ObjectId),
                "FormDeleteBranch" => new FormDeleteBranch(commands, ["main"]),
                "FormDeleteRemoteBranch" => new FormDeleteRemoteBranch(commands, "origin/main"),
                _ => throw new ArgumentOutOfRangeException(nameof(name)),
            };
            configure?.Invoke(form);
            form.Show();
            Dispatcher.UIThread.RunJobs();
            using var frame = form.CaptureRenderedFrame();
            await check(form, portal);
        }
        finally
        {
            try
            {
                if (form is FormDeleteRemoteBranch)
                {
                    TaskManager manager = (TaskManager)typeof(FormDeleteRemoteBranch).GetField("_taskManager", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
                    using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
                    await manager.JoinPendingOperationsAsync(deadline.Token);
                    deadline.Token.ThrowIfCancellationRequested();
                }

                form?.Close();
                (form as IDisposable)?.Dispose();
            }
            finally
            {
                OsShellUtil.TestAccessor.MockExecutable = previousExecutable;
                for (int index = 0; index < fields.Length; index++)
                {
                    fields[index].SetValue(null, previousServices[index]);
                }

                TestDirectory.Delete(directory);
            }
        }
    }
}
