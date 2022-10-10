using System.Composition;
using GitUI;

namespace GitExtensions.UITests.CommandsDialogs
{
    [Shared, PartNotDiscoverable]
    [Export(typeof(IWindowsJumpListManager))]
    internal class MockWindowsJumpListManager : IWindowsJumpListManager
    {
        public bool NeedsJumpListCreation { get; } = false;

        public void AddToRecent(string workingDir)
        {
        }

        public void CreateJumpList(IntPtr windowHandle, WindowsThumbnailToolbarButtons buttons)
        {
        }

        public void DisableThumbnailToolbar()
        {
        }

        public void Dispose()
        {
        }

        public void UpdateCommitIcon(Image image)
        {
        }
    }
}
