using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public class FormContributors : GitExtensionsForm
    {
        private static readonly string[] tabCaptions = { "The Coders", "The Translators", "The Designers" };

        private readonly TextBox[] _textboxes = new TextBox[tabCaptions.Length];
        private readonly TabPage[] _tabPages = new TabPage[tabCaptions.Length];
        private TabControl _tabControl;

        public FormContributors()
        {
            SetupForm();
            Translate();
            this.AdjustForDpiScaling();
        }

        private static TextBox GetNewTextBox()
        {
            return new TextBox
            {
                BackColor = Color.White,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                Margin = new Padding(0),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                TabStop = false
            };
        }

        private static TabPage GetNewTabPage(TextBox tb, string caption)
        {
            var tp = new TabPage
            {
                Margin = new Padding(0),
                Text = caption
            };
            tp.Controls.Add(tb);
            return tp;
        }

        private static TabControl GetNewTabControl()
        {
            return new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
                SelectedIndex = 0,
            };
        }

        private void SetupForm()
        {
            SuspendLayout();
            Controls.Clear();

            _tabControl = GetNewTabControl();
            _tabControl.SuspendLayout();

            for (int i = 0; i < tabCaptions.Length; i++)
            {
                _textboxes[i] = GetNewTextBox();
                _tabPages[i] = GetNewTabPage(_textboxes[i], tabCaptions[i]);
                _tabControl.Controls.Add(_tabPages[i]);
            }

            Controls.Add(_tabControl);

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(624, 442);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Thanks to...";

            ResumeLayout(false);
        }

        public void LoadContributors(string coders, string translators, string designers)
        {
            const string NEWLINES = @"\r\n?|\n";
            _textboxes[0].Text = Regex.Replace(coders, NEWLINES, " ");
            _textboxes[1].Text = Regex.Replace(translators, NEWLINES, " ");
            _textboxes[2].Text = Regex.Replace(designers, NEWLINES, " ");
        }
    }
}
