using System;
using System.Diagnostics;
using System.Drawing;
using ResourceManager;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormDonate : GitExtensionsForm
    {
        private readonly TranslationString _donateText = new TranslationString("We have a dedicated team of collaborators that spends a lot of time maintaining the app, working on new features and fixing bugs." +
                                                                              "You can support the project by making a financial contribution. Donations will be used to cover running costs " +
                                                                              "and to get the resources needed to keep the project running. We will also use donations to thank collaborators for their efforts.\r\n\r\n" +
                                                                              "Click on the button below to get more information about making a donation.");

        public static readonly string DonationUrl =
            @"https://opencollective.com/gitextensions";

        public FormDonate()
        {
            InitializeComponent();
            InitializeComplete();

            lblText.Text = _donateText.Text;
            lblText.MaximumSize = new Size(lblText.Width, 0);
            lblText.AutoSize = true;
        }

        private void PictureBox1Click(object sender, EventArgs e)
        {
            Process.Start(DonationUrl);
        }
    }
}