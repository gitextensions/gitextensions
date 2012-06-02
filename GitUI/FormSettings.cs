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

namespace GitUI
{
    public sealed partial class FormSettings : GitExtensionsForm
    {
        #region Translation
        private readonly TranslationString _homeIsSetToString = new TranslationString("HOME is set to:");
        private readonly TranslationString __diffToolSuggestCaption = new TranslationString("Suggest difftool cmd");
        private readonly TranslationString __mergeToolSuggestCaption = new TranslationString("Suggest mergetool cmd");

        private readonly TranslationString _loadingSettingsFailed =
            new TranslationString("Could not load settings.");

        private readonly TranslationString _cantFindGitMessage =
            new TranslationString("The command to run git is not configured correct." + Environment.NewLine +
                "You need to set the correct path to be able to use GitExtensions." + Environment.NewLine +
                Environment.NewLine + "Do you want to set the correct command now?");

        private readonly TranslationString _cantFindGitMessageCaption =
            new TranslationString("Incorrect path");

        private static readonly TranslationString _cantReadRegistry =
            new TranslationString("GitExtensions has insufficient permissions to check the registry.");

        private static readonly TranslationString _cantReadRegistryAddEntryManually =
            new TranslationString("GitExtensions has insufficient permissions to modify the registry." +
                                Environment.NewLine + "Please add this key to the registry manually." +
                                Environment.NewLine + "Path:  {0}\\{1}" + Environment.NewLine +
                                "Value:  {2} = {3}");

        private readonly TranslationString _cantRegisterShellExtension =
            new TranslationString("Could not register the shell extension because '{0}' could not be found.");

        private readonly TranslationString _noDiffToolConfigured =
            new TranslationString("There is no difftool configured. Do you want to configure kdiff3 as your difftool?" +
                Environment.NewLine + "Select no if you want to configure a different difftool yourself.");

        private readonly TranslationString _noDiffToolConfiguredCaption =
            new TranslationString("Difftool");

        private readonly TranslationString _kdiff3NotFoundAuto =
            new TranslationString("Path to kdiff3 could not be found automatically." + Environment.NewLine +
                "Please make sure KDiff3 is installed or set path manually.");

        private readonly TranslationString _noMergeToolConfigured =
            new TranslationString("There is no mergetool configured. Do you want to configure kdiff3 as your mergetool?" +
                Environment.NewLine + "Select no if you want to configure a different mergetool yourself.");

        private readonly TranslationString _noMergeToolConfiguredCaption =
            new TranslationString("Mergetool");

        private readonly TranslationString _solveGitCommandFailed =
            new TranslationString("The command to run git could not be determined automatically." + Environment.NewLine +
                "Please make sure git (msysgit or cygwin) is installed or set the correct command manually.");

        private readonly TranslationString _solveGitCommandFailedCaption =
            new TranslationString("Locate git");

        private readonly TranslationString _gitCanBeRun =
            new TranslationString("Git can be run using: {0}");

        private readonly TranslationString _gitCanBeRunCaption =
            new TranslationString("Locate git");

        private readonly TranslationString _linuxToolsShNotFound =
            new TranslationString("The path to linux tools (sh) could not be found automatically." + Environment.NewLine +
                "Please make sure there are linux tools installed (through msysgit or cygwin) or set the correct path manually.");

        private readonly TranslationString _linuxToolsShNotFoundCaption =
            new TranslationString("Locate linux tools");

        private readonly TranslationString _shCanBeRun =
            new TranslationString("Command sh can be run using: {0}sh");

        private readonly TranslationString _shCanBeRunCaption =
            new TranslationString("Locate linux tools");

        private static readonly TranslationString _selectFile =
            new TranslationString("Select file");

        private readonly TranslationString _puttyFoundAuto =
            new TranslationString("All paths needed for PuTTY could be automatically found and are set.");

        private readonly TranslationString _puttyFoundAutoCaption =
            new TranslationString("PuTTY");

        private readonly TranslationString _noDictFilesFound =
            new TranslationString("No dictionary files found in: {0}");

        private readonly TranslationString _noLanguageConfigured =
            new TranslationString("There is no language configured for Git Extensions.");

        private readonly TranslationString _languageConfigured =
            new TranslationString("The configured language is {0}.");

        private readonly TranslationString _plinkputtyGenpageantNotFound =
            new TranslationString("PuTTY is configured as SSH client but cannot find plink.exe, puttygen.exe or pageant.exe.");

        private readonly TranslationString _puttyConfigured =
            new TranslationString("SSH client PuTTY is configured properly.");

        private readonly TranslationString _opensshUsed =
            new TranslationString("Default SSH client, OpenSSH, will be used. (commandline window will appear on pull, push and clone operations)");

        private readonly TranslationString _unknownSshClient =
            new TranslationString("Unknown SSH client configured: {0}.");

        private readonly TranslationString _linuxToolsSshNotFound =
            new TranslationString("Linux tools (sh) not found. To solve this problem you can set the correct path in settings.");

        private readonly TranslationString _linuxToolsSshFound =
            new TranslationString("Linux tools (sh) found on your computer.");

        private readonly TranslationString _gitNotFound =
            new TranslationString("Git not found. To solve this problem you can set the correct path in settings.");

        private readonly TranslationString _wrongGitVersion =
            new TranslationString("Git found but version {0} is not supported. Upgrage to version {1} or later");

        private readonly TranslationString _gitVersionFound =
            new TranslationString("Git {0} is found on your computer.");

        private readonly TranslationString _adviceDiffToolConfiguration =
            new TranslationString("You should configure a diff tool to show file diff in external program (kdiff3 for example).");

        private readonly TranslationString _kdiffAsDiffConfiguredButNotFound =
            new TranslationString("KDiff3 is configured as difftool, but the path to kdiff.exe is not configured.");

        private readonly TranslationString _kdiffAsDiffConfigured =
            new TranslationString("KDiff3 is configured as difftool.");

        private readonly TranslationString _toolSuggestPath =
            new TranslationString("Please enter the path to {0} and press suggest.");

        private readonly TranslationString _diffToolXConfigured =
            new TranslationString("There is a difftool configured: {0}");

        private readonly TranslationString _configureMergeTool =
            new TranslationString("You need to configure merge tool in order to solve mergeconflicts (kdiff3 for example).");

        private readonly TranslationString _kdiffAsMergeConfiguredButNotFound =
            new TranslationString("KDiff3 is configured as mergetool, but the path to kdiff.exe is not configured.");

        private readonly TranslationString _kdiffAsMergeConfigured =
            new TranslationString("KDiff3 is configured as mergetool.");

        private readonly TranslationString _mergeToolXConfiguredNeedsCmd =
            new TranslationString("{0} is configured as mergetool, this is a custom mergetool and needs a custom cmd to be configured.");

        private readonly TranslationString _customMergeToolXConfigured =
            new TranslationString("There is a custom mergetool configured: {0}");

        private readonly TranslationString _mergeToolXConfigured =
            new TranslationString("There is a custom mergetool configured.");

        private readonly TranslationString _noEmailSet =
            new TranslationString("You need to configure a username and an email address.");

        private readonly TranslationString _emailSet =
            new TranslationString("A username and an email address are configured.");

        private readonly TranslationString _shellExtNoInstalled =
            new TranslationString("Shell extensions are not installed. Run the installer to install the shell extensions.");

        private readonly TranslationString _shellExtNeedsToBeRegistered =
            new TranslationString("{0} needs to be registered in order to use the shell extensions.");

        private readonly TranslationString _shellExtRegistered =
            new TranslationString("Shell extensions registered properly.");

        private readonly TranslationString _registryKeyGitExtensionsMissing =
            new TranslationString("Registry entry missing [Software\\GitExtensions\\GitExtensions\\InstallDir].");

        private readonly TranslationString _registryKeyGitExtensionsFaulty =
            new TranslationString("Invalid installation directory stored in [Software\\GitExtensions\\GitExtensions\\InstallDir].");

        private readonly TranslationString _registryKeyGitExtensionsCorrect =
            new TranslationString("GitExtensions is properly registered.");
        #endregion
        
        private Font diffFont;
        private const string GitExtensionsShellExName = "GitExtensionsShellEx32.dll";
        private string IconName = "bug";

        public FormSettings()
        {
            InitializeComponent();
            Translate();

            noImageService.Items.AddRange(GravatarService.DynamicServices.Cast<object>().ToArray());

            FillEncodings(Global_FilesEncoding);
            FillEncodings(Global_AppEncoding);
            FillEncodings(Local_FilesEncoding);
            FillEncodings(Local_AppEncoding);
            
            GlobalEditor.Items.AddRange(new Object[] { "\"" + Settings.GetGitExtensionsFullPath() + "\" fileeditor", "vi", "notepad", "notepad++" });

            SetCurrentDiffFont(Settings.DiffFont);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            SavePosition("settings");
        }

        public static bool AutoSolveAllSettings()
        {
            if (!Settings.RunningOnWindows())
                return SolveGitCommand();

            return SolveGitCommand() &&
                   SolveLinuxToolsDir() &&
                   SolveMergeToolForKDiff() &&
                   SolveDiffToolForKDiff() &&
                   SolveGitExtensionsDir() &&
                   SolveEditor();
        }

        private static bool SolveEditor()
        {
            string editor = Settings.Module.GetGlobalPathSetting("core.editor");
            if (string.IsNullOrEmpty(editor))
            {
                Settings.Module.SetGlobalPathSetting("core.editor", "\"" + Settings.GetGitExtensionsFullPath() + "\" fileeditor");
            }

            return true;
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

        private static string GetGlobalDiffToolFromConfig()
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
                return GitCommandHelpers.GetGlobalConfig().GetValue("diff.guitool");
            return GitCommandHelpers.GetGlobalConfig().GetValue("diff.tool");
        }

        private static void SetGlobalDiffToolToConfig(ConfigFile configFile, string diffTool)
        {
            if (GitCommandHelpers.VersionInUse.GuiDiffToolExist)
            {
                configFile.SetValue("diff.guitool", diffTool);
                return;
            }
            configFile.SetValue("diff.tool", diffTool);
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
                GitCommandHelpers.SetEnvironmentVariable();
                homeIsSetToLabel.Text = string.Concat(_homeIsSetToString.Text, " ", GitCommandHelpers.GetHomeDir());

                scriptEvent.DataSource = Enum.GetValues(typeof(ScriptEvent));
                EncodingToCombo(Settings.GetFilesEncoding(false), Global_FilesEncoding);
                EncodingToCombo(Settings.GetAppEncoding(false, false), Global_AppEncoding);
                EncodingToCombo(Settings.GetFilesEncoding(true), Local_FilesEncoding);
                EncodingToCombo(Settings.GetAppEncoding(true, false), Local_AppEncoding);

                chkWarnBeforeCheckout.Checked = Settings.DirtyDirWarnBeforeCheckoutBranch;
                chkStartWithRecentWorkingDir.Checked = Settings.StartWithRecentWorkingDir;

                chkUsePatienceDiffAlgorithm.Checked = Settings.UsePatienceDiffAlgorithm;

                chkShowCurrentBranchInVisualStudio.Checked = Settings.ShowCurrentBranchInVisualStudio;

                RevisionGridQuickSearchTimeout.Value = Settings.RevisionGridQuickSearchTimeout;

                chkFollowRenamesInFileHistory.Checked = Settings.FollowRenamesInFileHistory;

                _NO_TRANSLATE_DaysToCacheImages.Value = Settings.AuthorImageCacheDays;

                _NO_TRANSLATE_authorImageSize.Value = Settings.AuthorImageSize;
                ShowAuthorGravatar.Checked = Settings.ShowAuthorGravatar;
                noImageService.Text = Settings.GravatarFallbackService;

                chkShowErrorsWhenStagingFiles.Checked = Settings.ShowErrorsWhenStagingFiles;
                chkStashUntrackedFiles.Checked = Settings.IncludeUntrackedFilesInAutoStash;

                Language.Items.Clear();
                Language.Items.Add("English");
                Language.Items.AddRange(Translator.GetAllTranslations());
                Language.Text = Settings.Translation;

                MulticolorBranches.Checked = Settings.MulticolorBranches;
                MulticolorBranches_CheckedChanged(null, null);
                DrawNonRelativesGray.Checked = Settings.RevisionGraphDrawNonRelativesGray;
                DrawNonRelativesTextGray.Checked = Settings.RevisionGraphDrawNonRelativesTextGray;
                chkShowCurrentChangesInRevisionGraph.Checked = Settings.RevisionGraphShowWorkingDirChanges;
                chkShowStashCountInBrowseWindow.Checked = Settings.ShowStashCount;
                BranchBorders.Checked = Settings.BranchBorders;
                StripedBanchChange.Checked = Settings.StripedBranchChange;

                chkShowGitStatusInToolbar.Checked = Settings.ShowGitStatusInBrowseToolbar;

                _NO_TRANSLATE_truncatePathMethod.Text = Settings.TruncatePathMethod;
                _NO_TRANSLATE_ColorGraphLabel.BackColor = Settings.GraphColor;
                _NO_TRANSLATE_ColorGraphLabel.Text = Settings.GraphColor.Name;
                _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorGraphLabel.BackColor);
                _NO_TRANSLATE_ColorTagLabel.BackColor = Settings.TagColor;
                _NO_TRANSLATE_ColorTagLabel.Text = Settings.TagColor.Name;
                _NO_TRANSLATE_ColorTagLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
                _NO_TRANSLATE_ColorBranchLabel.BackColor = Settings.BranchColor;
                _NO_TRANSLATE_ColorBranchLabel.Text = Settings.BranchColor.Name;
                _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
                _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = Settings.RemoteBranchColor;
                _NO_TRANSLATE_ColorRemoteBranchLabel.Text = Settings.RemoteBranchColor.Name;
                _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
                _NO_TRANSLATE_ColorOtherLabel.BackColor = Settings.OtherTagColor;
                _NO_TRANSLATE_ColorOtherLabel.Text = Settings.OtherTagColor.Name;
                _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);


                _NO_TRANSLATE_ColorAddedLineLabel.BackColor = Settings.DiffAddedColor;
                _NO_TRANSLATE_ColorAddedLineLabel.Text = Settings.DiffAddedColor.Name;
                _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
                _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = Settings.DiffAddedExtraColor;
                _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = Settings.DiffAddedExtraColor.Name;
                _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);

                _NO_TRANSLATE_ColorRemovedLine.BackColor = Settings.DiffRemovedColor;
                _NO_TRANSLATE_ColorRemovedLine.Text = Settings.DiffRemovedColor.Name;
                _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = Settings.DiffRemovedExtraColor;
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = Settings.DiffRemovedExtraColor.Name;
                _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
                _NO_TRANSLATE_ColorSectionLabel.BackColor = Settings.DiffSectionColor;
                _NO_TRANSLATE_ColorSectionLabel.Text = Settings.DiffSectionColor.Name;
                _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                    ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);

                SmtpServer.Text = Settings.Smtp;

                _NO_TRANSLATE_MaxCommits.Value = Settings.MaxRevisionGraphCommits;

                GitPath.Text = Settings.GitCommand;
                GitBinPath.Text = Settings.GitBinDir;

                ConfigFile localConfig = Settings.Module.GetLocalConfig();
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();

                UserName.Text = localConfig.GetValue("user.name");
                UserEmail.Text = localConfig.GetValue("user.email");
                Editor.Text = localConfig.GetPathValue("core.editor");
                LocalMergeTool.Text = localConfig.GetValue("merge.tool");

                Dictionary.Text = Settings.Dictionary;

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

                string iconColor = Settings.IconColor.ToLower();
                DefaultIcon.Checked = iconColor == "default";
                BlueIcon.Checked    = iconColor == "blue";
                GreenIcon.Checked   = iconColor == "green";
                PurpleIcon.Checked  = iconColor == "purple";
                RedIcon.Checked     = iconColor == "red";
                YellowIcon.Checked  = iconColor == "yellow";
                RandomIcon.Checked  = iconColor == "random";

                IconStyle.Text = Settings.IconStyle;

                GlobalDiffTool.Text = GetGlobalDiffToolFromConfig();

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

                PlinkPath.Text = Settings.Plink;
                PuttygenPath.Text = Settings.Puttygen;
                PageantPath.Text = Settings.Pageant;
                AutostartPageant.Checked = Settings.AutoStartPageant;

                chkCloseProcessDialog.Checked = Settings.CloseProcessDialog;
                chkShowGitCommandLine.Checked = Settings.ShowGitCommandLine;

                chkUseFastChecks.Checked = Settings.UseFastChecks;
                chkShowRelativeDate.Checked = Settings.RelativeDate;

                if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                    OpenSSH.Checked = true;
                else if (GitCommandHelpers.Plink())
                    Putty.Checked = true;
                else
                {
                    OtherSsh.Text = GitCommandHelpers.GetSsh();
                    Other.Checked = true;
                }

                EnableSshOptions();
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
            SaveScripts();

            if (Settings.RunningOnWindows())
                FormFixHome.CheckHomePath();

            GitCommandHelpers.SetEnvironmentVariable(true);

            Settings.DirtyDirWarnBeforeCheckoutBranch = chkWarnBeforeCheckout.Checked;
            Settings.StartWithRecentWorkingDir = chkStartWithRecentWorkingDir.Checked;

            Settings.UsePatienceDiffAlgorithm = chkUsePatienceDiffAlgorithm.Checked;

            Settings.TruncatePathMethod = _NO_TRANSLATE_truncatePathMethod.Text;

            Settings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.Checked;

            Settings.ShowErrorsWhenStagingFiles = chkShowErrorsWhenStagingFiles.Checked;
            Settings.IncludeUntrackedFilesInAutoStash = chkStashUntrackedFiles.Checked;

            Settings.FollowRenamesInFileHistory = chkFollowRenamesInFileHistory.Checked;

            if ((int)_NO_TRANSLATE_authorImageSize.Value != Settings.AuthorImageSize)
            {
                Settings.AuthorImageSize = (int)_NO_TRANSLATE_authorImageSize.Value;
                GravatarService.ClearImageCache();
            }

            Settings.Translation = Language.Text;
            Strings.Reinit();

            Settings.ShowGitStatusInBrowseToolbar = chkShowGitStatusInToolbar.Checked;

            Settings.AuthorImageCacheDays = (int)_NO_TRANSLATE_DaysToCacheImages.Value;

            Settings.Smtp = SmtpServer.Text;

            Settings.GitCommand = GitPath.Text;
            Settings.GitBinDir = GitBinPath.Text;

            Settings.ShowAuthorGravatar = ShowAuthorGravatar.Checked;
            Settings.GravatarFallbackService = noImageService.Text;

            Settings.CloseProcessDialog = chkCloseProcessDialog.Checked;
            Settings.ShowGitCommandLine = chkShowGitCommandLine.Checked;

            Settings.UseFastChecks = chkUseFastChecks.Checked;
            Settings.RelativeDate = chkShowRelativeDate.Checked;

            Settings.Dictionary = Dictionary.Text;

            Settings.MaxRevisionGraphCommits = (int)_NO_TRANSLATE_MaxCommits.Value;

            Settings.Plink = PlinkPath.Text;
            Settings.Puttygen = PuttygenPath.Text;
            Settings.Pageant = PageantPath.Text;
            Settings.AutoStartPageant = AutostartPageant.Checked;
            Settings.SetFilesEncoding(false, ComboToEncoding(Global_FilesEncoding));
            Settings.SetAppEncoding(false, ComboToEncoding(Global_AppEncoding));
            Settings.SetFilesEncoding(true, ComboToEncoding(Local_FilesEncoding));
            Settings.SetAppEncoding(true, ComboToEncoding(Local_AppEncoding));
            Settings.RevisionGridQuickSearchTimeout = (int)RevisionGridQuickSearchTimeout.Value;
            Settings.MulticolorBranches = MulticolorBranches.Checked;
            Settings.RevisionGraphDrawNonRelativesGray = DrawNonRelativesGray.Checked;
            Settings.RevisionGraphDrawNonRelativesTextGray = DrawNonRelativesTextGray.Checked;
            Settings.RevisionGraphShowWorkingDirChanges = chkShowCurrentChangesInRevisionGraph.Checked;
            Settings.ShowStashCount = chkShowStashCountInBrowseWindow.Checked;
            Settings.BranchBorders = BranchBorders.Checked;
            Settings.StripedBranchChange = StripedBanchChange.Checked;
            Settings.GraphColor = _NO_TRANSLATE_ColorGraphLabel.BackColor;
            Settings.TagColor = _NO_TRANSLATE_ColorTagLabel.BackColor;
            Settings.BranchColor = _NO_TRANSLATE_ColorBranchLabel.BackColor;
            Settings.RemoteBranchColor = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor;
            Settings.OtherTagColor = _NO_TRANSLATE_ColorOtherLabel.BackColor;

            Settings.DiffAddedColor = _NO_TRANSLATE_ColorAddedLineLabel.BackColor;
            Settings.DiffRemovedColor = _NO_TRANSLATE_ColorRemovedLine.BackColor;
            Settings.DiffAddedExtraColor = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor;
            Settings.DiffRemovedExtraColor = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor;
            Settings.DiffFont = diffFont;

            Settings.DiffSectionColor = _NO_TRANSLATE_ColorSectionLabel.BackColor;

            Settings.IconColor = GetSelectedApplicationIconColor();

            Settings.IconStyle = IconStyle.Text;

            EnableSettings();

            if (!CanFindGitCmd())
            {
                if (MessageBox.Show(this, _cantFindGitMessage.Text, _cantFindGitMessageCaption.Text,
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return false;
            }
            else
            {
                handleCanFindGitCommand();
            }

            if (OpenSSH.Checked)
                GitCommandHelpers.UnsetSsh();

            if (Putty.Checked)
                GitCommandHelpers.SetSsh(PlinkPath.Text);

            if (Other.Checked)
                GitCommandHelpers.SetSsh(OtherSsh.Text);

            Settings.SaveSettings();

            return true;
        }

        private string GetSelectedApplicationIconColor()
        {
            if (BlueIcon.Checked)
                return "blue";
            if (LightblueIcon.Checked)
                return "lightblue";
            if (GreenIcon.Checked)
                return "green";
            if (PurpleIcon.Checked)
                return "purple";
            if (RedIcon.Checked)
                return "red";
            if (YellowIcon.Checked)
                return "yellow";
            if (RandomIcon.Checked)
                return "random";
            return "default";
        }

        private void handleCanFindGitCommand()
        {
            ConfigFile localConfig = Settings.Module.GetLocalConfig();
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

            SetGlobalDiffToolToConfig(globalConfig, GlobalDiffTool.Text);

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
            setEncoding(Settings.GetLogOutputEncoding(false), false, "i18n.logoutputencoding");
            setEncoding(Settings.GetLogOutputEncoding(true), true, "i18n.logoutputencoding");
            setEncoding(Settings.GetCommitEncoding(false), false, "i18n.commitEncoding");
            setEncoding(Settings.GetCommitEncoding(true), true, "i18n.commitEncoding");
            setEncoding(Settings.GetFilesEncoding(false), false, "i18n.filesEncoding");
            setEncoding(Settings.GetFilesEncoding(true), true, "i18n.filesEncoding");            


            globalConfig.Save();

            //Only save local settings when we are inside a valid working dir
            if (Settings.Module.ValidWorkingDir())
                localConfig.Save();
        }

        private static string GetRegistryValue(RegistryKey root, string subkey, string key)
        {
            string value = null;
            try
            {
                RegistryKey registryKey = root.OpenSubKey(subkey, false);
                if (registryKey != null)
                {
                    using (registryKey)
                    {
                        value = registryKey.GetValue(key) as string;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(_cantReadRegistry.Text);
            }
            return value ?? string.Empty;
        }

        private static void SetRegistryValue(RegistryKey root, string subkey, string key, string value)
        {
            try
            {
                value = value.Replace("\\", "\\\\");
                string reg = "Windows Registry Editor Version 5.00" + Environment.NewLine + Environment.NewLine + "[" + root +
                             "\\" + subkey + "]" + Environment.NewLine + "\"" + key + "\"=\"" + value + "\"";

                TextWriter tw = new StreamWriter(Path.GetTempPath() + "GitExtensions.reg", false);
                tw.Write(reg);
                tw.Close();
                Settings.Module.RunCmd("regedit", "\"" + Path.GetTempPath() + "GitExtensions.reg" + "\"");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(String.Format(_cantReadRegistryAddEntryManually.Text, root, subkey, key, value));
            }
        }

        public bool CheckSettings()
        {
            bool bValid = true;
            try
            {
                // once a check fails, we want bValid to stay false
                bValid = CheckGitCmdValid();
                bValid = CheckGlobalUserSettingsValid() && bValid;
                bValid = CheckMergeTool() && bValid;
                bValid = CheckDiffToolConfiguration() && bValid;
                bValid = CheckTranslationConfigSettings() && bValid;

                if (Settings.RunningOnWindows())
                {
                    bValid = CheckGitExtensionsInstall() && bValid;
                    bValid = CheckGitExtensionRegistrySettings() && bValid;
                    bValid = CheckGitExe() && bValid;
                    bValid = CheckSSHSettings() && bValid;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

            CheckAtStartup.Checked = getCheckAtStartupChecked(bValid);
            return bValid;
        }

        private static bool CanFindGitCmd()
        {
            return !string.IsNullOrEmpty(Settings.Module.RunGitCmd(""));
        }

        private void GitExtensionsInstall_Click(object sender, EventArgs e)
        {
            SolveGitExtensionsDir();

            CheckSettings();
        }

        public static bool SolveGitExtensionsDir()
        {
            string fileName = Settings.GetGitExtensionsDirectory();

            if (Directory.Exists(fileName))
            {
                Settings.SetInstallDir(fileName);
                return true;
            }

            return false;
        }

        private void ShellExtensionsRegistered_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(Settings.GetInstallDir(), GitExtensionsShellExName);
            if (!File.Exists(path))
            {
                path = Assembly.GetAssembly(GetType()).Location;
                path = Path.GetDirectoryName(path);
                path = Path.Combine(path, GitExtensionsShellExName);
            }
            if (File.Exists(path))
            {
                Settings.Module.RunCmd("regsvr32", string.Format("\"{0}\"", path));
            }
            else
            {
                MessageBox.Show(this, string.Format(_cantRegisterShellExtension.Text, GitExtensionsShellExName));
            }

            CheckSettings();
        }

        private void UserNameSet_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab("tpGlobalSettings");
        }

        private static string GetMergeTool()
        {
            return Settings.Module.GetGlobalSetting("merge.tool");
        }

        private static bool IsMergeTool(string toolName)
        {
            return GetMergeTool().Equals(toolName,
                StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool SolveMergeToolForKDiff()
        {
            string mergeTool = GetMergeTool();
            if (string.IsNullOrEmpty(mergeTool))
            {
                mergeTool = "kdiff3";
                Settings.Module.SetGlobalSetting("merge.tool", mergeTool);
            }

            if (mergeTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveMergeToolPathForKDiff();

            return true;
        }

        public static bool SolveDiffToolForKDiff()
        {
            string diffTool = GetGlobalDiffToolFromConfig();
            if (string.IsNullOrEmpty(diffTool))
            {
                diffTool = "kdiff3";
                ConfigFile globalConfig = GitCommandHelpers.GetGlobalConfig();
                SetGlobalDiffToolToConfig(globalConfig, diffTool);
                globalConfig.Save();
            }

            if (diffTool.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                return SolveDiffToolPathForKDiff();

            return true;
        }

        public static bool SolveDiffToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Settings.Module.GetGlobalSetting("difftool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            Settings.Module.SetGlobalPathSetting("difftool.kdiff3.path", kdiff3path);
            return true;
        }

        public static bool SolveMergeToolPathForKDiff()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Settings.Module.GetGlobalSetting("mergetool.kdiff3.path"));
            if (string.IsNullOrEmpty(kdiff3path))
                return false;

            Settings.Module.SetGlobalPathSetting("mergetool.kdiff3.path", kdiff3path);
            return true;
        }

        private void ResolveDiffToolPath()
        {
            string kdiff3path = MergeToolsHelper.FindPathForKDiff(Settings.Module.GetGlobalSetting("difftool.kdiff3.path"));
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

            string exeName;
            string exeFile = MergeToolsHelper.Instance.FindDiffToolExeFile(GlobalDiffTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                DifftoolPath.SelectAll();
                DifftoolPath.SelectedText = "";
                DifftoolCmd.SelectAll();
                DifftoolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(_toolSuggestPath.Text, exeName),
                        __diffToolSuggestCaption.Text);
                return;
            }
            DifftoolPath.SelectAll(); // allow Undo action
            DifftoolPath.SelectedText = exeFile;
            DifftoolCmd.SelectAll();
            DifftoolCmd.SelectedText = MergeToolsHelper.Instance.DiffToolCmdSuggest(GlobalDiffTool.Text, exeFile);
        }

        private void MergeToolCmdSuggest_Click(object sender, EventArgs e)
        {
            if (!Settings.RunningOnWindows())
                return;

            string exeName;
            string exeFile = MergeToolsHelper.Instance.FindMergeToolExeFile(GlobalMergeTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                MergetoolPath.SelectAll();
                MergetoolPath.SelectedText = "";
                MergeToolCmd.SelectAll();
                MergeToolCmd.SelectedText = "";
                if (sender != null)
                    MessageBox.Show(this, String.Format(_toolSuggestPath.Text, exeName),
                        __mergeToolSuggestCaption.Text);
                return;
            }
            MergetoolPath.SelectAll(); // allow Undo action
            MergetoolPath.SelectedText = exeFile;
            MergeToolCmd.SelectAll();
            MergeToolCmd.SelectedText = MergeToolsHelper.Instance.MergeToolcmdSuggest(GlobalMergeTool.Text, exeFile);
        }

        private void AutoConfigMergeToolCmd(bool silent)
        {
            string exeName;
            string exeFile = MergeToolsHelper.Instance.FindMergeToolExeFile(GlobalMergeTool.Text, out exeName);
            if (String.IsNullOrEmpty(exeFile))
            {
                MergetoolPath.Text = "";
                MergeToolCmd.Text = "";
                if (!silent)
                    MessageBox.Show(this, String.Format(_toolSuggestPath.Text, exeName),
                        __mergeToolSuggestCaption.Text);
                return;
            }
            MergetoolPath.Text = exeFile;
            MergeToolCmd.Text = MergeToolsHelper.Instance.AutoConfigMergeToolCmd(GlobalMergeTool.Text, exeFile);
        }

        private void DiffToolFix_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GetGlobalDiffToolFromConfig()))
            {
                if (MessageBox.Show(this, _noDiffToolConfigured.Text, _noDiffToolConfiguredCaption.Text, 
                        MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveDiffToolForKDiff();
                    GlobalDiffTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("tpGlobalSettings");
                    return;
                }
            }

            if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                SolveDiffToolPathForKDiff();
            }

            if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(Settings.Module.GetGlobalSetting("difftool.kdiff3.path")))
            {
                MessageBox.Show(this, _kdiff3NotFoundAuto.Text);
                tabControl1.SelectTab("tpGlobalSettings");
                return;
            }

            Rescan_Click(null, null);
        }

        private void MergeToolFix_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(GetMergeTool()))
            {
                if (
                    MessageBox.Show(this, _noMergeToolConfigured.Text,
                        _noMergeToolConfiguredCaption.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SolveMergeToolForKDiff();
                    GlobalMergeTool.Text = "kdiff3";
                }
                else
                {
                    tabControl1.SelectTab("tpGlobalSettings");
                    return;
                }
            }

            if (IsMergeTool("kdiff3"))
            {
                SolveMergeToolPathForKDiff();
            }
            else if (IsMergeTool("p4merge") || IsMergeTool("TortoiseMerge"))
            {
                AutoConfigMergeToolCmd(true);

                Settings.Module.SetGlobalPathSetting(
                    string.Format("mergetool.{0}.cmd", GetMergeTool()), MergeToolCmd.Text);
            }
            
            if (IsMergeTool("kdiff3") &&
                string.IsNullOrEmpty(Settings.Module.GetGlobalSetting("mergetool.kdiff3.path")))
            {
                MessageBox.Show(this, _kdiff3NotFoundAuto.Text);
                tabControl1.SelectTab("tpGlobalSettings");
                return;
            }

            Rescan_Click(null, null);
        }

        private void BrowseMergeTool_Click(object sender, EventArgs e)
        {
            string mergeTool = GlobalMergeTool.Text.ToLowerInvariant();
            if (mergeTool == "kdiff3")
                MergetoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", MergetoolPath.Text);
            else if (mergeTool == "p4merge")
                MergetoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", MergetoolPath.Text);
            else if (mergeTool == "tortoisemerge")
                MergetoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe",
                                                MergetoolPath.Text);
            else
                MergetoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", MergetoolPath.Text);
        }

        private void GlobalDiffTool_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            DifftoolPath.Text = Settings.Module.GetGlobalSetting(string.Format("difftool.{0}.path", GlobalDiffTool.Text.Trim()));
            DifftoolCmd.Text = Settings.Module.GetGlobalSetting(string.Format("difftool.{0}.cmd", GlobalDiffTool.Text.Trim()));

            if (GlobalDiffTool.Text.Trim().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                ResolveDiffToolPath();

            DiffToolCmdSuggest_Click(null, null);
        }

        private void BrowseDiffTool_Click(object sender, EventArgs e)
        {
            if (GlobalDiffTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
            {
                DifftoolPath.Text = SelectFile(".", "kdiff3.exe (kdiff3.exe)|kdiff3.exe", DifftoolPath.Text);
            }
            else if (GlobalDiffTool.Text.Equals("p4merge", StringComparison.CurrentCultureIgnoreCase))
                DifftoolPath.Text = SelectFile(".", "p4merge.exe (p4merge.exe)|p4merge.exe", DifftoolPath.Text);
            else if (GlobalDiffTool.Text.Equals("tmerge", StringComparison.CurrentCultureIgnoreCase))
                DifftoolPath.Text = SelectFile(".", "TortoiseMerge.exe (TortoiseMerge.exe)|TortoiseMerge.exe",
                                               DifftoolPath.Text);
            else
                DifftoolPath.Text = SelectFile(".", "*.exe (*.exe)|*.exe", DifftoolPath.Text);
        }

        private void GlobalMergeTool_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            MergetoolPath.Text = Settings.Module.GetGlobalSetting(string.Format("mergetool.{0}.path", GlobalMergeTool.Text.Trim()));
            MergeToolCmd.Text = Settings.Module.GetGlobalSetting(string.Format("mergetool.{0}.cmd", GlobalMergeTool.Text.Trim()));

            if (GlobalMergeTool.Text.Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty(MergeToolCmd.Text))
                MergeToolCmd.Enabled = false;
            else
                MergeToolCmd.Enabled = true;

            MergeToolCmdSuggest_Click(null, null);
        }

        private void GitFound_Click(object sender, EventArgs e)
        {
            if (!SolveGitCommand())
            {
                MessageBox.Show(this, _solveGitCommandFailed.Text, _solveGitCommandFailedCaption.Text);

                tabControl1.SelectTab("tpGit");
                return;
            }

            MessageBox.Show(this, String.Format(_gitCanBeRun.Text, Settings.GitCommand), _gitCanBeRunCaption.Text);

            GitPath.Text = Settings.GitCommand;
            Rescan_Click(null, null);
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
            bool canFindGitCmd = CanFindGitCmd();
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

            bool valid = Settings.Module.ValidWorkingDir() && canFindGitCmd;
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

        private void CheckAtStartup_CheckedChanged(object sender, EventArgs e)
        {
            Settings.SetBool("checksettings", CheckAtStartup.Checked);
        }

        private void Rescan_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            Save();
            LoadSettings();
            CheckSettings();
            Cursor.Current = Cursors.Default;
        }

        private void BrowseGitPath_Click(object sender, EventArgs e)
        {
            SolveGitCommand();

            var browseDialog = new OpenFileDialog
                                   {
                                       FileName = Settings.GitCommand,
                                       Filter = "Git.cmd (git.cmd)|git.cmd|Git.exe (git.exe)|git.exe|Git (git)|git"
                                   };

            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                GitPath.Text = browseDialog.FileName;
            }
        }

        private void TabPageGitExtensions_Click(object sender, EventArgs e)
        {
            GitPath.Text = Settings.GitCommand;
        }

        private void GitPath_TextChanged(object sender, EventArgs e)
        {
            if (loadingSettings)
                return;
            Settings.GitCommand = GitPath.Text;
            LoadSettings();
        }

        private void GitBinFound_Click(object sender, EventArgs e)
        {
            if (!SolveLinuxToolsDir())
            {
                MessageBox.Show(this, _linuxToolsShNotFound.Text, _linuxToolsShNotFoundCaption.Text);
                tabControl1.SelectTab("tpGit");
                return;
            }

            MessageBox.Show(this, String.Format(_shCanBeRun.Text, Settings.GitBinDir), _shCanBeRunCaption.Text);
            GitBinPath.Text = Settings.GitBinDir;
            Rescan_Click(null, null);
        }

        public static bool CheckIfFileIsInPath(string fileName)
        {
            string path = string.Concat(Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.User), ";",
                                        Environment.GetEnvironmentVariable("path", EnvironmentVariableTarget.Machine));

            return path.Split(';').Any(dir => File.Exists(dir + " \\" + fileName) || File.Exists(dir + fileName));
        }

        public static bool SolveLinuxToolsDir()
        {
            if (!Settings.RunningOnWindows())
            {
                Settings.GitBinDir = "";
                return true;
            }

            if (CheckIfFileIsInPath("sh.exe") || CheckIfFileIsInPath("sh"))
            {
                Settings.GitBinDir = "";
                return true;
            }

            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
            {
                Settings.GitBinDir = @"c:\Program Files\Git\bin\";
                if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                {
                    Settings.GitBinDir = @"c:\Program Files (x86)\Git\bin\";
                    if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                    {
                        Settings.GitBinDir = "C:\\cygwin\\bin\\";
                        if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                        {
                            Settings.GitBinDir = Settings.GitCommand;
                            Settings.GitBinDir =
                                Settings.GitBinDir.Replace("\\cmd\\git.cmd", "\\bin\\").Replace("\\bin\\git.exe",
                                                                                                "\\bin\\");
                            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh"))
                            {
                                Settings.GitBinDir =
                                    GetRegistryValue(Registry.LocalMachine,
                                                     "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1",
                                                     "InstallLocation") + "\\bin\\";
                                if (!File.Exists(Settings.GitBinDir + "sh.exe") &&
                                    !File.Exists(Settings.GitBinDir + "sh"))
                                {
                                    Settings.GitBinDir = "";
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void OpenSSH_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void Putty_CheckedChanged(object sender, EventArgs e)
        {
            if (Putty.Checked)
            {
                AutoFindPuttyPaths();
            }
            EnableSshOptions();
        }

        private bool AutoFindPuttyPaths()
        {
            if (!Settings.RunningOnWindows())
                return false;

            if (AutoFindPuttyPathsInDir("c:\\Program Files\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("c:\\Program Files (x86)\\PuTTY\\")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseGit\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files\\TortoiseSvn\\bin")) return true;
            if (AutoFindPuttyPathsInDir("C:\\Program Files (x86)\\TortoiseSvn\\bin")) return true;
            if (
                AutoFindPuttyPathsInDir(GetRegistryValue(Registry.LocalMachine,
                                                         "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\PuTTY_is1",
                                                         "InstallLocation"))) return true;
            if (AutoFindPuttyPathsInDir(Settings.GetInstallDir() + "\\PuTTY\\")) return true;

            return false;
        }

        private bool AutoFindPuttyPathsInDir(string installdir)
        {
            if (!installdir.EndsWith("\\"))
                installdir += "\\";

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "plink.exe"))
                    PlinkPath.Text = installdir + "plink.exe";
            }

            if (!File.Exists(PlinkPath.Text))
            {
                if (File.Exists(installdir + "TortoisePlink.exe"))
                    PlinkPath.Text = installdir + "TortoisePlink.exe";
            }

            if (!File.Exists(PuttygenPath.Text))
            {
                if (File.Exists(installdir + "puttygen.exe"))
                    PuttygenPath.Text = installdir + "puttygen.exe";
            }

            if (!File.Exists(PageantPath.Text))
            {
                if (File.Exists(installdir + "pageant.exe"))
                    PageantPath.Text = installdir + "pageant.exe";
            }

            if (File.Exists(PlinkPath.Text) && File.Exists(PuttygenPath.Text) && File.Exists(PageantPath.Text))
                return true;
            return false;
        }

        private static string SelectFile(string initialDirectory, string filter, string prev)
        {
            var dialog = new OpenFileDialog
                             {
                                 Filter = filter,
                                 InitialDirectory = initialDirectory,
                                 Title = _selectFile.Text
                             };
            return (dialog.ShowDialog() == DialogResult.OK) ? dialog.FileName : prev;
        }

        private void OtherSshBrowse_Click(object sender, EventArgs e)
        {
            OtherSsh.Text = SelectFile(".", "Executable file (*.exe)|*.exe", OtherSsh.Text);
        }

        private void Other_CheckedChanged(object sender, EventArgs e)
        {
            EnableSshOptions();
        }

        private void EnableSshOptions()
        {
            OtherSsh.Enabled = Other.Checked;
            OtherSshBrowse.Enabled = Other.Checked;

            PlinkPath.Enabled = Putty.Checked;
            PuttygenPath.Enabled = Putty.Checked;
            PageantPath.Enabled = Putty.Checked;
            PlinkBrowse.Enabled = Putty.Checked;
            PuttygenBrowse.Enabled = Putty.Checked;
            PageantBrowse.Enabled = Putty.Checked;
            AutostartPageant.Enabled = Putty.Checked;
        }

        private void PuttyBrowse_Click(object sender, EventArgs e)
        {
            PlinkPath.Text = SelectFile(".",
                                        "Plink.exe (plink.exe)|plink.exe|TortoisePlink.exe (tortoiseplink.exe)|tortoiseplink.exe",
                                        PlinkPath.Text);
        }

        private void PuttygenBrowse_Click(object sender, EventArgs e)
        {
            PuttygenPath.Text = SelectFile(".", "puttygen.exe (puttygen.exe)|puttygen.exe", PuttygenPath.Text);
        }

        private void PageantBrowse_Click(object sender, EventArgs e)
        {
            PageantPath.Text = SelectFile(".", "pageant.exe (pageant.exe)|pageant.exe", PageantPath.Text);
        }

        private void SshConfig_Click(object sender, EventArgs e)
        {
            if (Putty.Checked)
            {
                if (AutoFindPuttyPaths())
                    MessageBox.Show(this, _puttyFoundAuto.Text, _puttyFoundAutoCaption.Text);
                else
                    tabControl1.SelectTab("ssh");
            }
        }

        private void BrowseGitBinPath_Click(object sender, EventArgs e)
        {
            SolveLinuxToolsDir();

            var browseDialog = new FolderBrowserDialog { SelectedPath = Settings.GitBinDir };

            if (browseDialog.ShowDialog(this) == DialogResult.OK)
            {
                GitBinPath.Text = browseDialog.SelectedPath;
            }
        }

        private void FormSettings_Shown(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            WindowState = FormWindowState.Normal;
            RestorePosition("settings");
            LoadSettings();
            CheckSettings();
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

        private void Dictionary_DropDown(object sender, EventArgs e)
        {
            try
            {
                Dictionary.Items.Clear();
                Dictionary.Items.Add("None");
                foreach (
                    string fileName in
                        Directory.GetFiles(Settings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
                {
                    var file = new FileInfo(fileName);
                    Dictionary.Items.Add(file.Name.Replace(".dic", ""));
                }
            }
            catch
            {
                MessageBox.Show(this, String.Format(_noDictFilesFound.Text, Settings.GetDictionaryDir()));
            }
        }

        private void ColorAddedLineDiffLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor
                                  };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorAddedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void _ColorGraphLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorGraphLabel.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorGraphLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorGraphLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorGraphLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineDiffLabel.BackColor);
        }

        private void label28_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorAddedLineLabel.BackColor
                                  };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorAddedLineLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorAddedLineLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorAddedLineLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorAddedLineLabel.BackColor);
        }

        private void ColorRemovedLineDiffLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor
                                  };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemovedLineDiffLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLineDiffLabel.BackColor);
        }

        private void ColorRemovedLine_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorRemovedLine.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorRemovedLine.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemovedLine.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemovedLine.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemovedLine.BackColor);
        }

        private void ColorSectionLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorSectionLabel.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorSectionLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorSectionLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorSectionLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorSectionLabel.BackColor);
        }

        private void ColorTagLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorTagLabel.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorTagLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorTagLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorTagLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorTagLabel.BackColor);
        }

        private void ColorBranchLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorBranchLabel.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorBranchLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorBranchLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorBranchLabel.BackColor);
        }

        private void ColorRemoteBranchLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog
                                  {
                                      Color = _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor
                                  };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorRemoteBranchLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorRemoteBranchLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorRemoteBranchLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorRemoteBranchLabel.BackColor);
        }

        private void ColorOtherLabel_Click(object sender, EventArgs e)
        {
            var colorDialog = new ColorDialog { Color = _NO_TRANSLATE_ColorOtherLabel.BackColor };
            colorDialog.ShowDialog(this);
            _NO_TRANSLATE_ColorOtherLabel.BackColor = colorDialog.Color;
            _NO_TRANSLATE_ColorOtherLabel.Text = colorDialog.Color.Name;
            _NO_TRANSLATE_ColorOtherLabel.ForeColor =
                ColorHelper.GetForeColorForBackColor(_NO_TRANSLATE_ColorOtherLabel.BackColor);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((TabControl)sender).TabPages[((TabControl)sender).SelectedIndex].Name.ToLower() == "tabpagehotkeys")
                controlHotkeys.ReloadSettings();
            else if (((TabControl)sender).TabPages[((TabControl)sender).SelectedIndex].Name.ToLower() == "scriptstab")
                populateSplitbutton();



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
            new FormTranslate().ShowDialog(this);
        }

        private void MulticolorBranches_CheckedChanged(object sender, EventArgs e)
        {
            if (MulticolorBranches.Checked)
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = false;
                StripedBanchChange.Enabled = true;
            }
            else
            {
                _NO_TRANSLATE_ColorGraphLabel.Visible = true;
                StripedBanchChange.Enabled = false;
            }
        }

        private static bool getCheckAtStartupChecked(bool bValid)
        {
            var retValue = Settings.GetValue<string>("checksettings", null) == null || Settings.GetValue<string>("checksettings", null) == "true";

            if (bValid && retValue)
            {
                Settings.SetValue("checksettings", false);
                retValue = false;
            }
            return retValue;
        }

        private bool CheckTranslationConfigSettings()
        {
            translationConfig.Visible = true;
            if (string.IsNullOrEmpty(Settings.Translation))
            {
                translationConfig.BackColor = Color.LightSalmon;
                translationConfig.Text = _noLanguageConfigured.Text;
                translationConfig_Fix.Visible = true;
                return false;
            }
            translationConfig.BackColor = Color.LightGreen;
            translationConfig_Fix.Visible = false;
            translationConfig.Text = String.Format(_languageConfigured.Text, Settings.Translation);
            return true;
        }

        private bool CheckSSHSettings()
        {
            SshConfig.Visible = true;
            if (GitCommandHelpers.Plink())
            {
                if (!File.Exists(Settings.Plink) || !File.Exists(Settings.Puttygen) || !File.Exists(Settings.Pageant))
                {
                    SshConfig.BackColor = Color.LightSalmon;
                    SshConfig.Text = _plinkputtyGenpageantNotFound.Text;
                    SshConfig_Fix.Visible = true;
                    return false;
                }
                SshConfig.BackColor = Color.LightGreen;
                SshConfig_Fix.Visible = false;
                SshConfig.Text = _puttyConfigured.Text;
                return true;
            }
            SshConfig.BackColor = Color.LightGreen;
            SshConfig_Fix.Visible = false;
            if (string.IsNullOrEmpty(GitCommandHelpers.GetSsh()))
                SshConfig.Text = _opensshUsed.Text;
            else
                SshConfig.Text = String.Format(_unknownSshClient.Text, GitCommandHelpers.GetSsh());
            return true;
        }

        private bool CheckGitExe()
        {
            GitBinFound.Visible = true;
            if (!File.Exists(Settings.GitBinDir + "sh.exe") && !File.Exists(Settings.GitBinDir + "sh") &&
                !CheckIfFileIsInPath("sh.exe") && !CheckIfFileIsInPath("sh"))
            {
                GitBinFound.BackColor = Color.LightSalmon;
                GitBinFound.Text = _linuxToolsSshNotFound.Text;
                GitBinFound_Fix.Visible = true;
                return false;
            }
            GitBinFound_Fix.Visible = false;
            GitBinFound.BackColor = Color.LightGreen;
            GitBinFound.Text = _linuxToolsSshFound.Text;
            return true;
        }

        private bool CheckGitCmdValid()
        {
            GitFound.Visible = true;
            if (!CanFindGitCmd())
            {
                GitFound.BackColor = Color.LightSalmon;
                GitFound.Text = _gitNotFound.Text;
                GitFound_Fix.Visible = true;
                return false;
            }

            if (GitCommandHelpers.VersionInUse < GitVersion.LastSupportedVersion)
            {
                GitFound.BackColor = Color.LightSalmon;
                GitFound.Text = String.Format(_wrongGitVersion.Text, GitCommandHelpers.VersionInUse, GitVersion.LastSupportedVersion);
                GitFound_Fix.Visible = true;
                return false;
            }

            GitFound_Fix.Visible = false;
            GitFound.BackColor = Color.LightGreen;
            GitFound.Text = String.Format(_gitVersionFound.Text, GitCommandHelpers.VersionInUse);
            return true;
        }

        private bool CheckDiffToolConfiguration()
        {
            DiffTool.Visible = true;
            if (string.IsNullOrEmpty(GetGlobalDiffToolFromConfig()))
            {
                DiffTool.BackColor = Color.LightSalmon;
                DiffTool_Fix.Visible = true;
                DiffTool.Text = _adviceDiffToolConfiguration.Text;
                return false;
            }
            if (Settings.RunningOnWindows())
            {
                if (GetGlobalDiffToolFromConfig().Equals("kdiff3", StringComparison.CurrentCultureIgnoreCase))
                {
                    string p = Settings.Module.GetGlobalSetting("difftool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        DiffTool.BackColor = Color.LightSalmon;
                        DiffTool.Text = _kdiffAsDiffConfiguredButNotFound.Text;
                        DiffTool_Fix.Visible = true;
                        return false;
                    }
                    DiffTool.BackColor = Color.LightGreen;
                    DiffTool.Text = _kdiffAsDiffConfigured.Text;
                    DiffTool_Fix.Visible = false;
                    return true;
                }
            }
            string difftool = GetGlobalDiffToolFromConfig();
            DiffTool.BackColor = Color.LightGreen;
            DiffTool.Text = String.Format(_diffToolXConfigured.Text, difftool);
            DiffTool_Fix.Visible = false;
            return true;
        }

        private bool CheckMergeTool()
        {
            MergeTool.Visible = true;
            if (string.IsNullOrEmpty(GetMergeTool()))
            {
                MergeTool.BackColor = Color.LightSalmon;
                MergeTool.Text = _configureMergeTool.Text;
                MergeTool_Fix.Visible = true;
                return false;
            }

            if (Settings.RunningOnWindows())
            {
                if (IsMergeTool("kdiff3"))
                {
                    string p = Settings.Module.GetGlobalSetting("mergetool.kdiff3.path");
                    if (string.IsNullOrEmpty(p) || !File.Exists(p))
                    {
                        MergeTool.BackColor = Color.LightSalmon;
                        MergeTool.Text = _kdiffAsMergeConfiguredButNotFound.Text;
                        MergeTool_Fix.Visible = true;
                        return false;
                    }
                    MergeTool.BackColor = Color.LightGreen;
                    MergeTool.Text = _kdiffAsMergeConfigured.Text;
                    MergeTool_Fix.Visible = false;
                    return true;
                }
                string mergetool = GetMergeTool().ToLowerInvariant();
                if (mergetool == "p4merge" || mergetool == "tmerge")
                {
                    string p = Settings.Module.GetGlobalSetting(string.Format("mergetool.{0}.cmd", mergetool));
                    if (string.IsNullOrEmpty(p))
                    {
                        MergeTool.BackColor = Color.LightSalmon;
                        MergeTool.Text = String.Format(_mergeToolXConfiguredNeedsCmd.Text, mergetool);
                        MergeTool_Fix.Visible = true;
                        return false;
                    }
                    MergeTool.BackColor = Color.LightGreen;
                    MergeTool.Text = String.Format(_customMergeToolXConfigured.Text, mergetool);
                    MergeTool_Fix.Visible = false;
                    return true;
                }
            }
            MergeTool.BackColor = Color.LightGreen;
            MergeTool.Text = _mergeToolXConfigured.Text;
            MergeTool_Fix.Visible = false;
            return true;
        }

        private bool CheckGlobalUserSettingsValid()
        {
            UserNameSet.Visible = true;
            if (string.IsNullOrEmpty(Settings.Module.GetGlobalSetting("user.name")) ||
                string.IsNullOrEmpty(Settings.Module.GetGlobalSetting("user.email")))
            {
                UserNameSet.BackColor = Color.LightSalmon;
                UserNameSet.Text = _noEmailSet.Text;
                UserNameSet_Fix.Visible = true;
                return false;
            }
            UserNameSet.BackColor = Color.LightGreen;
            UserNameSet.Text = _emailSet.Text;
            UserNameSet_Fix.Visible = false;
            return true;
        }

        private bool CheckGitExtensionRegistrySettings()
        {
            if (!Settings.RunningOnWindows())
                return true;

            ShellExtensionsRegistered.Visible = true;

            if (
                string.IsNullOrEmpty(GetRegistryValue(Registry.LocalMachine,
                                                      "Software\\Microsoft\\Windows\\CurrentVersion\\Shell Extensions\\Approved",
                                                      "{3C16B20A-BA16-4156-916F-0A375ECFFE24}")) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "*\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\shellex\\ContextMenuHandlers\\GitExtensions2", null)) ||
                string.IsNullOrEmpty(GetRegistryValue(Registry.ClassesRoot,
                                                      "Directory\\Background\\shellex\\ContextMenuHandlers\\GitExtensions2",
                                                      null)))
            {
                //Check if shell extensions are installed
                string path = Path.Combine(Settings.GetInstallDir(), GitExtensionsShellExName);
                if (!File.Exists(path))
                {
                    ShellExtensionsRegistered.BackColor = Color.LightGreen;
                    ShellExtensionsRegistered.Text = String.Format(_shellExtNoInstalled.Text);
                    ShellExtensionsRegistered_Fix.Visible = false;
                    return true;
                }

                ShellExtensionsRegistered.BackColor = Color.LightSalmon;
                ShellExtensionsRegistered.Text = String.Format(_shellExtNeedsToBeRegistered.Text, GitExtensionsShellExName);
                ShellExtensionsRegistered_Fix.Visible = true;
                return false;
            }
            ShellExtensionsRegistered.BackColor = Color.LightGreen;
            ShellExtensionsRegistered.Text = _shellExtRegistered.Text;
            ShellExtensionsRegistered_Fix.Visible = false;
            return true;
        }

        private bool CheckGitExtensionsInstall()
        {
            if (!Settings.RunningOnWindows())
                return true;

            GitExtensionsInstall.Visible = true;
            if (string.IsNullOrEmpty(Settings.GetInstallDir()))
            {
                GitExtensionsInstall.BackColor = Color.LightSalmon;
                GitExtensionsInstall.Text = _registryKeyGitExtensionsMissing.Text;
                GitExtensionsInstall_Fix.Visible = true;
                return false;
            }
            if (Settings.GetInstallDir() != null && Settings.GetInstallDir().EndsWith(".exe"))
            {
                GitExtensionsInstall.BackColor = Color.LightSalmon;
                GitExtensionsInstall.Text = _registryKeyGitExtensionsFaulty.Text;
                GitExtensionsInstall_Fix.Visible = true;
                return false;
            }
            GitExtensionsInstall.BackColor = Color.LightGreen;
            GitExtensionsInstall.Text = _registryKeyGitExtensionsCorrect.Text;
            GitExtensionsInstall_Fix.Visible = false;
            return true;
        }

        private static IEnumerable<string> GetWindowsCommandLocations()
        {
            if (!string.IsNullOrEmpty(Settings.GitCommand))
                yield return Settings.GitCommand;

            yield return @"C:\cygwin\bin\git.exe";
            yield return @"C:\cygwin\bin\git";
            yield return
                GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") +
                "bin\\git.exe";
            string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            string programFilesX86 = null;
            if (8 == IntPtr.Size
                || !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))
                programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\Git\bin\git.exe";
            yield return programFiles + @"\Git\bin\git.exe";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\msysgit\bin\git.exe";
            yield return programFiles + @"\msysgit\bin\git.exe";
            yield return
                GetRegistryValue(Registry.LocalMachine,
                                 "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Git_is1", "InstallLocation") +
                "cmd\\git.cmd";
            if (programFilesX86 != null)
                yield return programFilesX86 + @"\Git\cmd\git.cmd";
            yield return programFiles + @"\Git\cmd\git.cmd";
            yield return "git";
            yield return "git.cmd";
            yield return @"C:\msysgit\bin\git.exe";
            yield return @"C:\msysgit\cmd\git.cmd";
        }

        private static bool SolveGitCommand()
        {
            if (Settings.RunningOnWindows())
            {
                var command = (from cmd in GetWindowsCommandLocations()
                               let output = Settings.Module.RunCmd(cmd, string.Empty)
                               where !string.IsNullOrEmpty(output)
                               select cmd).FirstOrDefault();

                if (command != null)
                {
                    Settings.GitCommand = command;
                    return true;
                }
                return false;
            }
            Settings.GitCommand = "git";
            return !string.IsNullOrEmpty(Settings.Module.RunGitCmd(""));
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
            ScriptList_SelectionChanged(null, null);//needed for linux
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
            var ofd = new OpenFileDialog
                          {
                              InitialDirectory = "c:\\",
                              Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                              RestoreDirectory = true
                          };
            if (ofd.ShowDialog(this) == DialogResult.OK)
                commandTextBox.Text = ofd.FileName;
        }

        private void argumentsTextBox_Enter(object sender, EventArgs e)
        {
            helpLabel.Visible = true;
        }

        private void argumentsTextBox_Leave(object sender, EventArgs e)
        {
            helpLabel.Visible = false;
        }

        private void translationConfig_Click(object sender, EventArgs e)
        {
            new FormChooseTranslation().ShowDialog(this);
            Translate();
            Language.Text = Settings.Translation;
            Rescan_Click(null, null);
        }

        private void downloadDictionary_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/spdr870/gitextensions/wiki/Spelling");
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

        protected override bool ExecuteCommand(int cmd)
        {

            Commands command = (Commands)cmd;

            switch (command)
            {
                default: ExecuteScriptCommand(cmd, Keys.None); break;
            }
            return true;
        }

        #endregion

        private void ShowIconPreview()
        {
            string color = IconStyle.Text.ToLowerInvariant();
            Icon icon = null;
            switch (color)
            {
                case "default":
                    IconPreview.Image = (new Icon(GetApplicationIcon("Large", GetSelectedApplicationIconColor()), 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(GetApplicationIcon("Small", GetSelectedApplicationIconColor()), 16, 16)).ToBitmap();
                    break;
                case "small":
                    icon = GetApplicationIcon("Small", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
                case "large":
                    icon = GetApplicationIcon("Large", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
                case "cow":
                    icon = GetApplicationIcon("Cow", GetSelectedApplicationIconColor());
                    IconPreview.Image = (new Icon(icon, 32, 32)).ToBitmap();
                    IconPreviewSmall.Image = (new Icon(icon, 16, 16)).ToBitmap();
                    break;
            }
        }

        private void IconStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void DefaultIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void LightblueIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void BlueIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void PurpleIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void GreenIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void RedIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void YellowIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void RandomIcon_CheckedChanged(object sender, EventArgs e)
        {
            ShowIconPreview();
        }

        private void diffFontChangeButton_Click(object sender, EventArgs e)
        {
            diffFontDialog.Font = diffFont;
            DialogResult result = diffFontDialog.ShowDialog(this);

            if (result == DialogResult.OK || result == DialogResult.Yes)
            {
                SetCurrentDiffFont(diffFontDialog.Font);
            }
        }

        private void SetCurrentDiffFont(Font font)
        {
            diffFont = font;

            diffFontChangeButton.Text =
                string.Format("{0}, {1}", diffFont.FontFamily.Name, (int)diffFont.Size);

        }

        private void BrowseCommitTemplate_Click(object sender, EventArgs e)
        {
            CommitTemplatePath.Text = SelectFile(".", "*.txt (*.txt)|*.txt", CommitTemplatePath.Text);
        }

        private void SplitButtonMenuItem_Click(object sender, EventArgs e)
        {
            //reset bold item to regular
            var item = contextMenuStrip_SplitButton.Items.OfType<ToolStripMenuItem>().First(s => s.Font.Bold);
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

        private void downloadMsysgit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"http://code.google.com/p/msysgit/");
        }

        private void ChangeHomeButton_Click(object sender, EventArgs e)
        {
            Save();
            new FormFixHome().ShowDialog(this);
            LoadSettings();
            Rescan_Click(null, null);
        }
    }
}
