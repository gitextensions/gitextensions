using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using JetBrains.Annotations;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public partial class GitExtensionsDialog : GitModuleForm
    {
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
        protected GitExtensionsDialog([NotNull] GitUICommands commands, bool enablePositionRestore)
            : base(commands, enablePositionRestore)
        {
            InitializeComponent();
        }
    }
}
