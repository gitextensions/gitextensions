using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.BrowseDialog.DashboardControl;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;
using NSubstitute;
using ResourceManager;

namespace GitExtensionsTests;

[TestFixture]
[NonParallelizable]
[NUnit.Framework.Category("P8.6i.126")]
public sealed class BrowseLowerPaneCommandRoutingTests
{
    [AvaloniaTest]
    [TestCase("Commit")]
    [TestCase("Diff")]
    [TestCase("File tree")]
    [TestCase("Collapsed")]
    public async Task File_commands_should_use_only_the_source_visible_pane_not_detached_tab_content(string activePane)
    {
        ThreadHelper.JoinableTaskContext = new JoinableTaskContext();
        Dictionary<string, string?> originalSettings = new(StringComparer.Ordinal)
        {
            ["Detailed.ShowSplitViewLayout"] = AppSettings.GetString("Detailed.ShowSplitViewLayout", null),
            ["Detailed.CommitInfoPosition"] = AppSettings.GetString("Detailed.CommitInfoPosition", null),
        };
        await TestContext.Progress.WriteLineAsync($"{activePane}: input split={AppSettings.ShowSplitViewLayout}; position={AppSettings.CommitInfoPosition}");
        string directory = Path.Combine(Path.GetTempPath(), $"GitExtensions.Avalonia.CommandRoute-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            AppSettings.ShowSplitViewLayout = true;
            AppSettings.CommitInfoPosition = CommitInfoPosition.BelowList;
            (FormBrowse form, IGitUICommands commands, IGitModule module) = CreateBrowse(directory);
            using (form)
            {
                AppSettingsPath layout = new("FormBrowse.Avalonia");
                foreach (SplitterManager.SplitterData splitter in form.GetTestAccessor().SplitterManager.GetTestAccessor().Splitters)
                {
                    foreach (string key in new[] { splitter.DistanceSettingsKey, splitter.SizeSettingsKey })
                    {
                        originalSettings.TryAdd(layout.PathFor(key), layout.GetValue(key));
                    }
                }

                form.Show();
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                form.CommitInfoTabControl.SelectedItem = activePane switch
                {
                    "Diff" => form.DiffTabPage,
                    "File tree" or "Collapsed" => form.TreeTabPage,
                    _ => form.CommitInfoTabPage,
                };
                Dispatcher.UIThread.RunJobs();
                form.UpdateLayout();
                if (activePane == "Collapsed")
                {
                    Button toggle = form.FindControl<Button>("toggleSplitViewLayout")
                        ?? throw new AssertionException("The source split-view action must remain available.");
                    toggle.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    AppSettings.ShowSplitViewLayout.Should().BeFalse();
                }

                Dispatcher.UIThread.RunJobs();
                using CancellationTokenSource deadline = new(TimeSpan.FromSeconds(15));
                await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                GitRevision first = new(ObjectId.Random());
                GitRevision second = new(ObjectId.Random());
                PrepareSelection(form.revisionDiff.FileStatusList, "diff.txt");
                PrepareSelection(form.fileTree.FileStatusList, "tree.txt");
                bool diffVisible = activePane == "Diff";
                bool treeVisible = activePane == "File tree";
                (form.revisionDiff.IsEffectivelyVisible && TopLevel.GetTopLevel(form.revisionDiff) == form).Should().Be(diffVisible);
                (form.fileTree.IsEffectivelyVisible && TopLevel.GetTopLevel(form.fileTree) == form).Should().Be(treeVisible);

                foreach ((FormBrowse.Command Command, RevisionDiffKind Kind, bool TreeAllowed) action in new[]
                         {
                             (FormBrowse.Command.OpenWithDifftool, RevisionDiffKind.DiffAB, true),
                             (FormBrowse.Command.OpenWithDifftoolFirstToLocal, RevisionDiffKind.DiffALocal, false),
                             (FormBrowse.Command.OpenWithDifftoolSelectedToLocal, RevisionDiffKind.DiffBLocal, false),
                         })
                {
                    commands.ClearReceivedCalls();
                    form.ExecuteCommand(action.Command).Should().BeTrue();
                    bool expected = diffVisible || (treeVisible && action.TreeAllowed);
                    if (expected)
                    {
                        commands.Received(1).OpenWithDifftool(
                            form,
                            Arg.Is<IReadOnlyList<GitRevision?>>(revisions => revisions.Count == 2 && revisions[0] == second && revisions[1] == first),
                            diffVisible ? "diff.txt" : "tree.txt",
                            null,
                            action.Kind,
                            true,
                            null);
                    }
                    else
                    {
                        commands.DidNotReceiveWithAnyArgs().OpenWithDifftool(null, [], string.Empty, null, default, false);
                    }
                }

                commands.ClearReceivedCalls();
                form.ExecuteCommand(FormBrowse.Command.EditFile).Should().BeTrue();
                if (diffVisible || treeVisible)
                {
                    commands.Received(1).StartFileEditorDialog(Path.Combine(directory, diffVisible ? "diff.txt" : "tree.txt"));
                }
                else
                {
                    commands.DidNotReceiveWithAnyArgs().StartFileEditorDialog(default);
                }

                foreach (FormBrowse.Command command in new[] { FormBrowse.Command.OpenAsTempFile, FormBrowse.Command.OpenAsTempFileWith })
                {
                    module.ClearReceivedCalls();
                    form.ExecuteCommand(command).Should().Be(treeVisible);
                    await ThreadHelper.JoinPendingOperationsAsync(deadline.Token);
                    if (treeVisible)
                    {
                        module.Received(1).GetFileBlobHash("tree.txt", second.ObjectId);
                    }
                    else
                    {
                        module.DidNotReceiveWithAnyArgs().GetFileBlobHash(string.Empty, default);
                    }
                }

                form.Close();
                return;

                void PrepareSelection(FileStatusList list, string name)
                {
                    if (!list.TryGetUICommandsDirect(out _))
                    {
                        list.UICommandsSource = form;
                    }

                    list.SelectFirstItemOnSetItems = false;
                    list.SetDiffs([new FileStatusWithDescription(first, second, string.Empty,
                        [new GitItemStatus(name) { IsTracked = true, IsChanged = true }])], isFileTreeMode: false);
                    list.SelectFileOrFolder(RelativePath.From(name), notify: false).Should().BeTrue();
                    (list.SelectedItem?.Item.Name).Should().Be(name);
                }
            }
        }
        finally
        {
            foreach ((string key, string? value) in originalSettings)
            {
                string? changed = AppSettings.GetString(key, null);
                if (changed != value)
                {
                    await TestContext.Progress.WriteLineAsync($"{activePane}: restore {key}: {changed ?? "<unset>"} -> {value ?? "<unset>"}");
                }

                AppSettings.SettingsContainer.SetValue(key, value);
            }

            AppSettings.SettingsContainer.SettingsCache.Save();
            foreach ((string key, string? value) in originalSettings)
            {
                AppSettings.GetString(key, null).Should().Be(value,
                    "closing a source-shaped command fixture must restore the exact previous layout setting, including absence");
            }

            TestDirectory.Delete(directory);
        }
    }

    private static (FormBrowse Form, IGitUICommands Commands, IGitModule Module) CreateBrowse(string directory)
    {
        IGitModule module = Substitute.For<IGitModule>();
        module.WorkingDir.Returns(directory);
        module.IsValidGitWorkingDir().Returns(true);
        // Temp-open routing is observed at the module boundary. A zero blob prevents
        // temporary-file creation and any desktop launch, even on an unfixed route.
        module.GetFileBlobHash(Arg.Any<string>(), Arg.Any<ObjectId>()).Returns(default(ObjectId));
        IAppTitleGenerator title = Substitute.For<IAppTitleGenerator>();
        title.Generate(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<string>()).Returns("Git Extensions");
        IHotkeySettingsLoader hotkeys = Substitute.For<IHotkeySettingsLoader>();
        hotkeys.LoadHotkeys(Arg.Any<string>()).Returns([]);
        IScriptsManager scripts = Substitute.For<IScriptsManager>();
        scripts.GetScripts().Returns(new BindingList<ScriptInfo>());
        IGitUICommands commands = Substitute.For<IGitUICommands>();
        commands.Module.Returns(module);
        commands.RepoChangedNotifier.Returns(Substitute.For<ILockableNotifier>());
        commands.GetService(typeof(IAppTitleGenerator)).Returns(title);
        commands.GetService(typeof(IHotkeySettingsLoader)).Returns(hotkeys);
        commands.GetService(typeof(IRepositoryHistoryUIService)).Returns(Substitute.For<IRepositoryHistoryUIService>());
        commands.GetService(typeof(IUserRepositoriesListController)).Returns(Substitute.For<IUserRepositoriesListController>());
        commands.GetService(typeof(ILinkFactory)).Returns(Substitute.For<ILinkFactory>());
        commands.GetService(typeof(IScriptsManager)).Returns(scripts);
        return (new FormBrowse(commands), commands, module);
    }
}
