using System.Windows.Forms;

namespace GitUI.Browsing.Dialogs
{
    /// <summary>
    /// Window container. At this point, refactoring will be needed later.
    /// </summary>
    internal interface IWindowContainer
    {
        IWin32Window Window { get; }
    }
}
