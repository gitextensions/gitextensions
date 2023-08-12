using GitCommands;
using GitCommands.Git.Commands;
using GitCommands.Utils;
using GitExtUtils;
using GitUI.HelperDialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCleanupRepository : GitModuleForm
    {
        private readonly TranslationString _reallyCleanupQuestion =
            new("Are you sure you want to cleanup the repository?");
        private readonly TranslationString _reallyCleanupQuestionCaption = new("Cleanup");

        [Obsolete("For VS designer and translation test only. Do not remove.")]
        private FormCleanupRepository()
        {
            InitializeComponent();
        }

        public FormCleanupRepository(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();
            PreviewOutput.ReadOnly = true;

            checkBoxPathFilter_CheckedChanged(this, EventArgs.Empty);
            checkBoxExcludePathFilter_CheckedChanged(this, EventArgs.Empty);
        }

        public void SetPathArgument(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                checkBoxExcludePathFilter.Checked = false;
                textBoxExcludePaths.Text = "";

                checkBoxIncludePathFilter.Checked = false;
                textBoxIncludePaths.Text = "";
            }
            else
            {
                checkBoxExcludePathFilter.Checked = true;
                textBoxExcludePaths.Text = path;

                checkBoxIncludePathFilter.Checked = true;
                textBoxIncludePaths.Text = path;
            }
        }

        private void CleanUp(bool dryRun)
        {
            string includePathArgument = GetInclusivePathArgumentFromGui();
            string excludePathArguments = GetExclusivePathArgumentFromGui();
            CleanMode mode = GetCleanMode();
            ArgumentString cleanUpCmd = GitCommandHelpers.CleanCmd(mode, dryRun, directories: RemoveDirectories.Checked,
                paths: includePathArgument, excludes: excludePathArguments);

            string cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: cleanUpCmd, Module.WorkingDir, input: null, useDialogSettings: true);
            PreviewOutput.Text = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);

            if (CleanSubmodules.Checked)
            {
                ArgumentString cleanSubmodulesCmd = GitCommandHelpers.CleanSubmodules(mode, dryRun, directories: RemoveDirectories.Checked, paths: includePathArgument);
                cmdOutput = FormProcess.ReadDialog(this, UICommands, arguments: cleanSubmodulesCmd, Module.WorkingDir, input: null, useDialogSettings: true);
                PreviewOutput.Text += EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);
            }
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            CleanUp(dryRun: true);
        }

        private void Cleanup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _reallyCleanupQuestion.Text, _reallyCleanupQuestionCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                CleanUp(dryRun: false);
            }
        }

        private CleanMode GetCleanMode()
        {
            if (RemoveAll.Checked)
            {
                return CleanMode.All;
            }

            if (RemoveNonIgnored.Checked)
            {
                return CleanMode.OnlyNonIgnored;
            }

            if (RemoveIgnored.Checked)
            {
                return CleanMode.OnlyIgnored;
            }

            throw new NotSupportedException($"Unknown value for {nameof(CleanMode)}.");
        }

        private string? GetInclusivePathArgumentFromGui()
        {
            if (!checkBoxIncludePathFilter.Checked)
            {
                return null;
            }

            // 1. get all lines from text box which are not empty
            // 2. wrap lines with ""
            // 3. join together with space as separator
            return string.Join(" ", textBoxIncludePaths.Lines.Where(p => !string.IsNullOrEmpty(p)).Select(p => $"\"{p}\""));
        }

        private string? GetExclusivePathArgumentFromGui()
        {
            if (!checkBoxExcludePathFilter.Checked)
            {
                return null;
            }

            // 1. get all lines from text box which are not empty
            // 2. Prepend lines with '--exclude='
            // 3. Replace whitespace with '?' and convert to POSIX path
            // 4. join together with space as separator
            return string.Join(" ", textBoxExcludePaths.Lines.Where(p => !string.IsNullOrEmpty(p)).Select(p => $"--exclude={p.Replace(" ", "?")}".ToPosixPath()));
        }

        private void Close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBoxPathFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool filterByPath = checkBoxIncludePathFilter.Checked;
            textBoxIncludePaths.Enabled = filterByPath;
            labelPathHintInclude.Visible = filterByPath;
        }

        private void checkBoxExcludePathFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool filterByPath = checkBoxExcludePathFilter.Checked;
            textBoxExcludePaths.Enabled = filterByPath;
            labelPathHintExclude.Visible = filterByPath;
        }

        private void AddIncludePath_Click(object sender, EventArgs e)
        {
            string path = RequestUserFolderPath();

            if (path is not null)
            {
                textBoxIncludePaths.Text += path;
            }
        }

        private void AddExcludePath_Click(object sender, EventArgs e)
        {
            string path = RequestUserFilePath();

            if (path is not null)
            {
                path = path.Replace(Module.WorkingDir, "");
                textBoxExcludePaths.Text += path;
            }
        }

        private string? RequestUserFolderPath()
        {
            using FolderBrowserDialog dialog = new()
            {
                SelectedPath = Module.WorkingDir,
            };

            var result = dialog.ShowDialog(this);

            string subFoldersToClean;
            if (result != DialogResult.OK
                || !(subFoldersToClean = dialog.SelectedPath).StartsWith(Module.WorkingDir)
                || !Directory.Exists(subFoldersToClean)
                || subFoldersToClean.Equals(Module.WorkingDirGitDir.TrimEnd(Path.DirectorySeparatorChar)))
            {
                return null;
            }

            checkBoxIncludePathFilter.Checked = true;
            textBoxIncludePaths.Enabled = true;
            if (textBoxIncludePaths.Text.Length != 0)
            {
                textBoxIncludePaths.Text += Environment.NewLine;
            }

            string userPath = string.Join(Environment.NewLine, subFoldersToClean);
            return userPath;
        }

        private string? RequestUserFilePath()
        {
            using OpenFileDialog openFileDialog = new();

            openFileDialog.InitialDirectory = Module.WorkingDir;
            openFileDialog.RestoreDirectory = true;

            var result = openFileDialog.ShowDialog(this);

            string fileToExclude;
            if (result != DialogResult.OK
                || !(fileToExclude = openFileDialog.FileName).StartsWith(Module.WorkingDir))
            {
                return null;
            }

            checkBoxExcludePathFilter.Checked = true;
            textBoxExcludePaths.Enabled = true;

            if (textBoxExcludePaths.Text.Length != 0)
            {
                textBoxExcludePaths.Text += Environment.NewLine;
            }

            string userPath = string.Join(Environment.NewLine, fileToExclude);
            return userPath;
        }
    }
}
