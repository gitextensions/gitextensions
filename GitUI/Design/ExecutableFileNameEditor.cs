using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace GitUI.Design
{
    internal class ExecutableFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            openFileDialog.Filter = "Executable file (*.exe;*.cmd;*.bat)|*.exe;*.cmd;*.bat|All files (*.*)|*.*";
        }
    }
}
