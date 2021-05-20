using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using GitExtensions.Plugins.Bitbucket.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitExtensions.Plugins.Bitbucket
{
    [Export(typeof(IGitPlugin))]
    public class BitbucketPlugin : GitPluginBase
    {
        public readonly StringSetting BitbucketUsername = new("Bitbucket Username", string.Empty);
        public readonly PasswordSetting BitbucketPassword = new("Bitbucket Password", string.Empty);
        public readonly StringSetting BitbucketBaseUrl = new("Specify the base URL to Bitbucket", "https://example.bitbucket.com");
        public readonly BoolSetting BitbucketDisableSsl = new("Disable SSL verification", false);

        private readonly TranslationString _yourRepositoryIsNotInBitbucket = new("Your repository is not hosted in BitBucket Server.");

        public BitbucketPlugin() : base(true)
        {
            Id = new Guid("0DA2C988-37A1-461C-BAD4-AFE4930C3157");
            Name = "Bitbucket Server";
            Translate();

            Icon = Resources.IconPluginBitbucket;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            Settings? settings = Bitbucket.Settings.Parse(args.GitModule, Settings, this);
            if (settings is null)
            {
                MessageBox.Show(args.OwnerForm,
                                _yourRepositoryIsNotInBitbucket.Text,
                                string.Empty,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return false;
            }

            using BitbucketPullRequestForm frm = new(settings, args.GitModule);
            frm.ShowDialog(args.OwnerForm);

            return true;
        }

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return BitbucketUsername;
            yield return BitbucketPassword;
            yield return BitbucketBaseUrl;
            yield return BitbucketDisableSsl;
        }
    }
}
