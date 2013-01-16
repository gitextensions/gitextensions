using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
