using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GitCommands;
using ResourceManager.Translation;
using System.IO;

namespace GitUI
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

            int x = 0;
            int y = 0;
            int imageHeight = 47;
            int imageWidth = 96;
            List<string> translations = new List<string>(Translator.GetAllTranslations());
            translations.Add("English");
            translations.Sort();

            foreach (string translation in translations)
            {
                PictureBox translationImage = new PictureBox();
                translationImage.Top = y + 34;
                translationImage.Left = x + 15;
                translationImage.Height = imageHeight;
                translationImage.Width = imageWidth;
                translationImage.BackgroundImageLayout = ImageLayout.Stretch;
                if (File.Exists(Translator.GetTranslationDir() + Settings.PathSeparator + translation + ".gif"))
                    translationImage.BackgroundImage = Bitmap.FromFile(Translator.GetTranslationDir() + Settings.PathSeparator + translation + ".gif");
                else
                    translationImage.BackColor = Color.Black;

                translationImage.Cursor = Cursors.Hand;
                translationImage.Tag = translation;
                translationImage.Click += new EventHandler(translationImage_Click);
                
                new ToolTip().SetToolTip(translationImage, translation);

                this.Controls.Add(translationImage);

                x += imageWidth + 6;
                if (x > imageWidth * 4)
                {
                    x = 0;
                    y += imageHeight + 6;
                }
            }

            this.Height = 34 + y + imageHeight + SystemInformation.CaptionHeight + 37;
            label2.Top = this.Height - SystemInformation.CaptionHeight - 25;
        }

        void translationImage_Click(object sender, EventArgs e)
        {
            Settings.Translation = ((PictureBox)sender).Tag.ToString();
            Close();
        }

        private void FormChooseTranslation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Translation))
                Settings.Translation = "English";
        }
    }
}
