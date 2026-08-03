using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Input;
using GitCommands;
using GitUI.Properties;

namespace GitUI.UserControls.Settings;

public partial class SettingsCheckBox : UserControl
{
    private string? _toolTipText;
    private ToolTipIcon _toolTipIcon;

    public SettingsCheckBox()
    {
        InitializeComponent();
        AutomationProperties.SetName(checkBox, Text);

        pictureBox.PointerReleased += (_, e) =>
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                InfoClicked?.Invoke(pictureBox, e);
            }

            if (e.InitialPressMouseButton != MouseButton.Left || string.IsNullOrWhiteSpace(ManualSectionAnchorName))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ManualSectionSubfolder))
            {
                ManualSectionSubfolder = "settings";
            }

            string url = UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
            OsShellUtil.OpenUrlInDefaultBrowser(url);
        };
        checkBox.IsCheckedChanged += (_, e) => CheckedChanged?.Invoke(checkBox, e);
    }

    public bool Checked
    {
        get => checkBox.IsChecked == true;
        set => checkBox.IsChecked = value;
    }

    /// <summary>
    /// Gets or sets the anchor pointing to a section in the manual pertaining to this control.
    /// </summary>
    /// <remarks>
    /// The URL structure:
    /// https://git-extensions-documentation.readthedocs.io/{ManualSectionSubfolder}.html#{ManualSectionAnchorName}.
    /// </remarks>
    public string? ManualSectionAnchorName { get; set; }

    /// <summary>
    /// Gets or sets the name of a document pertaining to this control.
    /// Default is "settings
    /// </summary>
    /// <remarks>
    /// The URL structure:
    /// https://git-extensions-documentation.readthedocs.io/{ManualSectionSubfolder}.html#{ManualSectionAnchorName}.
    /// </remarks>
    [DefaultValue(null)]
    public string? ManualSectionSubfolder { get; set; }

    [EditorBrowsable(EditorBrowsableState.Always)]
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [Bindable(true)]
    [AllowNull]
    public string Text
    {
        get => checkBox.Content?.ToString() ?? string.Empty;
        set
        {
            checkBox.Content = value;

            // Avalonia exposes the native checkbox directly to automation, so keep its name aligned with the original text.
            AutomationProperties.SetName(checkBox, value);
        }
    }

    public string? ToolTipText
    {
        get => _toolTipText;
        set
        {
            _toolTipText = value;
            ToolTip.SetTip(checkBox, _toolTipText);
            ToolTip.SetTip(pictureBox, _toolTipText);
            pictureBox.IsVisible = !string.IsNullOrEmpty(_toolTipText);
        }
    }

    public ToolTipIcon ToolTipIcon
    {
        get => _toolTipIcon;
        set
        {
            _toolTipIcon = value;
            pictureBox.Source = _toolTipIcon switch
            {
                ToolTipIcon.Warning => Images.Warning,
                ToolTipIcon.Information => Images.Information,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public event EventHandler? InfoClicked;

    public event EventHandler? CheckedChanged;

    // parity-scaffolding: Exposes the original named fields to focused tests and paired capture seeding.
    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(SettingsCheckBox control)
    {
        internal CheckBox CheckBox => control.checkBox;
        internal Image PictureBox => control.pictureBox;
    }
}
