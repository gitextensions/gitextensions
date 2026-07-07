using System.Collections.Concurrent;
using System.Text;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils.GitUI;
using GitUI.AI;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.Editor.Diff;
using GitUI.UserControls;

namespace GitUI;

partial class FileStatusList
{
    // Diffs are fetched concurrently to speed up large change sets.
    private const int _maxDiffFetchConcurrency = 8;

    // Ask for confirmation before analyzing very large change sets (cost/time guard).
    private const int _largeDiffConfirmThreshold = 150;

    private readonly CancellationTokenSequence _aiFilterSequence = new();

    // The files hidden by the AI filter (consulted by IsFilterMatch).
    private readonly HashSet<GitItemStatus> _aiHiddenItems = [];

    // The last classification result, so category toggles can be re-applied without another API call.
    private readonly Dictionary<GitItemStatus, DiffNoiseCategory> _aiClassifications = [];

    private bool _aiFilterActive;
    private bool _aiFilterRunning;

    private void AiFilter_ButtonClick(object? sender, EventArgs e)
    {
        if (_aiFilterRunning || _aiFilterActive)
        {
            ClearAiFilterState();
            FilterFiles(cboFilterComboBox.Text);
            return;
        }

        DiffNoiseFilterOptions options = DiffNoiseFilterOptions.FromSettings();
        if (!options.AnyEnabled)
        {
            GitExtensions.Extensibility.MessageBoxes.Show(this,
                "No AI filter categories are enabled. Enable at least one category from the button's drop-down menu.",
                "AI filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        RunAiFilter(options);
    }

    private void AiFilter_DropDownOpening(object? sender, EventArgs e)
    {
        tsmiAiFilterImports.Checked = AppSettings.AiFilterImports.Value;
        tsmiAiFilterCallerRenames.Checked = AppSettings.AiFilterCallerSiteRenames.Value;
        tsmiAiFilterSyncToAsync.Checked = AppSettings.AiFilterSyncToAsync.Value;
        tsmiAiFilterStyleOnly.Checked = AppSettings.AiFilterStyleOnly.Value;
    }

    private void AiFilterCategory_Click(object? sender, EventArgs e)
    {
        AppSettings.AiFilterImports.Value = tsmiAiFilterImports.Checked;
        AppSettings.AiFilterCallerSiteRenames.Value = tsmiAiFilterCallerRenames.Checked;
        AppSettings.AiFilterSyncToAsync.Value = tsmiAiFilterSyncToAsync.Checked;
        AppSettings.AiFilterStyleOnly.Value = tsmiAiFilterStyleOnly.Checked;

        // If a classification already exists, re-derive the hidden set from it without a new request.
        if (_aiFilterActive)
        {
            ApplyClassifications(DiffNoiseFilterOptions.FromSettings());
            UpdateAiFilterButton();
            FilterFiles(cboFilterComboBox.Text);
        }
    }

    private void AiFilterConfigure_Click(object? sender, EventArgs e)
    {
        UICommands.StartSettingsDialog(FindForm(), AiFilterSettingsPage.GetPageReference());
    }

    private void RunAiFilter(DiffNoiseFilterOptions options)
    {
        List<FileStatusItem> candidates = AllItems.Where(IsAiFilterCandidate).ToList();
        if (candidates.Count == 0)
        {
            GitExtensions.Extensibility.MessageBoxes.Show(this, "There are no files whose diff can be analyzed by the AI filter.",
                "AI filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (candidates.Count > _largeDiffConfirmThreshold
            && !MessageBoxes.Confirm(this,
                $"The AI filter will analyze {candidates.Count} files. This may take a while and consume API/plan usage. Continue?",
                "AI filter"))
        {
            return;
        }

        CancellationToken cancellationToken = _aiFilterSequence.Next();
        IGitModule module = Module;
        Encoding encoding = module.FilesEncoding;
        Progress<DiffNoiseProgress> progress = new(OnAiFilterProgress);

        _aiFilterRunning = true;
        UpdateAiFilterButton();

        ThreadHelper.FileAndForget(async () =>
        {
            try
            {
                // Phase 1: fetch diffs concurrently and resolve the cheap categories locally (no AI call).
                ConcurrentDictionary<string, DiffNoiseCategory> deterministic = new();
                ConcurrentBag<DiffFileContent> pendingFiles = [];
                ParallelOptions gatherOptions = new() { MaxDegreeOfParallelism = _maxDiffFetchConcurrency, CancellationToken = cancellationToken };
                await Parallel.ForEachAsync(candidates, gatherOptions, async (item, ct) =>
                {
                    string? diff = await GetItemDiffTextAsync(module, item, encoding, extraDiffArguments: "", ct);
                    if (string.IsNullOrWhiteSpace(diff))
                    {
                        return;
                    }

                    string path = item.Item.Name;

                    if (options.Imports && DeterministicDiffNoise.IsImportOnly(path, diff))
                    {
                        deterministic[path] = DiffNoiseCategory.Imports;
                        return;
                    }

                    if (options.StyleOnly && DeterministicDiffNoise.IsWhitespaceInsignificant(path))
                    {
                        string? ignoreWsDiff = await GetItemDiffTextAsync(module, item, encoding, "--ignore-all-space --ignore-blank-lines", ct);
                        if (!DeterministicDiffNoise.DiffHasContentChanges(ignoreWsDiff))
                        {
                            deterministic[path] = DiffNoiseCategory.StyleOnly;
                            return;
                        }
                    }

                    pendingFiles.Add(new DiffFileContent(path, diff));
                });

                // Phase 2: classify the remaining files with AI (skipped entirely if everything was resolved locally).
                List<DiffFileContent> pendingDiffs = [.. pendingFiles];
                IReadOnlyList<DiffNoiseClassification> classifications = pendingDiffs.Count > 0
                    ? await DiffNoiseClassifierFactory.Create().ClassifyAsync(pendingDiffs, options, progress, cancellationToken)
                    : [];

                Dictionary<string, DiffNoiseCategory> byPath = new(deterministic);
                foreach (DiffNoiseClassification classification in classifications)
                {
                    byPath[classification.Path] = classification.Category;
                }

                await this.SwitchToMainThreadAsync(cancellationToken);

                _aiClassifications.Clear();
                foreach (FileStatusItem item in candidates)
                {
                    if (byPath.TryGetValue(item.Item.Name, out DiffNoiseCategory category) && category != DiffNoiseCategory.None)
                    {
                        _aiClassifications[item.Item] = category;
                    }
                }

                _aiFilterRunning = false;
                _aiFilterActive = true;
                ApplyClassifications(options);
                UpdateAiFilterButton();
                FilterFiles(cboFilterComboBox.Text);

                if (_aiHiddenItems.Count == 0)
                {
                    GitExtensions.Extensibility.MessageBoxes.Show(this,
                        "The AI filter did not find any files whose changes are only noise.",
                        "AI filter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelled by the user toggling off or by a new diff being loaded.
            }
            catch (Exception ex)
            {
                Exception error = ex is AggregateException aggregate && aggregate.InnerException is not null
                    ? aggregate.InnerException
                    : ex;
                if (error is OperationCanceledException)
                {
                    return;
                }

                await this.SwitchToMainThreadAsync();
                _aiFilterRunning = false;
                _aiFilterActive = false;
                _aiHiddenItems.Clear();
                _aiClassifications.Clear();
                UpdateAiFilterButton();

                string message = error is DiffNoiseClassifierException
                    ? error.Message
                    : $"The AI filter failed: {error.Message}";
                MessageBoxes.ShowError(this, message, "AI filter");
            }
        });
    }

    private void ApplyClassifications(DiffNoiseFilterOptions options)
    {
        _aiHiddenItems.Clear();
        foreach ((GitItemStatus item, DiffNoiseCategory category) in _aiClassifications)
        {
            if (options.IsHidden(category))
            {
                _aiHiddenItems.Add(item);
            }
        }
    }

    private void OnAiFilterProgress(DiffNoiseProgress progress)
    {
        // Invoked on the UI thread (Progress<T> captures the creating thread's context).
        if (_aiFilterRunning)
        {
            btnAiFilter.Text = progress.TotalFiles > 0
                ? $"{progress.ProcessedFiles}/{progress.TotalFiles}"
                : "…";
        }
    }

    private void UpdateAiFilterButton()
    {
        // Reflect the state as a pressed (checked) button so there is clear visual feedback.
        btnAiFilter.Checked = _aiFilterActive || _aiFilterRunning;

        btnAiFilter.ToolTipText = _aiFilterRunning
            ? "AI filter: analyzing changes…"
            : _aiFilterActive
                ? $"AI filter active — {_aiHiddenItems.Count} file(s) hidden. Click to turn off."
                : "Filter out noise changes using AI";

        if (_aiFilterRunning)
        {
            btnAiFilter.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            btnAiFilter.Text = "…";
        }
        else if (_aiFilterActive)
        {
            btnAiFilter.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            btnAiFilter.Text = _aiHiddenItems.Count.ToString();
        }
        else
        {
            btnAiFilter.DisplayStyle = ToolStripItemDisplayStyle.Image;
            btnAiFilter.Text = string.Empty;
        }
    }

    private void ClearAiFilterState()
    {
        if (!_aiFilterActive && !_aiFilterRunning && _aiHiddenItems.Count == 0 && _aiClassifications.Count == 0)
        {
            return;
        }

        _aiFilterSequence.CancelCurrent();
        _aiFilterActive = false;
        _aiFilterRunning = false;
        _aiHiddenItems.Clear();
        _aiClassifications.Clear();
        UpdateAiFilterButton();
    }

    private static bool IsAiFilterCandidate(FileStatusItem item)
        => item.Item is { IsSubmodule: false, IsRangeDiff: false, IsStatusOnly: false, IsNew: false, IsDeleted: false, IsTracked: true }
           && string.IsNullOrEmpty(item.Item.GrepString);

    private static async Task<string?> GetItemDiffTextAsync(IGitModule module, FileStatusItem item, Encoding encoding, string extraDiffArguments, CancellationToken cancellationToken)
    {
        ObjectId firstId = item.FirstRevision?.ObjectId ?? item.SecondRevision.FirstParentId;
        if (firstId.IsZero)
        {
            return null;
        }

        bool isTracked = item.Item.IsTracked || (!item.Item.TreeId.IsZero && !item.SecondRevision.ObjectId.IsZero);

        (Patch? patch, _) = await module.GetSingleDiffAsync(
            firstId,
            item.SecondRevision.ObjectId,
            item.Item.Name,
            item.Item.OldName,
            extraDiffArguments,
            encoding,
            cacheResult: true,
            isTracked,
            useGitColoring: false,
            PatchHighlightService.GetGitCommandConfiguration(module, useGitColoring: false),
            cancellationToken);

        return patch?.Text;
    }
}
