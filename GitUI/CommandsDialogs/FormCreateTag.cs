using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands.Git.Commands;
using GitCommands.Git.Extensions;
using GitCommands.Git.Tag;
using GitUI.HelperDialogs;
using GitUI.Script;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    public sealed partial class FormCreateTag : GitModuleForm
    {
        private readonly TranslationString _messageCaption = new("Tag");
        private readonly TranslationString _noRevisionSelected = new("Select 1 revision to create the tag on.");
        private readonly TranslationString _pushToCaption = new("Push tag to '{0}'");
        private static readonly TranslationString _trsLightweight = new("Lightweight tag");
        private static readonly TranslationString _trsAnnotated = new("Annotated tag");
        private static readonly TranslationString _trsSignDefault = new("Sign with default GPG");
        private static readonly TranslationString _trsSignSpecificKey = new("Sign with specific GPG");

        private readonly IGitTagController _gitTagController;
        private string _currentRemote = "";

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormCreateTag()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormCreateTag(GitUICommands commands, ObjectId? objectId)
            : base(commands)
        {
            InitializeComponent();
            InitializeComplete();

            annotate.Items.AddRange(new object[] { _trsLightweight.Text, _trsAnnotated.Text, _trsSignDefault.Text, _trsSignSpecificKey.Text });
            annotate.SelectedIndex = 0;

            tagMessage.MistakeFont = new Font(tagMessage.MistakeFont, FontStyle.Underline);

            if (objectId is not null && objectId.IsArtificial)
            {
                objectId = null;
            }

            objectId ??= Module.GetCurrentCheckout();
            if (objectId is not null)
            {
                commitPickerSmallControl1.SetSelectedCommitHash(objectId.ToString());
            }

            _gitTagController = new GitTagController(commands);
        }

        private void FormCreateTag_Load(object sender, EventArgs e)
        {
            textBoxTagName.Select();
            _currentRemote = Module.GetCurrentRemote();
            if (string.IsNullOrEmpty(_currentRemote))
            {
                _currentRemote = "origin";
            }

            pushTag.Text = string.Format(_pushToCaption.Text, _currentRemote);
        }

        private void OkClick(object sender, EventArgs e)
        {
            try
            {
                var tagName = CreateTag();

                if (pushTag.Checked && !string.IsNullOrEmpty(tagName))
                {
                    PushTag(tagName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, TranslatedStrings.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CreateTag()
        {
            var objectId = commitPickerSmallControl1.SelectedObjectId;

            if (objectId is null)
            {
                MessageBox.Show(this, _noRevisionSelected.Text, _messageCaption.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return "";
            }

            GitCreateTagArgs createTagArgs = new(textBoxTagName.Text,
                                                     objectId,
                                                     GetSelectedOperation(annotate.SelectedIndex),
                                                     tagMessage.Text,
                                                     textBoxGpgKey.Text,
                                                     ForceTag.Checked);
            var success = _gitTagController.CreateTag(createTagArgs, this);
            if (!success)
            {
                return "";
            }

            DialogResult = DialogResult.OK;
            return textBoxTagName.Text;
        }

        private void PushTag(string tagName)
        {
            var pushCmd = GitCommandHelpers.PushTagCmd(_currentRemote, tagName, false);

            bool success = ScriptManager.RunEventScripts(this, ScriptEvent.BeforePush);
            if (!success)
            {
                return;
            }

            using FormRemoteProcess form = new(UICommands, process: null, pushCmd)
            {
                Remote = _currentRemote,
                Text = string.Format(_pushToCaption.Text, _currentRemote),
            };
            form.ShowDialog();

            if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
            {
                ScriptManager.RunEventScripts(this, ScriptEvent.AfterPush);
            }
        }

        private void AnnotateDropDownChanged(object sender, EventArgs e)
        {
            TagOperation tagOperation = GetSelectedOperation(annotate.SelectedIndex);
            textBoxGpgKey.Enabled = tagOperation == TagOperation.SignWithSpecificKey;
            keyIdLbl.Enabled = tagOperation == TagOperation.SignWithSpecificKey;
            bool providesMessage = tagOperation.CanProvideMessage();
            tagMessage.Enabled = providesMessage;
            tagMessage.BorderStyle = providesMessage ? BorderStyle.FixedSingle : BorderStyle.None;
        }

        private static TagOperation GetSelectedOperation(int dropdownSelection)
        {
            return dropdownSelection switch
            {
                0 => TagOperation.Lightweight,
                1 => TagOperation.Annotate,
                2 => TagOperation.SignWithDefaultKey,
                3 => TagOperation.SignWithSpecificKey,
                _ => throw new NotSupportedException("Invalid dropdownSelection")
            };
        }
    }
}
