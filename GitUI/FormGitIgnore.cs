using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using GitCommands;

namespace GitUI
{
    public partial class FormGitIgnore : GitExtensionsForm
    {
        public string GitIgnoreFile;

        public FormGitIgnore()
        {
            InitializeComponent();
            Translate();
            GitIgnoreFile = "";

            try
            {
                if (File.Exists(Settings.WorkingDir + ".gitignore"))
                {
                    _NO_TRANSLATE_GitIgnoreEdit.ViewFile(Settings.WorkingDir + ".gitignore");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SaveClick(object sender, EventArgs e)
        {
            try
            {
                FileInfoExtensions
                    .MakeFileTemporaryWritable(
                        Settings.WorkingDir + ".gitignore",
                        x =>
                            {
                                // Enter a newline to work around a wierd bug 
                                // that causes the first line to include 3 extra bytes. (encoding marker??)
                                GitIgnoreFile = Environment.NewLine + _NO_TRANSLATE_GitIgnoreEdit.GetText().Trim();
                                using (var tw = new StreamWriter(x, false, Settings.Encoding))
                                {
                                    tw.Write(GitIgnoreFile);
                                }
                                Close();
                            });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FormGitIgnoreFormClosing(object sender, FormClosingEventArgs e)
        {
            SavePosition("edit-git-ignore");
        }

        private void FormGitIgnoreLoad(object sender, EventArgs e)
        {
            RestorePosition("edit-git-ignore");
            if (!Settings.IsBareRepository()) return;
            MessageBox.Show(".gitignore is only supported when there is a working dir.");
            Close();
        }

        private void AddDefaultClick(object sender, EventArgs e)
        {
            _NO_TRANSLATE_GitIgnoreEdit.ViewText(".gitignore", 
                _NO_TRANSLATE_GitIgnoreEdit.GetText() +
                Environment.NewLine + "#ignore thumbnails created by windows" +
                Environment.NewLine + "Thumbs.db" +
                Environment.NewLine + "#Ignore files build by Visual Studio" +
                Environment.NewLine + "*.obj" +
                Environment.NewLine + "*.exe" +
                Environment.NewLine + "*.pdb" +
                Environment.NewLine + "*.user" +
                Environment.NewLine + "*.aps" +
                Environment.NewLine + "*.pch" +
                Environment.NewLine + "*.vspscc" +
                Environment.NewLine + "*_i.c" +
                Environment.NewLine + "*_p.c" +
                Environment.NewLine + "*.ncb" +
                Environment.NewLine + "*.suo" +
                Environment.NewLine + "*.tlb" +
                Environment.NewLine + "*.tlh" +
                Environment.NewLine + "*.bak" +
                Environment.NewLine + "*.cache" +
                Environment.NewLine + "*.ilk" +
                Environment.NewLine + "*.log" +
                Environment.NewLine + "[Bb]in" +
                Environment.NewLine + "[Dd]ebug*/" +
                Environment.NewLine + "*.lib" +
                Environment.NewLine + "*.sbr" +
                Environment.NewLine + "obj/" +
                Environment.NewLine + "[Rr]elease*/" +
                Environment.NewLine + "_ReSharper*/" +
                Environment.NewLine + "[Tt]est[Rr]esult*" +
                Environment.NewLine + "");
        }
    }
}