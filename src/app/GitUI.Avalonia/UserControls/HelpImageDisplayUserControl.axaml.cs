using Avalonia.Controls;
using Avalonia.Input;
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

        linkLabelShowHelp.Content = $"Show{Environment.NewLine}help";
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

    private void UpdateControlSize()
    {
        double width = IsExpanded
            ? Math.Max(Image1?.PixelSize.Width ?? 40, Image2?.PixelSize.Width ?? 40)
            : 30;
        double widthDelta = width - _hostContributionWidth;

        Width = width;
        MinWidth = width;
        if (TopLevel.GetTopLevel(this) is not Window form || widthDelta == 0)
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
