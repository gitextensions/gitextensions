using Avalonia.Controls;
using GitCommands;
using GitExtensions.Extensibility.Translations;
using ResourceManager;

namespace GitUI.UserControls.Settings;

// Twin of UserControls/Settings/SettingsLinkLabel. The containing settings page or dialog
// owns its established translation key; this reusable control only presents the link and
// optional information icon.
public sealed partial class SettingsLinkLabel : TranslatedControl
{
    private string? _toolTipText;
    private ToolTipIcon _toolTipIcon;

    public SettingsLinkLabel()
    {
        InitializeComponent();
        linkLabel.Click += linkLabel_LinkClicked;
        pictureBox.Click += pictureBox_Click;
        InitializeComplete();
    }

    public string? ManualSectionAnchorName { get; set; }

    public string? ManualSectionSubfolder { get; set; }

    public string? Text
    {
        get => linkLabel.Content as string;
        set => linkLabel.Content = value;
    }

    public string? ToolTipText
    {
        get => _toolTipText;
        set
        {
            _toolTipText = value;
            ToolTip.SetTip(this, value);
            ToolTip.SetTip(linkLabel, value);
            ToolTip.SetTip(pictureBox, value);
            pictureBox.IsVisible = !string.IsNullOrEmpty(value);
        }
    }

    public ToolTipIcon ToolTipIcon
    {
        get => _toolTipIcon;
        set
        {
            _toolTipIcon = value;
            ((Image)pictureBox.Content!).Source = value switch
            {
                ToolTipIcon.Warning => Properties.Images.Warning,
                ToolTipIcon.Information => Properties.Images.Information,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public event EventHandler? InfoClicked;

    public event EventHandler? LinkClicked;

    // This component has no standalone strings. Its parent retains the original field name
    // and translates Text/ToolTipText in that parent's category.
    public override void AddTranslationItems(ITranslation translation)
    {
        // The owning settings page supplies this control's translated text and tooltip.
    }

    public override void TranslateItems(ITranslation translation)
    {
        // The owning settings page applies this control's translated text and tooltip.
    }

    private void pictureBox_Click(object? sender, EventArgs e)
    {
        InfoClicked?.Invoke(this, e);
        if (string.IsNullOrWhiteSpace(ManualSectionAnchorName))
        {
            return;
        }

        string subfolder = string.IsNullOrWhiteSpace(ManualSectionSubfolder)
            ? "settings"
            : ManualSectionSubfolder;
        OsShellUtil.OpenUrlInDefaultBrowser(
            UserManual.UserManual.UrlFor(subfolder, ManualSectionAnchorName));
    }

    private void linkLabel_LinkClicked(object? sender, EventArgs e)
    {
        LinkClicked?.Invoke(this, e);
    }
}
