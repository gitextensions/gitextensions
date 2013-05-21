using System;

namespace Gerrit
{
    internal class GerritSettingsException : Exception
    {
        public GerritSettingsException(string message)
            : base(message)
        {
        }
    }
}
