using System.ComponentModel;
using GitCommands;

namespace GitUI.UserControls.Settings
{
    public partial class SettingsCheckBox : UserControl
    {
        private string? _helpTopic;
        private string? _toolTipText;
        private ToolTipIcon _toolTipIcon;
        private ToolTip? _tooltip;

        public SettingsCheckBox()
        {
            InitializeComponent();

            pictureBox.Click += (_, _) =>
            {
                if (HelpTopic is null)
                {
                    return;
                }

                OsShellUtil.OpenUrlInDefaultBrowser($"https://git-extensions-documentation.readthedocs.io/settings.html#{HelpTopic}");
            };
        }

        public bool Checked
        {
            get => checkBox.Checked;
            set => checkBox.Checked = value;
        }

        public string? HelpTopic
        {
            get => _helpTopic;
            set
            {
                _helpTopic = value;
                pictureBox.Cursor = _helpTopic is null ? Cursors.Default : Cursors.Hand;
            }
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

        public string? ToolTipText
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

        public ToolTipIcon ToolTipIcon
        {
            get => _toolTipIcon;
            set
            {
                _toolTipIcon = value;
                pictureBox.Image = _toolTipIcon switch
                {
                    ToolTipIcon.Warning => Properties.Resources.Warning,
                    ToolTipIcon.Information => Properties.Resources.information,
                    _ => throw new NotImplementedException(),
                };
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
