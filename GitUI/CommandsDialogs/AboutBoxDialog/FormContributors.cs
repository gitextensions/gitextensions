using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using GitUI.Properties;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public sealed class FormContributors : GitExtensionsForm
    {
        private static readonly string[] tabCaptions = { "The Coders", "The Translators", "The Designers" };

        private readonly TextBox[] _textBoxes = new TextBox[tabCaptions.Length];
        private readonly TabPage[] _tabPages = new TabPage[tabCaptions.Length];

        public FormContributors()
        {
            InitialiseComponent();
            InitializeComplete();

            void InitialiseComponent()
            {
                SuspendLayout();
                Controls.Clear();

                var tabControl = GetNewTabControl();

                for (var i = 0; i < tabCaptions.Length; i++)
                {
                    _textBoxes[i] = GetNewTextBox();
                    _tabPages[i] = GetNewTabPage(_textBoxes[i], tabCaptions[i]);
                }

                const string NEWLINES = @"\r\n?|\n";
                _textBoxes[0].Text = Regex.Replace(Resources.Coders, NEWLINES, " ");
                _textBoxes[1].Text = Regex.Replace(Resources.Translators, NEWLINES, " ");
                _textBoxes[2].Text = Regex.Replace(Resources.Designers, NEWLINES, " ");

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

                return;

                TextBox GetNewTextBox()
                {
                    return new TextBox
                    {
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.None,
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                        Margin = new Padding(0),
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        TabStop = false
                    };
                }

                TabPage GetNewTabPage(TextBox textBox, string caption)
                {
                    var tabPage = new TabPage
                    {
                        BorderStyle = BorderStyle.None,
                        Margin = new Padding(0),
                        Padding = new Padding(0),
                        Text = caption
                    };
                    tabPage.Controls.Add(textBox);
                    tabControl.Controls.Add(tabPage);
                    return tabPage;
                }

                TabControl GetNewTabControl()
                {
                    return new FullBleedTabControl
                    {
                        Dock = DockStyle.Fill,
                        Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, 0),
                        SelectedIndex = 0,
                    };
                }
            }
        }
    }
}
