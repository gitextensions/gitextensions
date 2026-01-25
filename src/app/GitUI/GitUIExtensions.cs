using System.Text;
using System.Text.RegularExpressions;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Editor;
using GitUI.Editor.Diff;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using ResourceManager;

namespace GitUI;

public static partial class GitUIExtensions
{
    [GeneratedRegex(@"\n\s*(@@|##)\s+(?<file>[^#:\n]+)", RegexOptions.ExplicitCapture)]
    private static partial Regex FileNameRegex { get; }

    /// <summary>
    /// View the changes between the revisions, if possible as a diff.
    /// </summary>
    /// <param name="fileViewer">Current FileViewer.</param>
    /// <param name="item">The FileStatusItem to present changes for.</param>
    /// <param name="line">The line to display.</param>
    /// <param name="defaultText">default text if no diff is possible.</param>
    /// <param name="openWithDiffTool">The difftool command to open with.</param>
    /// <param name="additionalCommandInfo">If the diff is range-diff, this contains the current path filter.</param>
    /// <returns>Task to view.</returns>
    public static async Task ViewChangesAsync(this FileViewer fileViewer,
        FileStatusItem? item,
        CancellationToken cancellationToken,
        int? line = null,
        string defaultText = "",
        Action? openWithDiffTool = null,
        string additionalCommandInfo = null,
        bool forceFileView = false)
    {
        if (item?.Item.IsStatusOnly ?? false)
        {
            // Present error (e.g. parsing Git)
            await fileViewer.ViewTextAsync(item.Item.Name, item.Item.ErrorMessage ?? "", cancellationToken: cancellationToken);
            return;
        }

        if (item?.Item is null || item.SecondRevision?.ObjectId is null)
        {
            if (!string.IsNullOrWhiteSpace(defaultText))
            {
                await fileViewer.ViewTextAsync(item?.Item?.Name, defaultText, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ClearAsync();
            return;
        }

        ObjectId? firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;

        openWithDiffTool ??= OpenWithDiffTool;

        if (forceFileView || (!item.Item.IsSubmodule && (item.Item.IsNew || firstId is null || (!item.Item.IsDeleted && FileHelper.IsImage(item.Item.Name)))))
        {
            // View blob guid from revision, or file for worktree
            await fileViewer.ViewGitItemAsync(item, line, openWithDiffTool, cancellationToken: cancellationToken);
            return;
        }

        if (item.Item.IsRangeDiff)
        {
            // Git range-diff has cubic runtime complexity and can be slow and memory consuming,
            // give an indication of what is going on
            string range = item.BaseA is null || item.BaseB is null
                ? $"{firstId}...{item.SecondRevision.ObjectId}"
                : $"{item.BaseA}..{firstId} {item.BaseB}..{item.SecondRevision.ObjectId}";
            await fileViewer.ViewTextAsync(fileName: null, $"git range-diff {range} -- {additionalCommandInfo}", cancellationToken: cancellationToken);

            ExecutionResult result = await fileViewer.Module.GetRangeDiffAsync(
                    firstId,
                    item.SecondRevision.ObjectId,
                    item.BaseA,
                    item.BaseB,
                    fileViewer.GetExtraDiffArguments(isRangeDiff: true),
                    additionalCommandInfo,
                    useGitColoring: true,
                    commandConfiguration: RangeDiffHighlightService.GetGitCommandConfiguration(fileViewer.Module),
                    cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                string output = $"{result.StandardError}{Environment.NewLine}Git output (exit code: {result.ExitCodeDisplay}): {Environment.NewLine}{result.StandardOutput}";
                await fileViewer.ViewTextAsync(item?.Item?.Name, text: output, cancellationToken: cancellationToken);
                return;
            }

            // Try set highlighting from first found filename
            Match match = FileNameRegex.Match(result.StandardOutput);
            string filename = match.Groups["file"].Success ? match.Groups["file"].Value : item.Item.Name;

            await fileViewer.ViewRangeDiffAsync(filename, result.StandardOutput, cancellationToken: cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(item.Item.GrepString))
        {
            IGitCommandConfiguration commandConfiguration = GrepHighlightService.GetGitCommandConfiguration(fileViewer.Module);
            ExecutionResult result = await fileViewer.Module.GetGrepFileAsync(
                    item.SecondRevision.ObjectId,
                    item.Item.Name,
                    fileViewer.GetExtraGrepArguments(),
                    item.Item.GrepString,
                    useGitColoring: true,
                    showFunctionName: true,
                    commandConfiguration: commandConfiguration,
                    fileViewer.Encoding,
                    cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                string output = $"{result.StandardError}{Environment.NewLine}Git command (exit code: {result.ExitCodeDisplay}): {result}{Environment.NewLine}";
                await fileViewer.ViewTextAsync(item?.Item?.Name, text: output, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ViewGrepAsync(item, text: result.StandardOutput, cancellationToken: cancellationToken);
            return;
        }

        if (firstId == ObjectId.CombinedDiffId)
        {
            bool result = fileViewer.Module.GetCombinedDiffContent(item.SecondRevision.ObjectId, item.Item.Name,
                fileViewer.GetExtraDiffArguments(isCombinedDiff: true),
                fileViewer.Encoding,
                out string diffOfConflict,
                useGitColoring: fileViewer.PatchUseGitColoring,
                commandConfiguration: CombinedDiffHighlightService.GetGitCommandConfiguration(fileViewer.Module, AppSettings.UseGitColoring.Value),
                cancellationToken);

            if (!result)
            {
                string output = $"Git command exit code: {result}{Environment.NewLine}{diffOfConflict}";
                await fileViewer.ViewTextAsync(item?.Item?.Name, text: diffOfConflict, cancellationToken: cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(diffOfConflict))
            {
                await fileViewer.ViewTextAsync(item?.Item?.Name, text: TranslatedStrings.UninterestingDiffOmitted, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ViewCombinedDiffAsync(item, text: diffOfConflict, line: line, openWithDifftool: openWithDiffTool, cancellationToken: cancellationToken);
            return;
        }

        if (item.Item.IsSubmodule)
        {
            GitSubmoduleStatus? status = item.Item.GetSubmoduleStatusAsync() is Task<GitSubmoduleStatus?> statusTask

                // Patch already evaluated, normal case for e.g. FileStatusList
                ? await statusTask
                : await SubmoduleHelpers.GetSubmoduleDiffChangesAsync(fileViewer.Module, item.Item.Name, item.Item.OldName, firstId, item.SecondRevision.ObjectId, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            string subText = status is null
                ? $"Failed to get status for submodule \"{item.Item.Name}\""
                : await Task.Run(() => SubmoduleResources.GetSubmoduleStatusText(fileViewer.Module, status))
                    .ConfigureAwait(false);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await fileViewer.ViewPatchAsync(item, text: subText, line: line, openWithDifftool: openWithDiffTool, cancellationToken: cancellationToken);
            return;
        }

        if (AppSettings.DiffDisplayAppearance.Value == GitCommands.Settings.DiffDisplayAppearance.Difftastic && fileViewer.IsDifftasticEnabled.Value)
        {
            bool isTracked = item.Item.IsTracked || (item.Item.TreeGuid is not null && item.SecondRevision.ObjectId is not null);
            (ArgumentString diffArgs, string extraCacheKey) = fileViewer.GetDifftasticArguments();

            // set file name as null to not change the restore lineno
            await fileViewer.ViewTextAsync(fileName: null, $"git difftool {diffArgs} -- {item.Item.Name}", cancellationToken: cancellationToken);

            ExecutionResult result = await fileViewer.Module.GetSingleDifftoolAsync(firstId, item.SecondRevision.ObjectId, item.Item.Name, item.Item.OldName,
                diffArgs,
                cacheResult: true,
                extraCacheKey,
                isTracked,
                useGitColoring: true,
                cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                string output = $"Git command exit code: {result.ExitCodeDisplay}{Environment.NewLine}{result.StandardError}";
                await fileViewer.ViewTextAsync(item?.Item?.Name, text: output, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ViewDifftasticAsync(item.Item.Name, text: result.StandardOutput, cancellationToken: cancellationToken);
            return;
        }

        // diff of text file
        string selectedPatch = (await GetSelectedPatchAsync(fileViewer, firstId, item.SecondRevision.ObjectId, item.Item, cancellationToken))
            ?? defaultText;

        await fileViewer.ViewPatchAsync(item, text: selectedPatch, line: line, openWithDifftool: openWithDiffTool, cancellationToken: cancellationToken);
        return;

        void OpenWithDiffTool()
        {
            fileViewer.Module.OpenWithDifftool(
                item.Item.Name,
                item.Item.OldName,
                firstId?.ToString(),
                item.SecondRevision.ToString(),
                isTracked: item.Item.IsTracked);
        }

        static async Task<string?> GetSelectedPatchAsync(
            FileViewer fileViewer,
            ObjectId firstId,
            ObjectId selectedId,
            GitItemStatus file,
            CancellationToken cancellationToken)
        {
            Patch? patch;
            string? errorMessage;

            IGitModule module = fileViewer.Module;
            bool isSkipWorktree = file.IsSkipWorktree;
            if (isSkipWorktree)
            {
                module.SkipWorktreeFiles([file], skipWorktree: false, out _);
            }

            try
            {
                (patch, errorMessage) = await GetItemPatchAsync(module, file, firstId, selectedId,
                    fileViewer.GetExtraDiffArguments(), fileViewer.PatchUseGitColoring, fileViewer.Encoding, cancellationToken);
            }
            finally
            {
                if (isSkipWorktree)
                {
                    try
                    {
                        file.IsSkipWorktree = false;
                        module.SkipWorktreeFiles([file], skipWorktree: true, out _);
                    }
                    finally
                    {
                        file.IsSkipWorktree = true;
                    }
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            return patch?.Text ?? errorMessage;

            static async Task<(Patch? patch, string? errorMessage)> GetItemPatchAsync(
                IGitModule module,
                GitItemStatus file,
                ObjectId? firstId,
                ObjectId? secondId,
                string diffArgs,
                bool useGitColoring,
                Encoding encoding,
                CancellationToken cancellationToken)
            {
                // Files with tree guid should be presented with normal diff
                bool isTracked = file.IsTracked || (file.TreeGuid is not null && secondId is not null);

                return await module.GetSingleDiffAsync(firstId, secondId, file.Name, file.OldName, diffArgs, encoding, true, isTracked,
                    useGitColoring,
                    PatchHighlightService.GetGitCommandConfiguration(module, useGitColoring),
                    cancellationToken);
            }
        }
    }

    public static void RemoveIfExists(this TabControl tabControl, TabPage page)
    {
        if (tabControl.TabPages.Contains(page))
        {
            tabControl.TabPages.Remove(page);
        }
    }

    public static void InsertIfNotExists(this TabControl tabControl, int index, TabPage page)
    {
        if (!tabControl.TabPages.Contains(page))
        {
            tabControl.TabPages.Insert(index, page);
        }
    }

    public static void Mask(this Control control)
    {
        if (FindMaskPanel(control) is null)
        {
            LoadingControl panel = new()
            {
                Dock = DockStyle.Fill,
                IsAnimating = true,
                BackColor = SystemColors.AppWorkspace
            };
            control.Controls.Add(panel);
            panel.BringToFront();
        }
    }

    public static void UnMask(this Control control)
    {
        LoadingControl panel = FindMaskPanel(control);
        if (panel is not null)
        {
            control.Controls.Remove(panel);
            panel.Dispose();
        }
    }

    private static LoadingControl? FindMaskPanel(Control control)
    {
        return control.Controls.Cast<Control>().OfType<LoadingControl>().FirstOrDefault();
    }

    public static IEnumerable<TreeNode> AllNodes(this TreeView tree)
    {
        return tree.Nodes.AllNodes();
    }

    private static IEnumerable<TreeNode> AllNodes(this TreeNodeCollection nodes)
    {
        foreach (TreeNode node in nodes)
        {
            yield return node;

            foreach (TreeNode subNode in node.Nodes.AllNodes())
            {
                yield return subNode;
            }
        }
    }

    public static Control FindFocusedControl(this ContainerControl container)
    {
        while (true)
        {
            if (container.ActiveControl is ContainerControl activeContainer)
            {
                container = activeContainer;
            }
            else
            {
                return container.ActiveControl;
            }
        }
    }
}
