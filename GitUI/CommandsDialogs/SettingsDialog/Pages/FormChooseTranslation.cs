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

            this.label1.Font = FontUtil.MainInstructionFont;
            this.label1.ForeColor = FontUtil.MainInstructionColor;
            Translate();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var translations = new List<string>(Translator.GetAllTranslations());
            translations.Sort();
            translations.Insert(0, "English");

            var imageList = new ImageList
            {
                ImageSize = new Size(150, 75),
            };

            foreach (string translation in translations)
            {
                var imagePath = Path.Combine(Translator.GetTranslationDir(), translation + ".gif");
                if (File.Exists(imagePath))
                {
                    var image = Image.FromFile(imagePath);
                    imageList.Images.Add(translation, image);
                }
            }

            this.lvTranslations.LargeImageList = imageList;

            foreach (string translation in translations)
            {
                if (imageList.Images.ContainsKey(translation))
                {
                    this.lvTranslations.Items.Add(new ListViewItem(translation, translation) { Tag = translation });
                }
                if (File.Exists(Path.Combine(Translator.GetTranslationDir(), translation + ".gif")))
                {
                    translationImage.BackgroundImage = Image.FromFile(Path.Combine(Translator.GetTranslationDir(), translation + ".gif"));
                }
                else
                {
                    this.lvTranslations.Items.Add(new ListViewItem(translation) { Tag = translation });
                }

                translationImage.Cursor = Cursors.Hand;
                translationImage.Tag = translation;
                translationImage.Click += translationImage_Click;

                Controls.Add(translationImage);

                    TextAlign = ContentAlignment.TopCenter
                };
                }
                label.Click += translationImage_Click;
                Controls.Add(label);
            }

            Height = 34 + y + imageHeight + labelHeight + SystemInformation.CaptionHeight + 37;
            }
            label2.Top = Height - SystemInformation.CaptionHeight - 25;
        }

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

        private void lvTranslations_ItemActivate(object sender, EventArgs e)
        {
            AppSettings.Translation = ((ListView)sender).SelectedItems[0].Tag.ToString();
            Close();
        }
    }
}
