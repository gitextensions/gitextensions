using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

internal abstract class OutputHistoryControllerBase : IDisposable
{
    protected IOutputHistoryProvider _outputHistoryProvider;
    protected RichTextBox _textBox;

    internal OutputHistoryControllerBase(IOutputHistoryProvider outputHistoryProvider, OutputHistoryControl outputHistoryControl)
    {
        _outputHistoryProvider = outputHistoryProvider;
        _textBox = outputHistoryControl.TextBox;

        _textBox.LinkClicked += (_, e) => OsShellUtil.OpenUrlInDefaultBrowser(e.LinkText);
        outputHistoryControl.tsmiClear.Click += (_, _) => _outputHistoryProvider.ClearHistory();
        outputHistoryControl.tsmiCopy.Click += CopyToClipboard;

        _outputHistoryProvider.HistoryChanged += Update;
        Update(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _outputHistoryProvider.HistoryChanged -= Update;
    }

    internal abstract bool FocusAndToggleIfPanel();

    private void Update(object sender, EventArgs args)
    {
        string history = _outputHistoryProvider.History;
        _textBox.InvokeAndForget(() =>
        {
            _textBox.Text = history;
            _textBox.SelectionStart = history.Length;
            _textBox.ScrollToCaret();
        });
    }

    private void CopyToClipboard(object sender, EventArgs args)
    {
        string text = _textBox.SelectedText;
        if (string.IsNullOrEmpty(text))
        {
            text = _textBox.Text;
        }

        Clipboard.SetText(text);
    }
}
