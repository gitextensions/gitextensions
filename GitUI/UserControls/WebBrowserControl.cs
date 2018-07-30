using System.Windows.Forms;

namespace GitUI.UserControls
{
    internal class WebBrowserControl : WebBrowser
    {
        public WebBrowserControl()
        {
            ScriptErrorsSuppressed = true;
        }
    }
}