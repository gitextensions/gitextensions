using GitCommands;
using GitUI.Models;

namespace GitUI.UserControls;

public abstract class ProcessHistoryControllerBase : IDisposable
{
    protected IProcessHistoryModel _processHistoryModel;
    protected RichTextBox _textBox;

    public ProcessHistoryControllerBase(IProcessHistoryModel processHistoryModel, ProcessHistoryControl processHistoryControl)
    {
        _processHistoryModel = processHistoryModel;
        _textBox = processHistoryControl.TextBox;

        _textBox.LinkClicked += (_, e) => OsShellUtil.OpenUrlInDefaultBrowser(e.LinkText);
        processHistoryControl.tsmiClear.Click += (_, _) => _processHistoryModel.Clear();
        processHistoryControl.tsmiCopy.Click += CopyToClipboard;

        _processHistoryModel.HistoryChanged += Update;
        Update(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        _processHistoryModel.HistoryChanged -= Update;
    }

    public abstract void ToggleControl();

    private void Update(object sender, EventArgs args)
    {
        string history = _processHistoryModel.History;
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
