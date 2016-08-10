using System.Windows.Forms;

namespace GitUI.UserControls
{
    class WebBrowserCtrl : WebBrowser
    {
        public WebBrowserCtrl()
        {
            ScriptErrorsSuppressed = true;
        }
    }
}