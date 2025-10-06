using GitExtensions.Extensibility.Settings;

namespace GitCommands.Settings;

internal sealed class DetailedSettings : IDetailedSettings
{
    private const string DetailedGroupName = "Detailed";

    private const bool GetRemoteBranchesDirectlyFromRemoteDefault = false;
    private const bool AddMergeLogMessagesDefault = false;
    private const int MergeLogMessagesCountDefault = 20;

    private readonly SettingsSource _settingsSource;

    public DetailedSettings(SettingsSource settingsSource)
    {
        _settingsSource = settingsSource;
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
