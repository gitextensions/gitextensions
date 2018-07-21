using System.ComponentModel.Composition;
using System.Windows.Forms;
using Bitbucket.Properties;
using GitUIPluginInterfaces;
using ResourceManager;

namespace Bitbucket
{
    [Export(typeof(IGitPlugin))]
    public class BitbucketPlugin : GitPluginBase
    {
        public readonly StringSetting BitbucketUsername = new StringSetting("Bitbucket Username", string.Empty);
        public readonly PasswordSetting BitbucketPassword = new PasswordSetting("Bitbucket Password", string.Empty);
        public readonly StringSetting BitbucketBaseUrl = new StringSetting("Specify the base URL to Bitbucket", "https://example.bitbucket.com");
        public readonly BoolSetting BitbucketDisableSsl = new BoolSetting("Disable SSL verification", false);

        private readonly TranslationString _yourRepositoryIsNotInBitbucket = new TranslationString("Your repository is not hosted in BitBucket Server.");

        public BitbucketPlugin()
        {
            SetNameAndDescription("Bitbucket Server");
            Translate();

            Icon = Resources.IconPluginBitbucket;
        }

        public override bool Execute(GitUIEventArgs args)
        {
            Settings settings = Bitbucket.Settings.Parse(args.GitModule, Settings, this);
            if (settings == null)
            {
                MessageBox.Show(args.OwnerForm,
                                _yourRepositoryIsNotInBitbucket.Text,
                                string.Empty,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                return false;
            }

            using (var frm = new BitbucketPullRequestForm(settings))
            {
                frm.ShowDialog(args.OwnerForm);
            }

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
