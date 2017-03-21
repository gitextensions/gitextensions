﻿using System.Windows.Forms;
using GitUIPluginInterfaces;
using ResourceManager;

namespace Stash
{
    public class StashPlugin : GitPluginBase
    {
        public readonly StringSetting StashUsername = new StringSetting("Bitbucket Username", string.Empty);
        public readonly PasswordSetting StashPassword = new PasswordSetting("Bitbucket Password", string.Empty);
        public readonly StringSetting StashBaseUrl = new StringSetting("Specify the base URL to Bitbucket", "https://example.stash.com");
        public readonly BoolSetting StashDisableSsl = new BoolSetting("Disable SSL verification", false);

        public StashPlugin()
        {
            SetNameAndDescription("Create Bitbucket Pull Request");
            Translate();
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            using (var frm = new StashPullRequestForm(this, base.Settings, gitUiCommands))
                frm.ShowDialog(gitUiCommands.OwnerForm);
            return true;
        }

        public override System.Collections.Generic.IEnumerable<ISetting> GetSettings()
        {
            yield return StashUsername;
            yield return StashPassword;
            yield return StashBaseUrl;
            yield return StashDisableSsl;
        }
    }
}
