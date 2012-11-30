using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Config;
using GitUI.Editor;
using GitUI.Script;
using Gravatar;
using Microsoft.Win32;
using ResourceManager.Translation;
using GitUI.SettingsDialog;
using GitUI.SettingsDialog.Pages;

namespace GitUI
{
    public sealed partial class FormSettings : GitModuleForm
    {
        #region Translation
        private readonly TranslationString __diffToolSuggestCaption = new TranslationString("Suggest difftool cmd");

        private readonly TranslationString _loadingSettingsFailed =
            new TranslationString("Could not load settings.");

        private readonly TranslationString _cantFindGitMessage =
            new TranslationString("The command to run git is not configured correct." + Environment.NewLine +
                "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine +
                Environment.NewLine + "Do you want to set the correct command now?");

        private readonly TranslationString _cantFindGitMessageCaption =
            new TranslationString("Incorrect path");

        private readonly TranslationString _linuxToolsShNotFound =
            new TranslationString("The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
                "Please make sure there are linux tools installed (through msysgit or cygwin) or set the correct path manually.");

        private readonly TranslationString _linuxToolsShNotFoundCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _shCanBeRun =
            new TranslationString("Command sh can be run using: {0}sh");

        private readonly TranslationString _shCanBeRunCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _puttyFoundAuto =
            new TranslationString("All paths needed for PuTTY could be automatically found and are set.");

        private readonly TranslationString _puttyFoundAutoCaption =
            new TranslationString("PuTTY");

        #endregion

        private string IconName = "bug";

        SettingsPageRegistry _settingsPageRegistry = new SettingsPageRegistry();
        CommonLogic _commonLogic;
        CheckSettingsLogic _checkSettingsLogic;
        ChecklistSettingsPage _checklistSettingsPage;
        GitSettingsPage _gitSettingsPage;
        GitExtensionsSettingsPage _gitExtensionsSettingsPage;
        AppearanceSettingsPage _appearanceSettingsPage;
        ColorsSettingsPage _colorsSettingsPage;
        GlobalSettingsSettingsPage _globalSettingsSettingsPage;
        HotkeysSettingsPage _hotkeysSettingsPage;
        LocalSettingsSettingsPage _localSettingsSettingsPage;
        ScriptsSettingsPage _scriptsSettingsPage;
        ShellExtensionSettingsPage _shellExtensionSettingsPage;
        SshSettingsPage _sshSettingsPage;
        StartPageSettingsPage _startPageSettingsPage;

        private FormSettings()
            : this(null)
        { }

        Panel settingsPagePanel;

        public FormSettings(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();

            FillEncodings(Global_FilesEncoding);
            FillEncodings(Local_FilesEncoding);

            string npp = MergeToolsHelper.FindFileInFolders("notepad++.exe", "Notepad++");
            if (string.IsNullOrEmpty(npp))
                npp = "notepad++";
            else
                npp = "\"" + npp + "\"";

            GlobalEditor.Items.AddRange(new Object[] { "\"" + Settings.GetGitExtensionsFullPath() + "\" fileeditor", "vi", "notepad", npp + " -multiInst -nosession" });

            // TODO:
            _commonLogic = new CommonLogic(Module); // TODO: use a common instance?
            _checkSettingsLogic = new CheckSettingsLogic(_commonLogic, Module); // TODO
            _checklistSettingsPage = new ChecklistSettingsPage(_commonLogic, _checkSettingsLogic, Module); // TODO
            _checkSettingsLogic.ChecklistSettingsPage = _checklistSettingsPage; // TODO
            _settingsPageRegistry.RegisterSettingsPage(_checklistSettingsPage);

            _gitSettingsPage = new GitSettingsPage(_checkSettingsLogic);
            _settingsPageRegistry.RegisterSettingsPage(_gitSettingsPage);

            _gitExtensionsSettingsPage = new GitExtensionsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_gitExtensionsSettingsPage);

            _appearanceSettingsPage = new AppearanceSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_appearanceSettingsPage);

            _colorsSettingsPage = new ColorsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_colorsSettingsPage);

            _startPageSettingsPage = new StartPageSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_startPageSettingsPage);

            _globalSettingsSettingsPage = new GlobalSettingsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_globalSettingsSettingsPage);

            _localSettingsSettingsPage = new LocalSettingsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_localSettingsSettingsPage);

            _sshSettingsPage = new SshSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_sshSettingsPage);

            _scriptsSettingsPage = new ScriptsSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_scriptsSettingsPage);
            
            _hotkeysSettingsPage = new HotkeysSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_hotkeysSettingsPage);
            
            _shellExtensionSettingsPage = new ShellExtensionSettingsPage();
            _settingsPageRegistry.RegisterSettingsPage(_shellExtensionSettingsPage);
            
            settingsTreeViewUserControl1.SetSettingsPages(_settingsPageRegistry);

            // todo: alter this when all tab pages are converted
            //this.tableLayoutPanel3.Controls.Add(this.tabControl1, 1, 0);
            tableLayoutPanel3.Controls.Remove(tabControl1);
            settingsPagePanel = new Panel();
            settingsPagePanel.Dock = DockStyle.Fill;
            tableLayoutPanel3.Controls.Add(settingsPagePanel, 1, 1);
        }

        private void settingsTreeViewUserControl1_SettingsPageSelected(object sender, SettingsPageSelectedEventArgs e)
        {
            settingsPagePanel.Controls.Clear();
            if (e.SettingsPageBase == null)
            {
                settingsPagePanel.Controls.Add(tabControl1);
                labelSettingsPageTitle.Text = "(TabControl to be migrated)";
            }
            else
            {
                settingsPagePanel.Controls.Add(e.SettingsPageBase);
                e.SettingsPageBase.Dock = DockStyle.Fill;
                labelSettingsPageTitle.Text = e.SettingsPageBase.Text;
            }
        }

        private static void SetCheckboxFromString(CheckBox checkBox, string str)
        {
            str = str.Trim().ToLower();

            switch (str)
            {
                case "true":
                    {
                        checkBox.CheckState = CheckState.Checked;
                        return;
                    }
                case "false":
                    {
                        checkBox.CheckState = CheckState.Unchecked;
                        return;
                    }
                default:
                    checkBox.CheckState = CheckState.Indeterminate;
                    return;
            }
        }

        private void EncodingToCombo(Encoding encoding, ComboBox combo)
        {
            if (encoding == null)
                combo.Text = "";
            else
                combo.Text = encoding.EncodingName;
        }

        private Encoding ComboToEncoding(ComboBox combo)
        {
            return combo.SelectedItem as Encoding;
        }

        private void FillEncodings(ComboBox combo)
        {
            combo.Items.AddRange(Settings.availableEncodings.Values.ToArray());
            combo.DisplayMember = "EncodingName";
        }

        private bool loadingSettings;

        public void LoadSettings()
        {
            loadingSettings = true;
            try
            {
                foreach (var settingsPage in _settingsPageRegistry.GetSettingsPages())
                {
                    settingsPage.LoadSettings();
                }

                scriptEvent.DataSource = Enum.GetValues(typeof(ScriptEvent));
                EncodingToCombo(Module.GetFilesEncoding(false), Global_FilesEncoding);
                EncodingToCombo(Module.GetFilesEncoding(true), Local_FilesEncoding);

                ConfigFile localConfig = Module.GetLocalConfig();
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

                UserName.Text = localConfig.GetValue("user.name");
                UserEmail.Text = localConfig.GetValue("user.email");
                Editor.Text = localConfig.GetPathValue("core.editor");
                LocalMergeTool.Text = localConfig.GetValue("merge.tool");

                GlobalUserName.Text = globalConfig.GetValue("user.name");
                GlobalUserEmail.Text = globalConfig.GetValue("user.email");
                GlobalEditor.Text = globalConfig.GetPathValue("core.editor");
                GlobalMergeTool.Text = globalConfig.GetValue("merge.tool");
                CommitTemplatePath.Text = globalConfig.GetValue("commit.template");

                SetCheckboxFromString(KeepMergeBackup, localConfig.GetValue("mergetool.keepBackup"));

                string autocrlf = localConfig.GetValue("core.autocrlf").ToLower();
                localAutoCrlfFalse.Checked = autocrlf == "false";
                localAutoCrlfInput.Checked = autocrlf == "input";
                localAutoCrlfTrue.Checked = autocrlf == "true";

                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergetoolPath.Text = globalConfig.GetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text));
                if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                    MergeToolCmd.Text = globalConfig.GetPathValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text));

                GlobalDiffTool.Text = CheckSettingsLogic.GetGlobalDiffToolFromConfig();

                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolPath.Text = globalConfig.GetPathValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text));
                if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                    DifftoolCmd.Text = globalConfig.GetPathValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text));

                SetCheckboxFromString(GlobalKeepMergeBackup, globalConfig.GetValue("mergetool.keepBackup"));

                string globalAutocrlf = string.Empty;
                if (globalConfig.HasValue("core.autocrlf"))
                {
                    globalAutocrlf = globalConfig.GetValue("core.autocrlf").ToLower();
                }
                else if (!string.IsNullOrEmpty(Settings.GitBinDir))
                {
                    try
                    {
                        //the following lines only work for msysgit (i think). MSysgit has a config file
                        //in the etc directory which is checked after the local and global config. In
                        //practice this is only used to core.autocrlf. If there are more cases, we might
                        //need to consider a better solution.
                        var configFile =
                            new ConfigFile(Path.GetDirectoryName(Settings.GitBinDir).Replace("bin", "etc\\gitconfig"), false);
                        globalAutocrlf = configFile.GetValue("core.autocrlf").ToLower();
                    }
                    catch
                    {
                    }
                }

                globalAutoCrlfFalse.Checked = globalAutocrlf == "false";
                globalAutoCrlfInput.Checked = globalAutocrlf == "input";
                globalAutoCrlfTrue.Checked = globalAutocrlf == "true";

                chkCascadedContextMenu.Checked = Settings.ShellCascadeContextMenu;

                for (int i = 0; i < Settings.ShellVisibleMenuItems.Length; i++)
                {
                    chlMenuEntries.SetItemChecked(i, Settings.ShellVisibleMenuItems[i] == '1');
                }

                LoadScripts();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _loadingSettingsFailed.Text + Environment.NewLine + ex);

                // Bail out before the user saves the incompletely loaded settings
                // and has their day ruined.
                DialogResult = DialogResult.Abort;
            }
            loadingSettings = false;
        }

        private void Ok_Click(object sender, EventArgs e)
        {
            Close();
        }

        private bool Save()
        {
            foreach (var settingsPage in _settingsPageRegistry.GetSettingsPages())
            {
                settingsPage.SaveSettings();
            }

            SaveScripts();

            if (Settings.RunningOnWindows())
                FormFixHome.CheckHomePath();

            GitCommandHelpers.SetEnvironmentVariable(true);

            Module.SetFilesEncoding(false, ComboToEncoding(Global_FilesEncoding));
            Module.SetFilesEncoding(true, ComboToEncoding(Local_FilesEncoding));

            // Shell Extension settings
            Settings.ShellCascadeContextMenu = chkCascadedContextMenu.Checked;

            String l_ShellVisibleMenuItems = "";

            for (int i = 0; i < chlMenuEntries.Items.Count; i++)
            {
                if (chlMenuEntries.GetItemChecked(i))
                {
                    l_ShellVisibleMenuItems += "1";
                }
                else
                {
                    l_ShellVisibleMenuItems += "0";
                }
            }

            Settings.ShellVisibleMenuItems = l_ShellVisibleMenuItems;

            EnableSettings();

            if (!_checkSettingsLogic.CanFindGitCmd())
            {
                if (MessageBox.Show(this, _cantFindGitMessage.Text, _cantFindGitMessageCaption.Text,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;
            }
            else
            {
                handleCanFindGitCommand();
            }

            Settings.SaveSettings();

            return true;
        }

        private void handleCanFindGitCommand()
        {
            ConfigFile localConfig = Module.GetLocalConfig();
            ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

            if (string.IsNullOrEmpty(UserName.Text) || !UserName.Text.Equals(localConfig.GetValue("user.name")))
                localConfig.SetValue("user.name", UserName.Text);
            if (string.IsNullOrEmpty(UserEmail.Text) || !UserEmail.Text.Equals(localConfig.GetValue("user.email")))
                localConfig.SetValue("user.email", UserEmail.Text);
            localConfig.SetPathValue("core.editor", Editor.Text);
            localConfig.SetValue("merge.tool", LocalMergeTool.Text);


            if (KeepMergeBackup.CheckState == CheckState.Checked)
                localConfig.SetValue("mergetool.keepBackup", "true");
            else if (KeepMergeBackup.CheckState == CheckState.Unchecked)
                localConfig.SetValue("mergetool.keepBackup", "false");

            if (localAutoCrlfFalse.Checked) localConfig.SetValue("core.autocrlf", "false");
            if (localAutoCrlfInput.Checked) localConfig.SetValue("core.autocrlf", "input");
            if (localAutoCrlfTrue.Checked) localConfig.SetValue("core.autocrlf", "true");

            if (string.IsNullOrEmpty(GlobalUserName.Text) ||
                !GlobalUserName.Text.Equals(globalConfig.GetValue("user.name")))
                globalConfig.SetValue("user.name", GlobalUserName.Text);
            if (string.IsNullOrEmpty(GlobalUserEmail.Text) ||
                !GlobalUserEmail.Text.Equals(globalConfig.GetValue("user.email")))
                globalConfig.SetValue("user.email", GlobalUserEmail.Text);
            if (string.IsNullOrEmpty(CommitTemplatePath.Text) ||
                !CommitTemplatePath.Text.Equals(globalConfig.GetValue("commit.template")))
                globalConfig.SetValue("commit.template", CommitTemplatePath.Text);
            globalConfig.SetPathValue("core.editor", GlobalEditor.Text);

            CheckSettingsLogic.SetGlobalDiffToolToConfig(globalConfig, GlobalDiffTool.Text);

            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                globalConfig.SetPathValue(string.Format("difftool.{0}.path", GlobalDiffTool.Text), DifftoolPath.Text);
            if (!string.IsNullOrEmpty(GlobalDiffTool.Text))
                globalConfig.SetPathValue(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text), DifftoolCmd.Text);

            globalConfig.SetValue("merge.tool", GlobalMergeTool.Text);

            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                globalConfig.SetPathValue(string.Format("mergetool.{0}.path", GlobalMergeTool.Text), MergetoolPath.Text);
            if (!string.IsNullOrEmpty(GlobalMergeTool.Text))
                globalConfig.SetPathValue(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text), MergeToolCmd.Text);

            if (GlobalKeepMergeBackup.CheckState == CheckState.Checked)
                globalConfig.SetValue("mergetool.keepBackup", "true");
            else if (GlobalKeepMergeBackup.CheckState == CheckState.Unchecked)
                globalConfig.SetValue("mergetool.keepBackup", "false");

            if (globalAutoCrlfFalse.Checked) globalConfig.SetValue("core.autocrlf", "false");
            if (globalAutoCrlfInput.Checked) globalConfig.SetValue("core.autocrlf", "input");
            if (globalAutoCrlfTrue.Checked) globalConfig.SetValue("core.autocrlf", "true");

            Action<Encoding, bool, string> setEncoding = delegate(Encoding e, bool local, string name)
            {
                string value = e == null ? "" : e.HeaderName;
                if (local)
                    localConfig.SetValue(name, value);
                else
                    globalConfig.SetValue(name, value);
            };
            setEncoding(Module.GetFilesEncoding(false), false, "i18n.filesEncoding");
            setEncoding(Module.GetFilesEncoding(true), true, "i18n.filesEncoding");


            globalConfig.Save();

            //Only save local settings when we are inside a valid working dir
            if (Module.ValidWorkingDir())
                localConfig.Save();
        }

        private void ResolveDiffToolPath()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Module.GetGlobalSetting("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return;

            kdiff3path = MergeToolsHelper.FindFileInFolders("kdiff3.exe", MergetoolPath.Text);
            if (string.IsNullOrEmpty(kdiff3path))
                return;

            DifftoolPath.Text = kdiff3path;
        }

        private void DiffToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            Module.SetGlobalPathSetting(string.Format("difftool.{0}.path", GlobalMergeTool.Text.Trim()), MergetoolPath.Text.Trim());
            string exeName;
            string exeFile;
            if (!String.IsNullOrEmpty(DifftoolPath.Text))
            {
                exeFile = DifftoolPath.Text;
                exeName = Path.GetFileName(exeFile);
            }
            else
                exeFile = MergeToolsHelper.FindDiffToolFullPath(GlobalDiffTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                DifftoolPath.SelectAll();
                DifftoolPath.SelectedText = "";
                DifftoolCmd.SelectAll();
                DifftoolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(CheckSettingsLogic._toolSuggestPath.Text, exeName),
                        __diffToolSuggestCaption.Text);
                return;
            }
            DifftoolPath.SelectAll(); // allow Undo action
            DifftoolPath.SelectedText = exeFile;
            DifftoolCmd.SelectAll();
            DifftoolCmd.SelectedText = MergeToolsHelper.DiffToolCmdSuggest(GlobalDiffTool.Text, exeFile);
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            string mergeTool = GlobalMergeTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetMergeToolExeFile(mergeTool);

            if (exeFile != null)
                MergetoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalMergeTool.Text, exeFile), MergetoolPath.Text);
            else
                MergetoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalMergeTool.Text), MergetoolPath.Text);
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            string diffTool = GlobalDiffTool.Text.Trim();
            DifftoolPath.Text = Module.GetGlobalSetting(string.Format("difftool.{0}.path", diffTool));
            DifftoolCmd.Text = Module.GetGlobalSetting(string.Format("difftool.{0}.cmd", diffTool));

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                ResolveDiffToolPath();

            DiffToolCmdSuggest_Click(null, null);
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            string diffTool = GlobalDiffTool.Text.ToLowerInvariant();
            string exeFile = MergeToolsHelper.GetDiffToolExeFile(diffTool);

            if (exeFile != null)
                DifftoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} ({1})|{1}", GlobalDiffTool.Text, exeFile), DifftoolPath.Text);
            else
                DifftoolPath.Text = CommonLogic.SelectFile(".", string.Format("{0} (*.exe)|*.exe", GlobalDiffTool.Text), DifftoolPath.Text);
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            if (DesignMode)
                return;

            EnableSettings();

            WindowState = FormWindowState.Normal;
        }

        private void EnableSettings()
        {
            bool canFindGitCmd = _checkSettingsLogic.CanFindGitCmd();
            GlobalUserName.Enabled = canFindGitCmd;
            GlobalUserEmail.Enabled = canFindGitCmd;
            GlobalEditor.Enabled = canFindGitCmd;
            CommitTemplatePath.Enabled = canFindGitCmd;
            GlobalMergeTool.Enabled = canFindGitCmd;
            MergetoolPath.Enabled = canFindGitCmd;
            MergeToolCmd.Enabled = canFindGitCmd;
            GlobalKeepMergeBackup.Enabled = canFindGitCmd;

            InvalidGitPathGlobal.Visible = !canFindGitCmd;
            InvalidGitPathLocal.Visible = !canFindGitCmd;

            bool valid = Module.ValidWorkingDir() && canFindGitCmd;
            UserName.Enabled = valid;
            UserEmail.Enabled = valid;
            Editor.Enabled = valid;
            LocalMergeTool.Enabled = valid;
            KeepMergeBackup.Enabled = valid;
            localAutoCrlfFalse.Enabled = valid;
            localAutoCrlfInput.Enabled = valid;
            localAutoCrlfTrue.Enabled = valid;
            NoGitRepo.Visible = !valid;
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            LoadSettings();
            _checklistSettingsPage.CheckSettings();
            Cursor.Current = Cursors.Default;
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            WindowState = FormWindowState.Normal;
            LoadSettings();
            _checklistSettingsPage.CheckSettings();
            WindowState = FormWindowState.Normal;
            Cursor.Current = Cursors.Default;
        }

        private void FormSettings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            if (DialogResult != DialogResult.Abort && !Save())
                e.Cancel = true;
            Cursor.Current = Cursors.Default;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tc = (TabControl)sender;
            if (tc.SelectedTab == tpScriptsTab)
                populateSplitbutton();
            else if (tc.SelectedTab == tpHotkeys)
                controlHotkeys.ReloadSettings();

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;
        }

        private void populateSplitbutton()
        {

            System.Resources.ResourceManager rm =
                new System.Resources.ResourceManager("GitUI.Properties.Resources",
                            System.Reflection.Assembly.GetExecutingAssembly());

            // dummy request; for some strange reason the ResourceSets are not loaded untill after the first object request... bug?
            rm.GetObject("dummy");

            System.Resources.ResourceSet resourceSet = rm.GetResourceSet(System.Globalization.CultureInfo.CurrentUICulture, true, true);

            contextMenuStrip_SplitButton.Items.Clear();

            foreach (System.Collections.DictionaryEntry icon in resourceSet)
            {
                //add entry to toolstrip
                if (icon.Value.GetType() == typeof(Icon))
                {
                    //contextMenuStrip_SplitButton.Items.Add(icon.Key.ToString(), (Image)((Icon)icon.Value).ToBitmap(), SplitButtonMenuItem_Click);
                }
                else if (icon.Value.GetType() == typeof(Bitmap))
                {
                    contextMenuStrip_SplitButton.Items.Add(icon.Key.ToString(), (Image)icon.Value, SplitButtonMenuItem_Click);
                }
                //var aa = icon.Value.GetType();
            }

            resourceSet.Close();
            rm.ReleaseAllResources();

        }

        public Bitmap ResizeBitmap(Bitmap b, int nWidth, int nHeight)
        {
            Bitmap result = new Bitmap(nWidth, nHeight);
            using (Graphics g = Graphics.FromImage((Image)result))
                g.DrawImage(b, 0, 0, nWidth, nHeight);
            return result;
        }

        private void ClearImageCache_Click(object sender, EventArgs e)
        {
            GravatarService.ClearImageCache();
        }

        private void helpTranslate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var frm = new FormTranslate()) frm.ShowDialog(this);
        }

        private void SaveScripts()
        {
            Settings.ownScripts = ScriptManager.SerializeIntoXml();
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
                return;

            ScriptInfo scriptInfo = ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo;

            nameTextBox.Text = scriptInfo.Name;
            commandTextBox.Text = scriptInfo.Command;
            argumentsTextBox.Text = scriptInfo.Arguments;
            inMenuCheckBox.Checked = scriptInfo.AddToRevisionGridContextMenu;
            scriptEnabled.Checked = scriptInfo.Enabled;
            scriptNeedsConfirmation.Checked = scriptInfo.AskConfirmation;
            scriptEvent.SelectedItem = scriptInfo.OnEvent;
            sbtn_icon.Image = scriptInfo.GetIcon();
            IconName = scriptInfo.Icon;

            foreach (ToolStripItem item in contextMenuStrip_SplitButton.Items)
            {
                if (item.ToString() == IconName)
                {
                    item.Font = new Font(item.Font, FontStyle.Bold);
                }
            }
        }

        private void addScriptButton_Click(object sender, EventArgs e)
        {
            ScriptList.ClearSelection();
            ScriptManager.GetScripts().AddNew();
            ScriptList.Rows[ScriptList.RowCount - 1].Selected = true;
            ScriptList_SelectionChanged(null, null); //needed for linux
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
                ScriptInfo selectedScriptInfo = ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo;
                selectedScriptInfo.HotkeyCommandIdentifier = ScriptList.SelectedRows[0].Index + 9000;
                selectedScriptInfo.Name = nameTextBox.Text;
                selectedScriptInfo.Command = commandTextBox.Text;
                selectedScriptInfo.Arguments = argumentsTextBox.Text;
                selectedScriptInfo.AddToRevisionGridContextMenu = inMenuCheckBox.Checked;
                selectedScriptInfo.Enabled = scriptEnabled.Checked;
                selectedScriptInfo.AskConfirmation = scriptNeedsConfirmation.Checked;
                selectedScriptInfo.OnEvent = (ScriptEvent)scriptEvent.SelectedItem;
                selectedScriptInfo.Icon = IconName;
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
                    commandTextBox.Text = ofd.FileName;
            }
        }

        private void argumentsTextBox_Enter(object sender, EventArgs e)
        {
            helpLabel.Visible = true;
        }

        private void argumentsTextBox_Leave(object sender, EventArgs e)
        {
            helpLabel.Visible = false;
        }

        private void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/gitextensions/gitextensions/wiki/Spelling");
        }

        private void ScriptList_SelectionChanged(object sender, EventArgs e)
        {
            if (ScriptList.SelectedRows.Count > 0)
            {
                RefreshScriptDetails();

                removeScriptButton.Enabled = true;
                moveDownButton.Enabled = moveUpButton.Enabled = false;
                if (ScriptList.SelectedRows[0].Index > 0)
                    moveUpButton.Enabled = true;
                if (ScriptList.SelectedRows[0].Index < ScriptList.RowCount - 1)
                    moveDownButton.Enabled = true;
            }
            else
            {
                removeScriptButton.Enabled = false;
                moveUpButton.Enabled = false;
                moveDownButton.Enabled = false;
                ClearScriptDetails();
            }
        }

        private void ScriptInfoEdit_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ScriptInfoFromEdits();
            ScriptList.Refresh();
        }

        private void ScriptList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ScriptList_SelectionChanged(null, null);//needed for linux
        }

        #region Hotkey commands

        public const string HotkeySettingsName = "Scripts";

        internal enum Commands
        {
            NothingYet
        }

        #endregion

        private void BrowseCommitTemplate_Click(object sender, EventArgs e)
        {
            CommitTemplatePath.Text = CommonLogic.SelectFile(".", "*.txt (*.txt)|*.txt", CommitTemplatePath.Text);
        }

        private void SplitButtonMenuItem_Click(object sender, EventArgs e)
        {
            //reset bold item to regular
            var item = contextMenuStrip_SplitButton.Items.OfType<ToolStripMenuItem>().FirstOrDefault(s => s.Font.Bold);
            if (item != null)
                item.Font = new Font(contextMenuStrip_SplitButton.Font, FontStyle.Regular);

            //make new item bold
            ((ToolStripMenuItem)sender).Font = new Font(((ToolStripMenuItem)sender).Font, FontStyle.Bold);

            //set new image on button
            sbtn_icon.Image = ResizeBitmap((Bitmap)((ToolStripMenuItem)sender).Image, 12, 12);

            IconName = ((ToolStripMenuItem)sender).Text;

            //store variables
            ScriptInfoEdit_Validating(sender, new System.ComponentModel.CancelEventArgs());
        }

        private void scriptEvent_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (scriptEvent.Text == ScriptEvent.ShowInUserMenuBar.ToString())
            {
                /*
                string icon_name = IconName;
                if (ScriptList.RowCount > 0)
                {
                    ScriptInfo scriptInfo = ScriptList.SelectedRows[0].DataBoundItem as ScriptInfo;
                    icon_name = scriptInfo.Icon;
                }*/

                sbtn_icon.Visible = true;
                lbl_icon.Visible = true;
            }
            else
            {
                //not a menubar item, so hide the text label and dropdown button
                sbtn_icon.Visible = false;
                lbl_icon.Visible = false;
            }
        }
    }
}
