using System.Text;
using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Plugins.GitStatistics.PieChart;
using GitExtUtils;
using GitUI;
using ResourceManager;
using AvaloniaApplication = Avalonia.Application;
using Color = Avalonia.Media.Color;

namespace GitExtensions.Plugins.GitStatistics;

public partial class FormGitStatistics : GitExtensionsFormBase
{
    private const int FileBatchSize = 128;

    private readonly TranslationString _commits = new("{0:N0} Commits");
    private readonly TranslationString _commitsBy = new("{0:N0} Commits by {1}");
    private readonly TranslationString _linesOfCodeInFiles = new("{0:N0} Lines of code in {1} files ({2:P1})");
    private readonly TranslationString _linesOfCode = new("{0:N0} Lines of code");
    private readonly TranslationString _linesOfCodeP = new("{0:N0} Lines of code ({1:P1})");
    private readonly TranslationString _linesOfTestCode = new("{0:N0} Lines of test code");
    private readonly TranslationString _linesOfTestCodeP = new("{0:N0} Lines of test code ({1:P1})");
    private readonly TranslationString _linesOfProductionCodeP = new("{0:N0} Lines of production code ({1:P1})");
    private readonly TranslationString _blankLinesP = new("{0:N0} Blank lines ({1:P1})");
    private readonly TranslationString _commentLinesP = new("{0:N0} Comment lines ({1:P1})");
    private readonly TranslationString _linesOfDesignerFilesP = new("{0:N0} Lines in designer files ({1:P1})");

    private readonly string _codeFilePattern;
    private readonly bool _countSubmodules;
    private readonly IGitModule _module;
    private readonly IGitExecutorProvider _executorProvider;
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private readonly CancellationTokenSource _lifetimeCancellation = new();

    private LineCounter? _lineCounter;
    private bool _initialized;

    protected Color[] DecentColors { get; } =
    [
        Colors.Red,
        Colors.Yellow,
        Colors.DodgerBlue,
        Colors.LightGreen,
        Colors.Coral,
        Colors.Goldenrod,
        Colors.YellowGreen,
        Colors.MediumPurple,
        Colors.LightGray,
        Colors.Brown,
        Colors.Pink,
        Colors.DarkBlue,
        Colors.Purple,
    ];

    public string DirectoriesToIgnore { get; set; } = "";

    public FormGitStatistics()
        : this(null!, null!, string.Empty, countSubmodules: false)
    {
    }

    public FormGitStatistics(
        IGitExecutorProvider executorProvider,
        IGitModule module,
        string codeFilePattern,
        bool countSubmodules)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        _executorProvider = executorProvider;
        _module = module;
        _codeFilePattern = codeFilePattern;
        _countSubmodules = countSubmodules;
        InitializeComponent();

        Tabs.SelectionChanged += TabsSelectedIndexChanged;
        SizeChanged += FormGitStatisticsSizeChanged;

        SetPieStyle(CommitCountPie);
        SetPieStyle(LinesOfCodeExtensionPie);
        SetPieStyle(LinesOfCodePie);
        SetPieStyle(TestCodePie);

        InitializeComplete();
    }

    private void FormGitStatisticsSizeChanged(object? sender, EventArgs e)
    {
        SetPieStyle(CommitCountPie);
        SetPieStyle(LinesOfCodeExtensionPie);
        SetPieStyle(LinesOfCodePie);
        SetPieStyle(TestCodePie);
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _operations.FileAndForget(() => LoadStatisticsAsync(_lifetimeCancellation.Token));
    }

    private async Task LoadStatisticsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.WhenAll(
                InitializeCommitCountAsync(cancellationToken),
                InitializeLinesOfCodeAsync(cancellationToken));
        }
        catch (OperationCanceledException)
        {
            // The owning dialog is closing.
        }
    }

    private async Task InitializeCommitCountAsync(CancellationToken cancellationToken)
    {
        (int totalCommits, Dictionary<string, int> commitsPerUser) = await Task.Run(
            () => _module.GetCommitsByContributor(),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        decimal[] commitCountValues = new decimal[commitsPerUser.Count];
        string[] commitCountLabels = new string[commitsPerUser.Count];
        StringBuilder builder = new();
        int index = 0;
        foreach ((string user, int commits) in commitsPerUser)
        {
            builder.AppendLine($"{commits:N0} {user}");
            commitCountValues[index] = commits;
            commitCountLabels[index] = string.Format(_commitsBy.Text, commits, user);
            index++;
        }

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        TotalCommits.Text = string.Format(_commits.Text, totalCommits);
        CommitCountPie.SetValues(commitCountValues);
        CommitCountPie.ToolTips = commitCountLabels;
        CommitStatistics.Text = builder.ToString();
    }

    private void SetPieStyle(PieChartControl pie)
    {
        pie.SetLeftMargin(10);
        pie.SetRightMargin(10);
        pie.SetTopMargin(10);
        pie.SetBottomMargin(10);
        pie.SetFitChart(false);
        pie.SetEdgeColorType(EdgeColorType.DarkerThanSurface);
        pie.SetSliceRelativeHeight(0.20f);
        pie.SetColors([.. DecentColors.Select(AdaptChartColor)]);
        pie.SetShadowStyle(ShadowStyle.GradualShadow);
    }

    private async Task InitializeLinesOfCodeAsync(CancellationToken cancellationToken)
    {
        LineStatistics statistics = await Task.Run(
            () => CountLines(cancellationToken),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        await _operations.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        UpdateUI(statistics);
    }

    private LineStatistics CountLines(CancellationToken cancellationToken)
    {
        _lineCounter = new LineCounter();
        LoadLinesOfCodeForModule(_module);

        if (_countSubmodules)
        {
            IEnumerable<GitModule> submodules = _module.GetSubmodulesInfo()
                .WhereNotNull()
                .Select(submodule => new GitModule(
                    _executorProvider,
                    Path.Combine(_module.WorkingDir, submodule.LocalPath)));

            foreach (GitModule submodule in submodules)
            {
                cancellationToken.ThrowIfCancellationRequested();
                LoadLinesOfCodeForModule(submodule);
            }
        }

        return new LineStatistics(
            _lineCounter.CommentLineCount,
            _lineCounter.TotalLineCount,
            _lineCounter.DesignerLineCount,
            _lineCounter.TestCodeLineCount,
            _lineCounter.BlankLineCount,
            _lineCounter.CodeLineCount,
            new Dictionary<string, int>(_lineCounter.LinesOfCodePerExtension));

        void LoadLinesOfCodeForModule(IGitModule module)
        {
            string[] filesToCheck = module
                .GetTree(commitId: default, full: true, cancellationToken: cancellationToken)
                .Select(file => Path.Combine(module.WorkingDir, file.Name))
                .ToArray();
            string directoryFilters = NormalizeDirectoryFilters(DirectoriesToIgnore);

            foreach (IEnumerable<string> batch in filesToCheck.Chunk(FileBatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();
                _lineCounter.FindAndAnalyzeCodeFiles(_codeFilePattern, directoryFilters, batch);
            }
        }
    }

    private void UpdateUI(LineStatistics lineCounter)
    {
        List<KeyValuePair<string, int>> linesOfCodePerExtension =
            [.. lineCounter.LinesOfCodePerExtension];
        linesOfCodePerExtension.Sort((first, next) => -first.Value.CompareTo(next.Value));

        decimal[] extensionValues = new decimal[linesOfCodePerExtension.Count];
        string[] extensionLabels = new string[linesOfCodePerExtension.Count];
        StringBuilder linesOfCodePerLanguageText = new();
        int index = 0;
        foreach ((string extension, int loc) in linesOfCodePerExtension)
        {
            double percent = GetRatio(loc, lineCounter.CodeLineCount);
            string line = string.Format(_linesOfCodeInFiles.Text, loc, extension, percent);
            linesOfCodePerLanguageText.AppendLine(line);
            extensionValues[index] = loc;
            extensionLabels[index] = line;
            index++;
        }

        TotalLinesOfTestCode.Text = string.Format(_linesOfTestCode.Text, lineCounter.TestCodeLineCount);

        TestCodePie.SetValues(
        [
            lineCounter.TestCodeLineCount,
            lineCounter.CodeLineCount - lineCounter.TestCodeLineCount,
        ]);

        double percentTest = GetRatio(lineCounter.TestCodeLineCount, lineCounter.CodeLineCount);
        double percentProd = GetRatio(
            lineCounter.CodeLineCount - lineCounter.TestCodeLineCount,
            lineCounter.CodeLineCount);
        TestCodePie.ToolTips =
        [
            string.Format(_linesOfTestCodeP.Text, lineCounter.TestCodeLineCount, percentTest),
            string.Format(
                _linesOfProductionCodeP.Text,
                lineCounter.CodeLineCount - lineCounter.TestCodeLineCount,
                percentProd),
        ];
        TestCodeText.Text = string.Join(Environment.NewLine, TestCodePie.ToolTips);

        double percentBlank = GetRatio(lineCounter.BlankLineCount, lineCounter.TotalLineCount);
        double percentComments = GetRatio(lineCounter.CommentLineCount, lineCounter.TotalLineCount);
        double percentCode = GetRatio(lineCounter.CodeLineCount, lineCounter.TotalLineCount);
        double percentDesigner = GetRatio(lineCounter.DesignerLineCount, lineCounter.TotalLineCount);
        LinesOfCodePie.SetValues(
        [
            lineCounter.BlankLineCount,
            lineCounter.CommentLineCount,
            lineCounter.CodeLineCount,
            lineCounter.DesignerLineCount,
        ]);
        LinesOfCodePie.ToolTips =
        [
            string.Format(_blankLinesP.Text, lineCounter.BlankLineCount, percentBlank),
            string.Format(_commentLinesP.Text, lineCounter.CommentLineCount, percentComments),
            string.Format(_linesOfCodeP.Text, lineCounter.CodeLineCount, percentCode),
            string.Format(_linesOfDesignerFilesP.Text, lineCounter.DesignerLineCount, percentDesigner),
        ];

        LinesOfCodePerTypeText.Text = string.Join(Environment.NewLine, LinesOfCodePie.ToolTips);
        LinesOfCodePerLanguageText.Text = linesOfCodePerLanguageText.ToString();
        LinesOfCodeExtensionPie.SetValues(extensionValues);
        LinesOfCodeExtensionPie.ToolTips = extensionLabels;
        TotalLinesOfCode2.Text = TotalLinesOfCode.Text =
            string.Format(_linesOfCode.Text, lineCounter.CodeLineCount);
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        Initialize();
        Tabs.IsVisible = true;
        LoadingLabel.IsVisible = false;
        FormGitStatisticsSizeChanged(this, EventArgs.Empty);
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetimeCancellation.Cancel();
        _operations.JoinPendingOperations();
        _lifetimeCancellation.Dispose();
        base.OnClosed(e);
    }

    private void TabsSelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
        => FormGitStatisticsSizeChanged(sender, EventArgs.Empty);

    private Color AdaptChartColor(Color color)
    {
        if (AvaloniaApplication.Current is not { } application
            || !application.TryGetResource(
                "GitExtensionsPanelBackgroundBrush",
                ActualThemeVariant,
                out object? resource)
            || resource is not ISolidColorBrush background
            || GetLuminance(background.Color) >= 0.35
            || GetLuminance(color) >= 0.25)
        {
            return color;
        }

        return Color.FromArgb(
            color.A,
            Blend(color.R),
            Blend(color.G),
            Blend(color.B));

        static byte Blend(byte channel)
            => (byte)Math.Round(channel + ((byte.MaxValue - channel) * 0.35));
    }

    private static double GetLuminance(Color color)
        => ((0.2126 * color.R) + (0.7152 * color.G) + (0.0722 * color.B)) / byte.MaxValue;

    private static double GetRatio(int value, int total)
        => total == 0 ? 0 : (double)value / total;

    private static string NormalizeDirectoryFilters(string filters)
        => filters
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormGitStatistics form)
    {
        public bool TabsVisible => form.Tabs.IsVisible;

        public string CommitStatisticsText => form.CommitStatistics.Text ?? string.Empty;

        public string LinesOfCodeText => form.TotalLinesOfCode.Text ?? string.Empty;

        public string LinesPerLanguageText => form.LinesOfCodePerLanguageText.Text ?? string.Empty;

        public PieChartControl CommitCountPie => form.CommitCountPie;

        public PieChartControl LinesOfCodeExtensionPie => form.LinesOfCodeExtensionPie;

        public Task LoadStatisticsAsync(CancellationToken cancellationToken = default)
            => form.LoadStatisticsAsync(cancellationToken);
    }

    private sealed record LineStatistics(
        int CommentLineCount,
        int TotalLineCount,
        int DesignerLineCount,
        int TestCodeLineCount,
        int BlankLineCount,
        int CodeLineCount,
        IReadOnlyDictionary<string, int> LinesOfCodePerExtension);
}
