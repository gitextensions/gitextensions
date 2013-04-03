using System;
﻿
namespace GitCommands.Config
{
    /// <summary>
    /// Defines the strings to access certain git config Settings.Default.
    /// Goal is to eliminate duplicate string constants in the code.
    /// </summary>
    public class SettingKeyString
    {
        /// <summary>
        /// "remote.{0}.pushurl"
        /// </summary>
        public const string RemotePushUrl = "remote.{0}.pushurl";

        /// <summary>
        /// "remote.{0}.url"
        /// </summary>
        public const string RemoteUrl = "remote.{0}.url";
    }
}
