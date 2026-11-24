using Avalonia.Controls;

namespace GitUI.UserControls;

/// <summary>
/// Native structured-content substitute for the source WinForms web-browser control.
/// </summary>
/// <remarks>
/// The pull-request discussion renders typed native rows instead of executable HTML. The
/// document-completed boundary remains so source scrolling behavior runs after rows materialize.
/// </remarks>
public sealed class WebBrowserControl : ListBox
{
    /// <summary>
    /// Occurs after the current structured document has materialized.
    /// </summary>
    public event EventHandler<WebBrowserDocumentCompletedEventArgs>? DocumentCompleted;

    protected override Type StyleKeyOverride => typeof(ListBox);

    internal void ObserveVisibility(TabControl tabControl, TabItem page)
    {
        tabControl.SelectionChanged += (_, _) =>
        {
            if (tabControl.SelectedItem == page && ItemCount > 0)
            {
                NotifyDocumentCompleted();
            }
        };
    }

    internal void NotifyDocumentCompleted()
        => DocumentCompleted?.Invoke(this, WebBrowserDocumentCompletedEventArgs.Completed);
}

/// <summary>
/// Carries completion of a native structured document.
/// </summary>
public sealed class WebBrowserDocumentCompletedEventArgs : EventArgs
{
    internal static WebBrowserDocumentCompletedEventArgs Completed { get; } = new();

    private WebBrowserDocumentCompletedEventArgs()
    {
    }
}
