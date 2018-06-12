using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public sealed class FormContributors : GitExtensionsForm
    {
        private static readonly string[] tabCaptions = { "The Coders", "The Translators", "The Designers" };

        private readonly TextBox[] _textBoxes = new TextBox[tabCaptions.Length];
        private readonly TabPage[] _tabPages = new TabPage[tabCaptions.Length];

        public FormContributors()
        {
            SetupForm();
            Translate();
            this.AdjustForDpiScaling();

            void SetupForm()
            {
                SuspendLayout();
                Controls.Clear();

                var tabControl = GetNewTabControl();
                ////tabControl.SuspendLayout();

                for (var i = 0; i < tabCaptions.Length; i++)
                {
                    _textBoxes[i] = GetNewTextBox();
                    _tabPages[i] = GetNewTabPage(_textBoxes[i], tabCaptions[i]);
                    tabControl.Controls.Add(_tabPages[i]);
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

                TextBox GetNewTextBox()
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

                TabPage GetNewTabPage(TextBox tb, string caption)
                {
                    var tp = new TabPage
                    {
                        Margin = new Padding(0),
                        Text = caption
                    };
                    tp.Controls.Add(tb);
                    return tp;
                }

                TabControl GetNewTabControl()
                {
                    return new TabControl
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        SelectedIndex = 0,
                    };
                }
            }
        }

        public void LoadContributors(string coders, string translators, string designers)
        {
            const string NEWLINES = @"\r\n?|\n";

            _textBoxes[0].Text = Regex.Replace(coders, NEWLINES, " ");
            _textBoxes[1].Text = Regex.Replace(translators, NEWLINES, " ");
            _textBoxes[2].Text = Regex.Replace(designers, NEWLINES, " ");
        }
    }
}
