using System.Windows.Forms;

namespace GitUI.Browsing.Dialogs
{
    internal sealed class WindowContainer : IWindowContainer
    {
        public WindowContainer(IWin32Window window)
        {
            Window = window;
        }

        public IWin32Window Window { get; }
    }
}
