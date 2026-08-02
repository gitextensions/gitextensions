using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using GitCommands.Utils;
using GitExtensions.Extensibility.Settings;
using GitExtensions.Extensibility.Translations;
using GitUI.Avatars;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class AppearanceSettingsPage : SettingsPageWithHeader
{
    private const string _spellingWikiURL = "https://github.com/gitextensions/gitextensions/wiki/Spelling";
    private const string _translationsWikiURL = "https://github.com/gitextensions/gitextensions/wiki/Translations";

    private static readonly string[] TruncatePathMethodItems = ["None", "Compact", "Trim start", "Filename only"];

    private readonly TranslationString _noDictFile = new("None");
    private readonly TranslationString _noDictFilesFound = new("No dictionary files found in: {0}");
    private readonly TranslationString _noImageServiceTooltip = new(
        "A default image, if the provider has no image for the email address.\r\n\r\nClick this info icon for more details.");
    private readonly TranslationString _avatarProviderTooltip = new(
        "The avatar provider defines the source for user-defined avatar images.\r\nThe \"Default\" provider uses GitHub and Gravatar,\r\nthe \"Custom\" provider allows you to set custom provider URLs and\r\n\"None\" disables user-defined avatars.\r\n\r\nClick this info icon for more details.");

    public AppearanceSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public AppearanceSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        SetTruncatePathMethodItems(TruncatePathMethodItems);
        FillComboBoxWithEnumValues<GitCommands.AvatarProvider>(AvatarProvider);
        FillComboBoxWithEnumValues<AvatarFallbackType>(_NO_TRANSLATE_NoImageService);
        ConfigurePlatformControls();
        ConfigureToolTips();
        WireEvents();
        InitializeComplete();
    }

    public static SettingsPageReference GetPageReference()
        => new SettingsPageReferenceByType(typeof(AppearanceSettingsPage));

    protected override void SettingsToPage()
    {
        chkShowRelativeDate.IsChecked = AppSettings.RelativeDate;
        chkShowRepoCurrentBranch.IsChecked = AppSettings.ShowRepoCurrentBranch;
        chkShowCurrentBranchInVisualStudio.IsChecked = AppSettings.ShowCurrentBranchInVisualStudio;
        chkEnableAutoScale.IsChecked = AppSettings.EnableAutoScale;
        truncatePathMethod.SelectedIndex = AppSettings.TruncatePathMethod switch
        {
            TruncatePathMethod.Compact => 1,
            TruncatePathMethod.TrimStart => 2,
            TruncatePathMethod.FileNameOnly => 3,
            _ => 0,
        };

        _NO_TRANSLATE_DaysToCacheImages.Value = AppSettings.AvatarImageCacheDays;
        ShowAuthorAvatarInCommitInfo.IsChecked = AppSettings.ShowAuthorAvatarInCommitInfo;
        ShowAuthorAvatarInCommitGraph.IsChecked = AppSettings.ShowAuthorAvatarColumn;
        SelectEnumValue(AvatarProvider, AppSettings.AvatarProvider);
        SelectEnumValue(_NO_TRANSLATE_NoImageService, AppSettings.AvatarFallbackType);
        txtCustomAvatarTemplate.Text = AppSettings.CustomAvatarTemplate;
        ManageAvatarOptionsDisplay();

        PopulateLanguages();
        PopulateSelectedDictionary();
        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        AppSettings.RelativeDate = chkShowRelativeDate.IsChecked == true;
        AppSettings.ShowRepoCurrentBranch = chkShowRepoCurrentBranch.IsChecked == true;
        AppSettings.ShowCurrentBranchInVisualStudio = chkShowCurrentBranchInVisualStudio.IsChecked == true;
        AppSettings.EnableAutoScale = chkEnableAutoScale.IsChecked == true;
        AppSettings.TruncatePathMethod = truncatePathMethod.SelectedIndex switch
        {
            1 => TruncatePathMethod.Compact,
            2 => TruncatePathMethod.TrimStart,
            3 => TruncatePathMethod.FileNameOnly,
            _ => TruncatePathMethod.None,
        };

        GitCommands.AvatarProvider avatarProvider = GetSelectedEnumValue(AvatarProvider, AppSettings.AvatarProvider);
        AvatarFallbackType fallbackType = GetSelectedEnumValue(_NO_TRANSLATE_NoImageService, AppSettings.AvatarFallbackType);
        string customTemplate = txtCustomAvatarTemplate.Text ?? string.Empty;
        bool shouldClearCache = AppSettings.AvatarProvider != avatarProvider
            || AppSettings.AvatarFallbackType != fallbackType
            || AppSettings.CustomAvatarTemplate != customTemplate;

        AppSettings.ShowAuthorAvatarColumn = ShowAuthorAvatarInCommitGraph.IsChecked == true;
        AppSettings.ShowAuthorAvatarInCommitInfo = ShowAuthorAvatarInCommitInfo.IsChecked == true;
        AppSettings.AvatarImageCacheDays = (int)(_NO_TRANSLATE_DaysToCacheImages.Value ?? AppSettings.AvatarImageCacheDays);
        AppSettings.CustomAvatarTemplate = customTemplate;
        AppSettings.AvatarProvider = avatarProvider;
        AppSettings.AvatarFallbackType = fallbackType;

        if (shouldClearCache)
        {
            AvatarService.UpdateAvatarProvider();
            ThreadHelper.FileAndForget(AvatarService.CacheCleaner.ClearCacheAsync);
        }

        AppSettings.Translation = Language.SelectedItem as string ?? Language.Text ?? "English";
        ResourceManager.TranslatedStrings.Reinitialize();
        GitUI.TranslatedStrings.Reinitialize();
        AppSettings.Dictionary = Dictionary.SelectedIndex == 0
            ? "none"
            : Dictionary.SelectedItem as string ?? Dictionary.Text ?? "none";

        base.PageToSettings();
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        for (int index = 0; index < TruncatePathMethodItems.Length; index++)
        {
            translation.AddTranslationItem(
                nameof(AppearanceSettingsPage),
                nameof(truncatePathMethod),
                $"Item{index}",
                TruncatePathMethodItems[index]);
        }
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        string[] translatedItems = new string[TruncatePathMethodItems.Length];
        for (int index = 0; index < TruncatePathMethodItems.Length; index++)
        {
            string source = TruncatePathMethodItems[index];
            translatedItems[index] = translation.TranslateItem(
                nameof(AppearanceSettingsPage),
                nameof(truncatePathMethod),
                $"Item{index}",
                () => source) ?? source;
        }

        SetTruncatePathMethodItems(translatedItems);
    }

    private void WireEvents()
    {
        Dictionary.DropDownOpened += Dictionary_DropDown;
        helpTranslate.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(_translationsWikiURL);
        downloadDictionary.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(_spellingWikiURL);
        ClearImageCache.Click += (_, _) => ThreadHelper.FileAndForget(AvatarService.CacheCleaner.ClearCacheAsync);
        pictureAvatarHelp.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "author-images-avatar-fallback"));
        avatarProviderHelp.Click += (_, _) => OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "author-images-avatar-provider"));
        AvatarProvider.SelectionChanged += (_, _) => ManageAvatarOptionsDisplay();
    }

    private void ConfigurePlatformControls()
    {
        chkShowCurrentBranchInVisualStudio.IsVisible = OperatingSystem.IsWindows();
    }

    private void ConfigureToolTips()
    {
        ToolTip.SetToolTip(_NO_TRANSLATE_NoImageService, _noImageServiceTooltip.Text);
        ToolTip.SetToolTip(pictureAvatarHelp, _noImageServiceTooltip.Text);
        ToolTip.SetToolTip(avatarProviderHelp, _avatarProviderTooltip.Text);
    }

    private void PopulateLanguages()
    {
        Language.Items.Clear();
        Language.Items.Add("English");
        foreach (string translation in Translator.GetAllTranslations())
        {
            Language.Items.Add(translation);
        }

        Language.SelectedItem = Language.Items
            .OfType<string>()
            .FirstOrDefault(item => string.Equals(item, AppSettings.Translation, StringComparison.Ordinal))
            ?? "English";
    }

    private void PopulateSelectedDictionary()
    {
        Dictionary.Items.Clear();
        Dictionary.Items.Add(_noDictFile.Text);
        if (AppSettings.Dictionary.Equals("none", StringComparison.InvariantCultureIgnoreCase))
        {
            Dictionary.SelectedIndex = 0;
            return;
        }

        string dictionaryFile = string.Concat(Path.Join(AppSettings.GetDictionaryDir(), AppSettings.Dictionary), ".dic");
        if (!File.Exists(dictionaryFile))
        {
            Dictionary.SelectedIndex = 0;
            return;
        }

        Dictionary.Items.Add(AppSettings.Dictionary);
        Dictionary.SelectedItem = AppSettings.Dictionary;
    }

    private void Dictionary_DropDown(object? sender, EventArgs e)
    {
        try
        {
            string currentDictionary = Dictionary.SelectedItem as string ?? Dictionary.Text ?? _noDictFile.Text;
            Dictionary.Items.Clear();
            Dictionary.Items.Add(_noDictFile.Text);
            foreach (string fileName in Directory.GetFiles(AppSettings.GetDictionaryDir(), "*.dic", SearchOption.TopDirectoryOnly))
            {
                Dictionary.Items.Add(Path.GetFileNameWithoutExtension(fileName));
            }

            Dictionary.SelectedItem = Dictionary.Items
                .OfType<string>()
                .FirstOrDefault(item => string.Equals(item, currentDictionary, StringComparison.Ordinal))
                ?? _noDictFile.Text;
        }
        catch
        {
            MessageBoxes.Show(
                TopLevel.GetTopLevel(this) as WinFormsShims.IWin32Window,
                string.Format(_noDictFilesFound.Text, AppSettings.GetDictionaryDir()),
                GitUI.TranslatedStrings.Error,
                WinFormsShims.MessageBoxButtons.OK,
                WinFormsShims.MessageBoxIcon.Error);
        }
    }

    private void SetTruncatePathMethodItems(IReadOnlyList<string> items)
    {
        int selectedIndex = truncatePathMethod.SelectedIndex;
        truncatePathMethod.ItemsSource = items.ToArray();
        truncatePathMethod.SelectedIndex = selectedIndex;
    }

    private static T GetSelectedEnumValue<T>(ComboBox comboBox, T fallback) where T : struct, Enum
        => comboBox.SelectedItem is ComboBoxItem<T> item ? item.Value : fallback;

    private void ManageAvatarOptionsDisplay()
    {
        bool showCustomTemplate = GetSelectedEnumValue(AvatarProvider, GitCommands.AvatarProvider.Default)
            == GitCommands.AvatarProvider.Custom;
        lblCustomAvatarTemplate.IsVisible = showCustomTemplate;
        txtCustomAvatarTemplate.IsVisible = showCustomTemplate;
    }

    private static void SelectEnumValue<T>(ComboBox comboBox, T value) where T : struct, Enum
    {
        comboBox.SelectedItem = comboBox.ItemsSource?
            .OfType<ComboBoxItem<T>>()
            .FirstOrDefault(item => EqualityComparer<T>.Default.Equals(item.Value, value));
    }

    private static void FillComboBoxWithEnumValues<T>(ComboBox comboBox) where T : struct, Enum
    {
        ComboBoxItem<T>[] items = EnumHelper.GetValues<T>()
            .Select(value => new ComboBoxItem<T>(value.GetDescription(), value))
            .ToArray();
        comboBox.ItemsSource = items;
        comboBox.ItemTemplate = new FuncDataTemplate<ComboBoxItem<T>>(
            (item, _) => new TextBlock { Text = item?.Text },
            supportsRecycling: true);
    }

    internal TestAccessor GetTestAccessor() => new(this);

    private sealed record ComboBoxItem<T>(string Text, T Value);

    internal readonly struct TestAccessor(AppearanceSettingsPage page)
    {
        public CheckBox ShowRelativeDate => page.chkShowRelativeDate;

        public CheckBox ShowRepositoryBranch => page.chkShowRepoCurrentBranch;

        public CheckBox ShowVisualStudioBranch => page.chkShowCurrentBranchInVisualStudio;

        public CheckBox EnableAutoScale => page.chkEnableAutoScale;

        public ComboBox TruncatePathMethod => page.truncatePathMethod;

        public ComboBox Language => page.Language;

        public ComboBox Dictionary => page.Dictionary;

        public Control AuthorImages => page.gbAuthorImages;

        public CheckBox ShowAvatarInCommitGraph => page.ShowAuthorAvatarInCommitGraph;

        public CheckBox ShowAvatarInCommitInfo => page.ShowAuthorAvatarInCommitInfo;

        public NumericUpDown CacheDays => page._NO_TRANSLATE_DaysToCacheImages;

        public ComboBox AvatarProvider => page.AvatarProvider;

        public ComboBox AvatarFallback => page._NO_TRANSLATE_NoImageService;

        public TextBox CustomAvatarTemplate => page.txtCustomAvatarTemplate;

        public Button ClearAvatarCache => page.ClearImageCache;
    }
}
