using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUIPluginInterfacesTests
{
    [TestFixture]
    public class ManagedExtensibilityTests
    {
        [TestCase]
        public void ThrowWhenUserPluginsPathAlreadyInitialized()
        {
            ManagedExtensibility.SetUserPluginsPath("A");
            Assert.Throws<InvalidOperationException>(() => ManagedExtensibility.SetUserPluginsPath("B"));
        }
    }
}
