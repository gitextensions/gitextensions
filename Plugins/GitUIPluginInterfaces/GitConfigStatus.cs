namespace GitUIPluginInterfaces
{
    /// <summary>
    /// Status of running git config
    /// https://git-scm.com/docs/git-config#_description This command will fail with non-
    /// zero status upon error.Some exit codes are:
    /// The section or key is invalid (ret= 1) this is returned when the key is not set also.
    /// No section or name was provided (ret= 2).
    /// The config file is invalid (ret=3).
    /// The config file cannot be written (ret=4).
    /// You try to unset an option which does not exist or you try to unset/set an
    /// option for which multiple lines match(ret = 5).
    /// You try to use an invalid regexp(ret = 6).
    /// On success, the command returns the exit code 0.
    /// </summary>
    public enum GitConfigStatus
    {
        CannotWriteToConfigFile = 4,
        CannotUnsetOrSet = 5,
        ConfigKeyInvalidOrNotSet = 1,
        InvalidConfigFileGiven = 3,
        InvalidRegexp = 6,
        NoConfigKeyGiven = 2,
        Success = 0,
    }
}
