using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using GitCommands;
using GitCommands.ExternalLinks;
using GitCommands.Git;
using GitCommands.Git.Gpg;
using GitCommands.Settings;
using GitCommands.Submodules;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Plugins.Gource;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI;
using GitUI.Avatars;
using GitUI.Blame;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.CommandsDialogs.Menus;
using GitUI.CommandsDialogs.SettingsDialog;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommandsDialogs.SubmodulesDialog;
using GitUI.CommandsDialogs.WorktreeDialog;
using GitUI.CommitInfo;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.Help;
using GitUI.HelperDialogs;
using GitUI.LeftPanel;
using GitUI.ScriptsEngine;
using GitUI.SpellChecker;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.Settings;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensionsTests;

/// <summary>
///  Discovers every ported AXAML view and, on request, captures a declared parity matrix
///  with headless Skia. PNG and control-tree output are local review artifacts rather than
///  golden test inputs, so normal test runs validate the inventory without rewriting it.
/// </summary>
[TestFixture]
public sealed partial class ParityScreenshotTests
{
    private const string CaptureCategory = "VisualParityCapture";
    private const string CaptureEnvironmentVariable = "GITEXT_CAPTURE_PARITY_SHOTS";
    private const string CaptureViewEnvironmentVariable = "GITEXT_CAPTURE_PARITY_VIEW";
    private const string CaptureRepoTreeContextEnvironmentVariable = "GITEXT_CAPTURE_REPO_TREE_CONTEXT";
    private const string CaptureFileStatusTreeEnvironmentVariable = "GITEXT_CAPTURE_FILE_STATUS_TREE";
    private const string CaptureWorkingDirectoryMenuEnvironmentVariable = "GITEXT_CAPTURE_WORKING_DIRECTORY_MENU";
    // parity-scaffolding: Gives before/after extraction captures identical rendered repository text.
    private const string CaptureDeterministicRepositoryEnvironmentVariable = "GITEXT_CAPTURE_PARITY_DETERMINISTIC_REPOSITORY";
    private const string AxamlExtension = ".axaml";
    private const string AppSourcePath = "src/App.cs";
    private const string FeatureBranchName = "feature/visual-parity";
    private const string MainBranchName = "main";
    private const string RemoteName = "origin";
    private const string InitialCommitSubject = "Add representative repository content";
    private const string HeadCommitSubject = "Establish the Avalonia application shell";

    [GeneratedRegex("x:Class=\"(?<className>[^\"]+)\"")]
    private static partial Regex ClassNameRegex { get; }

    [Test]
    public void Ported_view_inventory_should_resolve_every_AXAML_class()
    {
        IReadOnlyList<ViewDescriptor> views = GetViewDescriptors();

        views.Should().NotBeEmpty();
        views.Select(view => view.ViewType).Should().OnlyHaveUniqueItems();
        views.Should().OnlyContain(view => typeof(Control).IsAssignableFrom(view.ViewType));
        views.Where(view => view.ViewType.GetConstructor(Type.EmptyTypes) is null).Should().BeEmpty();
    }

    [AvaloniaTest]
    [Category(CaptureCategory)]
    public async Task Capture_declared_parity_matrix()
    {
        if (Environment.GetEnvironmentVariable(CaptureEnvironmentVariable) != "1")
        {
            return;
        }

        await CaptureParityPlanAsync();
    }

    private static async Task<ManifestEntry> CaptureViewAsync(
        CaptureContext context,
        ViewDescriptor descriptor,
        string themeName,
        ThemeVariant themeVariant,
        string scaleName,
        double scaleFactor,
        string outputDirectory)
    {
        bool originalAlwaysShowAdvancedOptions = AppSettings.AlwaysShowAdvOpt;
        bool originalRevisionGraphShowArtificialCommits = AppSettings.RevisionGraphShowArtificialCommits;
        Control view = CreateView(context, descriptor.ViewType);
        (double width, double height) = GetCaptureSize(descriptor.ViewType);
        Window window;
        bool isWindow = view is Window;
        if (view is Window viewWindow)
        {
            window = viewWindow;
            window.Width = width;
            window.Height = height;
            window.SizeToContent = SizeToContent.Manual;
        }
        else
        {
            window = new Window
            {
                Title = descriptor.ClassName,
                Width = width,
                Height = height,
                SizeToContent = SizeToContent.Manual,
                Content = view,
            };
        }

        window.RequestedThemeVariant = themeVariant;
        if (descriptor.ViewType == typeof(FormRemotes))
        {
            AppSettings.AlwaysShowAdvOpt = true;
        }
        else if (descriptor.ViewType == typeof(FormChooseCommit))
        {
            AppSettings.RevisionGraphShowArtificialCommits = true;
        }

        try
        {
            PrepareView(view, context);
            window.Show();
            window.SetRenderScaling(scaleFactor);
            if (!isWindow)
            {
                await SeedStandaloneControlAsync(view, context);
            }

            await WaitForAsyncViewsAsync(view);
            Dispatcher.UIThread.RunJobs();
            if (view is FormBrowse formBrowse)
            {
                if (Environment.GetEnvironmentVariable(CaptureWorkingDirectoryMenuEnvironmentVariable) == "1")
                {
                    WorkingDirectoryToolStripSplitButton selector =
                        formBrowse.FindControl<WorkingDirectoryToolStripSplitButton>("_NO_TRANSLATE_WorkingDir")!;
                    selector.GetTestAccessor().ShowDropDown(
                        [
                            new Repository(context.WorkingDirectory)
                            {
                                Anchor = Repository.RepositoryAnchor.AnchoredInTop,
                                Category = "Development",
                            },
                        ],
                        [
                            new Repository(context.WorkingDirectory)
                            {
                                Anchor = Repository.RepositoryAnchor.AnchoredInTop,
                            },
                            new Repository(Path.GetDirectoryName(context.WorkingDirectory)!),
                        ]);
                }
                else
                {
                    formBrowse.commandsToolStripMenuItem.IsSubMenuOpen = true;
                }

                Dispatcher.UIThread.RunJobs();
            }
            else if (view is FileStatusList fileStatusList)
            {
                FileStatusList.TestAccessor accessor = fileStatusList.GetTestAccessor();
                if (accessor.List.IsVisible)
                {
                    accessor.UpdateContextMenu();
                    accessor.ContextMenu.Open(accessor.List);
                    Dispatcher.UIThread.RunJobs();
                }
                else if (accessor.Tree.IsVisible)
                {
                    for (int pass = 0; pass < 3; pass++)
                    {
                        foreach (TreeViewItem item in accessor.Tree.GetVisualDescendants().OfType<TreeViewItem>())
                        {
                            item.IsExpanded = true;
                        }

                        Dispatcher.UIThread.RunJobs();
                    }
                }
            }
            else if (view is RepoObjectsTree repoObjectsTree)
            {
                RepoObjectsTree.TestAccessor accessor = repoObjectsTree.GetTestAccessor();
                if (Environment.GetEnvironmentVariable(CaptureRepoTreeContextEnvironmentVariable) == "submodule")
                {
                    SubmoduleTree submoduleTree = accessor.Tree.Items.Cast<TreeViewItem>()
                        .Select(item => item.Tag)
                        .OfType<SubmoduleTree>()
                        .Single();
                    SubmoduleNode submodule = submoduleTree.DescendantsAndSelf()
                        .OfType<SubmoduleNode>()
                        .First(node => !node.IsCurrent);
                    accessor.Tree.SelectedItem = submodule.TreeViewNode;
                }
                else if (Environment.GetEnvironmentVariable(CaptureRepoTreeContextEnvironmentVariable) == "worktree")
                {
                    WorktreeTree worktreeTree = accessor.Tree.Items.Cast<TreeViewItem>()
                        .Select(item => item.Tag)
                        .OfType<WorktreeTree>()
                        .Single();
                    WorktreeNode worktree = worktreeTree.DescendantsAndSelf()
                        .OfType<WorktreeNode>()
                        .First(node => !node.IsCurrent && !node.Worktree.IsDeleted);
                    accessor.Tree.SelectedItem = worktree.TreeViewNode;
                }
                else
                {
                    TreeViewItem stashRoot = accessor.Tree.Items.Cast<TreeViewItem>().Last();
                    stashRoot.IsExpanded = true;
                    TreeViewItem stashItem = stashRoot.Items.Cast<TreeViewItem>().Single();
                    accessor.Tree.SelectedItem = stashItem;
                }

                accessor.UpdateContextMenu();
                accessor.ContextMenu.Open(accessor.Tree);
                Dispatcher.UIThread.RunJobs();
            }

            WriteableBitmap? frame = window.CaptureRenderedFrame();
            frame.Should().NotBeNull($"{descriptor.ClassName} should render with headless Skia");

            string relativeFile = Path.Combine(themeName, scaleName, descriptor.RelativePathWithoutExtension + ".png");
            string outputPath = Path.Combine(outputDirectory, relativeFile);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            using FileStream stream = File.Create(outputPath);
            frame!.Save(stream, PngBitmapEncoderOptions.Default);

            return new ManifestEntry(
                descriptor.RelativePath,
                descriptor.ClassName,
                themeName,
                scaleName,
                isWindow ? "Window" : "UserControl",
                (int)frame.PixelSize.Width,
                (int)frame.PixelSize.Height,
                relativeFile.Replace(Path.DirectorySeparatorChar, '/'));
        }
        finally
        {
            window.Close();
            if (!ReferenceEquals(window, view) && view is IDisposable disposableView)
            {
                disposableView.Dispose();
            }

            if (window is IDisposable disposableWindow)
            {
                disposableWindow.Dispose();
            }

            AppSettings.AlwaysShowAdvOpt = originalAlwaysShowAdvancedOptions;
            AppSettings.RevisionGraphShowArtificialCommits = originalRevisionGraphShowArtificialCommits;
        }
    }

    private static IEnumerable<string> SearchCandidates(string value)
        => new[] { AppSourcePath, "src/Commands/Checkout.cs", "tests/SearchTests.cs" }
            .Where(candidate => candidate.Contains(value, StringComparison.OrdinalIgnoreCase));

    private static Control CreateView(CaptureContext context, Type viewType)
    {
        if (viewType == typeof(SimplePrompt))
        {
            return new SimplePrompt("Script input", "Branch name", FeatureBranchName);
        }

        if (viewType == typeof(FormFilePrompt))
        {
            FormFilePrompt prompt = new();
            prompt.FindControl<TextBox>("txtFilePath")!.Text = @"C:\source\first.txt C:\source\second.txt";
            return prompt;
        }

        if (viewType == typeof(FormQuickItemSelector))
        {
            FormQuickStringSelector selector = new();
            selector.Init([MainBranchName, FeatureBranchName, "release/5.0", "support/legacy"]);
            return selector;
        }

        if (viewType == typeof(FormSettings))
        {
            return new FormSettings(context.Commands);
        }

        if (viewType == typeof(HotkeysSettingsPage))
        {
            return new HotkeysSettingsPage(context.Commands);
        }

        if (viewType == typeof(SimpleHelpDisplayDialog))
        {
            return new SimpleHelpDisplayDialog
            {
                DialogTitle = "Arguments help",
                ContentText = "Use {option} for normal replacement.\n\nWorking directory:\n{WorkingDir}\n\nSelected revision:\n{sHash}",
            };
        }

        if (viewType == typeof(FormBrowse))
        {
            return new FormBrowse(context.Commands);
        }

        if (viewType == typeof(FormRecentReposSettings))
        {
            return new FormRecentReposSettings(
            [
                new Repository(context.WorkingDirectory)
                {
                    Anchor = Repository.RepositoryAnchor.AnchoredInTop,
                },
                new Repository(Path.GetDirectoryName(context.WorkingDirectory)!),
            ]);
        }

        if (viewType == typeof(FormUpdates))
        {
            FormUpdates form = new(new Version(98, 0));
            form.GetTestAccessor().DisplayReleases(
                """
                [Version "99.1"]
                    ReleaseType = Major
                    DownloadPage = https://github.com/gitextensions/gitextensions/releases/download/v99.1/GitExtensions-x64-99.1.msi
                    NetRuntimeVersion = 99.0.2
                """);
            return form;
        }

        if (viewType == typeof(FormAddFiles))
        {
            return new FormAddFiles(context.Commands);
        }

        if (viewType == typeof(FormCheckoutBranch))
        {
            return new FormCheckoutBranch(context.Commands, FeatureBranchName, remote: false);
        }

        if (viewType == typeof(FormGoToCommit))
        {
            return new FormGoToCommit(context.Commands);
        }

        if (viewType == typeof(FormCheckoutRevision))
        {
            FormCheckoutRevision form = new(context.Commands);
            form.SetRevision(context.HeadRevision.ObjectId.ToString());
            return form;
        }

        if (viewType == typeof(FormDiff))
        {
            return new FormDiff(
                context.Commands,
                context.ParentRevision.ObjectId,
                context.HeadRevision.ObjectId,
                "HEAD~1",
                "HEAD");
        }

        if (viewType == typeof(FormCompareToBranch))
        {
            return new FormCompareToBranch(context.Commands, context.HeadRevision.ObjectId);
        }

        if (viewType == typeof(FormFormatPatch))
        {
            return new FormFormatPatch(context.Commands);
        }

        if (viewType == typeof(SearchControl))
        {
            return new SearchControl<string>(SearchCandidates, _ => { });
        }

        if (viewType == typeof(SearchWindow))
        {
            return new SearchWindow<string>(SearchCandidates);
        }

        if (viewType == typeof(FormCherryPick))
        {
            IGitModule module = Substitute.For<IGitModule>();
            module.WorkingDir.Returns(context.WorkingDirectory);
            module.IsMerge(context.HeadRevision.ObjectId).Returns(true);
            GitRevision secondParent = context.ParentRevision.Clone();
            secondParent.Author = "Second Parent Author";
            secondParent.Subject = "Preserve the second side of the representative merge";
            module.GetParentRevisions(context.HeadRevision.ObjectId).Returns([context.ParentRevision, secondParent]);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.GetService(Arg.Any<Type>()).Returns(call => context.Commands.GetService(call.Arg<Type>()));
            return new FormCherryPick(commands, context.HeadRevision);
        }

        if (viewType == typeof(FormRevertCommit))
        {
            IGitModule module = Substitute.For<IGitModule>();
            module.WorkingDir.Returns(context.WorkingDirectory);
            module.IsMerge(context.HeadRevision.ObjectId).Returns(true);
            GitRevision secondParent = context.ParentRevision.Clone();
            secondParent.Author = "Second Parent Author";
            secondParent.Subject = "Preserve the second side of the representative merge";
            module.GetParentRevisions(context.HeadRevision.ObjectId).Returns([context.ParentRevision, secondParent]);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            commands.GetService(Arg.Any<Type>()).Returns(call => context.Commands.GetService(call.Arg<Type>()));
            return new FormRevertCommit(commands, context.HeadRevision);
        }

        if (viewType == typeof(FormResetCurrentBranch))
        {
            return FormResetCurrentBranch.Create(
                context.Commands,
                context.ParentRevision,
                FormResetCurrentBranch.ResetType.Hard);
        }

        if (viewType == typeof(FormEdit))
        {
            FormEdit form = new();
            form.GetTestAccessor().LoadText(
                "recovered.cs",
                "namespace Recovered;\n\npublic static class LostObject\n{\n    public static string Restore() => \"Recovered content\";\n}\n");
            form.IsReadOnly = true;
            return form;
        }

        if (viewType == typeof(FormEditor))
        {
            FormEditor form = new();
            form.GetTestAccessor().LoadText(
                "COMMIT_EDITMSG",
                "Port the file editor command\n\nPreserve the original encoding and preamble.\n",
                hasChanges: true);
            form.FindControl<Border>("panelMessage")!.IsVisible = true;
            return form;
        }

        if (viewType == typeof(FormVerify))
        {
            FormVerify form = new();
            FormVerify.TestAccessor accessor = form.GetTestAccessor();
            accessor.SetPreviewRows();
            accessor.ShowOther.IsChecked = true;
            return form;
        }

        if (viewType == typeof(FormClone))
        {
            return new FormClone(
                context.Commands,
                "https://github.com/gitextensions/gitextensions.git",
                openedFromProtocolHandler: false,
                gitModuleChanged: null);
        }

        if (viewType == typeof(FormCommit))
        {
            return new FormCommit(context.Commands, commitMessage: "Describe the representative Avalonia changes");
        }

        if (viewType == typeof(FormCreateBranch))
        {
            return new FormCreateBranch(context.Commands, context.HeadRevision.ObjectId, "feature/");
        }

        if (viewType == typeof(FormCreateTag))
        {
            return new FormCreateTag(context.Commands, context.HeadRevision.ObjectId);
        }

        if (viewType == typeof(FormDeleteBranch))
        {
            return new FormDeleteBranch(context.Commands, [FeatureBranchName]);
        }

        if (viewType == typeof(FormDeleteTag))
        {
            return new FormDeleteTag(context.Commands, "v1.0");
        }

        if (viewType == typeof(FormFileHistory))
        {
            return new FormFileHistory(context.Commands, AppSourcePath, context.HeadRevision);
        }

        if (viewType == typeof(FormCommitDiff))
        {
            return new FormCommitDiff(context.Commands, context.HeadRevision.ObjectId);
        }

        if (viewType == typeof(FormViewPatch))
        {
            FormViewPatch form = new(context.Commands);
            form.GetTestAccessor().LoadPatchText(context.SamplePatch);
            return form;
        }

        if (viewType == typeof(FormInit))
        {
            return new FormInit(context.Commands, context.WorkingDirectory, gitModuleChanged: null);
        }

        if (viewType == typeof(FormMergeBranch))
        {
            return new FormMergeBranch(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormPull))
        {
            return new FormPull(context.Commands, MainBranchName, RemoteName, GitPullAction.Default);
        }

        if (viewType == typeof(FormPush))
        {
            return new FormPush(context.Commands, MainBranchName);
        }

        if (viewType == typeof(FormRebase))
        {
            return new FormRebase(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormApplyPatch))
        {
            return new FormApplyPatch(context.Commands);
        }

        if (viewType == typeof(FormArchive))
        {
            FormArchive form = new(context.Commands) { SelectedRevision = context.HeadRevision };
            form.SetDiffSelectedRevision(context.ParentRevision);
            return form;
        }

        if (viewType == typeof(FormRemotes))
        {
            return new FormRemotes(context.Commands) { PreselectRemoteOnLoad = RemoteName };
        }

        if (viewType == typeof(FormReflog))
        {
            return new FormReflog();
        }

        if (viewType == typeof(FormRenameBranch))
        {
            return new FormRenameBranch(context.Commands, FeatureBranchName);
        }

        if (viewType == typeof(FormResolveConflicts))
        {
            return new FormResolveConflicts(context.Commands);
        }

        if (viewType == typeof(FormMergeSubmodule))
        {
            const string filename = "submodule";
            ObjectId remoteId = ObjectId.Parse("3333333333333333333333333333333333333333");
            IGitModule module = Substitute.For<IGitModule>();
            IGitModule submodule = Substitute.For<IGitModule>();
            module.GetConflictAsync(filename).Returns(new ConflictData(
                new ConflictedFileData(context.ParentRevision.ObjectId, filename),
                new ConflictedFileData(context.HeadRevision.ObjectId, filename),
                new ConflictedFileData(remoteId, filename)));
            module.GetSubmodule(filename).Returns(submodule);
            submodule.GetCurrentCheckout().Returns(context.HeadRevision.ObjectId);
            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(module);
            return new FormMergeSubmodule(commands, filename);
        }

        if (viewType == typeof(FormStash))
        {
            return new FormStash(context.Commands);
        }

        if (viewType == typeof(FormChooseCommit))
        {
            return new FormChooseCommit(
                context.Commands,
                ObjectId.WorkTreeId.ToString(),
                showArtificial: true);
        }

        if (viewType == typeof(FormSelectMultipleBranches))
        {
            return new FormSelectMultipleBranches(context.Refs.Where(gitRef => gitRef.IsHead).ToArray());
        }

        if (viewType == typeof(FormResetChanges))
        {
            return new FormResetChanges(
                hasExistingFiles: true,
                hasNewFiles: true,
                "Reset the representative staged and unstaged changes?");
        }

        if (viewType == typeof(FindAndReplaceForm))
        {
            FindAndReplaceForm form = new();
            TextEditor editor = new() { Text = "Find representative text in the diff viewer." };
            FindAndReplaceForm.TestAccessor accessor = form.GetTestAccessor();
            accessor.SetEditor(editor);
            accessor.TxtLookFor.Text = "representative";
            form.ReplaceMode = false;
            return form;
        }

        // parity-scaffolding: Seeds the internal editor surface for paired capture.
        if (viewType == typeof(FileViewerInternal))
        {
            FileViewerInternal viewer = new();
            viewer.GetTestAccessor().TextEditor.Text = context.SamplePatch;
            return viewer;
        }

        // parity-scaffolding: Seeds the modeless git-grep dialog without opening it from a consumer.
        if (viewType == typeof(FormFindInCommitFilesGitGrep))
        {
            FormFindInCommitFilesGitGrep form = new(context.Commands)
            {
                GitGrepExpressionText = "representative|editor",
            };
            form.SetSearchItems(["representative|editor", "TODO|FIXME"]);
            form.SetShowFindInCommitFilesGitGrep(visible: true);
            return form;
        }

        if (viewType == typeof(FormGoToLine))
        {
            FormGoToLine form = new();
            form.SetMaxLineNumber(120);
            return form;
        }

        return (Control)(Activator.CreateInstance(viewType)
            ?? throw new InvalidOperationException($"Could not construct {viewType.FullName}."));
    }

    private static void PrepareView(Control root, CaptureContext context)
    {
        if (root is EditNetSpell editNetSpell)
        {
            editNetSpell.Text = $"Describe the Avalonia spell-checking changes.{Environment.NewLine}{Environment.NewLine}This sentnce contains a deliberate misspeling and an overlong commit-message body line for visual verification.";
            editNetSpell.CheckSpelling();
        }

        if (root is ColorsSettingsPage colorsSettingsPage)
        {
            colorsSettingsPage.LoadSettings();
        }

        if (root is GitSettingsPage gitSettingsPage)
        {
            gitSettingsPage.LoadSettings();
        }

        if (root is AppearanceSettingsPage appearanceSettingsPage)
        {
            appearanceSettingsPage.LoadSettings();
        }

        if (root is SortingSettingsPage sortingSettingsPage)
        {
            sortingSettingsPage.LoadSettings();
        }

        if (root is AppearanceFontsSettingsPage appearanceFontsSettingsPage)
        {
            appearanceFontsSettingsPage.LoadSettings();
        }

        if (root is ConsoleStyleSettingsPage consoleStyleSettingsPage)
        {
            consoleStyleSettingsPage.LoadSettings();
        }

        if (root is RevisionLinksSettingsPage revisionLinksSettingsPage)
        {
            DistributedSettings settings = new(
                lowerPriority: null,
                new GitExtSettingsCache(
                    Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.settings"),
                    autoSave: false),
                GitExtensions.Extensibility.Settings.SettingLevel.Global);
            new ExternalLinksStorage().Save(
                settings,
                [
                    new ExternalLinkDefinition
                    {
                        Name = "GitHub - Issues",
                        Enabled = true,
                        SearchInParts =
                        {
                            ExternalLinkDefinition.RevisionPart.Message,
                            ExternalLinkDefinition.RevisionPart.LocalBranches,
                        },
                        SearchPattern = @"(?i)#\d+",
                        NestedSearchPattern = @"\d+",
                        UseRemotesPattern = "upstream|origin",
                        UseOnlyFirstRemote = true,
                        RemoteSearchInParts = { ExternalLinkDefinition.RemotePart.URL },
                        LinkFormats =
                        {
                            new ExternalLinkFormat
                            {
                                Caption = "Issue #{0}",
                                Format = "https://github.com/gitextensions/gitextensions/issues/{0}",
                            },
                        },
                    },
                ]);
            revisionLinksSettingsPage.GetTestAccessor().LoadFromSettings(settings);
        }

        if (root is HotkeysSettingsPage hotkeysSettingsPage)
        {
            hotkeysSettingsPage.LoadSettings();
            hotkeysSettingsPage.GetTestAccessor().Hotkeys.Settings.SelectedIndex = 0;
        }

        if (root is AdvancedSettingsPage advancedSettingsPage)
        {
            advancedSettingsPage.LoadSettings();
        }

        if (root is ConfirmationsSettingsPage confirmationsSettingsPage)
        {
            confirmationsSettingsPage.LoadSettings();
        }

        if (root is BlameViewerSettingsPage blameViewerSettingsPage)
        {
            blameViewerSettingsPage.LoadSettings();
        }

        if (root is CommitDialogSettingsPage commitDialogSettingsPage)
        {
            commitDialogSettingsPage.LoadSettings();
        }

        if (root is FormBrowseRepoSettingsPage formBrowseRepoSettingsPage)
        {
            formBrowseRepoSettingsPage.LoadSettings();
        }

        if (root is ShellExtensionSettingsPage shellExtensionSettingsPage)
        {
            shellExtensionSettingsPage.LoadSettings();
        }

        if (root is ScriptsSettingsPage scriptsSettingsPage)
        {
            scriptsSettingsPage.LoadSettings();
        }

        if (root is OutputHistoryControl outputHistory)
        {
            outputHistory.TextBox.Text =
                $"12:34 git fetch --all{Environment.NewLine}" +
                $"See https://gitextensions.github.io/ for details.{Environment.NewLine}{Environment.NewLine}" +
                $"12:35 git push origin main{Environment.NewLine}" +
                $"Remote: https://github.com/gitextensions/gitextensions{Environment.NewLine}{Environment.NewLine}" +
                $"###{Environment.NewLine}";
        }

        if (root is FormStatus formStatus)
        {
            FieldInfo processCallback = typeof(FormStatus)
                .GetField("ProcessCallback", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException("FormStatus.ProcessCallback could not be found.");
            processCallback.SetValue(formStatus, (Action<FormStatus>)(_ => { }));
        }

        if (root is FormPush formPush)
        {
            HyperlinkButton showOptions = formPush.FindControl<HyperlinkButton>("ShowOptions")
                ?? throw new InvalidOperationException("FormPush.ShowOptions could not be found.");
            showOptions.RaiseEvent(new Avalonia.Interactivity.RoutedEventArgs(Button.ClickEvent));
        }

        if (root is FormSubmodules formSubmodules)
        {
            formSubmodules.GetTestAccessor().SetModules(
            [
                new GitSubmoduleInfo(
                    "avalonia",
                    "externals/avalonia",
                    "https://github.com/AvaloniaUI/Avalonia.git",
                    context.HeadRevision.ObjectId,
                    MainBranchName,
                    isInitialized: true,
                    isUpToDate: true),
                new GitSubmoduleInfo(
                    "documentation",
                    "docs",
                    "https://github.com/gitextensions/gitextensionsdoc.git",
                    context.ParentRevision.ObjectId,
                    "stable",
                    isInitialized: true,
                    isUpToDate: false),
            ]);
        }

        if (root is FormAddSubmodule formAddSubmodule)
        {
            FormAddSubmodule.TestAccessor accessor = formAddSubmodule.GetTestAccessor();
            accessor.Directory.Text = "https://github.com/AvaloniaUI/Avalonia.git";
            accessor.Branch.Text = "master";
        }

        if (root is FormCreateWorktree formCreateWorktree)
        {
            IGitRef feature = Substitute.For<IGitRef>();
            feature.Name.Returns("feature/visual-parity");
            feature.LocalName.Returns("feature/visual-parity");
            FormCreateWorktree.TestAccessor accessor = formCreateWorktree.GetTestAccessor();
            accessor.SetBranches([feature]);
            accessor.WorktreeDirectory.Text = Path.Combine(context.WorkingDirectory, "..", "visual-parity-worktree");
        }

        if (root is FormManageWorktree formManageWorktree)
        {
            string rootPath = OperatingSystem.IsWindows() ? @"C:\Repos" : "/home/user/repos";
            GitWorktree main = new(Path.Combine(rootPath, "gitextensions"), GitWorktreeHeadType.Branch, context.HeadRevision.ObjectId.ToShortString(), MainBranchName, IsDeleted: false);
            GitWorktree feature = new(Path.Combine(rootPath, "gitextensions-feature"), GitWorktreeHeadType.Branch, context.ParentRevision.ObjectId.ToShortString(), FeatureBranchName, IsDeleted: false);
            GitWorktree deleted = new(Path.Combine(rootPath, "gitextensions-old"), GitWorktreeHeadType.Detached, context.ParentRevision.ObjectId.ToShortString(), null, IsDeleted: true);
            formManageWorktree.GetTestAccessor().SetWorktrees([main, feature, deleted], feature.Path);
        }

        if (root is FormReflog formReflog)
        {
            FormReflog.TestAccessor accessor = formReflog.GetTestAccessor();
            accessor.Branches.ItemsSource = new[] { "HEAD", MainBranchName, $"{RemoteName}/{MainBranchName}" };
            accessor.Branches.SelectedIndex = 0;
            accessor.CurrentBranch.Content = $"current branch ({MainBranchName})";
            accessor.SetRefLines(
            [
                new RefLine(context.HeadRevision.ObjectId, "HEAD@{0}", "commit: Add Avalonia reflog dialog"),
                new RefLine(context.ParentRevision.ObjectId, "HEAD@{1}", "checkout: moving from feature/visual-parity to main"),
                new RefLine(ObjectId.Parse("3333333333333333333333333333333333333333"), "HEAD@{2}", "commit: Prepare representative repository"),
            ]);
        }

        if (root is SettingsLinkLabel settingsLinkLabel)
        {
            // parity-scaffolding: Give the standalone settings link visible representative content.
            GetRequiredControl<HyperlinkButton>(settingsLinkLabel, "linkLabel").Content = "Review this setting";
            GetRequiredControl<Button>(settingsLinkLabel, "pictureBox").IsVisible = true;
        }

        if (root is Window)
        {
            return;
        }

        foreach (GitModuleControl moduleControl in new[] { root }
                     .Concat(root.GetLogicalDescendants().OfType<Control>())
                     .OfType<GitModuleControl>())
        {
            moduleControl.UICommandsSource = context;
        }
    }

    private static async Task SeedStandaloneControlAsync(Control root, CaptureContext context)
    {
        if (root is BranchSelector branchSelector)
        {
            branchSelector.Initialize(remote: false, containObjectIds: null);
            return;
        }

        if (root is InteractiveGitActionControl interactiveGitActionControl)
        {
            interactiveGitActionControl.GetTestAccessor().SetGitAction(
                InteractiveGitActionControl.GitAction.Rebase,
                conflicts: false);
            return;
        }

        if (root is GitUI.UserControls.Settings.SettingsCheckBox settingsCheckBox)
        {
            settingsCheckBox.Text = "Enable representative setting";
            settingsCheckBox.ToolTipText = "Representative setting information";
            return;
        }

        if (root is WaitSpinner waitSpinner)
        {
            waitSpinner.IsAnimating = false;
            waitSpinner.SetProgressForCapture(7);
            return;
        }

        if (root is LoadingControl loadingControl)
        {
            loadingControl.IsAnimating = false;
            WaitSpinner loadingSpinner = loadingControl.GetLogicalDescendants().OfType<WaitSpinner>().Single();
            loadingSpinner.SetProgressForCapture(7);
            return;
        }

        if (root is WatermarkComboBox watermarkComboBox)
        {
            watermarkComboBox.Watermark = "Filter files using a regular expression...";
            return;
        }

        if (root is CaseSensitiveComboBox caseSensitiveComboBox)
        {
            caseSensitiveComboBox.ItemsSource = new[] { "Main", "main", "release/1.0" };
            caseSensitiveComboBox.Text = "main";
            caseSensitiveComboBox.NotifyAutoCompleteForTest();
            return;
        }

        if (root is Dashboard dashboard)
        {
            IRepositoryHistoryUIService history = Substitute.For<IRepositoryHistoryUIService>();
            IUserRepositoriesListController controller = Substitute.For<IUserRepositoriesListController>();
            Repository recent = new(context.WorkingDirectory);
            Repository favourite = new(Path.GetDirectoryName(context.WorkingDirectory)!)
            {
                Category = "Development",
            };
            controller.PreRenderRepositories(Arg.Any<string>()).Returns((
                new[] { new RecentRepoInfo(recent, topRepo: true, anchored: true) { Caption = "gitextensions" } },
                new[] { new RecentRepoInfo(favourite, topRepo: false, anchored: false) { Caption = "avalonia-port" } }));
            controller.IsValidGitWorkingDir(Arg.Any<string>()).Returns(true);
            controller.GetCurrentBranchName(recent.Path).Returns(MainBranchName);
            controller.GetCurrentBranchName(favourite.Path).Returns(FeatureBranchName);
            dashboard.Initialize(controller, history);
            dashboard.RefreshContent();
            return;
        }

        if (root is RevisionGpgInfoControl revisionGpgInfo)
        {
            revisionGpgInfo.DisplayGpgInfo(new GpgInfo(
                CommitStatus.GoodSignature,
                "Good signature from Visual Parity <visual@example.com>\nPrimary key fingerprint: 0123 4567 89AB CDEF",
                TagStatus.OneGood,
                "Good signature on tag v1.0.0\nTagger: Visual Parity <visual@example.com>"));
            return;
        }

        if (root is RevisionDiffControl revisionFileTree)
        {
            IReadOnlyList<GitItemStatus> treeItems = context.Module.GetTreeFiles(context.HeadRevision.ObjectId, full: true);
            revisionFileTree.FileStatusList.SetDiffs(
                [new FileStatusWithDescription(null, context.HeadRevision, "File tree", treeItems)],
                isFileTreeMode: true);
            revisionFileTree.FileStatusList.SelectFileOrFolder(RelativePath.From(AppSourcePath));
            FileStatusItem fileStatusItem = revisionFileTree.FileStatusList.SelectedFileStatusItem!;
            await revisionFileTree.FileViewer.ViewGitItemAsync(fileStatusItem);
            return;
        }

        if (root is CommitDiff commitDiff)
        {
            commitDiff.SetRevision(context.HeadRevision.ObjectId, AppSourcePath);
            await WaitForCommitDiffAsync(commitDiff);
            return;
        }

        if (root is BlameControl blame)
        {
            await SeedBlameControlAsync(blame, context);
            return;
        }

        Control[] controls =
        [
            root,
            .. root.GetLogicalDescendants().OfType<Control>(),
        ];

        foreach (Control control in controls)
        {
            switch (control)
            {
                case RepoObjectsTree repoObjectsTree:
                    repoObjectsTree.SetRefs(context.Refs, context.Stashes, MainBranchName);
                    string worktreeRoot = OperatingSystem.IsWindows() ? @"C:\Repos" : "/home/user/repos";
                    repoObjectsTree.GetTestAccessor().SetWorktrees(
                    [
                        new GitWorktree(Path.Combine(worktreeRoot, "gitextensions"), GitWorktreeHeadType.Branch, context.HeadRevision.ObjectId.ToString(), MainBranchName, IsDeleted: false),
                        new GitWorktree(Path.Combine(worktreeRoot, "gitextensions.worktrees", "feature-ui"), GitWorktreeHeadType.Branch, context.ParentRevision.ObjectId.ToString(), FeatureBranchName, IsDeleted: false),
                        new GitWorktree(Path.Combine(worktreeRoot, "gitextensions.worktrees", "old-review"), GitWorktreeHeadType.Detached, context.ParentRevision.ObjectId.ToString(), null, IsDeleted: true),
                    ], Path.Combine(worktreeRoot, "gitextensions"));
                    repoObjectsTree.GetTestAccessor().SetSubmodules(new SubmoduleInfoResult
                    {
                        TopProject = new SubmoduleInfo("gitextensions (main)", context.WorkingDirectory, bold: true),
                        AllSubmodules =
                        {
                            new SubmoduleInfo("externals/AvaloniaEdit (main)", Path.Combine(context.WorkingDirectory, "externals", "AvaloniaEdit"), bold: false),
                            new SubmoduleInfo("externals/NetSpell.SpellChecker (release)", Path.Combine(context.WorkingDirectory, "externals", "NetSpell.SpellChecker"), bold: false),
                            new SubmoduleInfo("samples/ThemePreview (feature/colors)", Path.Combine(context.WorkingDirectory, "externals", "AvaloniaEdit", "samples", "ThemePreview"), bold: false),
                        },
                    });
                    break;

                case CommitInfo commitInfo:
                    commitInfo.Revision = context.HeadRevision;
                    break;

                case CommitInfoHeader commitInfoHeader:
                    commitInfoHeader.ShowCommitInfo(context.HeadRevision, [context.ParentRevision.ObjectId]);
                    break;

                case CommitSummaryUserControl summary:
                    summary.Revision = context.HeadRevision;
                    break;

                case BranchComboBox branchComboBox:
                    branchComboBox.BranchesToSelect = context.Refs.Where(gitRef => gitRef.IsHead).ToArray();
                    branchComboBox.SetSelectedText(MainBranchName);
                    break;

                case FileViewer fileViewer:
                    fileViewer.ViewPatch(context.SamplePatch);
                    break;

                case FileStatusList fileStatusList:
                    int splitIndex = Math.Max(1, context.ChangedFiles.Count / 2);
                    fileStatusList.GroupByRevision = true;
                    fileStatusList.SetDiffs(
                    [
                        new FileStatusWithDescription(
                            null,
                            context.HeadRevision,
                            "Diff with parent",
                            [.. context.ChangedFiles.Take(splitIndex)]),
                        new FileStatusWithDescription(
                            null,
                            context.HeadRevision,
                            "Working directory",
                            [.. context.ChangedFiles.Skip(splitIndex)]),
                    ],
                    isFileTreeMode:
                        Environment.GetEnvironmentVariable(CaptureFileStatusTreeEnvironmentVariable) == "1");
                    if (ReferenceEquals(fileStatusList, root)
                        && Environment.GetEnvironmentVariable(CaptureFileStatusTreeEnvironmentVariable) != "1")
                    {
                        fileStatusList.SetFilter("src|CHANGELOG");
                    }

                    break;

                case ChecklistSettingsPage checklist:
                    SeedChecklist(checklist);
                    break;

                case RevisionGridControl revisionGrid:
                    revisionGrid.ReloadRevisions(context.Module);
                    break;

                case PatchGrid patchGrid:
                    SeedPatchGrid(patchGrid, context);
                    break;

                case CommitPickerSmallControl commitPicker:
                    SeedCommitPicker(commitPicker, context);
                    break;

                case PasswordInput passwordInput:
                    GetRequiredControl<TextBox>(passwordInput, "Password").Text = "representative-secret";
                    break;

                case RemotesComboboxControl remotes:
                    ComboBox comboBox = GetRequiredControl<ComboBox>(remotes, "comboBoxRemotes");
                    comboBox.ItemsSource = new[] { RemoteName, "upstream" };
                    comboBox.SelectedIndex = 0;
                    break;
            }
        }
    }

    private static async Task SeedBlameControlAsync(BlameControl blame, CaptureContext context)
    {
        string[] lines = context.SampleBlame
            .ReplaceLineEndings("\n")
            .TrimEnd('\n')
            .Split('\n');
        BlameControl.TestAccessor accessor = blame.GetTestAccessor();
        IReadOnlyList<System.Drawing.Color> colors = accessor.GetAgeBucketGradientColors();
        byte[] avatar = (await new InitialsAvatarProvider().GetAvatarAsync(
            "avalonia.contributor@example.com",
            "Avalonia Contributor",
            blame.BlameAuthor.AvatarSize))!;
        GitBlameEntry[] entries = lines
            .Select((_, index) => new GitBlameEntry
            {
                Avatar = index % 4 == 0 ? avatar : null,
                AgeBucketIndex = index % colors.Count,
                AgeBucketColor = colors[index % colors.Count],
            })
            .ToArray();
        string gutter = string.Join(
            '\n',
            lines.Select((_, index) => index % 4 == 0 ? $"2026-07-{20 - (index % 7):00} - Avalonia Contributor" : string.Empty));

        await accessor.BlameFile.ViewTextAsync(AppSourcePath, context.SampleBlame);
        blame.BlameAuthor.Initialize(gutter, entries, showAvatars: true);
    }

    private static void SeedChecklist(ChecklistSettingsPage checklist)
    {
        (string Name, string Message, bool Repair)[] rows =
        [
            ("GitFound", "Git 2.52.0 is found on your computer.", false),
            ("UserNameSet", "A username and an email address are configured.", false),
            ("MergeTool", "There is a mergetool configured: kdiff3", false),
            ("DiffTool", "There is a difftool configured: kdiff3", false),
            ("ShellExtensionsRegistered", "Shell extensions are not installed. Run the installer to install the shell extensions.", false),
            ("GitBinFound", "Linux tools (sh) found on your computer.", false),
            ("GitExtensionsInstall", "Git Extensions is properly registered.", false),
            ("SshConfig", "Default SSH client, OpenSSH, will be used.", false),
            ("translationConfig", "The configured language is English.", false),
            ("GcmDetected", "Obsolete git-credential-winstore.exe detected", true),
        ];
        foreach ((string name, string message, bool repair) in rows)
        {
            Button status = GetRequiredControl<Button>(checklist, name);
            Button fix = GetRequiredControl<Button>(
                checklist,
                name == "GcmDetected" ? "GcmDetectedFix" : $"{name}_Fix");
            status.Content = message;
            status.IsVisible = true;
            fix.IsVisible = repair;
        }
    }

    private static void SeedPatchGrid(PatchGrid patchGrid, CaptureContext context)
    {
        patchGrid.IsManagingRebase = true;
        PatchFile[] patches =
        [
            new PatchFile
            {
                Action = "pick",
                Name = "0001",
                ObjectId = context.HeadRevision.ObjectId,
                Subject = HeadCommitSubject,
                Author = "Avalonia Contributor",
                Date = "2026-07-17",
                IsNext = true,
            },
            new PatchFile
            {
                Action = "reword",
                Name = "0002",
                ObjectId = context.ParentRevision.ObjectId,
                Subject = InitialCommitSubject,
                Author = "Git Extensions Team",
                Date = "2026-07-16",
            },
        ];
        GetRequiredControl<ListBox>(patchGrid, "Patches").ItemsSource = patches;
    }

    private static void SeedCommitPicker(CommitPickerSmallControl commitPicker, CaptureContext context)
    {
        GetRequiredControl<TextBox>(commitPicker, "textBoxCommitHash").Text = context.HeadRevision.ObjectId.ToShortString();
        GetRequiredControl<TextBlock>(commitPicker, "lbCommits").Text = "(+2-0)";
    }

    private static T GetRequiredControl<T>(Control root, string name)
        where T : Control
        => root.FindControl<T>(name)
            ?? throw new InvalidOperationException($"{root.GetType().Name}.{name} could not be found.");

    private static async Task WaitForAsyncViewsAsync(Control root)
    {
        if (root is FormGoToCommit)
        {
            // parity-scaffolding: Wait for the reference/tag loaders so the capture records the settled dialog.
            ComboBox tags = GetRequiredControl<ComboBox>(root, "comboBoxTags");
            ComboBox branches = GetRequiredControl<ComboBox>(root, "comboBoxBranches");
            Stopwatch refsStopwatch = Stopwatch.StartNew();
            while ((tags.Text == GitUI.TranslatedStrings.LoadingData || branches.Text == GitUI.TranslatedStrings.LoadingData)
                   && refsStopwatch.Elapsed < TimeSpan.FromSeconds(5))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            tags.Text.Should().NotBe(GitUI.TranslatedStrings.LoadingData);
            branches.Text.Should().NotBe(GitUI.TranslatedStrings.LoadingData);
        }

        if (root is FormDiff formDiff)
        {
            // parity-scaffolding: Wait for the diff list and selected-file viewer to settle.
            FormDiff.TestAccessor accessor = formDiff.GetTestAccessor();
            Stopwatch diffStopwatch = Stopwatch.StartNew();
            while ((accessor.DiffFiles.AllItemsCount == 0 || string.IsNullOrEmpty(accessor.DiffText.TextEditor.Text))
                   && diffStopwatch.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            accessor.DiffFiles.AllItems.Should().NotBeEmpty();
            accessor.DiffText.TextEditor.Text.Should().NotBeEmpty();
        }

        if (FindNamedControl(root, "listBoxSearchResult") is ListBox searchResults
            && FindNamedControl(root, "txtSearchBox") is TextBox searchText
            && !string.IsNullOrEmpty(searchText.Text))
        {
            // parity-scaffolding: Wait for SearchControl's AsyncLoader before driving result-list states.
            Stopwatch searchStopwatch = Stopwatch.StartNew();
            while (searchResults.ItemCount == 0 && searchStopwatch.Elapsed < TimeSpan.FromSeconds(5))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            searchResults.ItemCount.Should().BeGreaterThan(0);
        }

        if (Environment.GetEnvironmentVariable(CaptureDeterministicRepositoryEnvironmentVariable) == "1"
            && root is FormBrowse formBrowse)
        {
            // parity-scaffolding: Wait for the seeded repository's status count so captures cannot race the monitor.
            Button commitButton = GetRequiredControl<Button>(formBrowse, "toolStripButtonCommit");
            Stopwatch statusStopwatch = Stopwatch.StartNew();
            while (!(commitButton.Content?.ToString()?.EndsWith("(3)", StringComparison.Ordinal) ?? false)
                   && statusStopwatch.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            commitButton.Content?.ToString().Should().EndWith("(3)");
        }

        if (Environment.GetEnvironmentVariable(CaptureDeterministicRepositoryEnvironmentVariable) == "1"
            && root is FormVerify)
        {
            // parity-scaffolding: Let Fluent scrollbars finish their fade before exact pixel comparison.
            await Task.Delay(1200);
            Dispatcher.UIThread.RunJobs();
        }

        if (Environment.GetEnvironmentVariable(CaptureDeterministicRepositoryEnvironmentVariable) == "1"
            && root is FormCommit formCommit)
        {
            // parity-scaffolding: Wait for the selected-file diff so the control tree cannot race its loader.
            FileViewer selectedDiff = GetRequiredControl<FileViewer>(formCommit, "SelectedDiff");
            Stopwatch diffStopwatch = Stopwatch.StartNew();
            while (string.IsNullOrEmpty(selectedDiff.TextEditor.Text)
                   && diffStopwatch.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            selectedDiff.TextEditor.Text.Should().NotBeEmpty();
        }

        if (root is EditNetSpell)
        {
            await Task.Delay(300);
            Dispatcher.UIThread.RunJobs();
        }

        if (root is FormFileHistory fileHistory)
        {
            FileViewer diff = GetRequiredControl<FileViewer>(fileHistory, "Diff");
            Stopwatch diffStopwatch = Stopwatch.StartNew();
            while (string.IsNullOrEmpty(diff.TextEditor.Text)
                   && diffStopwatch.Elapsed < TimeSpan.FromSeconds(15))
            {
                Dispatcher.UIThread.RunJobs();
                await Task.Delay(10);
            }

            diff.TextEditor.Text.Should().NotBeEmpty();
        }

        CommitDiff? commitDiff = root as CommitDiff
            ?? root.GetLogicalDescendants()
                .OfType<CommitDiff>()
                .FirstOrDefault(control => TopLevel.GetTopLevel(control) is not null);
        if (commitDiff is not null)
        {
            await WaitForCommitDiffAsync(commitDiff);
        }

        TextBlock[] loadingStatuses =
        [
            .. new[] { root }
                .Concat(root.GetLogicalDescendants().OfType<Control>())
                .OfType<RevisionGridControl>()
                .Select(revisionGrid => revisionGrid.FindControl<TextBlock>("lblLoadingStatus"))
                .OfType<TextBlock>(),
        ];
        if (loadingStatuses.Length == 0)
        {
            Dispatcher.UIThread.RunJobs();
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();
        while (loadingStatuses.Any(status => !IsLoadingComplete(status.Text))
               && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        loadingStatuses.Should().OnlyContain(status => IsLoadingComplete(status.Text));
    }

    private static async Task WaitForCommitDiffAsync(CommitDiff commitDiff)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (string.IsNullOrEmpty(commitDiff.FileViewer.TextEditor.Text)
               && stopwatch.Elapsed < TimeSpan.FromSeconds(15))
        {
            Dispatcher.UIThread.RunJobs();
            await Task.Delay(10);
        }

        commitDiff.FileStatusList.AllItems.Should().NotBeEmpty();
        commitDiff.FileViewer.TextEditor.Text.Should().NotBeEmpty();
    }

    private static bool IsLoadingComplete(string? status)
        => status?.EndsWith(" revisions", StringComparison.Ordinal) == true;

    private static (double Width, double Height) GetCaptureSize(Type viewType)
    {
        if (viewType == typeof(BranchSelector))
        {
            return (325, 54);
        }

        if (viewType == typeof(InteractiveGitActionControl))
        {
            return (424, 34);
        }

        if (viewType == typeof(GitUI.UserControls.Settings.SettingsCheckBox))
        {
            return (104, 17);
        }

        if (viewType == typeof(WaitSpinner))
        {
            return (48, 48);
        }

        if (viewType == typeof(EmptyRepoControl))
        {
            return (827, 189);
        }

        if (viewType == typeof(RevisionGridControl))
        {
            return (682, 235);
        }

        if (viewType == typeof(ErrorControl))
        {
            return (2080, 1447);
        }

        if (viewType == typeof(LoadingControl))
        {
            return (32, 32);
        }

        if (viewType == typeof(WatermarkComboBox) || viewType == typeof(CaseSensitiveComboBox))
        {
            return (250, 23);
        }

        if (viewType == typeof(SimplePrompt))
        {
            return (334, 104);
        }

        if (viewType == typeof(FormFilePrompt))
        {
            return (549, 78);
        }

        if (viewType == typeof(FormGoToCommit))
        {
            return (604, 302);
        }

        if (viewType == typeof(FormCheckoutRevision))
        {
            return (481, 131);
        }

        if (viewType == typeof(FormDiff))
        {
            return (1042, 685);
        }

        if (viewType == typeof(FormCompareToBranch))
        {
            return (434, 110);
        }

        if (viewType == typeof(FormFormatPatch))
        {
            return (1030, 665);
        }

        if (viewType == typeof(SearchControl))
        {
            return (325, 91);
        }

        if (viewType == typeof(SearchWindow))
        {
            return (325, 113);
        }

        if (viewType == typeof(FormDashboardCategoryTitle))
        {
            return (400, 92);
        }

        if (viewType == typeof(FormQuickItemSelector))
        {
            return (190, 134);
        }

        if (viewType == typeof(SimpleHelpDisplayDialog))
        {
            return (299, 439);
        }

        if (viewType == typeof(FormBrowse))
        {
            return (1400, 850);
        }

        if (viewType == typeof(Dashboard))
        {
            return (686, 358);
        }

        if (viewType == typeof(FormRecentReposSettings))
        {
            return (684, 361);
        }

        if (viewType == typeof(RepoObjectsTree))
        {
            return (360, 560);
        }

        if (viewType == typeof(CommitInfo) || viewType == typeof(CommitSummaryUserControl))
        {
            return (620, 320);
        }

        if (viewType == typeof(EditNetSpell))
        {
            return (386, 336);
        }

        if (viewType == typeof(BranchComboBox)
            || viewType == typeof(CommitPickerSmallControl)
            || viewType == typeof(FolderBrowserButton)
            || viewType == typeof(GotoUserManualControl)
            || viewType == typeof(PasswordInput)
            || viewType == typeof(RemotesComboboxControl))
        {
            return (620, 100);
        }

        if (viewType == typeof(HelpImageDisplayUserControl))
        {
            return (420, 360);
        }

        if (viewType == typeof(BlameControl))
        {
            return (1000, 650);
        }

        if (viewType == typeof(CommitDiff))
        {
            return (885, 578);
        }

        if (viewType == typeof(FormCommitDiff))
        {
            return (717, 529);
        }

        if (viewType == typeof(FormViewPatch))
        {
            return (689, 501);
        }

        if (viewType == typeof(FormMergeSubmodule))
        {
            return (595, 254);
        }

        if (viewType == typeof(FormSubmodules))
        {
            return (782, 372);
        }

        if (viewType == typeof(FormAddSubmodule))
        {
            return (492, 150);
        }

        if (viewType == typeof(FormCreateWorktree))
        {
            return (608, 208);
        }

        if (viewType == typeof(FormManageWorktree))
        {
            return (710, 361);
        }

        if (viewType == typeof(FormReflog))
        {
            return (782, 555);
        }

        if (viewType == typeof(FormArchive))
        {
            return (610, 609);
        }

        if (viewType == typeof(FormCherryPick))
        {
            return (630, 470);
        }

        if (viewType == typeof(FormRevertCommit))
        {
            return (630, 410);
        }

        if (viewType == typeof(FormResetCurrentBranch))
        {
            return (479, 469);
        }

        if (viewType == typeof(FormRevisionFilter))
        {
            return (430, 580);
        }

        if (viewType == typeof(FormEdit))
        {
            return (733, 571);
        }

        if (viewType == typeof(FormEditor))
        {
            return (659, 543);
        }

        if (viewType == typeof(FormVerify))
        {
            return (900, 600);
        }

        if (viewType == typeof(FormUpdates))
        {
            return (462, 157);
        }

        if (viewType == typeof(FindAndReplaceForm))
        {
            return (419, 169);
        }

        if (viewType == typeof(FormGoToLine))
        {
            return (237, 100);
        }

        if (viewType == typeof(FontDialogWindow))
        {
            return (520, 300);
        }

        if (viewType == typeof(HotkeysSettingsPage))
        {
            return (791, 525);
        }

        if (viewType == typeof(AdvancedSettingsPage))
        {
            return (791, 525);
        }

        if (viewType == typeof(ConfirmationsSettingsPage))
        {
            return (791, 760);
        }

        if (viewType == typeof(BlameViewerSettingsPage))
        {
            return (341, 272);
        }

        if (viewType == typeof(CommitDialogSettingsPage))
        {
            return (1014, 950);
        }

        if (viewType == typeof(FormBrowseRepoSettingsPage))
        {
            return (738, 438);
        }

        if (viewType == typeof(ShellExtensionSettingsPage))
        {
            return (1502, 331);
        }

        if (viewType == typeof(FormChooseTranslation))
        {
            // The WinForms full-window PrintWindow surface includes non-client chrome.
            return (814.5, 577);
        }

        if (viewType == typeof(ColorsSettingsPage))
        {
            return (1527, 599);
        }

        if (viewType == typeof(GourceStart))
        {
            return (718, 165);
        }

        if (typeof(Window).IsAssignableFrom(viewType))
        {
            return (900, 620);
        }

        return (900, 560);
    }

    private static IReadOnlyList<ViewDescriptor> GetViewDescriptors([CallerFilePath] string thisFilePath = "")
    {
        string repositoryRoot = FindRepositoryRoot(thisFilePath);
        string viewRoot = Path.Combine(repositoryRoot, "src", "app", "GitUI.Avalonia");
        Assembly viewAssembly = typeof(FormBrowse).Assembly;

        ViewDescriptor[] axamlViews = Directory.EnumerateFiles(viewRoot, "*" + AxamlExtension, SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(viewRoot, path).StartsWith("Styles" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .Select(path => CreateDescriptor(viewRoot, path, viewAssembly))
            .ToArray();

        // parity-scaffolding: Code-only controls need descriptors so the shared capture plan can render them without product wrappers.
        return
        [
            .. axamlViews,
            new("UserControls/RevisionGrid/ErrorControl.cs", "UserControls/RevisionGrid/ErrorControl", typeof(ErrorControl).FullName!, typeof(ErrorControl)),
            new("UserControls/RevisionGrid/LoadingControl.cs", "UserControls/RevisionGrid/LoadingControl", typeof(LoadingControl).FullName!, typeof(LoadingControl)),
            new("UserControls/WaitSpinner.cs", "UserControls/WaitSpinner", typeof(WaitSpinner).FullName!, typeof(WaitSpinner)),
            new("UserControls/WatermarkComboBox.cs", "UserControls/WatermarkComboBox", typeof(WatermarkComboBox).FullName!, typeof(WatermarkComboBox)),
            new("UserControls/CaseSensitiveComboBox.cs", "UserControls/CaseSensitiveComboBox", typeof(CaseSensitiveComboBox).FullName!, typeof(CaseSensitiveComboBox)),
            // parity-scaffolding: Plugin-owned AXAML lives outside GitUI.Avalonia but shares the capture schema and state driver.
            new("../../plugins/Gource/GourceStart.axaml", "../../plugins/Gource/GourceStart", typeof(GourceStart).FullName!, typeof(GourceStart)),
        ];
    }

    private static ViewDescriptor CreateDescriptor(string viewRoot, string path, Assembly viewAssembly)
    {
        string axaml = File.ReadAllText(path);
        Match classMatch = ClassNameRegex.Match(axaml);
        classMatch.Success.Should().BeTrue($"{path} should declare x:Class");
        string className = classMatch.Groups["className"].Value;
        Type viewType = viewAssembly.GetType(className)
            ?? throw new InvalidOperationException($"The AXAML class {className} could not be resolved.");
        string relativePath = Path.GetRelativePath(viewRoot, path).Replace(Path.DirectorySeparatorChar, '/');
        return new ViewDescriptor(
            relativePath,
            relativePath[..^AxamlExtension.Length],
            className,
            viewType);
    }

    private static string GetOutputDirectory([CallerFilePath] string thisFilePath = "")
        => Path.Combine(FindRepositoryRoot(thisFilePath), "eng", "avalonia", "parity-shots");

    private static string FindRepositoryRoot(string startPath)
    {
        DirectoryInfo? directory = new(Path.GetDirectoryName(startPath)!);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GitExtensions.Avalonia.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException($"Could not find the repository root from {startPath}.");
    }

    private sealed record ViewDescriptor(
        string RelativePath,
        string RelativePathWithoutExtension,
        string ClassName,
        Type ViewType);

    private sealed record ManifestEntry(
        string View,
        string ClassName,
        string Theme,
        string ScalePercent,
        string Kind,
        int Width,
        int Height,
        string File);

    private sealed class StubMessageBoxHost : WinFormsShims.IMessageBoxHost
    {
        public WinFormsShims.DialogResult Show(
            WinFormsShims.IWin32Window? owner,
            string? text,
            string? caption,
            WinFormsShims.MessageBoxButtons buttons,
            WinFormsShims.MessageBoxIcon icon,
            WinFormsShims.MessageBoxDefaultButton defaultButton)
            => buttons switch
            {
                WinFormsShims.MessageBoxButtons.YesNo or WinFormsShims.MessageBoxButtons.YesNoCancel => WinFormsShims.DialogResult.Yes,
                _ => WinFormsShims.DialogResult.OK,
            };
    }

    private sealed class CaptureClipboard : WinFormsShims.IClipboard
    {
        public bool ContainsText() => false;

        public string GetText() => string.Empty;

        public void SetText(string value)
        {
        }
    }

    private sealed class CaptureContext : IDisposable, IGitUICommandsSource
    {
        private readonly ServiceContainer _serviceContainer;
        private readonly string _workingDirectory;

        public CaptureContext()
        {
            _serviceContainer = new ServiceContainer();
            GitExtUtils.ServiceContainerRegistry.RegisterServices(_serviceContainer);

            System.IO.Abstractions.FileSystem fileSystem = new();
            GitDirectoryResolver gitDirectoryResolver = new(fileSystem);
            RepositoryDescriptionProvider repositoryDescriptionProvider = new(gitDirectoryResolver);
            _serviceContainer.AddService<System.IO.Abstractions.IFileSystem>(fileSystem);
            _serviceContainer.AddService<IGitDirectoryResolver>(gitDirectoryResolver);
            _serviceContainer.AddService<IRepositoryDescriptionProvider>(repositoryDescriptionProvider);
            _serviceContainer.AddService<IAppTitleGenerator>(new AppTitleGenerator(repositoryDescriptionProvider));
            _serviceContainer.AddService<ILinkFactory>(new LinkFactory());
            GitCommands.ServiceContainerRegistry.RegisterServices(_serviceContainer);
            GitUI.ServiceContainerRegistry.RegisterServices(_serviceContainer);

            string repositoryDirectoryName = Environment.GetEnvironmentVariable(CaptureDeterministicRepositoryEnvironmentVariable) == "1"
                ? "GitExtensions.Parity-Deterministic"
                : $"GitExtensions.Parity-{Guid.NewGuid():N}";
            _workingDirectory = Path.Combine(Path.GetTempPath(), repositoryDirectoryName);
            if (Directory.Exists(_workingDirectory))
            {
                TestDirectory.Delete(_workingDirectory);
            }

            Directory.CreateDirectory(_workingDirectory);
            Module = new GitModule(_serviceContainer.GetRequiredService<IGitExecutorProvider>(), _workingDirectory);
            CreateRepository();
            Commands = new GitUICommands(_serviceContainer, Module);

            Refs = Module.GetRefs(RefsFilter.NoFilter);
            ObjectId headId = Module.GetCurrentCheckout();
            ObjectId parentId = Module.RevParse("HEAD~1");
            long headTime = new DateTimeOffset(2026, 7, 17, 10, 30, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            long parentTime = new DateTimeOffset(2026, 7, 16, 16, 45, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            ParentRevision = CreateRevision(parentId, InitialCommitSubject, parentTime, []);
            HeadRevision = CreateRevision(headId, HeadCommitSubject, headTime, [parentId]);
            Stashes =
            [
                new GitRevision(ObjectId.Parse("4444444444444444444444444444444444444444"))
                {
                    ReflogSelector = "refs/stash@{0}",
                    Subject = "On main: compact toolbar refinements",
                    ParentIds = [headId],
                },
            ];
            ChangedFiles = Module.GetAllChangedFilesWithSubmodulesStatus();
        }

        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged
        {
            add { }
            remove { }
        }

        public GitModule Module { get; }

        public string WorkingDirectory => _workingDirectory;

        public GitUICommands Commands { get; }

        IGitUICommands IGitUICommandsSource.UICommands => Commands;

        public IReadOnlyList<IGitRef> Refs { get; }

        public GitRevision HeadRevision { get; }

        public GitRevision ParentRevision { get; }

        public IReadOnlyCollection<GitRevision> Stashes { get; }

        public IReadOnlyList<GitItemStatus> ChangedFiles { get; }

        public string SamplePatch =>
            """
            diff --git a/src/App.cs b/src/App.cs
            index 8c1d2ab..b437e55 100644
            --- a/src/App.cs
            +++ b/src/App.cs
            @@ -1,3 +1,4 @@
             using Avalonia;
            +using GitUI;
             namespace GitExtensions;
            -// Windows-only application shell
            +// Cross-platform application shell

            """;

        public string SampleBlame =>
            """
            1  Avalonia Contributor  public partial class App
            2  Git Extensions Team  {
            3  Avalonia Contributor      public void Initialize()
            4  Git Extensions Team      {
            5  Avalonia Contributor          AvaloniaXamlLoader.Load(this);
            6  Git Extensions Team      }
            7  Avalonia Contributor  }

            """;

        public void Dispose()
        {
            _serviceContainer.Dispose();
            TestDirectory.Delete(_workingDirectory);
        }

        private void CreateRepository()
        {
            string? originalAuthorDate = Environment.GetEnvironmentVariable("GIT_AUTHOR_DATE");
            string? originalCommitterDate = Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
            try
            {
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("init") { "--quiet", "-b", MainBranchName });
                Module.SetSetting("user.name", "Avalonia Contributor");
                Module.SetSetting("user.email", "avalonia@example.com");

                string sourceDirectory = Path.Combine(_workingDirectory, "src");
                Directory.CreateDirectory(sourceDirectory);
                File.WriteAllText(Path.Combine(_workingDirectory, "README.md"), "# Visual parity sample\n");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "README.md" });
                Environment.SetEnvironmentVariable("GIT_AUTHOR_DATE", "2026-07-16T16:45:00+00:00");
                Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", "2026-07-16T16:45:00+00:00");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", '"' + InitialCommitSubject + '"' });

                File.WriteAllText(Path.Combine(sourceDirectory, "App.cs"), "namespace GitExtensions;\n\npublic partial class App;\n");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", AppSourcePath });
                Environment.SetEnvironmentVariable("GIT_AUTHOR_DATE", "2026-07-17T10:30:00+00:00");
                Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", "2026-07-17T10:30:00+00:00");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("commit") { "--quiet", "-m", '"' + HeadCommitSubject + '"' });
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("branch") { FeatureBranchName, "HEAD~1" });
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("tag") { "v1.0", "HEAD~1" });
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("remote") { "add", RemoteName, "https://example.com/gitextensions/parity.git" });
                Module.SetSetting($"remote.{RemoteName}.color", "#7B3FB2");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("update-ref") { $"refs/remotes/{RemoteName}/{MainBranchName}", "HEAD" });

                File.AppendAllText(Path.Combine(sourceDirectory, "App.cs"), "// Unstaged visual parity adjustment\n");
                File.WriteAllText(Path.Combine(_workingDirectory, "CHANGELOG.md"), "Avalonia visual parity harness\n");
                Module.GitExecutable.RunCommand(new GitArgumentBuilder("add") { "--", "CHANGELOG.md" });
                File.WriteAllText(Path.Combine(_workingDirectory, "notes.txt"), "untracked review notes\n");
            }
            finally
            {
                Environment.SetEnvironmentVariable("GIT_AUTHOR_DATE", originalAuthorDate);
                Environment.SetEnvironmentVariable("GIT_COMMITTER_DATE", originalCommitterDate);
            }
        }

        private GitRevision CreateRevision(ObjectId objectId, string subject, long unixTime, IReadOnlyList<ObjectId> parentIds)
            => new(objectId)
            {
                Author = "Avalonia Contributor",
                AuthorEmail = "avalonia@example.com",
                AuthorUnixTime = unixTime,
                Committer = "Git Extensions Team",
                CommitterEmail = "team@gitextensions.org",
                CommitUnixTime = unixTime,
                Subject = subject,
                Body = subject + "\n\nRepresentative content used by the visual parity screenshot harness.",
                ParentIds = parentIds,
                Refs = Refs.Where(gitRef => gitRef.ObjectId == objectId).ToArray(),
            };
    }
}
