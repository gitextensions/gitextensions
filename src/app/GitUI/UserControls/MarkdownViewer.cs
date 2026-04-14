using GitCommands;
using GitExtUtils.GitUI;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace GitUI.UserControls;

/// <summary>
///  A read-only control that renders Markdown text in a WebView2 control
///  using <see cref="Editor.MarkdownToHtmlConverter"/>.
/// </summary>
public class MarkdownViewer : UserControl
{
    private readonly WebView2 _webView;
    private string _markdownText = string.Empty;
    private string? _pendingHtml;
    private bool _isWebViewReady;
    private bool _isInitializing;

    public MarkdownViewer()
    {
        _webView = new WebView2
        {
            Dock = DockStyle.Fill,
            DefaultBackgroundColor = SystemColors.Window,
        };

        _webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
        _webView.NavigationStarting += WebView_NavigationStarting;

        // Bubble mouse events so the floating toolbar in FileViewer works.
        _webView.MouseMove += (s, e) => OnMouseMove(e);
        _webView.MouseLeave += (s, e) => OnMouseLeave(e);

        Controls.Add(_webView);
    }

    /// <summary>
    ///  Gets or sets the Markdown text to render.
    /// </summary>
    public string MarkdownText
    {
        get => _markdownText;
        set
        {
            _markdownText = value ?? string.Empty;
            RenderMarkdown();
        }
    }

    /// <summary>
    ///  When <see langword="true"/>, the WebView2 does not scroll internally and
    ///  forwards wheel events to the nearest scrollable WinForms parent.
    ///  Use for embedded panels like CommitInfo. Default is <see langword="false"/>.
    /// </summary>
    public bool DisableScrolling { get; set; }

    private async Task EnsureInitializedAsync()
    {
        if (_isWebViewReady || _isInitializing)
        {
            return;
        }

        _isInitializing = true;

        CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(
            userDataFolder: Path.Combine(Path.GetTempPath(), "GitExtensions_WebView2"));
        await _webView.EnsureCoreWebView2Async(environment);
    }

    private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
    {
        if (!e.IsSuccess)
        {
            return;
        }

        _isWebViewReady = true;

        CoreWebView2Settings settings = _webView.CoreWebView2.Settings;
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreDevToolsEnabled = false;
        settings.IsStatusBarEnabled = false;
        settings.IsZoomControlEnabled = false;

        // Forward mouse wheel events to the parent scroll container.
        // WebView2 is a separate HWND that captures wheel events, preventing
        // the WinForms parent (e.g. CommitInfo with AutoScroll) from scrolling.
        _webView.CoreWebView2.WebMessageReceived += (s, args) =>
        {
            if (int.TryParse(args.WebMessageAsJson, out int delta))
            {
                ScrollableControl? scrollParent = FindScrollParent();
                if (scrollParent is not null)
                {
                    Point pos = scrollParent.AutoScrollPosition;
                    scrollParent.AutoScrollPosition = new Point(-pos.X, -pos.Y + delta);
                }
            }
        };

        if (_pendingHtml is not null)
        {
            _webView.NavigateToString(_pendingHtml);
            _pendingHtml = null;
        }
    }

    private void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        // Allow initial content load (about:blank and data URIs) but open
        // clicked links in the default browser instead of navigating.
        if (e.Uri is not null
            && !e.Uri.StartsWith("about:", StringComparison.OrdinalIgnoreCase)
            && !e.Uri.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel = true;
            OsShellUtil.OpenUrlInDefaultBrowser(e.Uri);
        }
    }

    private void RenderMarkdown()
    {
        string html = Editor.MarkdownToHtmlConverter.Convert(_markdownText, DisableScrolling);

        if (_isWebViewReady)
        {
            _webView.NavigateToString(html);
        }
        else
        {
            _pendingHtml = html;

            // EnsureCoreWebView2Async must run on the UI thread (STA).
            // RenderMarkdown is called from property setters on the UI thread,
            // so we can safely fire-and-forget here.
            EnsureInitializedAsync().FileAndForget();
        }
    }

    private ScrollableControl? FindScrollParent()
    {
        for (Control? current = Parent; current is not null; current = current.Parent)
        {
            if (current is ScrollableControl { AutoScroll: true } scrollable)
            {
                return scrollable;
            }
        }

        return null;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _webView.CoreWebView2InitializationCompleted -= WebView_CoreWebView2InitializationCompleted;
            _webView.NavigationStarting -= WebView_NavigationStarting;
            _webView.Dispose();
        }

        base.Dispose(disposing);
    }
}
