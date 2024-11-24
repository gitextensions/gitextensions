using System.ComponentModel;

namespace GitUI.CommandsDialogs.SettingsDialog
{
    public partial class SimpleHelpDisplayDialog : Form
    {
        public SimpleHelpDisplayDialog()
        {
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? DialogTitle { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? ContentText { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = DialogTitle;
            textBox1.Text = ContentText;
            textBox1.Select(0, 0);
        }
    }
}
