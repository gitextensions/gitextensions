﻿using System.Text.RegularExpressions;
using GitUI.Properties;
using ResourceManager;

namespace GitUI.CommandsDialogs.AboutBoxDialog
{
    public sealed class FormContributors : GitExtensionsForm
    {
        private readonly TranslationString _developers = new("Developers");
        private readonly TranslationString _translators = new("Translators");
        private readonly TranslationString _designers = new("Designers");
        private readonly TranslationString _team = new("Team");
        private readonly TranslationString _contributors = new("Contributors");
        private readonly TranslationString _caption = new("The application would not be possible without...");

        public FormContributors()
        {
            InitialiseComponent();
            InitializeComplete();

            void InitialiseComponent()
            {
                SuspendLayout();
                Controls.Clear();

                var tabControl = GetNewTabControl();

                var tabCaptions = new[] { _developers.Text, _translators.Text, _designers.Text };
                var textBoxes = new TextBox[tabCaptions.Length];
                var tabPages = new TabPage[tabCaptions.Length];
                for (var i = 0; i < tabCaptions.Length; i++)
                {
                    textBoxes[i] = GetNewTextBox();
                    tabPages[i] = GetNewTabPage(textBoxes[i], tabCaptions[i]);
                }

                const string NEWLINES = @"\r\n?|\n";
                textBoxes[0].Text = string.Format("{0}:\r\n{1}\r\n\r\n{2}:\r\n{3}",
                    _team.Text, Regex.Replace(Resources.Team, NEWLINES, " "),
                    _contributors.Text, Regex.Replace(Resources.Coders, NEWLINES, " "));
                textBoxes[1].Text = Regex.Replace(Resources.Translators, NEWLINES, " ");
                textBoxes[2].Text = Regex.Replace(Resources.Designers, NEWLINES, " ");

                Controls.Add(tabControl);

                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;
                ClientSize = new Size(624, 442);
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                StartPosition = FormStartPosition.CenterParent;
                Text = _caption.Text;

                ResumeLayout(false);

                return;

                TextBox GetNewTextBox()
                {
                    return new TextBox
                    {
                        BackColor = SystemColors.Window,
                        ForeColor = SystemColors.WindowText,
                        BorderStyle = BorderStyle.None,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0),
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        TabStop = false
                    };
                }

                TabPage GetNewTabPage(TextBox textBox, string caption)
                {
                    TabPage tabPage = new()
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
                        SelectedIndex = 0,
                    };
                }
            }
        }
    }
}
