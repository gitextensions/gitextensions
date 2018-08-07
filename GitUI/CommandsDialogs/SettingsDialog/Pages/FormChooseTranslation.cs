using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using GitCommands;
using GitExtUtils.GitUI;
using ResourceManager;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormChooseTranslation : GitExtensionsForm
    {
        public FormChooseTranslation()
        {
            InitializeComponent();
            label1.Font = FontUtil.MainInstructionFont;
            label1.ForeColor = FontUtil.MainInstructionColor;
            InitializeComplete();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var translations = new List<string>(Translator.GetAllTranslations());
            translations.Sort();
            translations.Insert(0, "English");

            var imageList = new ImageList
            {
                ImageSize = DpiUtil.Scale(new Size(150, 75)),
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

            lvTranslations.LargeImageList = imageList;

            foreach (string translation in translations)
            {
                if (imageList.Images.ContainsKey(translation))
                {
                    lvTranslations.Items.Add(new ListViewItem(translation, translation) { Tag = translation });
                }
                else
                {
                    lvTranslations.Items.Add(new ListViewItem(translation) { Tag = translation });
                }
            }
        }

        private void FormChooseTranslation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (string.IsNullOrEmpty(AppSettings.Translation))
            {
                AppSettings.Translation = "English";
            }
        }

        private void lvTranslations_ItemActivate(object sender, EventArgs e)
        {
            AppSettings.Translation = ((ListView)sender).SelectedItems[0].Tag.ToString();
            Close();
        }
    }
}
