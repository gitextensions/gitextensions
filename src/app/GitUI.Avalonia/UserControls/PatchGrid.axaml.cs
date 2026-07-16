using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI;

public partial class PatchGrid : GitModuleControl
{
    private readonly TranslationString _unableToShowPatchDetails = new("Unable to show details of patch file.");
    private readonly ICommitDataManager _commitDataManager;
    private IList<PatchFile> _skipped = [];
    private bool _isManagingRebase;

    [GeneratedRegex(@"^(?<header_key>[-A-Za-z0-9]+)(?::[ \t]*)(?<header_value>.*)$", RegexOptions.ExplicitCapture)]
    private static partial Regex HeadersRegex { get; }

    [GeneratedRegex(@"=\?(?<qr1>[\w-]+)\?q\?(?<qr2>.*)\?=$", RegexOptions.ExplicitCapture)]
    private static partial Regex QuotedRegex { get; }

    public PatchGrid()
    {
        _commitDataManager = new CommitDataManager(() => Module);
        InitializeComponent();
        Patches.ItemTemplate = new FuncDataTemplate<PatchFile>(
            (_, _) => new PatchRow(this),
            supportsRecycling: true);
        InitializeComplete();
    }

    public bool IsManagingRebase
    {
        get => _isManagingRebase;
        set
        {
            _isManagingRebase = value;
            UpdateState(value);
        }
    }

    public IReadOnlyList<PatchFile>? PatchFiles { get; private set; }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItems(translation, nameof(Status), "Status");
        AddHeaderTranslationItems(translation, nameof(Action), "Action");
        AddHeaderTranslationItems(translation, nameof(FileName), "Name");
        AddHeaderTranslationItems(translation, nameof(subjectDataGridViewTextBoxColumn), "Subject");
        AddHeaderTranslationItems(translation, nameof(authorDataGridViewTextBoxColumn), "Author");
        AddHeaderTranslationItems(translation, nameof(dateDataGridViewTextBoxColumn), "Date");
        AddHeaderTranslationItems(translation, nameof(CommitHash), "Commit hash");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, nameof(Status), Status, "Status");
        TranslateHeader(translation, nameof(Action), Action, "Action");
        TranslateHeader(translation, nameof(FileName), FileName, "Name");
        TranslateHeader(translation, nameof(subjectDataGridViewTextBoxColumn), subjectDataGridViewTextBoxColumn, "Subject");
        TranslateHeader(translation, nameof(authorDataGridViewTextBoxColumn), authorDataGridViewTextBoxColumn, "Author");
        TranslateHeader(translation, nameof(dateDataGridViewTextBoxColumn), dateDataGridViewTextBoxColumn, "Date");
        TranslateHeader(translation, nameof(CommitHash), CommitHash, "Commit hash");
    }

    public void Initialize()
    {
        UpdateState(IsManagingRebase);
        DisplayPatches(GetPatches());
    }

    public void RefreshGrid()
    {
        Validates.NotNull(PatchFiles);
        IReadOnlyList<PatchFile> currentPatches = PatchFiles!;

        IReadOnlyList<PatchFile> updatedPatches = GetPatches();
        for (int i = 0; i < Math.Min(updatedPatches.Count, currentPatches.Count); i++)
        {
            updatedPatches[i].IsSkipped = currentPatches[i].IsSkipped;
        }

        DisplayPatches(updatedPatches);
    }

    public void SelectCurrentlyApplyingPatch()
    {
        PatchFile? applyingPatch = PatchFiles?.FirstOrDefault(patchFile => patchFile.IsNext);
        if (applyingPatch is null)
        {
            return;
        }

        Patches.SelectedItem = applyingPatch;
        Patches.ScrollIntoView(applyingPatch);
    }

    public void SetSkipped(IList<PatchFile> skipped)
    {
        ArgumentNullException.ThrowIfNull(skipped);
        _skipped = skipped;
    }

    private IReadOnlyList<PatchFile> GetInteractiveRebasePatchFiles()
    {
        string rebaseDir = Module.GetRebaseDir();
        string currentFilePath = $"{rebaseDir}stopped-sha";
        string doneFilePath = $"{rebaseDir}done";
        string rebaseTodoFilePath = $"{rebaseDir}git-rebase-todo";

        string[] doneCommits = ReadCommitsDataFromRebaseFile(doneFilePath);
        string[] todoCommits = ReadCommitsDataFromRebaseFile(rebaseTodoFilePath);
        string commentChar = Module.GetEffectiveSetting("core.commentchar", defaultValue: "#");
        string[][] commitsInfos = [.. doneCommits.Concat(todoCommits)
            .Where(line => !line.StartsWith(commentChar, StringComparison.Ordinal))
            .Select(line => line.Split(Delimiters.Space))
            .Where(parts => parts.Length >= 3)];

        List<PatchFile> patchFiles = [];
        if (commitsInfos.Length == 0)
        {
            return patchFiles;
        }

        RevisionReader reader = new(Module, allBodies: true);
        Dictionary<string, GitRevision> rebasedCommitsRevisions;
        try
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            rebasedCommitsRevisions = reader.GetRevisionsFromRange(commitsInfos[0][1], commitsInfos[^1][1], cts.Token)
                .ToDictionary(revision => revision.Guid, revision => revision);
        }
        catch (OperationCanceledException)
        {
            rebasedCommitsRevisions = [];
        }

        string? currentCommitShortHash = File.Exists(currentFilePath)
            ? File.ReadAllText(currentFilePath).Trim()
            : null;
        bool isCurrentFound = false;
        foreach (string[] parts in commitsInfos)
        {
            string commitHash = parts[1];
            CommitData? data = rebasedCommitsRevisions.TryGetValue(commitHash, out GitRevision? commitRevision)
                ? _commitDataManager.CreateFromRevision(commitRevision, null)
                : _commitDataManager.GetCommitData(commitHash);
            bool isApplying = currentCommitShortHash is not null
                && commitHash.StartsWith(currentCommitShortHash, StringComparison.Ordinal);
            isCurrentFound |= isApplying;

            ObjectId objectId;
            if (data is not null)
            {
                objectId = data.ObjectId;
            }
            else if (!ObjectId.TryParse(commitHash, out objectId))
            {
                Trace.Write($"PatchGrid: unable to parse interactive rebase commit '{commitHash}'.");
                continue;
            }

            patchFiles.Add(new PatchFile
            {
                Action = parts[0],
                ObjectId = objectId,
                Subject = data?.Body ?? string.Join(' ', parts.Skip(2)),
                Author = data?.Author,
                Date = data?.CommitDate.LocalDateTime.ToString(),
                IsNext = isApplying,
                IsApplied = !isCurrentFound,
            });
        }

        return patchFiles;

        static string[] ReadCommitsDataFromRebaseFile(string filePath)
            => File.Exists(filePath)
                ? File.ReadAllText(filePath).Trim().Split(
                    Delimiters.LineFeedAndCarriageReturn,
                    StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                : [];
    }

    private void DisplayPatches(IReadOnlyList<PatchFile> patchFiles)
    {
        PatchFiles = patchFiles;
        Patches.ItemsSource = patchFiles.ToArray();
        SelectCurrentlyApplyingPatch();
    }

    private IReadOnlyList<PatchFile> GetPatches()
    {
        string rebaseTodoFilePath = $"{Module.GetRebaseDir()}git-rebase-todo";
        IReadOnlyList<PatchFile> patches = File.Exists(rebaseTodoFilePath)
            ? GetInteractiveRebasePatchFiles()
            : GetRebasePatchFiles();

        if (_skipped.Count == 0)
        {
            return patches;
        }

        IEnumerable<PatchFile> skippedPatches = patches
            .TakeWhile(patchFile => !patchFile.IsNext)
            .Where(patchFile => _skipped.Any(skipped =>
                patchFile.ObjectId == skipped.ObjectId
                && patchFile.Name == skipped.Name));
        foreach (PatchFile patchFile in skippedPatches)
        {
            patchFile.IsSkipped = true;
        }

        return patches;
    }

    private IReadOnlyList<PatchFile> GetRebasePatchFiles()
    {
        List<PatchFile> patchFiles = [];
        string rebaseDir = Module.GetRebaseDir();
        string nextFilePath = $"{rebaseDir}next";
        string nextFile = File.Exists(nextFilePath) ? File.ReadAllText(nextFilePath).Trim() : string.Empty;
        if (!int.TryParse(nextFile, out int next))
        {
            next = 0;
        }

        string[] files = Directory.Exists(rebaseDir) ? Directory.GetFiles(rebaseDir) : [];
        foreach (string fullFileName in files)
        {
            string file = PathUtil.GetFileName(fullFileName);
            if (!int.TryParse(file, out int number))
            {
                continue;
            }

            PatchFile patchFile = new()
            {
                Name = file,
                FullName = fullFileName,
                IsApplied = number < next,
                IsNext = number == next,
            };
            PopulatePatchHeaders(patchFile);
            patchFiles.Add(patchFile);
        }

        return patchFiles;
    }

    private static void PopulatePatchHeaders(PatchFile patchFile)
    {
        if (string.IsNullOrEmpty(patchFile.FullName) || !File.Exists(patchFile.FullName))
        {
            return;
        }

        string? key = null;
        string value = string.Empty;
        foreach (string line in File.ReadLines(patchFile.FullName))
        {
            Match match = HeadersRegex.Match(line);
            if (key is null)
            {
                if (!string.IsNullOrWhiteSpace(line) && !match.Success)
                {
                    continue;
                }
            }
            else if (string.IsNullOrWhiteSpace(line) || match.Success)
            {
                value = Attachment.CreateAttachmentFromString(string.Empty, value).Name!;
                switch (key)
                {
                    case "From":
                        if (value.IndexOf('<') > 0)
                        {
                            string author = RFC2047Decoder.Parse(value);
                            patchFile.Author = author[..author.IndexOf('<')].Trim();
                        }
                        else
                        {
                            patchFile.Author = value;
                        }

                        break;
                    case "Date":
                        patchFile.Date = value.IndexOf('+') > 0
                            ? value[..value.IndexOf('+')].Trim()
                            : value;
                        break;
                    case "Subject":
                        patchFile.Subject = value;
                        break;
                }
            }

            if (match.Success)
            {
                key = match.Groups["header_key"].Value;
                value = match.Groups["header_value"].Value;
            }
            else if (!string.IsNullOrEmpty(line))
            {
                value = AppendQuotedString(value, line.Trim());
            }

            if (string.IsNullOrEmpty(line)
                || (!string.IsNullOrEmpty(patchFile.Author)
                    && !string.IsNullOrEmpty(patchFile.Date)
                    && !string.IsNullOrEmpty(patchFile.Subject)))
            {
                break;
            }
        }
    }

    private static string AppendQuotedString(string first, string second)
    {
        Match firstMatch = QuotedRegex.Match(first);
        Match secondMatch = QuotedRegex.Match(second);
        if (!firstMatch.Success || !secondMatch.Success)
        {
            return first + second;
        }

        return $"{first.AsSpan()[..^2]}{secondMatch.Groups["qr2"].ValueSpan}?=";
    }

    private void UpdateState(bool isManagingRebase)
    {
        Action.IsVisible = isManagingRebase;
        FileName.IsVisible = !isManagingRebase;
        CommitHash.IsVisible = isManagingRebase;
        columnsGrid.ColumnDefinitions[1].Width = new GridLength(isManagingRebase ? 82 : 0);
        columnsGrid.ColumnDefinitions[2].Width = new GridLength(isManagingRebase ? 0 : 70);
        columnsGrid.ColumnDefinitions[6].Width = new GridLength(isManagingRebase ? 110 : 0);
        Patches.ItemTemplate = new FuncDataTemplate<PatchFile>(
            (_, _) => new PatchRow(this),
            supportsRecycling: true);
    }

    private static void AddHeaderTranslationItems(ITranslation translation, string fieldName, string text)
    {
        translation.AddTranslationItem(nameof(PatchGrid), fieldName, "HeaderText", text);
    }

    private static void TranslateHeader(
        ITranslation translation,
        string fieldName,
        Border header,
        string defaultText)
    {
        string? text = translation.TranslateItem(
            nameof(PatchGrid),
            fieldName,
            "HeaderText",
            () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor()
        => new(this);

    internal readonly struct TestAccessor
    {
        private readonly PatchGrid _control;

        public TestAccessor(PatchGrid control)
        {
            _control = control;
        }

        public IReadOnlyList<PatchFile> GetInteractiveRebasePatchFiles()
            => _control.GetInteractiveRebasePatchFiles();
    }

    private sealed class PatchRow : Grid
    {
        private readonly PatchGrid _owner;

        public PatchRow(PatchGrid owner)
        {
            _owner = owner;
            MinWidth = 700;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            Children.Clear();
            if (DataContext is not PatchFile patchFile)
            {
                return;
            }

            ColumnDefinitions = new ColumnDefinitions(
                _owner.IsManagingRebase
                    ? "82,82,0,*,150,140,110"
                    : "82,0,70,*,150,140,0");
            AddCell(patchFile.Status, 0, patchFile.IsNext ? Brushes.OrangeRed : null);
            AddCell(patchFile.Action, 1, null, _owner.IsManagingRebase);
            AddCell(patchFile.Name, 2, null, !_owner.IsManagingRebase);
            AddCell(patchFile.Subject, 3);
            AddCell(patchFile.Author, 4);
            AddCell(patchFile.Date, 5);
            AddCell(patchFile.ObjectId.IsZero ? string.Empty : patchFile.ObjectId.ToShortString(), 6, null, _owner.IsManagingRebase);
        }

        private void AddCell(string? text, int column, IBrush? foreground = null, bool visible = true)
        {
            TextBlock textBlock = new()
            {
                Text = text ?? string.Empty,
                Margin = new Avalonia.Thickness(6, 4),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Foreground = foreground,
                IsVisible = visible,
            };
            SetColumn(textBlock, column);
            Children.Add(textBlock);
        }
    }
}
