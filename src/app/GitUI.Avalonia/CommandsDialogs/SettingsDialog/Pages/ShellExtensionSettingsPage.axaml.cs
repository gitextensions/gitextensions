using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitUI.CommandsDialogs.SettingsDialog.ShellExtension;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages;

public partial class ShellExtensionSettingsPage : SettingsPageWithHeader
{
    private const char Checked_InMenu = '0';
    private const char Indeterminate_InSubMenu = '1';
    private const char Unchecked_NotInMenu = '2';

    private static readonly string[] MenuEntries =
    [
        "Add files...",
        "Apply patch...",
        "Open repository",
        "Create branch...",
        "Checkout branch...",
        "Checkout revision...",
        "Clone...",
        "Commit...",
        "Create new repository...",
        "Open with difftool",
        "File history",
        "Pull/Fetch...",
        "Push...",
        "Reset file changes..",
        "Revert",
        "Settings",
        "View stash",
        "View changes",
    ];

    private readonly TranslationString _noItems = new("no items");
    private readonly TranslationString _menuHelp = new(@"* Checked: at top level for direct access
* Intermediate: in a cascaded context menu
* Unchecked: not added to the menu");

    private readonly List<CheckBox> _menuEntryControls = [];
#pragma warning disable SX1309 // Preserve the original Designer field name used by translation and parity tooling.
    private readonly ToolTip toolTip1 = new();
#pragma warning restore SX1309
    private bool _isLoading = false;

    public ShellExtensionSettingsPage()
        : this(EmptyServiceProvider.Instance)
    {
    }

    public ShellExtensionSettingsPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        InitializeComponent();
        CreateMenuEntries();
        RegisterButton.Click += RegisterButton_Click;
        UnregisterButton.Click += UnregisterButton_Click;
        menuHelp.PointerReleased += menuHelp_Click;
        cbAlwaysShowAllCommands.IsCheckedChanged += (_, _) => UpdatePreview();
        UpdateRegistrationStatus();
        InitializeComplete();

        // when the dock is set in the designer it causes weird visual artifacts in scaled Windows environments
        // Avalonia constraint: the native ListBox owns its scrolling and fills the available grid cell without Dock.

        toolTip1.Content = _menuHelp.Text;
        Avalonia.Controls.ToolTip.SetTip(menuHelp, toolTip1);
    }

    protected override void SettingsToPage()
    {
        _isLoading = true;
        for (int i = 0; i < _menuEntryControls.Count; i++)
        {
            char state = i < AppSettings.CascadeShellMenuItems.Length
                ? AppSettings.CascadeShellMenuItems[i]
                : Unchecked_NotInMenu;
            _menuEntryControls[i].IsChecked = state switch
            {
                Checked_InMenu => true,
                Indeterminate_InSubMenu => null,
                _ => false,
            };
        }

        _isLoading = false;

        cbAlwaysShowAllCommands.IsChecked = AppSettings.AlwaysShowAllCommands;

        UpdatePreview();

        base.SettingsToPage();
    }

    protected override void PageToSettings()
    {
        string l_CascadeShellMenuItems = "";

        foreach (CheckBox checkBox in _menuEntryControls)
        {
            l_CascadeShellMenuItems += checkBox.IsChecked switch
            {
                null => Indeterminate_InSubMenu,
                true => Checked_InMenu,
                false => Unchecked_NotInMenu,
            };
        }

        AppSettings.CascadeShellMenuItems = l_CascadeShellMenuItems;
        AppSettings.AlwaysShowAllCommands = cbAlwaysShowAllCommands.IsChecked == true;

        base.PageToSettings();
    }

    private void chlMenuEntries_SelectedValueChanged(object? sender, EventArgs e)
    {
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        string topLevel = "";
        string cascaded = "";

        foreach (CheckBox checkBox in _menuEntryControls)
        {
            switch (checkBox.IsChecked)
            {
                case true:
                    topLevel += "GitExt " + checkBox.Content + "\r\n";
                    break;
                case null:
                    cascaded += "       " + checkBox.Content + "\r\n";
                    break;
            }
        }

        labelPreview.Text = topLevel;
        if (!string.IsNullOrWhiteSpace(cascaded))
        {
            labelPreview.Text += "Git Extensions > \r\n" + cascaded;
        }
        else if (string.IsNullOrWhiteSpace(topLevel))
        {
            labelPreview.Text += $"({_noItems.Text})";
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        ShellExtensionManager.Register();
        UpdateRegistrationStatus();
    }

    private void UnregisterButton_Click(object? sender, EventArgs e)
    {
        ShellExtensionManager.Unregister();
        UpdateRegistrationStatus();
    }

    private void UpdateRegistrationStatus()
    {
        gbExplorerIntegration.IsEnabled = OperatingSystem.IsWindows() && ShellExtensionManager.FilesExist();
        RegisterButton.IsEnabled = gbExplorerIntegration.IsEnabled && !ShellExtensionManager.IsRegistered();
        UnregisterButton.IsEnabled = gbExplorerIntegration.IsEnabled && ShellExtensionManager.IsRegistered();
    }

    private void chlMenuEntries_ItemCheck(object? sender, EventArgs e)
    {
        if (_isLoading || sender is not CheckBox checkBox)
        {
            return;
        }

        // Avalonia's native three-state order differs from CheckedListBox, so remap the
        // post-click value to preserve Unchecked -> Intermediate -> Checked -> Unchecked.
        checkBox.IsChecked = checkBox.IsChecked switch
        {
            true => null,
            false => true,
            null => false,
        };
        chlMenuEntries_SelectedValueChanged(sender, e);
    }

    private void menuHelp_Click(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            OsShellUtil.OpenUrlInDefaultBrowser(UserManual.UserManual.UrlFor("settings", "shell-extension"));
        }
    }

    private void CreateMenuEntries()
    {
        foreach (string entry in MenuEntries)
        {
            CheckBox checkBox = new()
            {
                Content = entry,
                IsThreeState = true,
            };
            checkBox.Click += chlMenuEntries_ItemCheck;
            _menuEntryControls.Add(checkBox);
        }

        _NO_TRANSLATE_chlMenuEntries.ItemsSource = _menuEntryControls;
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(ShellExtensionSettingsPage page)
    {
        internal IReadOnlyList<CheckBox> MenuEntries => page._menuEntryControls;
        internal CheckBox AlwaysShowAllCommands => page.cbAlwaysShowAllCommands;
        internal TextBlock Preview => page.labelPreview;
        internal Control ExplorerIntegration => page.gbExplorerIntegration;
    }
}
