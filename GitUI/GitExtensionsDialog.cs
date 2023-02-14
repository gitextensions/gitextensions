using System.ComponentModel;
using GitCommands;
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

        /// <summary>
        ///  <para>Gets the minimum height preferred by the form.</para>
        ///  <para>
        ///   <b>NOTE:</b> This requires that the <see cref="MainPanel"/> content is configured as follows:<br />
        ///   - <c>AutoSize = true</c><br />
        ///   - <c>AutoSizeMode = AutoSizeMode.GrowAndShrink</c><br />
        ///   - <c>Dock = DockStyle.Fil</c>
        ///  </para>
        /// </summary>
        public int PreferredMinimumHeight
            => MainPanel.PreferredSize.Height + ControlsPanel.PreferredSize.Height + (Size.Height - ClientSize.Height);

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
