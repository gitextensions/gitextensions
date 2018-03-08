using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormChooseTranslation : GitExtensionsForm
    {
        public FormChooseTranslation()
        {
            InitializeComponent();
            Translate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            const int labelHeight = 20;
            const int imageHeight = 75;
            const int imageWidth = 150;
            int x = -(imageWidth + 6);
            int y = 0;
            var translations = new List<string>(Translator.GetAllTranslations());
            translations.Sort();
            translations.Insert(0, "English");

            foreach (string translation in translations)
            {
                x += imageWidth + 6;
                if (x > imageWidth * 4)
                {
                    x = 0;
                    y += imageHeight + 6 + labelHeight;
                }

                var translationImage = new PictureBox
                {
                    Top = y + 34,
                    Left = x + 15,
                    Height = imageHeight,
                    Width = imageWidth,
                    BackgroundImageLayout = ImageLayout.Stretch
                };
                if (File.Exists(Path.Combine(Translator.GetTranslationDir(), translation + ".gif")))
                {
                    translationImage.BackgroundImage = Image.FromFile(Path.Combine(Translator.GetTranslationDir(), translation + ".gif"));
                }
                else
                {
                    translationImage.BackColor = Color.Black;
                }

                translationImage.Cursor = Cursors.Hand;
                translationImage.Tag = translation;
                translationImage.Click += translationImage_Click;

                Controls.Add(translationImage);

                var label = new Label
                {
                    Text = translation,
                    Tag = translation,
                    Left = translationImage.Left,
                    Width = translationImage.Width,
                    Top = translationImage.Bottom,
                    Height = labelHeight,
                    TextAlign = ContentAlignment.TopCenter
                };
                label.Click += translationImage_Click;
                Controls.Add(label);
            }

            Height = 34 + y + imageHeight + labelHeight + SystemInformation.CaptionHeight + 37;
            Width = ((imageWidth + 6) * 4) + 24;
            label2.Top = Height - SystemInformation.CaptionHeight - 25;
        }

        private void translationImage_Click(object sender, EventArgs e)
        {
            AppSettings.Translation = ((Control)sender).Tag.ToString();
            Close();
        }

        private void FormChooseTranslation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                AppSettings.Translation = "English";
            }
        }
    }
}
