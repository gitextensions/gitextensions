using Avalonia.Controls;
using Avalonia.Media;
using GitCommands;
using GitCommands.DiffMergeTools;
using GitCommands.Git;
using GitExtensions.Extensibility.Settings;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public sealed partial class ChecklistSettingsPage : SettingsPageWithHeader
{
    private readonly TranslationString _adviceDiffToolConfiguration =
        new("You should configure a diff tool to show file diff in external program.");
    private readonly TranslationString _configureMergeTool =
        new("You need to configure merge tool in order to solve merge conflicts.");
    private readonly TranslationString _diffToolXConfigured = new("There is a difftool configured: {0}");
    private readonly TranslationString _emailSet = new("A username and an email address are configured.");
    private readonly TranslationString _gitNotFound =
        new("Git not found. To solve this problem you can set the correct path in settings.");
    private readonly TranslationString _gitVersionFound = new("Git {0} is found on your computer.");
    private readonly TranslationString _languageConfigured = new("The configured language is {0}.");
    private readonly TranslationString _mergeToolXConfigured = new("There is a mergetool configured: {0}");
    private readonly TranslationString _noEmailSet = new("You need to configure a username and an email address.");
    private readonly TranslationString _noLanguageConfigured =
        new("There is no language configured for Git Extensions.");

    public ChecklistSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ChecklistSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        GitFound_Fix.Click += (_, _) => PageHost.GotoPage(GitSettingsPage.GetPageReference());
        UserNameSet_Fix.Click += (_, _) => PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());
        MergeTool_Fix.Click += (_, _) => PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());
        DiffTool_Fix.Click += (_, _) => PageHost.GotoPage(GitConfigSettingsPage.GetPageReference());
        translationConfig_Fix.Click += (_, _) => PageHost.GotoPage(GeneralSettingsPage.GetPageReference());
        Rescan.Click += (_, _) =>
        {
            PageHost.SaveAll();
            PageHost.LoadAll();
            CheckSettings();
        };
        CheckAtStartup.Click += (_, _) => AppSettings.CheckSettings = CheckAtStartup.IsChecked == true;
        InitializeComplete();
    }

    public override bool IsInstantSavePage => true;

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(ChecklistSettingsPage));

    public override void OnPageShown() => CheckSettings();

    public bool CheckSettings()
    {
        ChecklistResult result = Evaluate(CommonLogic);
        Render(GitFound, GitFound_Fix, result.Git, result.GitMessage);
        Render(UserNameSet, UserNameSet_Fix, result.Identity, result.IdentityMessage);
        Render(MergeTool, MergeTool_Fix, result.MergeTool, result.MergeToolMessage);
        Render(DiffTool, DiffTool_Fix, result.DiffTool, result.DiffToolMessage);
        Render(translationConfig, translationConfig_Fix, result.Translation, result.TranslationMessage);
        if (result.IsValid && AppSettings.CheckSettings)
        {
            AppSettings.CheckSettings = false;
        }

        CheckAtStartup.IsChecked = AppSettings.CheckSettings;
        return result.IsValid;
    }

    internal ChecklistResult Evaluate(CommonLogic commonLogic)
    {
        string gitVersion = string.Empty;
        bool git = TryCheck(() =>
        {
            if (string.IsNullOrWhiteSpace(commonLogic.Module.GitExecutable.GetOutput(arguments: "--version")))
            {
                return false;
            }

            gitVersion = GitVersion.Current.ToString();
            return GitVersion.Current >= GitVersion.LastRecommendedVersion;
        });
        bool identity = git && TryCheck(() =>
        {
            string? userName = commonLogic.GitConfigSettingsSet.GlobalSettings.GetValue("user.name");
            string? userEmail = commonLogic.GitConfigSettingsSet.GlobalSettings.GetValue("user.email");
            return !string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(userEmail);
        });
        DiffMergeToolConfigurationManager tools = new(
            () => commonLogic.GitConfigSettingsSet.EffectiveSettings);
        string? mergeTool = null;
        bool merge = git && TryCheck(() =>
        {
            mergeTool = tools.ConfiguredMergeTool;
            return !string.IsNullOrWhiteSpace(mergeTool)
                   && !string.IsNullOrWhiteSpace(
                       tools.GetToolCommand(mergeTool, DiffMergeToolType.Merge));
        });
        string? diffTool = null;
        bool diff = git && TryCheck(() =>
        {
            diffTool = tools.ConfiguredDiffTool;
            return !string.IsNullOrWhiteSpace(diffTool)
                   && !string.IsNullOrWhiteSpace(
                       tools.GetToolCommand(diffTool, DiffMergeToolType.Diff));
        });
        bool editor = git && TryCheck(() => !string.IsNullOrWhiteSpace(commonLogic.GetGlobalEditor()));
        bool translation = !string.IsNullOrWhiteSpace(AppSettings.Translation);
        return new ChecklistResult(
            git,
            identity,
            merge,
            diff,
            editor,
            translation,
            git ? string.Format(_gitVersionFound.Text, gitVersion) : _gitNotFound.Text,
            identity ? _emailSet.Text : _noEmailSet.Text,
            merge ? string.Format(_mergeToolXConfigured.Text, mergeTool) : _configureMergeTool.Text,
            diff ? string.Format(_diffToolXConfigured.Text, diffTool) : _adviceDiffToolConfiguration.Text,
            translation ? string.Format(_languageConfigured.Text, AppSettings.Translation) : _noLanguageConfigured.Text);

        static bool TryCheck(Func<bool> check)
        {
            try
            {
                return check();
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    protected override void SettingsToPage()
    {
        base.SettingsToPage();
        CheckSettings();
    }

    private static void Render(Button status, Button repair, bool valid, string message)
    {
        status.Content = message;
        status.Foreground = valid ? Brushes.Green : Brushes.OrangeRed;
        repair.IsVisible = !valid;
    }

    internal sealed record ChecklistResult(
        bool Git,
        bool Identity,
        bool MergeTool,
        bool DiffTool,
        bool Editor,
        bool Translation,
        string GitMessage,
        string IdentityMessage,
        string MergeToolMessage,
        string DiffToolMessage,
        string TranslationMessage)
    {
        internal bool IsValid => Git && Identity && MergeTool && DiffTool && Editor && Translation;
    }
}
