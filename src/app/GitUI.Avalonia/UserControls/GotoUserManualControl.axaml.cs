using Avalonia.Controls;
using GitCommands;
using ResourceManager;

namespace GitUI.UserControls;

public sealed partial class GotoUserManualControl : GitExtensionsControl
{
    private readonly TranslationString _gotoUserManualControlTooltip =
        new("Read more about this feature at {0}");
    private string _manualSectionAnchorName = string.Empty;
    private string _manualSectionSubfolder = string.Empty;

    public GotoUserManualControl()
    {
        InitializeComponent();
        linkLabelHelp.Click += linkLabelHelp_LinkClicked;
        InitializeComplete();
    }

    public string ManualSectionAnchorName
    {
        get => _manualSectionAnchorName;
        set
        {
            _manualSectionAnchorName = value;
            UpdateTooltip();
        }
    }

    public string ManualSectionSubfolder
    {
        get => _manualSectionSubfolder;
        set
        {
            _manualSectionSubfolder = value;
            UpdateTooltip();
        }
    }

    private void UpdateTooltip()
    {
        if (string.IsNullOrEmpty(ManualSectionAnchorName) || string.IsNullOrEmpty(ManualSectionSubfolder))
        {
            return;
        }

        string url;
        try
        {
            url = GetUrl();
        }
        catch (InvalidOperationException)
        {
            // The designer and construction tests do not run the application startup that
            // initializes the documentation base URL.
            return;
        }

        string caption = string.Format(_gotoUserManualControlTooltip.Text, url);
        ToolTip.SetTip(pictureBoxHelpIcon, caption);
        ToolTip.SetTip(linkLabelHelp, caption);
    }

    private void OpenManual()
    {
        OsShellUtil.OpenUrlInDefaultBrowser(GetUrl());
    }

    private string GetUrl()
    {
        return UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
    }

    private void linkLabelHelp_LinkClicked(object? sender, EventArgs e)
    {
        OpenManual();
    }
}
