using System;
using ResourceManager;

namespace GitUI.CommandsDialogs
{
    partial class FormRemotes
    {
        private readonly TranslationString _remoteBranchDataError =
           new TranslationString("Invalid ´{1}´ found for branch ´{0}´." + Environment.NewLine +
                                 "Value has been reset to empty value.");

        private readonly TranslationString _questionAutoPullBehaviour =
            new TranslationString("You have added a new remote repository." + Environment.NewLine +
                                  "Do you want to automatically configure the default push and pull behavior for this remote?");

        private readonly TranslationString _questionAutoPullBehaviourCaption =
            new TranslationString("New remote");

        private readonly TranslationString _gitMessage =
            new TranslationString("Message");

        private readonly TranslationString _questionDeleteRemote =
            new TranslationString("Are you sure you want to delete this remote?");

        private readonly TranslationString _questionDeleteRemoteCaption =
            new TranslationString("Delete");

        private readonly TranslationString _sshKeyOpenFilter =
            new TranslationString("Private key (*.ppk)");

        private readonly TranslationString _sshKeyOpenCaption =
            new TranslationString("Select ssh key file");

        private readonly TranslationString _warningNoKeyEntered =
            new TranslationString("No SSH key file entered");

        private readonly TranslationString _labelUrlAsFetch =
            new TranslationString("Fetch Url");

        private readonly TranslationString _labelUrlAsFetchPush =
            new TranslationString("Url");

        private readonly TranslationString _gbMgtPanelHeaderNew =
            new TranslationString("Create New Remote");

        private readonly TranslationString _gbMgtPanelHeaderEdit =
            new TranslationString("Edit Remote Details");
    }
}
