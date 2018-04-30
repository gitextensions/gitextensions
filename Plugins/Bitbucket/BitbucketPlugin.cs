using System.ComponentModel.Composition;
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

        public BitbucketPlugin()
        {
            SetNameAndDescription("Bitbucket Server");
            Translate();
        }

        public override bool Execute(GitUIEventArgs args)
        {
            using (var frm = new BitbucketPullRequestForm(this, Settings, args))
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
