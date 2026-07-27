using System.Net;
using System.Text;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI;
using Microsoft.VisualStudio.Threading;
using ResourceManager;
using MessageBoxes = GitUI.MessageBoxes;

namespace GitExtensions.Plugins.ReleaseNotesGenerator;

/// <summary>
/// Test on GE repository from "2.00" to "2.10". Should display 687 items.
/// </summary>
public partial class ReleaseNotesGeneratorForm : GitExtensionsFormBase
{
    private readonly TranslationString _commitLogFrom = new("Commit log from '{0}' to '{1}' ({2}):");
    private readonly TranslationString _fromCommitNotSpecified = new("'From' commit must be specified");
    private readonly TranslationString _toCommitNotSpecified = new("'To' commit must be specified");
    private readonly TranslationString _caption = new("Invalid input");

    private const string MostRecentHint = "most recent changes are listed on top";
    private readonly IGitLogLineParser _gitLogLineParser = new GitLogLineParser();
    private readonly GitUIEventArgs? _gitUiCommands;
    private readonly TaskManager _operations = ThreadHelper.CreateTaskManager();
    private IEnumerable<LogLine> _lastGeneratedLogLines = [];

    public ReleaseNotesGeneratorForm()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public ReleaseNotesGeneratorForm(GitUIEventArgs gitUiCommands)
    {
        _gitUiCommands = gitUiCommands;

        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        // XML normalizes attribute line endings. Restore the exact existing XLF sources before
        // the central translation walk while still keeping layout in AXAML.
        label7.Text = "(Commit expressions can be commit hashes,\r\nbranch names, tag names)";
        label11.Text = "Clipboard will contain HTML code (plain text) and HTML format\r\nwhich can be pasted to programs like MS Word or LibreOffice Writer.";

        buttonGenerate.Click += buttonGenerate_Click;
        buttonCopyOrigOutput.Click += buttonCopyOrigOutput_Click;
        buttonCopyAsTextTableTab.Click += buttonCopyAsPlainText_Click;
        buttonCopyAsTextTableSpace.Click += buttonCopyAsTextTableSpace_Click;
        buttonCopyAsHtml.Click += buttonCopyAsHtml_Click;
        textBoxResult.TextChanged += textBoxResult_TextChanged;
        AcceptButton = buttonGenerate;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);
        Icon = (Owner as Avalonia.Controls.Window)?.Icon;
        textBoxResult_TextChanged(this, EventArgs.Empty);
    }

    protected override void OnClosed(EventArgs e)
    {
        _operations.JoinPendingOperations();
        base.OnClosed(e);
    }

    private void buttonGenerate_Click(object? sender, EventArgs e)
    {
        textBoxResult.Text = string.Empty;

        if (string.IsNullOrWhiteSpace(textBoxRevFrom.Text))
        {
            MessageBoxes.ShowError(this, _fromCommitNotSpecified.Text, _caption.Text);
            textBoxRevFrom.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_NO_TRANSLATE_textBoxRevTo.Text))
        {
            MessageBoxes.ShowError(this, _toCommitNotSpecified.Text, _caption.Text);
            _NO_TRANSLATE_textBoxRevTo.Focus();
            return;
        }

        GitArgumentBuilder args = new("log")
        {
            string.Format(_NO_TRANSLATE_textBoxGitLogArguments.Text ?? string.Empty, textBoxRevFrom.Text, _NO_TRANSLATE_textBoxRevTo.Text)
        };

        string result = GetGitUiCommands().GitModule.GitExecutable.GetOutput(args);

        if (OperatingSystem.IsWindows())
        {
            result = string.Join(Environment.NewLine, result.Split([Environment.NewLine], StringSplitOptions.None).SelectMany(line => line.Split('\n')));
        }

        textBoxResult.Text = result;
        try
        {
            _lastGeneratedLogLines = _gitLogLineParser.Parse(SplitLines(result));
            labelRevCount.Text = _lastGeneratedLogLines.Count().ToString();
        }
        catch
        {
            labelRevCount.Text = "n/a";
        }

        textBoxResult_TextChanged(sender, e);
    }

    private void textBoxResult_TextChanged(object? sender, EventArgs e)
    {
        groupBoxCopy.IsEnabled = _lastGeneratedLogLines.Any();
    }

    private void buttonCopyOrigOutput_Click(object? sender, EventArgs e)
    {
        ClipboardUtil.TrySetText(textBoxResult.Text ?? string.Empty);
    }

    private void buttonCopyAsPlainText_Click(object? sender, EventArgs e)
    {
        string result = CreateTextTable(_lastGeneratedLogLines, true, true);
        ClipboardUtil.TrySetText(result);
    }

    private void buttonCopyAsTextTableSpace_Click(object? sender, EventArgs e)
    {
        string result = CreateTextTable(_lastGeneratedLogLines, true, false);
        ClipboardUtil.TrySetText(result);
    }

    private void buttonCopyAsHtml_Click(object? sender, EventArgs e)
    {
        _operations.FileAndForget(CopyAsHtmlAsync);
    }

    private async Task CopyAsHtmlAsync()
    {
        string headerHtml = string.Format(
            "<p>Commit log from '{0}' to '{1}' ({2}):</p>",
            textBoxRevFrom.Text,
            _NO_TRANSLATE_textBoxRevTo.Text,
            MostRecentHint);
        string tableHtml = CreateHtmlTable(_lastGeneratedLogLines);
        await HtmlFragment.CopyToClipboardAsync(this, headerHtml + tableHtml);
    }

    private string CreateTextTable(IEnumerable<LogLine> logLines, bool suppressEmptyLines = true, bool separateColumnWithTabInsteadOfSpaces = true)
    {
        string headerText = string.Format(
            _commitLogFrom.Text,
            textBoxRevFrom.Text,
            _NO_TRANSLATE_textBoxRevTo.Text,
            MostRecentHint);

        string colSeparatorFirstLine = separateColumnWithTabInsteadOfSpaces ? "\t" : " ";
        string colSeparatorRestLines = separateColumnWithTabInsteadOfSpaces ? "\t" : "        ";

        StringBuilder stringBuilder = new();

        foreach (LogLine logLine in logLines)
        {
            string message = string.Join(
                Environment.NewLine + colSeparatorRestLines,
                logLine.MessageLines.Where(line => !suppressEmptyLines || !string.IsNullOrWhiteSpace(line)));
            stringBuilder.AppendFormat(
                "{0}{1}{2}{3}",
                logLine.Commit,
                colSeparatorFirstLine,
                message,
                Environment.NewLine);
        }

        string result = headerText + Environment.NewLine + stringBuilder;
        return result;
    }

    private static string CreateHtmlTable(IEnumerable<LogLine> logLines)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append("<table>\r\n");
        foreach (LogLine logLine in logLines)
        {
            string message = string.Join("<br/>", logLine.MessageLines.Select(line => WebUtility.HtmlEncode(line)));
            stringBuilder.AppendFormat(
                "<tr>\r\n  <td>{0}</td>\r\n  <td>{1}</td>\r\n</tr>\r\n",
                logLine.Commit,
                message);
        }

        stringBuilder.Append("</table>");
        return stringBuilder.ToString();
    }

    private static IEnumerable<string> SplitLines(string text)
        => text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private GitUIEventArgs GetGitUiCommands()
        => _gitUiCommands ?? throw new InvalidOperationException($"{nameof(ReleaseNotesGeneratorForm)} was constructed incorrectly.");

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ReleaseNotesGeneratorForm form)
    {
        public bool CopyActionsEnabled => form.groupBoxCopy.IsEnabled;
        public string Result => form.textBoxResult.Text ?? string.Empty;
        public string RevisionCount => form.labelRevCount.Text ?? string.Empty;

        public string CreateHtmlTable(IEnumerable<LogLine> lines) => ReleaseNotesGeneratorForm.CreateHtmlTable(lines);
        public string CreateTextTable(IEnumerable<LogLine> lines, bool tabs) => form.CreateTextTable(lines, separateColumnWithTabInsteadOfSpaces: tabs);
        public void Generate(string from, string to)
        {
            form.textBoxRevFrom.Text = from;
            form._NO_TRANSLATE_textBoxRevTo.Text = to;
            form.buttonGenerate_Click(form.buttonGenerate, EventArgs.Empty);
        }
    }
}
