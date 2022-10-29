using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

public abstract class OutputHistoryControllerBase : IDisposable
{
    protected IOutputHistoryModel _outputHistoryModel;
    protected RichTextBox _textBox;

    public OutputHistoryControllerBase(IOutputHistoryModel outputHistoryModel, OutputHistoryControl outputHistoryControl)
    {
        _outputHistoryModel = outputHistoryModel;
        _textBox = outputHistoryControl.TextBox;

        _textBox.LinkClicked += (_, e) => OsShellUtil.OpenUrlInDefaultBrowser(e.LinkText);
        outputHistoryControl.tsmiClear.Click += (_, _) => _outputHistoryModel.Clear();
        outputHistoryControl.tsmiCopy.Click += CopyToClipboard;

        _outputHistoryModel.HistoryChanged += Update;
        Update(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _outputHistoryModel.HistoryChanged -= Update;
    }

    public abstract bool ToggleControl();

    private void Update(object sender, EventArgs args)
    {
        string history = _outputHistoryModel.History;
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
