using Avalonia.Controls;
using GitExtUtils;
using GitUI.Models;

namespace GitUI.UserControls;

internal abstract class OutputHistoryControllerBase : IDisposable
{
    protected IOutputHistoryProvider _outputHistoryProvider;
    protected TextBox _textBox;

    internal OutputHistoryControllerBase(IOutputHistoryProvider outputHistoryProvider, OutputHistoryControl outputHistoryControl)
    {
        _outputHistoryProvider = outputHistoryProvider;
        _textBox = outputHistoryControl.TextBox;

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

    private void Update(object? sender, EventArgs args)
    {
        string history = _outputHistoryProvider.History;
        _textBox.InvokeAndForget(() =>
        {
            _textBox.Text = history;
            _textBox.CaretIndex = history.Length;
        });
    }

    private void CopyToClipboard(object? sender, EventArgs args)
    {
        string text = _textBox.SelectedText;
        if (string.IsNullOrEmpty(text))
        {
            text = _textBox.Text ?? string.Empty;
        }

        ClipboardUtil.TrySetText(text);
    }
}
