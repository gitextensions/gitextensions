using GitCommands;
using GitExtUtils;

namespace GitUI.CommandsDialogs.Menus
{
    internal partial class CopyPathsToolStripMenuItem : ToolStripMenuItemEx
    {
        private Func<IEnumerable<string?>> _getSelectedFilePaths;

        public CopyPathsToolStripMenuItem()
        {
            InitializeComponent();

            copyFullPathsNativeToolStripMenuItem.Font = new(copyFullPathsNativeToolStripMenuItem.Font, FontStyle.Bold);
        }

        public CopyPathsToolStripMenuItem Initialize(Func<GitUICommands> getUICommands, Func<IEnumerable<string?>> getSelectedFilePaths)
        {
            Initialize(getUICommands);
            _getSelectedFilePaths = getSelectedFilePaths;
            return this;
        }

        private void CopyPathsToClipboard(string prefixDir, Func<string, string> convertPath)
        {
            IEnumerable<string?> selectedFilePaths
                = (_getSelectedFilePaths ?? throw new InvalidOperationException("The menu is not initialized.")).Invoke();
            string filePaths = GetFilePaths(selectedFilePaths, prefixDir, convertPath);
            if (!string.IsNullOrWhiteSpace(filePaths))
            {
                ClipboardUtil.TrySetText(filePaths);
            }
        }

        private static string GetFilePaths(IEnumerable<string?> selectedFilePaths, string prefixDir, Func<string, string> convertPath)
        {
            return selectedFilePaths
                .Where(path => !string.IsNullOrEmpty(path))
                .Select(path => convertPath(Path.Combine(prefixDir, path)))
                .Join(Environment.NewLine);
        }

        private void CopyFullPathsNativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetCurrentParent().Hide();
            CopyPathsToClipboard(Module.WorkingDir, PathUtil.ToNativePath);
        }

        private void CopyFullPathsCygwinToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPathsToClipboard(Module.WorkingDir, PathUtil.ToCygwinPath);
        }

        private void CopyFullPathsWslToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPathsToClipboard(Module.WorkingDir, PathUtil.ToWslPath);
        }

        private void CopyRelativePathsNativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPathsToClipboard(prefixDir: "", PathUtil.ToNativePath);
        }

        private void CopyRelativePathsPosixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyPathsToClipboard(prefixDir: "", PathUtil.ToPosixPath);
        }
    }
}
