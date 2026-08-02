using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitUI;
using ResourceManager;
using AvaloniaCheckBox = Avalonia.Controls.CheckBox;
using AvaloniaControl = Avalonia.Controls.Control;
using AvaloniaListBox = Avalonia.Controls.ListBox;
using MessageBoxes = GitUI.MessageBoxes;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitExtensions.Plugins.FindLargeFiles;

public sealed partial class FindLargeFilesForm : GitExtensionsFormBase
{
    private readonly TranslationString _areYouSureToDelete = new("Are you sure to delete the selected files?");
    private readonly TranslationString _deleteCaption = new("Delete");

    private readonly float _threshold;
    private readonly IGitUICommands? _commands;
    private readonly IGitModule? _gitModule;
    private readonly Dictionary<string, GitObject> _list = [];
    private readonly List<GitObject> _gitObjects = [];
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private readonly CancellationTokenSource _scanCancellation = new();
    private string? _sortColumn;
    private bool _sortAscending = true;

    public FindLargeFilesForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FindLargeFilesForm(float threshold, IGitUICommands? commands)
    {
        _threshold = threshold;
        _commands = commands;
        _gitModule = commands?.Module;

        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        BranchesGrid.ItemTemplate = new FuncDataTemplate<GitObject>(CreateGitObjectRow, supportsRecycling: false);
        sHADataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        pathDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        sizeDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        CompressedSize.PointerReleased += Header_PointerReleased;
        commitCountDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        lastCommitDateDataGridViewTextBoxColumn.PointerReleased += Header_PointerReleased;
        Delete.Click += Delete_Click;
        Cancel.Click += Cancel_Click;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (_gitModule is not null)
        {
            Delete.IsEnabled = false;
            _operations.FileAndForget(() => FindLargeFilesFunctionAsync(_scanCancellation.Token));
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _scanCancellation.Cancel();
        _operations.JoinPendingOperations();
        _scanCancellation.Dispose();
        base.OnClosed(e);
    }

    private AvaloniaControl CreateGitObjectRow(GitObject? gitObject, INameScope? nameScope)
    {
        Grid row = new()
        {
            ColumnDefinitions = new ColumnDefinitions("54,*,52,100,88,103,70"),
        };
        if (gitObject is null)
        {
            return row;
        }

        row.Children.Add(CreateCell(gitObject.SHA, column: 0));
        row.Children.Add(CreateCell(gitObject.Path, column: 1));
        row.Children.Add(CreateCell(gitObject.Size, column: 2));
        row.Children.Add(CreateCell(gitObject.CompressedSize, column: 3));
        row.Children.Add(CreateCell(gitObject.CommitCount.ToString(), column: 4));
        row.Children.Add(CreateCell(gitObject.LastCommitDate.ToString(), column: 5));

        AvaloniaCheckBox delete = new()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsChecked = gitObject.Delete,
        };
        delete.IsCheckedChanged += (_, _) => gitObject.Delete = delete.IsChecked == true;
        Grid.SetColumn(delete, 6);
        row.Children.Add(delete);
        return row;

        static TextBlock CreateCell(string text, int column)
        {
            TextBlock cell = new()
            {
                Text = text,
                Margin = new Avalonia.Thickness(6, 2),
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(cell, column);
            return cell;
        }
    }

    private async Task FindLargeFilesFunctionAsync(CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<GitObject> gitObjects = await FindLargeFilesAsync(cancellationToken);
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            _gitObjects.Clear();
            _gitObjects.AddRange(gitObjects);
            RefreshRows();
        }
        catch (OperationCanceledException)
        {
            // The owning dialog is closing.
        }
        catch (Exception ex)
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            MessageBoxes.ShowError(this, ex.Message);
        }
        finally
        {
            await _operations.JoinableTaskFactory.SwitchToMainThreadAsync();
            pbRevisions.IsVisible = false;
            BranchesGrid.IsEnabled = true;
            Delete.IsEnabled = true;
        }
    }

    private async Task<IReadOnlyList<GitObject>> FindLargeFilesAsync(CancellationToken cancellationToken)
    {
        IGitModule gitModule = GetGitModule();
        _list.Clear();

        GitArgumentBuilder revListArguments = new("rev-list") { "HEAD" };
        string[] revList = (await GetOutputAsync(gitModule, revListArguments, cancellationToken))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        await SetProgressAsync(value: 0, maximum: Math.Max(1, (int)(revList.Length * 1.1f)), cancellationToken);

        Dictionary<string, DateTime> revData = [];
        int thresholdSize = (int)(_threshold * 1024 * 1024);
        for (int i = 0; i < revList.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await SetProgressAsync(i, maximum: null, cancellationToken);

            string rev = revList[i];
            GitArgumentBuilder lsTreeArguments = new("ls-tree")
            {
                "-zrl",
                rev.Quote(),
            };
            string[] objects = (await GetOutputAsync(gitModule, lsTreeArguments, cancellationToken))
                .Split(['\0'], StringSplitOptions.RemoveEmptyEntries);
            foreach (string objectData in objects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int pathSeparator = objectData.IndexOf('\t');
                if (pathSeparator < 0)
                {
                    continue;
                }

                string[] data = objectData[..pathSeparator].Split([' '], count: 4, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length != 4
                    || data[1] != "blob"
                    || !int.TryParse(data[3], out int size)
                    || size < thresholdSize)
                {
                    continue;
                }

                string path = objectData[(pathSeparator + 1)..];
                GitObject gitObject = new(data[2], path, size, rev);
                DateTime date = await GetRevisionDateAsync(gitModule, rev, revData, cancellationToken);
                if (!_list.TryGetValue(gitObject.SHA, out GitObject? currentGitObject))
                {
                    gitObject.LastCommitDate = date;
                    _list.Add(gitObject.SHA, gitObject);
                }
                else if (currentGitObject.Commit.Add(rev) && currentGitObject.LastCommitDate < date)
                {
                    currentGitObject.LastCommitDate = date;
                }
            }
        }

        string objectsPackDirectory = gitModule.ResolveGitInternalPath("objects/pack/");
        if (Directory.Exists(objectsPackDirectory))
        {
            string[] packFiles = Directory.GetFiles(objectsPackDirectory, "pack-*.idx");
            foreach (string pack in packFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                GitArgumentBuilder verifyPackArguments = new("verify-pack")
                {
                    "-v",
                    pack.Quote(),
                };
                string[] objects = (await GetOutputAsync(gitModule, verifyPackArguments, cancellationToken)).Split('\n');
                await AdvancePackProgressAsync(revList.Length, packFiles.Length, cancellationToken);
                foreach (string objectData in objects.Where(item => item.Contains(" blob ", StringComparison.Ordinal)))
                {
                    string[] dataFields = objectData.Split([' '], StringSplitOptions.RemoveEmptyEntries);
                    if (dataFields.Length > 3
                        && _list.TryGetValue(dataFields[0], out GitObject? currentGitObject)
                        && int.TryParse(dataFields[3], out int compressedSize))
                    {
                        currentGitObject.CompressedSizeInBytes = compressedSize;
                    }
                }
            }
        }

        return [.. _list.Values];
    }

    private static async Task<string> GetOutputAsync(
        IGitModule gitModule,
        ArgumentString arguments,
        CancellationToken cancellationToken)
    {
        ExecutionResult result = await gitModule.GitExecutable.ExecuteAsync(
            arguments,
            cancellationToken: cancellationToken);
        return result.StandardOutput;
    }

    private static async Task<DateTime> GetRevisionDateAsync(
        IGitModule gitModule,
        string revision,
        Dictionary<string, DateTime> revData,
        CancellationToken cancellationToken)
    {
        if (revData.TryGetValue(revision, out DateTime date))
        {
            return date;
        }

        GitArgumentBuilder arguments = new("show")
        {
            "-s",
            revision,
            "--format=\"%ci\"",
        };
        string revDate = await GetOutputAsync(gitModule, arguments, cancellationToken);
        if (!DateTime.TryParse(revDate, out date))
        {
            Trace.WriteLine($"Could not parse date '{revDate}' for commit '{revision}'");
            date = DateTime.MinValue;
        }

        revData.Add(revision, date);
        return date;
    }

    private async Task SetProgressAsync(int value, int? maximum, CancellationToken cancellationToken)
    {
        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        if (maximum.HasValue)
        {
            pbRevisions.Maximum = maximum.Value;
        }

        pbRevisions.Value = Math.Min(value, pbRevisions.Maximum);
    }

    private async Task AdvancePackProgressAsync(
        int revisionCount,
        int packFileCount,
        CancellationToken cancellationToken)
    {
        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        pbRevisions.Value = Math.Min(
            pbRevisions.Maximum,
            pbRevisions.Value + ((revisionCount * 0.1f) / packFileCount));
    }

    private void Header_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        string column = ReferenceEquals(sender, sHADataGridViewTextBoxColumn)
            ? nameof(GitObject.SHA)
            : ReferenceEquals(sender, pathDataGridViewTextBoxColumn)
                ? nameof(GitObject.Path)
                : ReferenceEquals(sender, sizeDataGridViewTextBoxColumn)
                    ? nameof(GitObject.Size)
                    : ReferenceEquals(sender, CompressedSize)
                        ? nameof(GitObject.CompressedSize)
                        : ReferenceEquals(sender, commitCountDataGridViewTextBoxColumn)
                            ? nameof(GitObject.CommitCount)
                            : nameof(GitObject.LastCommitDate);
        SortBy(column);
        e.Handled = true;
    }

    private void SortBy(string column)
    {
        _sortAscending = _sortColumn != column || !_sortAscending;
        _sortColumn = column;
        RefreshRows();
    }

    private void RefreshRows()
    {
        IEnumerable<GitObject> rows = _sortColumn switch
        {
            nameof(GitObject.SHA) => _gitObjects.OrderBy(item => item.SHA, StringComparer.Ordinal),
            nameof(GitObject.Path) => _gitObjects.OrderBy(item => item.Path, StringComparer.CurrentCulture),
            nameof(GitObject.Size) => _gitObjects.OrderBy(item => item.SizeInBytes),
            nameof(GitObject.CompressedSize) => _gitObjects.OrderBy(item => item.CompressedSizeInBytes),
            nameof(GitObject.CommitCount) => _gitObjects.OrderBy(item => item.CommitCount),
            nameof(GitObject.LastCommitDate) => _gitObjects.OrderBy(item => item.LastCommitDate),
            _ => _gitObjects,
        };
        if (!_sortAscending && _sortColumn is not null)
        {
            rows = rows.Reverse();
        }

        BranchesGrid.ItemsSource = rows.ToList();
    }

    private static string GenerateCommand(IEnumerable<GitObject> gitObjects, bool useWindowsBatch)
        => useWindowsBatch ? GenerateWindowsCommand(gitObjects) : GeneratePosixCommand(gitObjects);

    private static string GenerateWindowsCommand(IEnumerable<GitObject> gitObjects)
    {
        StringBuilder sb = new();
        sb.AppendLine($"SET gitexe=\"{AppSettings.GitCommand}\"");

        foreach (GitObject gitObject in gitObjects.Where(item => item.Delete))
        {
            string path = gitObject.Path.Replace("%", "%%", StringComparison.Ordinal);
            string quotedPath = $"'{path.Replace("'", "'\\''", StringComparison.Ordinal)}'";
            sb.AppendLine($"%gitexe% filter-branch --index-filter \"git rm -r -f --cached --ignore-unmatch {quotedPath}\" --prune-empty -- --all");
        }

        sb.AppendLine("for /f \"usebackq\" %%a IN (`\"%gitexe% for-each-ref --format=\"%%^(refname^)\" refs/original/\"`) DO %gitexe% update-ref -d %%a");
        sb.AppendLine("%gitexe% reflog expire --expire=now --all");
        sb.AppendLine("%gitexe% gc --aggressive --prune=now");
        return sb.ToString();
    }

    private static string GeneratePosixCommand(IEnumerable<GitObject> gitObjects)
    {
        StringBuilder sb = new();
        sb.Append("#!/bin/sh\nset -e\n");
        sb.Append("gitexe=").Append(QuoteForPosixShell(AppSettings.GitCommand)).Append('\n');

        foreach (GitObject gitObject in gitObjects.Where(item => item.Delete))
        {
            sb.Append("\"$gitexe\" filter-branch --index-filter ")
                .Append(QuoteForPosixShell(
                    $"git rm -r -f --cached --ignore-unmatch -- {QuoteForPosixShell(gitObject.Path)}"))
                .Append(" --prune-empty -- --all\n");
        }

        sb.Append("\"$gitexe\" for-each-ref --format='%(refname)' refs/original/ | while IFS= read -r ref; do\n");
        sb.Append("  \"$gitexe\" update-ref -d \"$ref\"\n");
        sb.Append("done\n");
        sb.Append("\"$gitexe\" reflog expire --expire=now --all\n");
        sb.Append("\"$gitexe\" gc --aggressive --prune=now\n");
        return sb.ToString();
    }

    private static string QuoteForPosixShell(string value)
        => $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";

    private void Delete_Click(object? sender, EventArgs e)
    {
        if (MessageBoxes.Show(
                this,
                _areYouSureToDelete.Text,
                _deleteCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning) == WinFormsShims.DialogResult.Yes)
        {
            GetCommands().StartBatchFileProcessDialog(
                GenerateCommand(_gitObjects, OperatingSystem.IsWindows()));
        }

        Close();
    }

    private void Cancel_Click(object? sender, EventArgs e)
    {
        DialogResult = WinFormsShims.DialogResult.Cancel;
    }

    private IGitUICommands GetCommands()
        => _commands ?? throw new InvalidOperationException($"{nameof(FindLargeFilesForm)} was constructed incorrectly.");

    private IGitModule GetGitModule()
        => _gitModule ?? throw new InvalidOperationException($"{nameof(FindLargeFilesForm)} was constructed incorrectly.");

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        AddHeaderTranslationItem(translation, nameof(sHADataGridViewTextBoxColumn), "SHA");
        AddHeaderTranslationItem(translation, nameof(pathDataGridViewTextBoxColumn), "Path");
        AddHeaderTranslationItem(translation, nameof(sizeDataGridViewTextBoxColumn), "Size");
        AddHeaderTranslationItem(translation, nameof(CompressedSize), "Compressed size");
        AddHeaderTranslationItem(translation, nameof(commitCountDataGridViewTextBoxColumn), "Commit count");
        AddHeaderTranslationItem(translation, nameof(lastCommitDateDataGridViewTextBoxColumn), "Last commit date");
        AddHeaderTranslationItem(translation, nameof(dataGridViewCheckBoxColumn1), "Delete");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, sHADataGridViewTextBoxColumn, nameof(sHADataGridViewTextBoxColumn), "SHA");
        TranslateHeader(translation, pathDataGridViewTextBoxColumn, nameof(pathDataGridViewTextBoxColumn), "Path");
        TranslateHeader(translation, sizeDataGridViewTextBoxColumn, nameof(sizeDataGridViewTextBoxColumn), "Size");
        TranslateHeader(translation, CompressedSize, nameof(CompressedSize), "Compressed size");
        TranslateHeader(translation, commitCountDataGridViewTextBoxColumn, nameof(commitCountDataGridViewTextBoxColumn), "Commit count");
        TranslateHeader(translation, lastCommitDateDataGridViewTextBoxColumn, nameof(lastCommitDateDataGridViewTextBoxColumn), "Last commit date");
        TranslateHeader(translation, dataGridViewCheckBoxColumn1, nameof(dataGridViewCheckBoxColumn1), "Delete");
    }

    private static void AddHeaderTranslationItem(ITranslation translation, string fieldName, string text)
        => translation.AddTranslationItem(nameof(FindLargeFilesForm), fieldName, "HeaderText", text);

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(
            nameof(FindLargeFilesForm),
            fieldName,
            "HeaderText",
            () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FindLargeFilesForm form)
    {
        public IReadOnlyList<GitObject> GitObjects => form._gitObjects;
        public AvaloniaListBox BranchesGrid => form.BranchesGrid;

        public Task<IReadOnlyList<GitObject>> FindLargeFilesAsync(CancellationToken cancellationToken = default)
            => form.FindLargeFilesAsync(cancellationToken);

        public string GenerateCommandForTesting(IEnumerable<GitObject> gitObjects, bool useWindowsBatch)
            => FindLargeFilesForm.GenerateCommand(gitObjects, useWindowsBatch);

        public string QuoteForPosixShellForTesting(string value)
            => FindLargeFilesForm.QuoteForPosixShell(value);

        public void SortByPath() => form.SortBy(nameof(GitObject.Path));
    }
}
