using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI.UserControls.RevisionGrid
{
    public sealed class LoadingControl : UserControl
    {
        private PictureBox _image;

        public LoadingControl()
        {
            InitializeComponent();

            _image.Image = Images.LoadingAnimation;
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
            this._image.BackColor = System.Drawing.SystemColors.AppWorkspace;
            this._image.Dock = System.Windows.Forms.DockStyle.Fill;
            this._image.Location = new System.Drawing.Point(0, 0);
            this._image.Name = "_image";
            this._image.Size = new System.Drawing.Size(811, 240);
            this._image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this._image.TabIndex = 0;
            this._image.TabStop = false;
            //
            // LoadingControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._image);
            this.Name = "LoadingControl";
            this.Size = new System.Drawing.Size(811, 240);
            ((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
            this.ResumeLayout(false);

        }
#pragma warning restore
    }
}
