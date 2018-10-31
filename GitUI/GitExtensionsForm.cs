using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public class GitExtensionsForm : GitExtensionsFormBase
    {
        private readonly WindowPositionManager _windowPositionManager = new WindowPositionManager();

        private bool _needsPositionRestore;
        private bool _windowCentred;

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> without position restore.</summary>
        public GitExtensionsForm()
            : this(enablePositionRestore: false)
        {
        }

        /// <summary>Creates a new <see cref="GitExtensionsForm"/> indicating position restore.</summary>
        /// <param name="enablePositionRestore">Indicates whether the <see cref="Form"/>'s position
        /// will be restored upon being re-opened.</param>
        protected GitExtensionsForm(bool enablePositionRestore)
        {
            var needsPositionSave = enablePositionRestore;
            _needsPositionRestore = enablePositionRestore;

            FormClosing += GitExtensionsForm_FormClosing;

            var cancelButton = new Button();
            cancelButton.Click += CancelButtonClick;
            CancelButton = cancelButton;

            void GitExtensionsForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (!needsPositionSave)
                {
                    return;
                }

                needsPositionSave = false;
                _windowPositionManager.SavePosition(this);
                TaskbarProgress.Clear();
            }
        }

        public virtual void CancelButtonClick(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            RestorePosition();

            // Should be called after restoring position
            base.OnLoad(e);

            if (!IsDesignModeActive)
            {
                OnRuntimeLoad(e);
            }
        }

        /// <summary>Invoked at runtime during the <see cref="OnLoad"/> method.</summary>
        /// <remarks>In particular, this method is not invoked when running in a designer.</remarks>
        protected virtual void OnRuntimeLoad(EventArgs e)
        {
        }

        /// <summary>
        ///   Restores the position of a form from the user settings. Does
        ///   nothing if there is no entry for the form in the settings, or the
        ///   setting would be invisible on the current display configuration.
        /// </summary>
        protected virtual void RestorePosition()
        {
            if (!_needsPositionRestore)
            {
                return;
            }

            if (WindowState == FormWindowState.Minimized)
            {
                return;
            }

            _windowCentred = StartPosition == FormStartPosition.CenterParent;

            var position = _windowPositionManager.LoadPosition(this);
            if (position == null)
            {
                return;
            }

            _needsPositionRestore = false;

            if (!Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(position.Rect)))
            {
                if (position.State == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                return;
            }

            SuspendLayout();

            StartPosition = FormStartPosition.Manual;

            if (FormBorderStyle == FormBorderStyle.Sizable ||
                FormBorderStyle == FormBorderStyle.SizableToolWindow)
            {
                Size = DpiUtil.Scale(position.Rect.Size, originalDpi: position.DeviceDpi);
            }

            if (Owner == null || !_windowCentred)
            {
                var location = DpiUtil.Scale(position.Rect.Location, originalDpi: position.DeviceDpi);

                if (WindowPositionManager.FindWindowScreen(location, Screen.AllScreens.Select(screen => screen.WorkingArea)) is Rectangle rect)
                {
                    location.Y = rect.Y;
                }

                DesktopLocation = location;
            }
            else
            {
                // Calculate location for modal form with parent
                Location = new Point(
                    Owner.Left + (Owner.Width / 2) - (Width / 2),
                    Math.Max(0, Owner.Top + (Owner.Height / 2) - (Height / 2)));
            }

            if (WindowState != position.State)
            {
                WindowState = position.State;
            }

            ResumeLayout();
        }
    }
}
