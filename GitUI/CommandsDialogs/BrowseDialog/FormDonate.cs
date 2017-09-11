using System;
using System.Diagnostics;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormDonate : GitExtensionsForm
    {
        public static readonly string DonationUrl =
            @"https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=WAL2SSDV8ND54&lc=US&item_name=GitExtensions&no_note=1&no_shipping=1&currency_code=EUR&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted";

        public FormDonate()
        {
            InitializeComponent();
            Translate();
        }

        private void PictureBox1Click(object sender, EventArgs e)
        {
            Process.Start(DonationUrl);
        }
    }
}