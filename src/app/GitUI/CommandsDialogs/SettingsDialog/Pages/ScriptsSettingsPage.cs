using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using GitCommands;
using GitCommands.Utils;
using GitExtUtils.GitUI;
using GitExtUtils.GitUI.Theming;
using GitUI.ScriptsEngine;
using GitUIPluginInterfaces;
using Microsoft;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class ScriptsSettingsPage : SettingsPageWithHeader
    {
        private readonly TranslationString _scriptSettingsPageHelpDisplayArgumentsHelp = new("Arguments help");
        private readonly TranslationString _scriptSettingsPageHelpDisplayContent = new(@"Use {option} for normal replacement.
Use {{option}} for quoted replacement.

User inputs:
{UserInput}
{UserInput:a popup label}
{UserInput:a popup label=a default value}
{UserInput:a popup label=a default value using {sLocalBranch}}
{UserFiles}

Working directory:
{WorkingDir}

Repository:
{RepoName}

Selected commits:
{sHashes}

Selected revision:
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
{sSubject}
{sAuthor}
{sCommitter}
{sAuthorDate}
{sCommitDate}

Currently checked out revision:
{HEAD}   (checked out branch name or checked out commit hash)
{cTag}
{cBranch}
{cLocalBranch}
{cRemoteBranch}
{cRemoteBranchName}   (without the remote's name)
{cHash}
{cMessage}
{cSubject}
{cAuthor}
{cCommitter}
{cAuthorDate}
{cCommitDate}
{cDefaultRemote}
{cDefaultRemoteUrl}
{cDefaultRemotePathFromUrl}

Diff selection:
{SelectedRelativePaths}   (relative paths as they were in the selected commit)
{LineNumber}");

        private static readonly string[] WatchedProxyPropertiesOnFocusChanged =
        [
            nameof(ScriptInfoProxy.Name),
            nameof(ScriptInfoProxy.Enabled),
            nameof(ScriptInfoProxy.OnEvent),
            nameof(ScriptInfoProxy.Command),
            nameof(ScriptInfoProxy.Arguments),
        ];

        private static readonly string[] WatchedProxyPropertiesOnValueChanged =
        [
            nameof(ScriptInfoProxy.Icon),
            nameof(ScriptInfoProxy.IconFilePath),
        ];

        private static readonly ImageList EmbeddedIcons = new()
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = DpiUtil.Scale(new Size(16, 16))
        };

        private readonly BindingList<ScriptInfoProxy> _scripts = [];
        private readonly IScriptsManager _scriptsManager;
        private SimpleHelpDisplayDialog? _argumentsCheatSheet;
        private bool _handlingCheck;

        // settings maybe loaded before page is shown or after
        // we need to track that so we load images before we bind the list
        private bool _imagesLoaded;

        public ScriptsSettingsPage(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _scriptsManager = serviceProvider.GetRequiredService<IScriptsManager>();

            InitializeComponent();

            // stop the localisation of the propertygrid
            propertyGrid1.Text = string.Empty;

            InitializeComplete();

            foreach (ColumnHeader column in lvScripts.Columns)
            {
                column.Width = DpiUtil.Scale(column.Width);
            }

            tableLayoutPanel1.Dock = DockStyle.Fill;
            int margin = propertyGrid1.Margin.Left;
            propertyGrid1.Margin = new Padding(margin, margin, panelButtons.Width, margin);

            propertyGrid1.SelectedGridItemChanged += (_, e) =>
                UpdateScripts(WatchedProxyPropertiesOnFocusChanged, e.OldSelection?.PropertyDescriptor?.Name ?? "");

            propertyGrid1.PropertyValueChanged += (_, e) =>
                UpdateScripts(WatchedProxyPropertiesOnValueChanged, e.ChangedItem?.PropertyDescriptor?.Name ?? "");

            void UpdateScripts(string[] watchedProperties, string changedItem)
            {
                if (watchedProperties.Contains(changedItem))
                {
                    SelectedScript?.SetImages(EmbeddedIcons);
                    BindScripts(_scripts, SelectedScript);
                    propertyGrid1.Focus();
                }
            }
        }

        private ScriptInfoProxy? SelectedScript { get; set; }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);

            if (Parent is null)
            {
                _argumentsCheatSheet?.Close();
            }
        }

        public override void OnPageShown()
        {
            if (!EnvUtils.RunningOnWindows())
            {
                return;
            }

            System.Resources.ResourceManager rm = new("GitUI.Properties.Images", Assembly.GetExecutingAssembly());

            // dummy request; for some strange reason the ResourceSets are not loaded until after the first object request... bug?
            rm.GetObject("dummy");

            using System.Resources.ResourceSet resourceSet = rm.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            Validates.NotNull(resourceSet);
            foreach (DictionaryEntry icon in resourceSet.Cast<DictionaryEntry>().OrderBy(icon => icon.Key))
            {
                if (icon.Value is Bitmap bitmap)
                {
                    EmbeddedIcons.Images.Add(icon.Key.ToString()!, bitmap.AdaptLightness());
                }
            }

            resourceSet.Close();
            rm.ReleaseAllResources();

            lvScripts.LargeImageList = lvScripts.SmallImageList = EmbeddedIcons;
            _imagesLoaded = true;

            BindScripts(_scripts, null);
        }

        protected override void SettingsToPage()
        {
            _scripts.Clear();

            foreach (ScriptInfo script in _scriptsManager.GetScripts())
            {
                ScriptInfoProxy scriptProxy = script;
                scriptProxy.SetImages(EmbeddedIcons);
                _scripts.Add(scriptProxy);
            }

            if (_imagesLoaded)
            {
                BindScripts(_scripts, null);
            }

            base.SettingsToPage();
        }

        protected override void PageToSettings()
        {
            // TODO: this is an abomination, the whole script persistence must be scorched and rewritten

            BindingList<ScriptInfo> scripts = _scriptsManager.GetScripts();
            scripts.Clear();

            foreach (ScriptInfoProxy proxy in _scripts)
            {
                scripts.Add(proxy);
            }

            AppSettings.OwnScripts = _scriptsManager.SerializeIntoXml();

            base.PageToSettings();
        }

        private void BindScripts(IList<ScriptInfoProxy> scripts, ScriptInfoProxy? selectedScript)
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
                    Color color = !script.Enabled ? SystemColors.GrayText : SystemColors.WindowText;

                    ListViewItem lvitem = new(script.Name)
                    {
                        ToolTipText = $"{script.Command} {script.Arguments}",
                        Tag = script,
                        ForeColor = color,
                        ImageKey = script.GetIconImageKey(),
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

                if (selectedScript is not null)
                {
                    ListViewItem lvi = lvScripts.Items.Cast<ListViewItem>().FirstOrDefault(x => x.Tag == selectedScript);
                    if (lvi is not null)
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

                // Last column size fit the content
                if (lvScripts.Items.Count > 0)
                {
                    chdrCommand.Width = -1;
                }

                SetPropertyGridWidthOnce();
            }
            finally
            {
                lvScripts.EndUpdate();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ScriptInfoProxy script = _scripts.AddNew();
            script.HotkeyCommandIdentifier = Math.Max(ScriptsManager.MinimumUserScriptID, _scripts.Max(s => s.HotkeyCommandIdentifier)) + 1;
            script.Name = "<New Script>";
            script.Enabled = true;

            BindScripts(_scripts, script);

            propertyGrid1.Focus();
        }

        private void btnArgumentsHelp_Click(object sender, EventArgs e)
        {
            if (_argumentsCheatSheet?.Visible ?? false)
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
            propertyGrid1.SelectedObject = script;

            int index = _scripts.IndexOf(script);
            btnMoveUp.Enabled = index > 0;
            btnMoveDown.Enabled = index < _scripts.Count - 1;
        }

        private void SetPropertyGridWidthOnce()
        {
            const string widthSetTag = "width_set";

            string? tag = propertyGrid1.GetTag<string?>();
            if (tag != widthSetTag)
            {
                propertyGrid1.SetLabelColumnWidth(DpiUtil.Scale(240));
                propertyGrid1.SetTag(widthSetTag);
            }
        }
    }
}
