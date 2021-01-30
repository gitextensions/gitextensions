﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Git.Commands;
using GitCommands.Utils;
using GitUI.HelperDialogs;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public partial class FormCleanupRepository : GitModuleForm
    {
        private readonly TranslationString _reallyCleanupQuestion =
            new TranslationString("Are you sure you want to cleanup the repository?");
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
            checkBoxPathFilter_CheckedChanged(null, null);
        }

        public void SetPathArgument(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                checkBoxPathFilter.Checked = false;
                textBoxPaths.Text = "";
            }
            else
            {
                checkBoxPathFilter.Checked = true;
                textBoxPaths.Text = path;
            }
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            var cleanUpCmd = GitCommandHelpers.CleanCmd(GetCleanMode(), dryRun: true, directories: RemoveDirectories.Checked, paths: GetPathArgumentFromGui());
            string cmdOutput = FormProcess.ReadDialog(this, process: null, arguments: cleanUpCmd, Module.WorkingDir, input: null, useDialogSettings: true);
            PreviewOutput.Text = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);
        }

        private void Cleanup_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, _reallyCleanupQuestion.Text, _reallyCleanupQuestionCaption.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var cleanUpCmd = GitCommandHelpers.CleanCmd(GetCleanMode(), dryRun: false, directories: RemoveDirectories.Checked, paths: GetPathArgumentFromGui());
                string cmdOutput = FormProcess.ReadDialog(this, process: null, arguments: cleanUpCmd, Module.WorkingDir, input: null, useDialogSettings: true);
                PreviewOutput.Text = EnvUtils.ReplaceLinuxNewLinesDependingOnPlatform(cmdOutput);
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

        [CanBeNull]
        private string GetPathArgumentFromGui()
        {
            if (!checkBoxPathFilter.Checked)
            {
                return null;
            }

            // 1. get all lines from text box which are not empty
            // 2. wrap lines with ""
            // 3. join together with space as separator
            return string.Join(" ", textBoxPaths.Lines.Where(a => !string.IsNullOrEmpty(a)).Select(a => string.Format("\"{0}\"", a)));
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void checkBoxPathFilter_CheckedChanged(object sender, EventArgs e)
        {
            bool filterByPath = checkBoxPathFilter.Checked;
            textBoxPaths.Enabled = filterByPath;
            labelPathHint.Visible = filterByPath;
        }

        private void AddPath_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = Module.WorkingDir,
                EnsurePathExists = true,
                EnsureFileExists = false,
                IsFolderPicker = true,
                ShowHiddenItems = false,
                Multiselect = true,
                AddToMostRecentlyUsedList = false,
            };

            var result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                var subFoldersToClean = dialog.FileNames
                    .Where(d => d.StartsWith(Module.WorkingDir)
                                && Directory.Exists(d)
                                && !d.Equals(Module.WorkingDirGitDir.TrimEnd(Path.DirectorySeparatorChar)))
                    .ToList();

                if (!subFoldersToClean.Any())
                {
                    return;
                }

                checkBoxPathFilter.Checked = true;
                textBoxPaths.Enabled = true;
                if (textBoxPaths.Text.Length != 0)
                {
                    textBoxPaths.Text += Environment.NewLine;
                }

                textBoxPaths.Text += string.Join(Environment.NewLine, subFoldersToClean);
            }
        }
    }
}
