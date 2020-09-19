using GitUI.Shells;
using NUnit.Framework;

namespace GitUI.Tests.Shells
{
    [TestFixture]
    internal sealed class ShellTests
    {
        [Test]
        public void Should_create_shell()
        {
            // Arrange
            // Act
            var shell = new Shell();

            // Assert
            Assert.That(shell, Is.Not.Null);
            Assert.That(shell.Name, Is.EqualTo(ShellConstants.DefaultName));
            Assert.That(shell.Icon, Is.EqualTo(ShellConstants.DefaultIcon));
            Assert.That(shell.Command, Is.EqualTo(ShellConstants.DefaultCommand));
            Assert.That(shell.Arguments, Is.EqualTo(ShellConstants.DefaultArguments));
            Assert.That(shell.Default, Is.False);
            Assert.That(shell.Default, Is.False);
        }
    }
}
