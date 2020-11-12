using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Bitbucket.Properties;
using GitExtensions.Core.Commands.Events;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Settings;

namespace Bitbucket
{
    [Export(typeof(IGitPlugin))]
    public sealed class Plugin : IGitPlugin,
        IGitPluginConfigurable,
        IGitPluginExecutable
    {
        public readonly StringSetting BitbucketUsername = new StringSetting("Bitbucket Username", Strings.BitbucketUsername, string.Empty);
        public readonly PasswordSetting BitbucketPassword = new PasswordSetting("Bitbucket Password", Strings.BitbucketPassword, string.Empty);
        public readonly StringSetting BitbucketBaseUrl = new StringSetting("Specify the base URL to Bitbucket", Strings.BitbucketBaseUrl, "https://example.bitbucket.com");
        public readonly BoolSetting BitbucketDisableSsl = new BoolSetting("Disable SSL verification", Strings.BitbucketDisableSsl, false);

        public string Name => "Bitbucket Server";

        public string Description => Strings.Description;

        public Image Icon => Images.IconPluginBitbucket;

        public IGitPluginSettingsContainer SettingsContainer { get; set; }

        public IEnumerable<ISetting> GetSettings()
        {
            yield return BitbucketUsername;
            yield return BitbucketPassword;
            yield return BitbucketBaseUrl;
            yield return BitbucketDisableSsl;
        }

        public bool Execute(GitUIEventArgs args)
        {
            Settings settings = Bitbucket.Settings.Parse(args.GitModule, SettingsContainer.GetSettingsSource(), this);
            if (settings == null)
            {
                MessageBox.Show(args.OwnerForm,
                                Strings.YourRepositoryIsNotInBitbucket,
                                string.Empty,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return false;
            }

            using var frm = new BitbucketPullRequestForm(settings, args.GitModule);

            frm.ShowDialog(args.OwnerForm);

            return true;
        }
    }
}
