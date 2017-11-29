﻿using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;
using GitUI.Script;
using ResourceManager;
using GitCommands.Git;
using GitCommands.Git.Tag;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCreateTag : GitModuleForm
    {
        private readonly TranslationString _messageCaption = new TranslationString("Tag");

        private readonly TranslationString _noRevisionSelected =
            new TranslationString("Select 1 revision to create the tag on.");

        private readonly TranslationString _pushToCaption = new TranslationString("Push tag to '{0}'");

        private string _currentRemote = "";

        private static readonly TranslationString _trsLigthweight = new TranslationString("Lightweight tag");
        private static readonly TranslationString _trsAnnotated = new TranslationString("Annotated tag");
        private static readonly TranslationString _trsSignDefault = new TranslationString("Sign with default GPG");
        private static readonly TranslationString _trsSignSpecificKey = new TranslationString("Sign with specific GPG");

        private static readonly string[] DropwdownTagOperation = { _trsLigthweight.Text, _trsAnnotated.Text, _trsSignDefault.Text, _trsSignSpecificKey.Text };
        private readonly IGitTagController _gitTagController;


        public FormCreateTag(GitUICommands aCommands, GitRevision revision)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            tagMessage.MistakeFont = new Font(SystemFonts.MessageBoxFont, FontStyle.Underline);
            commitPickerSmallControl1.UICommandsSource = this;
            if (IsUICommandsInitialized)
                commitPickerSmallControl1.SetSelectedCommitHash(revision == null ? Module.GetCurrentCheckout() : revision.Guid);

            if (aCommands != null)
            {
                _gitTagController = new GitTagController(() => aCommands.Module.WorkingDir);
            }
        }

        private void FormCreateTag_Load(object sender, EventArgs e)
        {
            textBoxTagName.Select();
            _currentRemote = Module.GetCurrentRemote();
            if (String.IsNullOrEmpty(_currentRemote))
                _currentRemote = "origin";
            pushTag.Text = string.Format(_pushToCaption.Text, _currentRemote);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var tagName = CreateTag();

                if (pushTag.Checked && !string.IsNullOrEmpty(tagName))
                    PushTag(tagName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        private string CreateTag()
        {
            if (string.IsNullOrWhiteSpace(commitPickerSmallControl1.SelectedCommitHash))
            {
                MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text);
                return string.Empty;
            }

            var createTagArgs = new GitCreateTagArgs(textBoxTagName.Text,
                                                     commitPickerSmallControl1.SelectedCommitHash,
                                                     GetSelectedOperation(annotate.SelectedIndex),
                                                     tagMessage.Text,
                                                     textBoxGpgKey.Text,
                                                     ForceTag.Checked);
            var cmd = _gitTagController.GetCreateTagCommand(createTagArgs);
            if (!UICommands.StartCommandLineProcessDialog(cmd, this))
            {
                return string.Empty;
            }

            DialogResult = DialogResult.OK;
            return textBoxTagName.Text;
        }

        private void PushTag(string tagName)
        {
            var pushCmd = GitCommandHelpers.PushTagCmd(_currentRemote, tagName, false);

            ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);

            using (var form = new FormRemoteProcess(Module, pushCmd)
            {
                Remote = _currentRemote,
                Text = string.Format(_pushToCaption.Text, _currentRemote),
            })
            {

                form.ShowDialog();

                if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
                {
                    ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
                }
            }
        }

        private void AnnotateDropDownChanged(object sender, EventArgs e)
        {
            TagOperation tagOperation = GetSelectedOperation(annotate.SelectedIndex);
            textBoxGpgKey.Enabled = tagOperation == TagOperation.SignWithSpecificKey;
            keyIdLbl.Enabled = tagOperation == TagOperation.SignWithSpecificKey;
            tagMessage.Enabled = tagOperation.CanProvideMessage();
        }

        private TagOperation GetSelectedOperation(int dropdownSelection)
        {
            TagOperation returnValue = TagOperation.Lightweight;
            switch (dropdownSelection)
            {
                case 0:
                    returnValue = TagOperation.Lightweight;
                    break;
                case 1:
                    returnValue = TagOperation.Annotate;
                    break;
                case 2:
                    returnValue = TagOperation.SignWithDefaultKey;
                    break;
                case 3:
                    returnValue = TagOperation.SignWithSpecificKey;
                    break;
                default:
                    throw new NotSupportedException("Invalid dropdownSelection");
            }
            return returnValue;
        }
    }
}
