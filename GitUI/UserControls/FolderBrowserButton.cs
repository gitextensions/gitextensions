using System;
using System.Windows.Forms;
using System.IO;

namespace GitUI.UserControls
{
    public partial class FolderBrowserButton : UserControl
    {
        public FolderBrowserButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The Text property of this control will be filled with the selected path
        /// and the Text property is used as path to initialize the folder browser's default selection
        /// </summary>
        public Control PathShowingControl { get; set; }

        /// <summary>
        /// Opens a a FolderBrowserDialog with the path in "getter" preselected and 
        /// if the DialogResult.OK is returned uses "setter" to set the path
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="setter"></param>
        public void ShowFolderBrowserDialogWithPreselectedPath(Func<string> getter, Action<string> setter)
        {
            string directoryInfoPath = null;
            try
            {
                directoryInfoPath = new DirectoryInfo(getter()).FullName;
            }
            catch (Exception)
            {
                // since the DirectoryInfo stuff is for convenience we swallow exceptions
            }

            using (var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop,
                // if we do not use the DirectoryInfo then a path with slashes instead of backslashes won't work
                SelectedPath = directoryInfoPath ?? getter()
            })
            {
                // TODO: do we need ParentForm or is "this" ok?
                if (dialog.ShowDialog(ParentForm) == DialogResult.OK)
                {
                    setter(dialog.SelectedPath);
                }
            }
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            ShowFolderBrowserDialogWithPreselectedPath(() => PathShowingControl.Text, path => PathShowingControl.Text = path);
        }
    }
}
