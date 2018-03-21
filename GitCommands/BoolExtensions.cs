using JetBrains.Annotations;

namespace System
{
    public static class BoolExtensions
    {
        /// <summary>
        /// Translates this bool value to the git command line force flag
        /// </summary>
        [NotNull]
        public static string AsForce(this bool force)
        {
            return force ? " -f " : string.Empty;
        }
    }
}