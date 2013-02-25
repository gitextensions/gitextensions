using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;

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
            var translations = new List<string>(Translator.GetAllTranslations()) {"English"};
            translations.Sort();

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
                if (File.Exists(Translator.GetTranslationDir() + Settings.PathSeparator + translation + ".gif"))
                    translationImage.BackgroundImage = Image.FromFile(Translator.GetTranslationDir() + Settings.PathSeparator + translation + ".gif");
                else
                    translationImage.BackColor = Color.Black;

                translationImage.Cursor = Cursors.Hand;
                translationImage.Tag = translation;
                translationImage.Click += translationImage_Click;

                Controls.Add(translationImage);

                Label label = new Label();
                label.Text = translation;
                label.Tag = translation;
                label.Left = translationImage.Left;
                label.Width = translationImage.Width;
                label.Top = translationImage.Bottom;
                label.Height = labelHeight;
                label.TextAlign = ContentAlignment.TopCenter;
                label.Click += translationImage_Click;
                Controls.Add(label);
            }

            Height = 34 + y + imageHeight + labelHeight + SystemInformation.CaptionHeight + 37;
            Width = (imageWidth + 6) * 4 + 24;
            label2.Top = Height - SystemInformation.CaptionHeight - 25;
        }

        void translationImage_Click(object sender, EventArgs e)
        {
            Settings.Translation = ((Control)sender).Tag.ToString();
            Close();
        }

        private void FormChooseTranslation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Translation))
                Settings.Translation = "English";
        }
    }
}
