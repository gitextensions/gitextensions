using GitCommands;
using GitExtensions.Extensibility.Translations;
using GitExtUtils.GitUI;

namespace GitUI.CommandsDialogs.SettingsDialog.Pages
{
    public partial class FormChooseTranslation : GitExtensionsForm
    {
        public FormChooseTranslation()
        {
            InitializeComponent();
            label1.Font = FontUtil.MainInstructionFont;
            label1.ForeColor = FontUtil.MainInstructionColor;
            Text = "Choose language";
            InitializeComplete();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            List<string> translations = new(Translator.GetAllTranslations());
            translations.Sort();
            translations.Insert(0, "English");

            ImageList imageList = new()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = DpiUtil.Scale(new Size(150, 75)),
            };

            foreach (string translation in translations)
            {
                string imagePath = Path.Combine(Translator.GetTranslationDir(), translation + ".gif");
                if (File.Exists(imagePath))
                {
                    Image image = Image.FromFile(imagePath);
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
            // take the selection if any, else see the fallback in FormChooseTranslation_FormClosing
            ListView.SelectedListViewItemCollection selectedItems = ((ListView)sender).SelectedItems;
            if (selectedItems.Count > 0)
            {
                AppSettings.Translation = selectedItems[0].Tag.ToString();
            }

            Close();
        }
    }
}
