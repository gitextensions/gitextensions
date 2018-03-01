using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public class FormContributors : GitExtensionsForm
    {

        private readonly static string[] tabCaptions = new string[] {
            "The Coders", "The Translators", "The Designers"
        };

        private readonly TextBox[] textboxes = new TextBox[tabCaptions.Length];
        private readonly TabPage[] tabPages = new TabPage[tabCaptions.Length];
        private TabControl tabControl;

        public FormContributors()
        {
            SetupForm();
            Translate();
        }

        private TextBox getNewTextBox()
        {
            TextBox tb = new TextBox();
            tb.BackColor = Color.White;
            tb.Dock = DockStyle.Fill;
            tb.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            tb.Margin = new System.Windows.Forms.Padding(0);
            tb.Multiline = true;
            tb.ReadOnly = true;
            tb.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            tb.TabStop = false;
            return tb;
        }

        private TabPage getNewTabPage(TextBox tb, string caption)
        {
            TabPage tp = new TabPage();
            tp.Margin = new System.Windows.Forms.Padding(0);
            tp.Text = caption;
            tp.Controls.Add(tb);
            return tp;
        }

        private TabControl getNewTabControl()
        {
            TabControl tc = new TabControl();
            tc.Dock = DockStyle.Fill;
            tc.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));
            tc.ItemSize = new Size(150, 26);
            tc.Margin = new Padding(0);
            tc.Padding = new Point(0, 0);
            tc.SelectedIndex = 0;
            tc.SizeMode = TabSizeMode.Fixed;
            return tc;
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

            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.ClientSize = new Size(624, 442);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Thanks to...";

            this.ResumeLayout(false);

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
