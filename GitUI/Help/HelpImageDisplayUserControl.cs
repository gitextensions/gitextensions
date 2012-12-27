using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GitUI.Help
{
    public partial class HelpImageDisplayUserControl : GitExtensionsControl
    {
        private Image _image1;
        private Image _image2;

        public HelpImageDisplayUserControl()
        {
            InitializeComponent();
            Translate();
        }

        private void HelpImageDisplayUserControl_Load(object sender, EventArgs e)
        {
            ShowImage2HoverText = "Hover to see scenario when fast forward is possible.";
            UpdateImageDisplay();
        }

        public Image Image1
        {
            get { return _image1; }
            set
            {
                _image1 = value;
                UpdateImageDisplay();
                UpdateControlSize();
            }
        }

        public Image Image2
        {
            get { return _image2; }
            set
            {
                _image2 = value;
                UpdateImageDisplay();
                UpdateControlSize();
            }
        }

        public bool ShowImage2OnHover
        {
            get { return _showImage2OnHover; }
            set
            {
                _showImage2OnHover = value;
                labelHoverText.Visible = value;
                UpdateImageDisplay();
            }
        }

        public string ShowImage2HoverText { get { return labelHoverText.Text; } set { labelHoverText.Text = value; } }

        private bool _isHover;
        private bool _showImage2OnHover;

        private bool IsHovering()
        {
            return _isHover;
        }

        private void UpdateControlSize()
        {
            if (_image1 != null && _image2 == null)
            {
                Size = Image1.Size;
            }
            else if (_image1 != null && Image2 != null)
            {
                Size = Image1.Size;
                // TODO: more cases (e. g. max(Image1 and Image2))
            }
           
            // TODO: null and null case
        }

        private void UpdateImageDisplay()
        {
            if (!ShowImage2OnHover)
            {
                pictureBox1.Image = Image1;
                return;
            }

            bool isHover = IsHovering();

            if (isHover)
            {
                pictureBox1.Image = Image2;
            }
            else
            {
                pictureBox1.Image = Image1;
            }
        }

        private void HelpImageDisplayUserControl_MouseEnter(object sender, EventArgs e)
        {
            if (ShowImage2OnHover)
            {
                _isHover = true;
                UpdateImageDisplay();
            }
        }

        private void HelpImageDisplayUserControl_MouseLeave(object sender, EventArgs e)
        {
            if (ShowImage2OnHover)
            {
                _isHover = false;
                UpdateImageDisplay();
            }
        }
    }
}
