using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media.Imaging;
using GitCommands;
using ResourceManager;

namespace GitUI.Help;

public partial class HelpImageDisplayUserControl : GitExtensionsControl
{
    private Bitmap? _image1;
    private Bitmap? _image2;
    private bool _isExpanded;
    private double _hostContributionWidth = 40;

    ////public const string fastForwardHoverText = "Hover to see scenario when fast forward is possible.";
    private bool _isLoaded;

    public HelpImageDisplayUserControl()
    {
        InitializeComponent();

        linkLabelShowHelp.Content = "Show\r\nhelp";
        buttonShowHelp.Click += buttonShowHelp_Click;
        linkLabelShowHelp.Click += linkLabelShowHelp_LinkClicked;
        linkLabelHide.Click += linkLabelHide_LinkClicked;
        PointerEntered += HelpImageDisplayUserControl_PointerEntered;
        PointerExited += HelpImageDisplayUserControl_PointerExited;
        AttachedToVisualTree += (_, _) => LoadSettings();

        InitializeComplete();
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            _isExpanded = value;
            UpdateIsExpandedState();
            if (_isLoaded)
            {
                // to avoid calling this when InitializeComponents is called
                /*
                                 * ...
                                            this.helpImageDisplayUserControl1.IsExpanded = false;                       // this before...
                                            this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
                                            this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(30, 50);
                                            this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";    // ...this gives wrong id!!!
                                 * ...

                                 */
                AppSettings.SetBool("HelpIsExpanded" + GetId(), value);
            }
        }
    }

    public string? UniqueIsExpandedSettingsId { get; set; }

    private void UpdateIsExpandedState()
    {
        linkLabelHide.IsVisible = _isExpanded;

        //// linkLabelShowHelp.IsVisible = false; // Why use button instead of label? Because button has constant width independent of language!
        ////linkLabelShowHelp.IsVisible = true;
        buttonShowHelp.IsVisible = !_isExpanded;
        pictureBox1.IsVisible = _isExpanded;
        labelHoverText.IsVisible = _isExpanded && IsOnHoverShowImage2;
        UpdateControlSize();
    }

    public Bitmap? Image1
    {
        get => _image1;
        set
        {
            _image1 = value;
            UpdateImageDisplay();
            if (IsExpanded)
            {
                UpdateControlSize();
            }
        }
    }

    public Bitmap? Image2
    {
        get => _image2;
        set
        {
            _image2 = value;
            UpdateImageDisplay();
            if (IsExpanded)
            {
                UpdateControlSize();
            }
        }
    }

    /// <summary>
    /// see also IsOnHoverShowImage2NoticeText.
    /// </summary>
    public bool IsOnHoverShowImage2
    {
        get => _showImage2OnHover;
        set
        {
            _showImage2OnHover = value;

            ////labelHoverText.Visible = value; // NOTE: would trigger OnLoad in WinForms
            UpdateIsExpandedState();
            UpdateImageDisplay();
        }
    }

    /// <summary>
    /// only shown when IsOnHoverShowImage2 is true.
    /// </summary>
    public string IsOnHoverShowImage2NoticeText
    {
        get => labelHoverText.Text ?? string.Empty;
        set => labelHoverText.Text = value;
    }

    private bool _isHover;

    /// <summary>
    /// NOTE: in WinForms, will also be called if designer code calls "this.helpImageDisplayUserControl1.IsOnHoverShowImage2 = true;".
    /// </summary>
    private void LoadSettings()
    {
        if (_isLoaded)
        {
            return;
        }

        _isExpanded = AppSettings.GetBool("HelpIsExpanded" + GetId(), _isExpanded);
        _isLoaded = true;
        UpdateIsExpandedState();
        UpdateImageDisplay();
    }

    private bool _showImage2OnHover;

    private string GetId() => UniqueIsExpandedSettingsId ?? "MUST_BE_SET";

    // The Designer may have assigned Width after IsExpanded: use that actual source baseline.
    // WinForms TopLevelControl finds its Form before showing; Avalonia's visual root may not yet exist.
    // The control already contributes this width when it is first attached. Remember
    // that pre-attachment state so loading an expanded preference does not add the
    // same width to the host window a second time.
    private void UpdateControlSize()
    {
        // default size
        double width = IsExpanded
            ? Math.Max(Image1?.PixelSize.Width ?? 40, Image2?.PixelSize.Width ?? 40)

            // width for "show help" button
            : 30;

        double widthDelta = width - (double.IsNaN(Width) ? _hostContributionWidth : Width);

        // apply size to control
        Width = width;
        MinWidth = width;

        Window? form = TopLevel.GetTopLevel(this) as Window ?? this.FindLogicalAncestorOfType<Window>();
        if (form is null)
        {
            _hostContributionWidth = width;
            return;
        }

        if (widthDelta == 0)
        {
            return;
        }

        form.Width += widthDelta;
        form.MinWidth = Math.Max(0, form.MinWidth + widthDelta);
        _hostContributionWidth = width;
    }

    private void UpdateImageDisplay()
    {
        pictureBox1.Source = IsOnHoverShowImage2 && _isHover
            ? Image2 ?? Image1
            : Image1;
    }

    private void HelpImageDisplayUserControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        _isHover = true;
        UpdateImageDisplay();
    }

    private void HelpImageDisplayUserControl_PointerExited(object? sender, PointerEventArgs e)
    {
        _isHover = false;
        UpdateImageDisplay();
    }

    private void linkLabelHide_LinkClicked(object? sender, EventArgs e)
    {
        IsExpanded = false;
    }

    private void buttonShowHelp_Click(object? sender, EventArgs e)
    {
        IsExpanded = true;
    }

    private void linkLabelShowHelp_LinkClicked(object? sender, EventArgs e)
    {
        IsExpanded = true;
    }
}
