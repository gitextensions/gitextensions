using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUI.UserControls.Settings
{
    public partial class SettingsCheckBox : UserControl
    {
        private string _toolTipText;
        private ToolTip _tooltip;

        public SettingsCheckBox()
        {
            InitializeComponent();
        }

        public bool Checked
        {
            get => checkBox.Checked;
            set => checkBox.Checked = value;
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text
        {
            get => checkBox.Text;
            set => checkBox.Text = value;
        }

        public string ToolTipText
        {
            get => _toolTipText;
            set
            {
                _toolTipText = value;
                _tooltip ??= new ToolTip();
                _tooltip.SetToolTip(checkBox, _toolTipText);
                _tooltip.SetToolTip(pictureBox, _toolTipText);
                pictureBox.Visible = !string.IsNullOrEmpty(_toolTipText);
            }
        }

        public event EventHandler InfoClicked
        {
            add { pictureBox.Click += value; }
            remove { pictureBox.Click -= value; }
        }

        public event EventHandler CheckedChanged
        {
            add { checkBox.CheckedChanged += value; }
            remove { checkBox.CheckedChanged -= value; }
        }
    }
}
