namespace GitUI.UserControls;

internal sealed class WebBrowserControl : WebBrowser
{
    public WebBrowserControl()
    {
        ScriptErrorsSuppressed = true;
    }
}
