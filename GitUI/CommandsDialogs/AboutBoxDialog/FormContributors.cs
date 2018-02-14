using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public class FormContributors : GitExtensionsForm
    {
        private static readonly string[] tabCaptions = { "The Coders", "The Translators", "The Designers" };

        private readonly TextBox[] textboxes = new TextBox[tabCaptions.Length];
        private readonly TabPage[] tabPages = new TabPage[tabCaptions.Length];
        private TabControl tabControl;

        public FormContributors()
        {
            SetupForm();
            Translate();
        }

        private static TextBox getNewTextBox()
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

        private static TabPage getNewTabPage(TextBox tb, string caption)
        {
            var tp = new TabPage
            {
                Margin = new Padding(0),
                Text = caption
            };
            tp.Controls.Add(tb);
            return tp;
        }

        private static TabControl getNewTabControl()
        {
            return new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ItemSize = new Size(150, 26),
                Margin = new Padding(0),
                Padding = new Point(0, 0),
                SelectedIndex = 0,
                SizeMode = TabSizeMode.Fixed
            };
        }

        private void SetupForm()
        {
            SuspendLayout();
            Controls.Clear();

            tabControl = getNewTabControl();
            tabControl.SuspendLayout();

            for (int i = 0; i < tabCaptions.Length; i++)
            {
                textboxes[i] = getNewTextBox();
                tabPages[i] = getNewTabPage(textboxes[i], tabCaptions[i]);
                tabControl.Controls.Add(tabPages[i]);
            }

            Controls.Add(tabControl);

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

        public void LoadContributors(string coders, string translators, string designers, string others)
        {
            const string NEWLINES = @"\r\n?|\n";
            textboxes[0].Text = Regex.Replace(coders, NEWLINES, " ");
            textboxes[1].Text = Regex.Replace(translators, NEWLINES, " ");
            textboxes[2].Text = Regex.Replace(designers, NEWLINES, " ");
        }
    }
}
