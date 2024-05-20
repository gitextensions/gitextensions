using System.Drawing;
using System.Windows.Forms;

namespace GitExtensions.Plugins.GitImpact
{
    partial class ImpactControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components is not null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();

            _scrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                LargeChange = 0,
                Location = new Point(0, 133),
                Maximum = 0,
                Name = "_scrollBar",
                SmallChange = 0,
                TabIndex = 0
            };
            _scrollBar.Scroll += OnScroll;

            Controls.Add(_scrollBar);
            Name = "ImpactControl";
            Paint += OnPaint;
            Resize += OnResize;
            ResumeLayout(false);
        }

        #endregion

        private HScrollBar _scrollBar;
    }
}
