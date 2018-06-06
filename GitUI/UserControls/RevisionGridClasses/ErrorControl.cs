using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.UserControls.RevisionGridClasses
{
    public sealed class ErrorControl : UserControl
    {
        private PictureBox _image;

        public ErrorControl()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            _image = new PictureBox();
            ((ISupportInitialize)_image).BeginInit();
            SuspendLayout();
            _image.Dock = DockStyle.Fill;
            _image.Image = Properties.Resources.error;
            _image.Location = new System.Drawing.Point(0, 0);
            _image.Name = "_image";
            _image.Size = new System.Drawing.Size(513, 260);
            _image.SizeMode = PictureBoxSizeMode.CenterImage;
            _image.TabIndex = 3;
            _image.TabStop = false;
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(_image);
            Name = "ErrorControl";
            Size = new System.Drawing.Size(513, 260);
            ((ISupportInitialize)_image).EndInit();
            ResumeLayout(performLayout: false);
        }
    }
}
