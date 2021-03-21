using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public partial class GitExtensionsDialog : GitModuleForm
    {
        private static readonly Pen FooterDividerPen = new(KnownColor.ControlLight.MakeBackgroundDarkerBy(0.04));

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        [Obsolete("For VS designer and translation test only. Do not remove.")]
        protected GitExtensionsDialog()
            : base()
        {
            InitializeComponent();
        }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        protected GitExtensionsDialog(GitUICommands? commands, bool enablePositionRestore)
            : base(commands, enablePositionRestore)
        {
            InitializeComponent();

            // Lighten up the control panel
            ControlsPanel.BackColor = KnownColor.ControlLight.MakeBackgroundDarkerBy(-0.04);

            // Draw a separator line at the top of the footer panel, similar to what Task Dialog does
            ControlsPanel.Paint += (s, e)
                => e.Graphics.DrawLine(FooterDividerPen, new Point(e.ClipRectangle.Left, 0), new Point(e.ClipRectangle.Right, 0));
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
                e.Cancel = true;
                return;
            }

            base.OnHelpButtonClicked(e);

            string url = UserManual.UserManual.UrlFor(ManualSectionSubfolder, ManualSectionAnchorName);
            OsShellUtil.OpenUrlInDefaultBrowser(url);

            // We've handled the event
            e.Cancel = true;
        }
    }
}
