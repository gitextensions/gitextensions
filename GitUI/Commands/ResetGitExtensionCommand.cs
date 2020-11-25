using System;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs;

namespace GitUI.Commands
{
    internal sealed class ResetGitExtensionCommand : IGitExtensionCommand
    {
        private readonly GitUICommands _gitUICommands;
        private readonly string _fileName;
        private readonly IFullPathResolver _fullPathResolver;

        public ResetGitExtensionCommand(
            GitUICommands gitUICommands,
            string fileName)
        {
            _gitUICommands = gitUICommands ?? throw new ArgumentNullException(nameof(gitUICommands));
            _fileName = fileName;
            _fullPathResolver = new FullPathResolver(() => _gitUICommands.Module.WorkingDir);
        }

        public bool Execute()
        {
            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetAction = FormResetChanges.ShowResetDialog(null, true, false);

            if (resetAction == FormResetChanges.ActionEnum.Cancel)
            {
                return false;
            }

            using (WaitCursorScope.Enter())
            {
                // Reset all changes.
                _gitUICommands.Module.ResetFile(_fileName);

                // Also delete new files, if requested.
                if (resetAction == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    string errorCaption = null;
                    string errorMessage = null;
                    string path = _fullPathResolver.Resolve(_fileName);

                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            errorCaption = Strings.ErrorCaptionFailedDeleteFile;
                            errorMessage = ex.Message;
                        }
                    }
                    else
                    {
                        errorCaption = Strings.ErrorCaptionFailedDeleteFolder;
                        path.TryDeleteDirectory(out errorMessage);
                    }

                    if (errorMessage != null)
                    {
                        MessageBox.Show(errorMessage, errorCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            return true;
        }
    }
}
