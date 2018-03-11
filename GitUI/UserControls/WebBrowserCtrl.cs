using System.Windows.Forms;

namespace GitUI.UserControls
{
    internal class WebBrowserCtrl : WebBrowser
    {
        public WebBrowserCtrl()
        {
            ScriptErrorsSuppressed = true;
        }
    }
}