using GitUIPluginInterfaces;

namespace GitUIPluginInterfacesTests;

[TestFixture]
public class ManagedExtensibilityTests
{
    [TestCase]
    public void ThrowWhenUserPluginsPathAlreadyInitialized()
    {
        ManagedExtensibility.SetUserPluginsPath("A");
        ClassicAssert.Throws<InvalidOperationException>(() => ManagedExtensibility.SetUserPluginsPath("B"));
    }
}
