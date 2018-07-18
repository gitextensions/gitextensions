using System;
using System.IO;
using System.Windows.Forms;
using ResourceManager;

namespace GitUI.UserControls
{
    public partial class FolderBrowserButton : GitExtensionsControl
    {
        public FolderBrowserButton()
        {
            InitializeComponent();
            InitializeComplete();
        }

        /// <summary>
        /// The Text property of this control will be filled with the selected path
        /// and the Text property is used as path to initialize the folder browser's default selection
        /// </summary>
        public Control PathShowingControl { get; set; }

        /// <summary>
        /// Opens a a folder picker dialog with the path in "getter" preselected and
        /// if OK is returned uses "setter" to set the path
        /// </summary>
        public void ShowFolderBrowserDialogWithPreselectedPath(Func<string> getter, Action<string> setter)
        {
            string directoryInfoPath = null;
            try
            {
                directoryInfoPath = new DirectoryInfo(getter()).FullName;
            }
            catch
            {
                // since the DirectoryInfo stuff is for convenience we swallow exceptions
            }

            // if we do not use the DirectoryInfo then a path with slashes instead of backslashes won't work
            if (directoryInfoPath == null)
            {
                directoryInfoPath = getter();
            }

            // TODO: do we need ParentForm or is "this" ok?
            var userSelectedPath = OsShellUtil.PickFolder(ParentForm, directoryInfoPath);

            if (userSelectedPath != null)
            {
                setter(userSelectedPath);
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            ShowFolderBrowserDialogWithPreselectedPath(() => PathShowingControl.Text, path => PathShowingControl.Text = path);
        }
    }
}
