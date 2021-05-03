using GitUIPluginInterfaces;
using GitUIPluginInterfaces.Settings;

namespace GitCommands.Settings
{
    internal sealed class DetailedSettings : IDetailedSettings
    {
        private const string DetailedGroupName = "Detailed";

        private const string SmtpServerDefault = "smtp.gmail.com";
        private const int SmtpPortDefault = 465;
        private const bool SmtpUseSslDefault = true;
        private const bool GetRemoteBranchesDirectlyFromRemoteDefault = false;
        private const bool AddMergeLogMessagesDefault = false;
        private const int MergeLogMessagesCountDefault = 20;

        private readonly ISettingsSource _settingsSource;

        public DetailedSettings(ISettingsSource settingsSource)
        {
            _settingsSource = settingsSource;
        }

        public string SmtpServer
        {
            get => _settingsSource.GetString(nameof(SmtpServer), SmtpServerDefault);
            set
            {
                if (SmtpServer == value)
                {
                    return;
                }

                _settingsSource.SetString(nameof(SmtpServer), value);
            }
        }

        public int SmtpPort
        {
            get => _settingsSource.GetInt(nameof(SmtpPort), SmtpPortDefault);
            set
            {
                if (SmtpPort == value)
                {
                    return;
                }

                _settingsSource.SetInt(nameof(SmtpPort), value);
            }
        }

        public bool SmtpUseSsl
        {
            get => _settingsSource.GetBool(nameof(SmtpUseSsl), SmtpUseSslDefault);
            set
            {
                if (SmtpUseSsl == value)
                {
                    return;
                }

                _settingsSource.SetBool(nameof(SmtpUseSsl), value);
            }
        }

        public bool GetRemoteBranchesDirectlyFromRemote
        {
            get => _settingsSource.GetBool($"{DetailedGroupName}.{nameof(GetRemoteBranchesDirectlyFromRemote)}", GetRemoteBranchesDirectlyFromRemoteDefault);
            set
            {
                if (GetRemoteBranchesDirectlyFromRemote == value)
                {
                    return;
                }

                _settingsSource.SetBool($"{DetailedGroupName}.{nameof(GetRemoteBranchesDirectlyFromRemote)}", value);
            }
        }

        public bool AddMergeLogMessages
        {
            get => _settingsSource.GetBool($"{DetailedGroupName}.{nameof(AddMergeLogMessages)}", AddMergeLogMessagesDefault);
            set
            {
                if (AddMergeLogMessages == value)
                {
                    return;
                }

                _settingsSource.SetBool($"{DetailedGroupName}.{nameof(AddMergeLogMessages)}", value);
            }
        }

        public int MergeLogMessagesCount
        {
            get => _settingsSource.GetInt($"{DetailedGroupName}.{nameof(MergeLogMessagesCount)}", MergeLogMessagesCountDefault);
            set
            {
                if (MergeLogMessagesCount == value)
                {
                    return;
                }

                _settingsSource.SetInt($"{DetailedGroupName}.{nameof(MergeLogMessagesCount)}", value);
            }
        }
    }
}
