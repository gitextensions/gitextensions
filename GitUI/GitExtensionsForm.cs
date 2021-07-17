using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitExtUtils.GitUI;
using GitUI.Interops.DwmApi;
using GitUI.Theming;
using ResourceManager;

namespace GitUI
{
    // NOTE do not make this class abstract as it breaks the WinForms designer in VS

    /// <summary>Base class for a Git Extensions <see cref="Form"/>.</summary>
    /// <remarks>Includes support for font, hotkey, icon, translation, and position restore.</remarks>
    public class GitExtensionsForm : GitExtensionsFormBase
    {
        private IWindowPositionManager _windowPositionManager = new WindowPositionManager();
        private Func<IReadOnlyList<Rectangle>> _getScreensWorkingArea = () => Screen.AllScreens.Select(screen => screen.WorkingArea).ToArray();
        private bool _needsPositionRestore;

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

            Button cancelButton = new();
            cancelButton.Click += CancelButtonClick;
            CancelButton = cancelButton;

            if (ThemeModule.IsDarkTheme)
            {
                // Warning: This call freezes the CI in AppVeyor, however dark theme is not used on build machines
                DwmApi.UseImmersiveDarkMode(Handle, true);
            }

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

            if (!IsDesignMode)
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
                // TODO: do we still need to assert when restored it is shown on the correct monitor?
                return;
            }

            var position = _windowPositionManager.LoadPosition(this);
            if (position is null)
            {
                return;
            }

            _needsPositionRestore = false;

            IReadOnlyList<Rectangle> workingArea = _getScreensWorkingArea();
            if (!workingArea.Any(screen => screen.IntersectsWith(position.Rect)))
            {
                if (position.State == FormWindowState.Maximized)
                {
                    WindowState = FormWindowState.Maximized;
                }

                return;
            }

            SuspendLayout();

            var windowCentred = StartPosition == FormStartPosition.CenterParent;
            StartPosition = FormStartPosition.Manual;

            if (FormBorderStyle == FormBorderStyle.Sizable ||
                FormBorderStyle == FormBorderStyle.SizableToolWindow)
            {
                Size = DpiUtil.Scale(position.Rect.Size, originalDpi: position.DeviceDpi);
            }

            if (Owner is null || !windowCentred)
            {
                Point calculatedLocation = DpiUtil.Scale(position.Rect.Location, originalDpi: position.DeviceDpi);

                DesktopLocation = WindowPositionManager.FitWindowOnScreen(new Rectangle(calculatedLocation, Size), workingArea);
            }
            else
            {
                // Calculate location for modal form with parent
                Point calculatedLocation = new(
                    Owner.Left + (Owner.Width / 2) - (Width / 2),
                    Owner.Top + (Owner.Height / 2) - (Height / 2));
                Location = WindowPositionManager.FitWindowOnScreen(new Rectangle(calculatedLocation, Size), workingArea);
            }

            if (WindowState != position.State)
            {
                WindowState = position.State;
            }

            ResumeLayout();
        }

        // This is a base class for many forms, which have own GetTestAccessor() methods. This has to be unique
        internal GitExtensionsFormTestAccessor GetGitExtensionsFormTestAccessor() => new(this);

        internal readonly struct GitExtensionsFormTestAccessor
        {
            private readonly GitExtensionsForm _form;

            public GitExtensionsFormTestAccessor(GitExtensionsForm form)
            {
                _form = form;
            }

            public IWindowPositionManager WindowPositionManager
            {
                get => _form._windowPositionManager;
                set => _form._windowPositionManager = value;
            }

            public Func<IReadOnlyList<Rectangle>> GetScreensWorkingArea
            {
                get => _form._getScreensWorkingArea;
                set => _form._getScreensWorkingArea = value;
            }
        }
    }
}
