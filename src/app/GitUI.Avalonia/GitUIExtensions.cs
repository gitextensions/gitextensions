using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI.Compat;
using GitUI.Editor;
using GitUI.Editor.Diff;
using GitUI.Theming;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using Microsoft.VisualStudio.Threading;
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
        string additionalCommandInfo = null!,
        bool forceFileView = false)
    {
        bool patchUseGitColoring = await fileViewer.GetOnOwnerMainThreadAsync(
            () => fileViewer.PatchUseGitColoring,
            cancellationToken);

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

        ObjectId firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;

        openWithDiffTool ??= OpenWithDiffTool;

        if (forceFileView || (!item.Item.IsSubmodule && (item.Item.IsNew || firstId.IsZero || (!item.Item.IsDeleted && FileHelper.IsImage(item.Item.Name)))))
        {
            // View blob guid from revision, or file for worktree
            await fileViewer.ViewGitItemAsync(item, line, openWithDiffTool, cancellationToken: cancellationToken);
            return;
        }

        if (item.Item.IsRangeDiff)
        {
            // Git range-diff has cubic runtime complexity and can be slow and memory consuming,
            // give an indication of what is going on
            string range = item.BaseA.IsZero || item.BaseB.IsZero
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
                await fileViewer.ViewTextAsync(item.Item.Name, text: output, cancellationToken: cancellationToken);
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
                await fileViewer.ViewTextAsync(item.Item.Name, text: output, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ViewGrepAsync(item, text: result.StandardOutput, cancellationToken: cancellationToken);
            return;
        }

        if (firstId == ObjectId.CombinedDiffId)
        {
            bool result = fileViewer.Module.GetCombinedDiffContent(
                item.SecondRevision.ObjectId,
                item.Item.Name,
                fileViewer.GetExtraDiffArguments(isCombinedDiff: true),
                fileViewer.Encoding,
                out string diffOfConflict,
                useGitColoring: patchUseGitColoring,
                commandConfiguration: CombinedDiffHighlightService.GetGitCommandConfiguration(fileViewer.Module, AppSettings.UseGitColoring.Value),
                cancellationToken);

            if (!result)
            {
                await fileViewer.ViewTextAsync(item.Item.Name, text: diffOfConflict, cancellationToken: cancellationToken);
                return;
            }

            if (string.IsNullOrWhiteSpace(diffOfConflict))
            {
                await fileViewer.ViewTextAsync(item.Item.Name, text: TranslatedStrings.UninterestingDiffOmitted, cancellationToken: cancellationToken);
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
                : await Task.Run(() => SubmoduleResources.GetSubmoduleStatusText(fileViewer.Module, status), cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await fileViewer.ViewPatchAsync(item, text: subText, line: line, openWithDifftool: openWithDiffTool, cancellationToken: cancellationToken);
            return;
        }

        if (AppSettings.DiffDisplayAppearance.Value == GitCommands.Settings.DiffDisplayAppearance.Difftastic && fileViewer.IsDifftasticEnabled.Value)
        {
            bool isTracked = item.Item.IsTracked || (!item.Item.TreeId.IsZero && !item.SecondRevision.ObjectId.IsZero);
            (ArgumentString diffArgs, string extraCacheKey) = fileViewer.GetDifftasticArguments();

            // set file name as null to not change the restore lineno
            await fileViewer.ViewTextAsync(fileName: null, $"git difftool {diffArgs} -- {item.Item.Name}", cancellationToken: cancellationToken);

            ExecutionResult result = await fileViewer.Module.GetSingleDifftoolAsync(
                firstId,
                item.SecondRevision.ObjectId,
                item.Item.Name,
                item.Item.OldName,
                diffArgs,
                cacheResult: true,
                extraCacheKey,
                isTracked,
                useGitColoring: true,
                cancellationToken);

            if (!result.ExitedSuccessfully)
            {
                string output = $"Git command exit code: {result.ExitCodeDisplay}{Environment.NewLine}{result.StandardError}";
                await fileViewer.ViewTextAsync(item.Item.Name, text: output, cancellationToken: cancellationToken);
                return;
            }

            await fileViewer.ViewDifftasticAsync(item.Item.Name, text: result.StandardOutput, cancellationToken: cancellationToken);
            return;
        }

        // diff of text file
        string selectedPatch = (await GetSelectedPatchAsync(
                fileViewer,
                firstId,
                item.SecondRevision.ObjectId,
                item.Item,
                patchUseGitColoring,
                cancellationToken))
            ?? defaultText;

        await fileViewer.ViewPatchAsync(item, text: selectedPatch, line: line, openWithDifftool: openWithDiffTool, cancellationToken: cancellationToken);

        void OpenWithDiffTool()
        {
            fileViewer.Module.OpenWithDifftool(
                item.Item.Name,
                item.Item.OldName,
                firstId.IsZero ? null : firstId.ToString(),
                item.SecondRevision.ObjectId.ToString(),
                isTracked: item.Item.IsTracked);
        }

        static async Task<string?> GetSelectedPatchAsync(
            FileViewer fileViewer,
            ObjectId firstId,
            ObjectId selectedId,
            GitItemStatus file,
            bool patchUseGitColoring,
            CancellationToken cancellationToken)
        {
            IGitModule module = fileViewer.Module;
            bool isSkipWorktree = file.IsSkipWorktree;
            if (isSkipWorktree)
            {
                module.SkipWorktreeFiles([file], skipWorktree: false, out _);
            }

            try
            {
                // Files with tree guid should be presented with normal diff
                bool isTracked = file.IsTracked || (!file.TreeId.IsZero && !selectedId.IsZero);
                (Patch? patch, string? errorMessage) = await module.GetSingleDiffAsync(
                    firstId,
                    selectedId,
                    file.Name,
                    file.OldName,
                    fileViewer.GetExtraDiffArguments(),
                    fileViewer.Encoding,
                    cacheResult: true,
                    isTracked,
                    patchUseGitColoring,
                    PatchHighlightService.GetGitCommandConfiguration(module, patchUseGitColoring),
                    cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                return patch?.Text ?? errorMessage;
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
        }
    }

    public static void Mask(this Control control)
    {
        Panel? host = FindMaskHost(control);
        if (host is not null && FindMaskPanel(control) is null)
        {
            LoadingControl panel = new()
            {
                IsAnimating = true,
                ZIndex = int.MaxValue,
                Background = new SolidColorBrush(AvaloniaThemeResources.ToMediaColor(
                    AvaloniaThemeResources.ResolveSystemColor(ThemeModule.Settings, System.Drawing.KnownColor.AppWorkspace))),
            };

            if (host is Grid grid)
            {
                Grid.SetRowSpan(panel, Math.Max(1, grid.RowDefinitions.Count));
                Grid.SetColumnSpan(panel, Math.Max(1, grid.ColumnDefinitions.Count));
            }

            host.Children.Add(panel);
        }
    }

    public static void UnMask(this Control control)
    {
        Panel? host = FindMaskHost(control);
        LoadingControl? panel = FindMaskPanel(control);
        if (host is not null && panel is not null)
        {
            panel.IsAnimating = false;
            host.Children.Remove(panel);
        }
    }

    private static LoadingControl? FindMaskPanel(Control control)
        => FindMaskHost(control)?.Children.OfType<LoadingControl>().FirstOrDefault();

    // Avalonia exposes a window's child tree through Content instead of Control.Controls.
    private static Panel? FindMaskHost(Control control)
        => control as Panel ?? (control as ContentControl)?.Content as Panel;
}
