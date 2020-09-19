using System;
using System.Linq;
using GitCommands;
using GitUI.Shells;
using NUnit.Framework;

namespace GitUI.Tests.Shells
{
    [TestFixture]
    internal sealed class ShellRepositoryTests
    {
        [Test]
        public void Should_return_predefined_shells()
        {
            // Arrange
            var settingValue = AppSettings.Shells.Value;

            AppSettings.Shells.Value = AppSettings.Shells.Default;

            var shellRepository = new ShellRepository();

            // Act
            var shells = shellRepository.Query();

            AppSettings.Shells.Value = settingValue;

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(4));
        }

        [Test]
        public void Should_add_shell()
        {
            // Arrange
            var shellRepository = new ShellRepository();
            var shell = new Shell();
            var shellsCount = shellRepository.Query()
                .Count();

            // Act
            shellRepository.Add(shell);

            var shells = shellRepository.Query();

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(shellsCount + 1));
        }

        [Test]
        public void Should_remove_shell()
        {
            // Arrange
            var shellRepository = new ShellRepository();

            shellRepository.Add(new Shell());

            var shellsCount = shellRepository.Query()
                .Count();

            var shell = shellRepository.Query()
                .FirstOrDefault();

            // Act
            shellRepository.Remove(shell);

            var shells = shellRepository.Query();

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(shellsCount - 1));
        }

        [Test]
        public void Should_save_shells()
        {
            // Arrange
            var settingValue = AppSettings.Shells.Value;
            var shellRepository = new ShellRepository();
            var shell = new Shell();
            var updated = false;

            AppSettings.Shells.Updated += (s, e) =>
            {
                updated = true;
            };

            // Act
            shellRepository.Add(shell);
            shellRepository.Save();

            AppSettings.Shells.Value = settingValue;

            // Assert
            Assert.That(updated, Is.True);
        }
    }
}
