using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces.RepositoryHosts
{
    public interface IPasswordHelper
    {
        string TryGetHelperPassword(string inputUrl);
    }
}
