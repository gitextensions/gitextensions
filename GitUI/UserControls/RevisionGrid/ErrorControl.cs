using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class ErrorControl : UserControl
    {
        private PictureBox _image;

        public ErrorControl()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            this.AdjustForDpiScaling();
        }

#pragma warning disable
        private void InitializeComponent()
        {
            this._image = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
            this.SuspendLayout();
            //
            // _image
            //
            this._image.Dock = System.Windows.Forms.DockStyle.Fill;
            this._image.Image = global::GitUI.Properties.Images.StatusBadgeError;
            this._image.Location = new System.Drawing.Point(0, 0);
            this._image.Name = "_image";
            this._image.Size = new System.Drawing.Size(2080, 1447);
            this._image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._image.TabIndex = 3;
            this._image.TabStop = false;
            //
            // ErrorControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._image);
            this.Name = "ErrorControl";
            this.Size = new System.Drawing.Size(2080, 1447);
            ((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
            this.ResumeLayout(false);
        }
#pragma warning enable
    }
}
