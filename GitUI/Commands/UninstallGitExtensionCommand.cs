using GitCommands;
using GitCommands.Settings;

namespace GitUI.Commands
{
    internal sealed class UninstallGitExtensionCommand : IGitExtensionCommand
    {
        public bool Execute()
        {
            var configFileGlobalSettings = ConfigFileSettings.CreateGlobal(allowCache: false);
            var coreEditor = configFileGlobalSettings.GetValue(setting: "core.editor");

            if (coreEditor.ToLowerInvariant().Contains(AppSettings.GetInstallDir().ToPosixPath().ToLowerInvariant()))
            {
                configFileGlobalSettings.SetValue(setting: "core.editor", value: string.Empty);
            }

            configFileGlobalSettings.Save();

            return true;
        }
    }
}
