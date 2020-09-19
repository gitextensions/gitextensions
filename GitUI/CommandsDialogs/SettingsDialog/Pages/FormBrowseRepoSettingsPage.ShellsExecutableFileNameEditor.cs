#nullable enable

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using GitUI.Design;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormBrowseRepoSettingsPage
    {
        private sealed class ShellsExecutableFileNameEditor : ExecutableFileNameEditor
        {
            private OpenFileDialog? _openFileDialog;
            private string? _path;

            protected override void InitializeDialog(OpenFileDialog openFileDialog)
            {
                base.InitializeDialog(openFileDialog);

                _openFileDialog = openFileDialog;

                _openFileDialog.RestoreDirectory = true;
                _openFileDialog.InitialDirectory = _path;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                try
                {
                    _path = Path.GetDirectoryName((string)value);

                    if (_openFileDialog != null)
                    {
                        _openFileDialog.InitialDirectory = _path;
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                return base.EditValue(context, provider, value);
            }
        }
    }
}
