using System;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.Help
{
    public partial class HelpImageDisplayUserControl : GitExtensionsControl
    {
        private Image _image1;
        private Image _image2;
        private bool _isExpanded;

        ////public const string fastForwardHoverText = "Hover to see scenario when fast forward is possible.";

        private bool _isLoaded;

        public HelpImageDisplayUserControl()
        {
            InitializeComponent();
            Translate();
        }

        /// <summary>
        /// NOTE: will also be called if designer code calls "this.helpImageDisplayUserControl1.ShowImage2OnHover = true;"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpImageDisplayUserControl_Load(object sender, EventArgs e)
        {
            IsExpanded = LoadIsExpandedValueFromSettings(IsExpanded);
            UpdateIsExpandedState();
            UpdateImageDisplay();
            UpdateControlSize();

            _isLoaded = true;
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                UpdateIsExpandedState();

                if (_isLoaded) // to avoid calling this when InitializeComponents is called
                {
                    /*
                     * ...
                                this.helpImageDisplayUserControl1.IsExpanded = false;                       // this before...
                                this.helpImageDisplayUserControl1.Location = new System.Drawing.Point(3, 3);
                                this.helpImageDisplayUserControl1.MinimumSize = new System.Drawing.Size(30, 50);
                                this.helpImageDisplayUserControl1.Name = "helpImageDisplayUserControl1";    // ...this gives wrong id!!!
                     * ...

                     */

                    SaveIsExpandedValueInSettings(value);
                }                
            }
        }

        public string UniqueIsExpandedSettingsId { get; set; }

        private void UpdateIsExpandedState()
        {
            if (_isExpanded)
            {
                linkLabelHide.Visible = true;

                buttonShowHelp.Visible = false;
                //// linkLabelShowHelp.Visible = false; // Why use button instead of label? Because button has constant width independent of language!

                pictureBox1.Visible = true;
                labelHoverText.Visible = IsOnHoverShowImage2;
            }
            else
            {
                linkLabelHide.Visible = false;

                buttonShowHelp.Visible = true;
                ////linkLabelShowHelp.Visible = true;

                pictureBox1.Visible = false;
                labelHoverText.Visible = false;
            }

            UpdateControlSize();
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

        /// <summary>
        /// see also IsOnHoverShowImage2NoticeText
        /// </summary>
        public bool IsOnHoverShowImage2
        {
            get { return _showImage2OnHover; }
            set
            {
                _showImage2OnHover = value;
                if (_isLoaded)
                {
                    UpdateIsExpandedState();
                    ////labelHoverText.Visible = value; // NOTE: would trigger OnLoad
                }
            }
        }

        /// <summary>
        /// only shown when IsOnHoverShowImage2 is true
        /// </summary>
        public string IsOnHoverShowImage2NoticeText { get { return labelHoverText.Text; } set { labelHoverText.Text = value; } }

        private bool _isHover;
        private bool _showImage2OnHover;

        private bool IsHovering()
        {
            return _isHover;
        }

        private string GetId()
        {
            return UniqueIsExpandedSettingsId ?? "MUST_BE_SET";
        }

        private void SaveIsExpandedValueInSettings(bool value)
        {
            Settings.SetBool("HelpIsExpanded" + GetId(), value);
        }

        private bool LoadIsExpandedValueFromSettings(bool defaultValue)
        {
            return Settings.GetBool("HelpIsExpanded" + GetId(), defaultValue);
        }

        private void UpdateControlSize()
        {
            var size = new Size(40, 40); // default size

            if (IsExpanded)
            {
                if (_image1 != null && _image2 == null)
                {
                    size = Image1.Size;
                }
                else if (_image1 != null && Image2 != null)
                {
                    int w = Math.Max(_image1.Size.Width, _image2.Width);
                    int h = Math.Max(_image1.Size.Height, _image2.Height);
                    size = new Size(w, h);
                }

                // add vertical space of other controls
                size.Height +=
                    /*labelHoverText.Height too much because when collapsed there is much wordwrap, use fixed value 30*/ 30
                    + flowLayoutPanel1.Height;
            }
            else
            {
                size = new Size(30, 50); // size for "show help" button
            }

            // apply size to control
            Size = size;
            MinimumSize = size;
        }

        private void UpdateImageDisplay()
        {
            if (!IsOnHoverShowImage2)
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
            if (IsOnHoverShowImage2)
            {
                _isHover = true;
                UpdateImageDisplay();
            }
        }

        private void HelpImageDisplayUserControl_MouseLeave(object sender, EventArgs e)
        {
            if (IsOnHoverShowImage2)
            {
                _isHover = false;
                UpdateImageDisplay();
            }
        }

        private void linkLabelHide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IsExpanded = false;
        }

        private void buttonShowHelp_Click(object sender, EventArgs e)
        {
            IsExpanded = true;
        }

        private void linkLabelShowHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            IsExpanded = true;
        }
    }
}
