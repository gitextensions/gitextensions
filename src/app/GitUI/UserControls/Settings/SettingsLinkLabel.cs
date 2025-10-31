using System.ComponentModel;
using GitCommands;

namespace GitUI.UserControls.Settings;

public partial class SettingsLinkLabel : UserControl
{
    private string? _toolTipText;
    private ToolTipIcon _toolTipIcon;
    private ToolTip? _tooltip;

    public SettingsLinkLabel()
    {
        InitializeComponent();

        pictureBox.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(ManualSectionAnchorName))
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
    }

    public LinkArea LinkArea
    {
        get => linkLabel.LinkArea;
        set => linkLabel.LinkArea = value;
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
    public override string Text
    {
        get => linkLabel.Text;
        set => linkLabel.Text = value;
    }

    public string? ToolTipText
    {
        get => _toolTipText;
        set
        {
            _toolTipText = value;
            _tooltip ??= new ToolTip();
            _tooltip.SetToolTip(linkLabel, _toolTipText);
            _tooltip.SetToolTip(pictureBox, _toolTipText);
            pictureBox.Visible = !string.IsNullOrEmpty(_toolTipText);
        }
    }

    public ToolTipIcon ToolTipIcon
    {
        get => _toolTipIcon;
        set
        {
            _toolTipIcon = value;
            pictureBox.Image = _toolTipIcon switch
            {
                ToolTipIcon.Warning => Properties.Resources.Warning,
                ToolTipIcon.Information => Properties.Resources.information,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public event EventHandler InfoClicked
    {
        add { pictureBox.Click += value; }
        remove { pictureBox.Click -= value; }
    }

    public event LinkLabelLinkClickedEventHandler LinkClicked
    {
        add { linkLabel.LinkClicked += value; }
        remove { linkLabel.LinkClicked -= value; }
    }
}
