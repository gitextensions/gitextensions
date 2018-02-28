using System.Diagnostics.CodeAnalysis;

namespace GitCommands.Settings
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Case must match setting")]
    public enum AutoCRLFType
    {
#pragma warning disable SA1300 // Element should begin with upper-case letter
        @true,
        input,
        @false
#pragma warning restore SA1300 // Element should begin with upper-case letter
    }

    public enum SettingsKind
    {
        /// <summary>
        /// Global for all repositories
        /// </summary>
        Global,

        /// <summary>
        /// Version-controlled, distributed with current repository
        /// </summary>
        Distributed,

        /// <summary>
        /// Local for current repository
        /// </summary>
        Local,

        /// <summary>
        /// Effective - first assigned value in the following order: Local, Distributable, Global
        /// </summary>
        Effective
    }
}
