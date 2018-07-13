using System.Drawing;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class LoadingControl : UserControl
    {
        private readonly WaitSpinner _waitSpinner;

        public LoadingControl()
        {
            var size = new Size(32, 32);

            _waitSpinner = new WaitSpinner
            {
                Dock = DockStyle.Fill,
                Size = size
            };

            SuspendLayout();
            Dock = DockStyle.Fill;
            AutoScaleDimensions = new SizeF(96, 96);
            AutoScaleMode = AutoScaleMode.Dpi;
            Controls.Add(_waitSpinner);
            Name = nameof(LoadingControl);
            Size = size;
            ResumeLayout(performLayout: false);

            this.AdjustForDpiScaling();
        }

        public bool IsAnimating
        {
            get => _waitSpinner.IsAnimating;
            set => _waitSpinner.IsAnimating = value;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _waitSpinner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
