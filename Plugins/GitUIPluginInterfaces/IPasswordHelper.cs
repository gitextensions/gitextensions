using System;
using System.Collections.Generic;
using System.Text;

namespace GitUIPluginInterfaces
{
    public interface IPasswordHelper
    {
        string TryGetHelperPassword(string inputUrl);
    }
}
