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
    private bool _isHover;
    private bool _isLoaded;
    private bool _showImage2OnHover;

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
                AppSettings.SetBool("HelpIsExpanded" + GetId(), value);
            }
        }
    }

    public string? UniqueIsExpandedSettingsId { get; set; }

    public Bitmap? Image1
    {
        get => _image1;
        set
        {
            _image1 = value;
            UpdateImageDisplay();
        }
    }

    public Bitmap? Image2
    {
        get => _image2;
        set
        {
            _image2 = value;
            UpdateImageDisplay();
        }
    }

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

    public string IsOnHoverShowImage2NoticeText
    {
        get => labelHoverText.Text ?? string.Empty;
        set => labelHoverText.Text = value;
    }

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

    private string GetId() => UniqueIsExpandedSettingsId ?? "MUST_BE_SET";

    private void UpdateIsExpandedState()
    {
        linkLabelHide.IsVisible = _isExpanded;
        buttonShowHelp.IsVisible = !_isExpanded;
        pictureBox1.IsVisible = _isExpanded;
        labelHoverText.IsVisible = _isExpanded && IsOnHoverShowImage2;
        MinWidth = _isExpanded ? 289 : 30;
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
