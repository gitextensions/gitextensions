using System.ComponentModel;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitUI.Properties;
using ResourceManager;

namespace GitUI;

/// <summary>Base class for a Git Extensions <see cref="Avalonia.Controls.Window"/>.</summary>
/// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
public class GitExtensionsDialog : GitModuleForm
{
    // Avalonia windows do not inherit visual trees, so each dialog's .axaml declares the
    // MainPanel/ControlsPanel chrome supplied by the WinForms base Designer.

    private Button? _NO_TRANSLATE_HelpButton;

    /// <summary>For the visual designer and construction tests only, like WinForms.</summary>
    protected GitExtensionsDialog()
    {
    }

    /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
    /// <param name="enablePositionRestore">Indicates whether the <see cref="Avalonia.Controls.Window"/>'s position
    /// will be restored upon being re-opened.</param>
    protected GitExtensionsDialog(IGitUICommands? commands, bool enablePositionRestore)
        : base(commands, enablePositionRestore)
    {
    }

    /// <summary>
    /// Gets or sets the anchor pointing to a section in the manual pertaining to this dialog.
    /// </summary>
    /// <remarks>
    /// The URL structure:
    /// https://git-extensions-documentation.readthedocs.io/{ManualSectionSubfolder}.html#{ManualSectionAnchorName}.
    /// </remarks>
    public string? ManualSectionAnchorName { get; set; }

    /// <summary>
    /// Gets or sets the name of a document pertaining to this dialog.
    /// </summary>
    /// <remarks>
    /// The URL structure:
    /// https://git-extensions-documentation.readthedocs.io/{ManualSectionSubfolder}.html#{ManualSectionAnchorName}.
    /// </remarks>
    public string? ManualSectionSubfolder { get; set; }

    protected override void OnHelpButtonClicked(CancelEventArgs e)
    {
        // If we show the Help button but we have failed to specify where the docs are -> hide the button, and exit
        if (string.IsNullOrWhiteSpace(ManualSectionAnchorName) || string.IsNullOrWhiteSpace(ManualSectionSubfolder))
        {
            HelpButton = false;
            if (_NO_TRANSLATE_HelpButton is not null)
            {
                _NO_TRANSLATE_HelpButton.IsVisible = false;
            }

            e.Cancel = true;
            return;
        }

        base.OnHelpButtonClicked(e);

        string url = UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
        OsShellUtil.OpenUrlInDefaultBrowser(url);

        // We've handled the event
        e.Cancel = true;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        if (!HelpButton || _NO_TRANSLATE_HelpButton is not null || this.FindControl<Control>("ControlsPanel") is not { } footer)
        {
            return;
        }

        Panel? host = footer switch
        {
            Panel panel => panel,
            Decorator { Child: Panel panel } => panel,
            _ => null,
        };
        if (host is null)
        {
            return;
        }

        // Avalonia has no caption Help button, so expose the same action in the footer and through F1.
        string caption = "Help";
        foreach (ITranslation translation in Translator.GetTranslation(AppSettings.CurrentTranslation).Values)
        {
            caption = translation.TranslateItem("GotoUserManualControl", "linkLabelHelp", "Text", () => caption) ?? caption;
        }

        _NO_TRANSLATE_HelpButton = new Button
        {
            Name = "HelpButton",
            Classes = { "gitextensions-toolbar-button", "gitextensions-icon-only" },
            Width = 25,
            Height = 25,
            MinWidth = 0,
            MinHeight = 0,
            Margin = new Thickness(8),
            Padding = default,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Content = new Image { Source = Images.GotoManual, Width = 16, Height = 16 },
        };
        AutomationProperties.SetName(_NO_TRANSLATE_HelpButton, caption);
        ToolTip.SetTip(_NO_TRANSLATE_HelpButton, caption + " (F1)");
        _NO_TRANSLATE_HelpButton.Click += (_, _) => OnHelpButtonClicked(new CancelEventArgs());
        if (host is Grid grid)
        {
            Grid.SetColumnSpan(_NO_TRANSLATE_HelpButton, Math.Max(1, grid.ColumnDefinitions.Count));
        }

        host.Children.Add(_NO_TRANSLATE_HelpButton);
    }
}
