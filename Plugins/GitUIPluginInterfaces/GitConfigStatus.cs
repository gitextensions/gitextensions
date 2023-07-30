namespace GitUIPluginInterfaces
{
    /// <summary>
    ///  Status of running git config https://git-scm.com/docs/git-config#_description.
    ///  This command will fail with non-zero status upon error.Some exit codes are:
    ///  <code>
    ///   The section or key is invalid (ret= 1) this is returned when the key is not set also.
    ///   No section or name was provided (ret= 2).
    ///   The config file is invalid (ret=3).
    ///   The config file cannot be written (ret=4).
    ///   You try to unset an option which does not exist or you try to unset/set an option for which multiple lines match (ret = 5).
    ///   You try to use an invalid regexp (ret = 6).
    ///   On success, the command returns the exit code 0.
    ///  </code>
    /// </summary>
    /// <remarks>
    /// See https://github.com/git/git/blob/fac96dfbb1c24369ba7d37a5affd8adfe6c650fd/config.h#L26C1-L36C1 and find latest version in master of those values.
    /// See https://github.com/git/git/blob/fac96dfbb1c24369ba7d37a5affd8adfe6c650fd/builtin/config.c#L927 for the flow of getting a config value.
    /// See https://github.com/git/git/blob/fac96dfbb1c24369ba7d37a5affd8adfe6c650fd/builtin/config.c#L893 for the flow of setting a config value.
    /// Following those flows, 0,1, and 7 are returned from a get call.
    /// 6 is only returned when doing --get-rexexp
    /// </remarks>
    public enum GitConfigStatus
    {
        /// <summary>
        /// Cannot lock config file for writing.
        /// </summary>
        CannotLockConfigFile = -1,

        /// <summary>
        /// Retrieved the value successfully. The setting was set.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Retrieved a blank value. The config value is not set or config key is invalid.
        /// </summary>
        ConfigKeyInvalidOrNotSet = 1,

        /// <summary>
        /// git config was provided no config key.
        /// </summary>
        NoConfigKeyGiven = 2,

        /// <summary>
        /// Invalid config file. Failed to open file or config file is corrupted.
        /// </summary>
        InvalidConfigFile = 3,

        /// <summary>
        /// Cannot write to config file.  File is read-only or permmisions do not allow write or file is corrupted.
        /// </summary>
        CannotWriteToConfigFile = 4,

        /// <summary>
        /// Trying to unset a value that is not set or current config already has too many values set and cannot replace multi values with a single value.
        /// </summary>
        CannotUnsetOrSet = 5,

        /// <summary>
        /// The key used to filter config keys while using --get-rexexp is invalid.
        /// </summary>
        InvalidRegexp = 6,

        /// <summary>
        /// Unknown error. Git did not provide a specific error.
        /// </summary>
        GenericError = 7
    }
}
