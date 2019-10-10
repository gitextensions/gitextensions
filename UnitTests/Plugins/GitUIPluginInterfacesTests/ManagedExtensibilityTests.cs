using System;
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
