using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.Script;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ScriptsSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _scriptSettingsPageHelpDisplayArgumentsHelp = new TranslationString("Arguments help");
        private readonly TranslationString _scriptSettingsPageHelpDisplayContent = new TranslationString(@"Use {option} for normal replacement.
Use {{option}} for quoted replacement.

User Input:
{UserInput}
{UserFiles}

Working Dir:
{WorkingDir}

Repository:
{RepoName}

Selected Commits:
{sHashes}

Selected Branch:
{sTag}
{sBranch}
{sLocalBranch}
{sRemoteBranch}
{sRemoteBranchName}   (without the remote's name)
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
{cRemoteBranchName}   (without the remote's name)
{cHash}
{cMessage}
{cAuthor}
{cCommitter}
{cAuthorDate}
{cCommitDate}
{cDefaultRemote}
{cDefaultRemoteUrl}
{cDefaultRemotePathFromUrl}");

        private static readonly string[] WatchedProxyProperties = new string[]
        {
            nameof(ScriptInfoProxy.Name),
            nameof(ScriptInfoProxy.Enabled),
            nameof(ScriptInfoProxy.Icon),
            nameof(ScriptInfoProxy.OnEvent),
            nameof(ScriptInfoProxy.Command),
            nameof(ScriptInfoProxy.Arguments),
        };
        private static ImageList EmbeddedIcons = new ImageList();
        private BindingList<ScriptInfoProxy> _scripts;
        private SimpleHelpDisplayDialog _argumentsCheatSheet;
        private bool _handlingCheck;

        // settings maybe loaded before page is shwon or after
        // we need to track that so we load images before we bind the list
        private bool _imagsLoaded;

        public ScriptsSettingsPage()
        {
            InitializeComponent();
            Text = "Scripts";

            // stop the localisation of the propertygrid
            propertyGrid1.Text = string.Empty;

            InitializeComplete();

            foreach (ColumnHeader column in lvScripts.Columns)
            {
                column.Width = DpiUtil.Scale(column.Width);
            }

            tableLayoutPanel1.Dock = DockStyle.Fill;
            var margin = propertyGrid1.Margin.Left;
            propertyGrid1.Margin = new Padding(margin, margin, panelButtons.Width, margin);

            propertyGrid1.SelectedGridItemChanged += (s, e) =>
            {
                if (WatchedProxyProperties.Contains(e.OldSelection?.PropertyDescriptor.Name ?? ""))
                {
                    BindScripts(_scripts, SelectedScript);
                    propertyGrid1.Focus();
                }
            };
        }

        private ScriptInfoProxy SelectedScript { get; set; }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent is null && _argumentsCheatSheet is object)
            {
                _argumentsCheatSheet.Close();
            }
        }

        public override void OnPageShown()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return;
            }

            var rm = new System.Resources.ResourceManager("GitUI.Properties.Images", Assembly.GetExecutingAssembly());

            // dummy request; for some strange reason the ResourceSets are not loaded untill after the first object request... bug?
            rm.GetObject("dummy");

            using System.Resources.ResourceSet resourceSet = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            foreach (DictionaryEntry icon in resourceSet.Cast<DictionaryEntry>().OrderBy(icon => icon.Key))
            {
                if (icon.Value is Bitmap bitmap)
                {
                    EmbeddedIcons.Images.Add(icon.Key.ToString(), bitmap.AdaptLightness());
                }
            }

            resourceSet.Close();
            rm.ReleaseAllResources();

            lvScripts.LargeImageList =
                lvScripts.SmallImageList = EmbeddedIcons;
            _imagsLoaded = true;

            if (_scripts is object)
            {
                BindScripts(_scripts, null);
            }
        }

        protected override void SettingsToPage()
        {
            _scripts = new BindingList<ScriptInfoProxy>();
            foreach (var script in ScriptManager.GetScripts())
            {
                _scripts.Add(script);
            }

            if (_imagsLoaded)
            {
                BindScripts(_scripts, null);
            }
        }

        protected override void PageToSettings()
        {
            // TODO: this is an abomination, the whole script persistence must be scorched and rewritten

            BindingList<ScriptInfo> scripts = ScriptManager.GetScripts();
            scripts.Clear();

            foreach (ScriptInfoProxy proxy in _scripts)
            {
                scripts.Add(proxy);
            }

            AppSettings.OwnScripts = ScriptManager.SerializeIntoXml();
        }

        private void BindScripts(IList<ScriptInfoProxy> scripts, ScriptInfoProxy selectedScript)
        {
            try
            {
                lvScripts.BeginUpdate();
                lvScripts.SelectedIndexChanged -= lvScripts_SelectedIndexChanged;
                lvScripts.ItemChecked -= lvScripts_ItemChecked;
                lvScripts.Items.Clear();

                if (scripts.Count < 1)
                {
                    btnAdd.Focus();
                    return;
                }

                foreach (ScriptInfoProxy script in scripts)
                {
                    var color = !script.Enabled ? SystemColors.GrayText : SystemColors.WindowText;

                    ListViewItem lvitem = new ListViewItem(script.Name)
                    {
                        ToolTipText = $"{script.Command} {script.Arguments}",
                        Tag = script,
                        ForeColor = color,
                        ImageKey = script.Icon,
                        Checked = script.Enabled
                    };
                    lvScripts.Items.Add(lvitem);

                    lvitem.SubItems.Add(script.OnEvent.ToString());
                    lvitem.SubItems.Add(string.Concat(script.Command, " ", script.Arguments));
                }

                lvScripts.SelectedIndexChanged += lvScripts_SelectedIndexChanged;
                lvScripts.ItemChecked += lvScripts_ItemChecked;
                lvScripts.SelectedIndices.Clear();

                SelectedScript = selectedScript;

                if (selectedScript is object)
                {
                    ListViewItem lvi = lvScripts.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Tag == selectedScript);
                    if (lvi != null)
                    {
                        lvi.Selected = true;
                        lvScripts.FocusedItem = lvi;
                    }
                }

                if (lvScripts.FocusedItem is null)
                {
                    lvScripts.Items[0].Selected = true;
                    lvScripts.FocusedItem = lvScripts.Items[0];
                }

                lvScripts.Select();
            }
            finally
            {
                lvScripts.EndUpdate();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ScriptInfoProxy script = _scripts.AddNew();
            script.HotkeyCommandIdentifier = Math.Max(ScriptManager.MinimumUserScriptID, _scripts.Max(s => s.HotkeyCommandIdentifier)) + 1;
            script.Name = "<New Script>";

            BindScripts(_scripts, script);
        }

        private void btnArgumentsHelp_Click(object sender, EventArgs e)
        {
            if (_argumentsCheatSheet is object)
            {
                _argumentsCheatSheet.BringToFront();
                return;
            }

            _argumentsCheatSheet = new SimpleHelpDisplayDialog
            {
                DialogTitle = _scriptSettingsPageHelpDisplayArgumentsHelp.Text,
                ContentText = _scriptSettingsPageHelpDisplayContent.Text.Replace("\n", Environment.NewLine)
            };

            _argumentsCheatSheet.Show(this);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (SelectedScript is null)
            {
                return;
            }

            _scripts.Remove(SelectedScript);
            BindScripts(_scripts, null);
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (SelectedScript is null)
            {
                return;
            }

            ScriptInfoProxy script = SelectedScript;

            int index = _scripts.IndexOf(script);
            _scripts.Remove(script);
            _scripts.Insert(Math.Min(index + 1, _scripts.Count), script);

            BindScripts(_scripts, script);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (SelectedScript is null)
            {
                return;
            }

            ScriptInfoProxy script = SelectedScript;

            int index = _scripts.IndexOf(script);
            _scripts.Remove(script);
            _scripts.Insert(Math.Max(index - 1, 0), script);

            BindScripts(_scripts, script);
        }

        private void lvScripts_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (_handlingCheck)
            {
                return;
            }

            _handlingCheck = true;
            e.Item.Selected = true;
            e.Item.Checked = !e.Item.Checked;
            _handlingCheck = false;
        }

        private void lvScripts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvScripts.SelectedItems.Count < 1 || !(lvScripts.SelectedItems[0].Tag is ScriptInfoProxy script))
            {
                propertyGrid1.SelectedObject = null;
                return;
            }

            SelectedScript = script;
            script.SetImages(EmbeddedIcons);

            propertyGrid1.SelectedObject = script;

            int index = _scripts.IndexOf(script);
            btnMoveUp.Enabled = index > 0;
            btnMoveDown.Enabled = index < _scripts.Count - 1;
        }
    }
}
