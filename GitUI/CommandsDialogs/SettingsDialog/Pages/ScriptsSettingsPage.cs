using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitUI.Script;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ScriptsSettingsPage : SettingsPageWithHeader
    {
        #region translation

        private readonly TranslationString _scriptSettingsPageHelpDisplayArgumentsHelp = new TranslationString("Arguments help");
        private readonly TranslationString _scriptSettingsPageHelpDisplayContent = new TranslationString(@"Use {option} for normal replacement.
Use {{option}} for quoted replacement.

User Input:
{UserInput}
{UserFiles}

Working Dir:
{WorkingDir}

Selected Commits:
{sHashes}

Selected Branch:
{sTag}
{sBranch}
{sLocalBranch}
{sRemoteBranch}
{sRemote}
{sRemoteUrl}
{sRemotePathFromUrl}
{sHash}
{sMessage}
{sAuthor}
{sCommitter}
{sAuthorDate}
{sCommitDate}

Current Branch:
{cTag}
{cBranch}
{cLocalBranch}
{cRemoteBranch}
{cHash}
{cMessage}
{cAuthor}
{cCommitter}
{cAuthorDate}
{cCommitDate}
{cDefaultRemote}
{cDefaultRemoteUrl}
{cDefaultRemotePathFromUrl}");

        #endregion

        private string _iconName = "bug";

        public ScriptsSettingsPage()
        {
            InitializeComponent();
            HotkeyCommandIdentifier.Width = DpiUtil.Scale(39);
            Text = "Scripts";
            InitializeComplete();

            HotkeyCommandIdentifier.DataPropertyName = nameof(ScriptInfo.HotkeyCommandIdentifier);
            EnabledColumn.DataPropertyName = nameof(ScriptInfo.Enabled);
            OnEvent.DataPropertyName = nameof(ScriptInfo.OnEvent);
            AskConfirmation.DataPropertyName = nameof(ScriptInfo.AskConfirmation);
        }

        public override bool IsInstantSavePage => true;

        public override void OnPageShown()
        {
            if (EnvUtils.RunningOnWindows())
            {
                System.Resources.ResourceManager rm =
                    new System.Resources.ResourceManager("GitUI.Properties.Images",
                                System.Reflection.Assembly.GetExecutingAssembly());

                // dummy request; for some strange reason the ResourceSets are not loaded untill after the first object request... bug?
                rm.GetObject("dummy");

                System.Resources.ResourceSet resourceSet = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);

                contextMenuStrip_SplitButton.Items.Clear();

                var iconItems = new List<ToolStripItem>();
                foreach (System.Collections.DictionaryEntry icon in resourceSet)
                {
                    // add entry to toolstrip
                    if (icon.Value is Icon)
                    {
                        ////contextMenuStrip_SplitButton.Items.Add(icon.Key.ToString(), (Image)((Icon)icon.Value).ToBitmap(), SplitButtonMenuItem_Click);
                    }
                    else if (icon.Value is Bitmap bitmap)
                    {
                        iconItems.Add(new ToolStripMenuItem(icon.Key.ToString(), bitmap, SplitButtonMenuItem_Click));
                    }
                }

                contextMenuStrip_SplitButton.Items.AddRange(iconItems.OrderBy(i => i.Text).ToArray());

                resourceSet.Close();
                rm.ReleaseAllResources();
            }
        }

        protected override void SettingsToPage()
        {
            scriptEvent.DataSource = Enum.GetValues(typeof(ScriptEvent));
            LoadScripts();
        }

        protected override void PageToSettings()
        {
            SaveScripts();
        }

        private static void SaveScripts()
        {
            AppSettings.OwnScripts = ScriptManager.SerializeIntoXml();
        }

        private void LoadScripts()
        {
            ScriptList.DataSource = ScriptManager.GetScripts();
        }

        private void ClearScriptDetails()
        {
            nameTextBox.Clear();
            commandTextBox.Clear();
            argumentsTextBox.Clear();
            inMenuCheckBox.Checked = false;
        }

        private void RefreshScriptDetails()
        {
            if (ScriptList.SelectedRows.Count == 0)
            {
                return;
            }

            var scriptInfo = (ScriptInfo)ScriptList.SelectedRows[0].DataBoundItem;

            nameTextBox.Text = scriptInfo.Name;
            commandTextBox.Text = scriptInfo.Command;
            argumentsTextBox.Text = scriptInfo.Arguments;
            scriptRunInBackground.Checked = scriptInfo.RunInBackground;
            scriptIsPowerShell.Checked = scriptInfo.IsPowerShell;
            inMenuCheckBox.Checked = scriptInfo.AddToRevisionGridContextMenu;
            scriptEnabled.Checked = scriptInfo.Enabled;
            scriptNeedsConfirmation.Checked = scriptInfo.AskConfirmation;
            scriptEvent.SelectedItem = scriptInfo.OnEvent;
            sbtn_icon.Image = ResizeForSplitButton(scriptInfo.GetIcon());
            _iconName = scriptInfo.Icon;

            foreach (ToolStripItem item in contextMenuStrip_SplitButton.Items)
            {
                if (item.ToString() == _iconName)
                {
                    item.Font = new Font(item.Font, FontStyle.Bold);
                }
            }
        }

        private void addScriptButton_Click(object sender, EventArgs e)
        {
            ScriptList.ClearSelection();
            ScriptInfo script = ScriptManager.GetScripts().AddNew();
            script.HotkeyCommandIdentifier = ScriptManager.NextHotkeyCommandIdentifier();
            ScriptList.Rows[ScriptList.RowCount - 1].Selected = true;
            ScriptList_SelectionChanged(null, null); // needed for linux
        }

        private void removeScriptButton_Click(object sender, EventArgs e)
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                ScriptManager.GetScripts().Remove(ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo);

                ClearScriptDetails();
            }
        }

        private void ScriptInfoFromEdits()
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                var selectedScriptInfo = (ScriptInfo)ScriptList.SelectedRows[0].DataBoundItem;

                selectedScriptInfo.Name = nameTextBox.Text;
                selectedScriptInfo.Command = commandTextBox.Text;
                selectedScriptInfo.Arguments = argumentsTextBox.Text;
                selectedScriptInfo.AddToRevisionGridContextMenu = inMenuCheckBox.Checked;
                selectedScriptInfo.Enabled = scriptEnabled.Checked;
                selectedScriptInfo.RunInBackground = scriptRunInBackground.Checked;
                selectedScriptInfo.IsPowerShell = scriptIsPowerShell.Checked;
                selectedScriptInfo.AskConfirmation = scriptNeedsConfirmation.Checked;
                selectedScriptInfo.OnEvent = (ScriptEvent)scriptEvent.SelectedItem;
                selectedScriptInfo.Icon = _iconName;
            }
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                ScriptInfo scriptInfo = ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo;
                int index = ScriptManager.GetScripts().IndexOf(scriptInfo);
                ScriptManager.GetScripts().Remove(scriptInfo);
                ScriptManager.GetScripts().Insert(Math.Max(index - 1, 0), scriptInfo);

                ScriptList.ClearSelection();
                ScriptList.Rows[Math.Max(index - 1, 0)].Selected = true;
                ScriptList.Focus();
            }
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                ScriptInfo scriptInfo = ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo;
                int index = ScriptManager.GetScripts().IndexOf(scriptInfo);
                ScriptManager.GetScripts().Remove(scriptInfo);
                ScriptManager.GetScripts().Insert(Math.Min(index + 1, ScriptManager.GetScripts().Count), scriptInfo);

                ScriptList.ClearSelection();
                ScriptList.Rows[Math.Max(index + 1, 0)].Selected = true;
                ScriptList.Focus();
            }
        }

        private void browseScriptButton_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                RestoreDirectory = true
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    commandTextBox.Text = ofd.FileName;
                }
            }
        }

        private void ScriptList_SelectionChanged(object sender, EventArgs e)
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                RefreshScriptDetails();

                removeScriptButton.Enabled = true;
                moveDownButton.Enabled = moveUpButton.Enabled = false;
                if (ScriptList.SelectedRows[0].Index > 0)
                {
                    moveUpButton.Enabled = true;
                }

                if (ScriptList.SelectedRows[0].Index < ScriptList.RowCount - 1)
                {
                    moveDownButton.Enabled = true;
                }
            }
            else
            {
                removeScriptButton.Enabled = false;
                moveUpButton.Enabled = false;
                moveDownButton.Enabled = false;
                ClearScriptDetails();
            }

            UpdateIconVisibility();
        }

        private void ScriptInfoEdit_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ScriptInfoFromEdits();
            ScriptList.Refresh();
        }

        private void ScriptList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ScriptList_SelectionChanged(null, null); // needed for linux
        }

        private void SplitButtonMenuItem_Click(object sender, EventArgs e)
        {
            // reset bold item to regular
            var item = contextMenuStrip_SplitButton.Items.OfType<ToolStripMenuItem>().FirstOrDefault(s => s.Font.Bold);
            if (item != null)
            {
                item.Font = new Font(contextMenuStrip_SplitButton.Font, FontStyle.Regular);
            }

            // make new item bold
            ((ToolStripMenuItem)sender).Font = new Font(((ToolStripMenuItem)sender).Font, FontStyle.Bold);

            // set new image on button
            sbtn_icon.Image = ResizeForSplitButton((Bitmap)((ToolStripMenuItem)sender).Image);

            _iconName = ((ToolStripMenuItem)sender).Text;

            // store variables
            ScriptInfoEdit_Validating(sender, new System.ComponentModel.CancelEventArgs());
        }

        private Bitmap ResizeForSplitButton(Bitmap b)
        {
            return ResizeBitmap(b, 12, 12);
        }

        [CanBeNull]
        public Bitmap ResizeBitmap(Bitmap b, int width, int height)
        {
            if (b == null)
            {
                return null;
            }

            var result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(b, 0, 0, width, height);
            }

            return result;
        }

        private void scriptEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateIconVisibility();
        }

        private void UpdateIconVisibility()
        {
            bool showIcon = scriptEvent.Text == ScriptEvent.ShowInUserMenuBar.ToString() || inMenuCheckBox.Checked;

            sbtn_icon.Visible = showIcon;
            lbl_icon.Visible = showIcon;
        }

        private void buttonShowArgumentsHelp_Click(object sender, EventArgs e)
        {
            var helpDisplayDialog = new SimpleHelpDisplayDialog
            {
                DialogTitle = _scriptSettingsPageHelpDisplayArgumentsHelp.Text,
                ContentText = _scriptSettingsPageHelpDisplayContent.Text.Replace("\n", Environment.NewLine)
            };

            helpDisplayDialog.ShowDialog();
        }

        private void argumentsTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert))
            {
                ((RichTextBox)sender).Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
                e.Handled = true;
            }
        }

        private void inMenuCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateIconVisibility();
        }
    }
}
