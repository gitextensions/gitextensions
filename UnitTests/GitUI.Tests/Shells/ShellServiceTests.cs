using System;
using System.Collections.Generic;
using System.Linq;
using GitUI.Shells;
using NSubstitute;
using NUnit.Framework;

namespace GitUI.Tests.Shells
{
    [TestFixture]
    internal sealed class ShellServiceTests
    {
        [Test]
        public void Should_return_all_shells()
        {
            // Arrange
            var shellRepository = Substitute.For<IShellRepository>();
            var count = 10;

            shellRepository.Query()
                .Returns(Enumerable.Repeat(new Shell(), count));

            var shellService = new ShellService(shellRepository);

            // Act
            var shells = shellService.AllShells();

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(count));
        }

        [Test]
        public void Should_return_enabled_shells()
        {
            // Arrange
            var shellRepository = Substitute.For<IShellRepository>();
            var count = 10;
            var enabledShell = new Shell
            {
                Enabled = true
            };

            shellRepository.Query()
                .Returns(Enumerable
                    .Repeat(new Shell(), count)
                    .Union(Enumerable
                        .Repeat(enabledShell, 1)));

            var shellService = new ShellService(shellRepository);

            // Act
            var shells = shellService.EnabledShells();

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Should_return_default_shell()
        {
            // Arrange
            var shellRepository = Substitute.For<IShellRepository>();
            var defaultShell = new Shell
            {
                Default = true
            };

            shellRepository.Query()
                .Returns(Enumerable.Repeat(defaultShell, 1));

            var shellService = new ShellService(shellRepository);

            // Act
            var shell = shellService.FindDefault();

            // Assert
            Assert.That(shell, Is.Not.Null);
            Assert.That(shell.Default, Is.True);
        }

        [Test]
        public void Should_not_return_default_shell()
        {
            // Arrange
            var shellRepository = Substitute.For<IShellRepository>();
            var defaultShell = new Shell
            {
                Default = false
            };

            shellRepository.Query()
                .Returns(Enumerable.Repeat(defaultShell, 1));

            var shellService = new ShellService(shellRepository);

            // Act
            var shell = shellService.FindDefault();

            // Assert
            Assert.That(shell, Is.Null);
        }

        [Test]
        public void Should_update_shells()
        {
            // Arrange
            var shellRepository = Substitute.For<IShellRepository>();

            var firstShellId = Guid.NewGuid();
            var firstShellName = Guid.NewGuid().ToString();
            var firstShell = new Shell
            {
                Id = firstShellId,
                Name = firstShellName
            };

            var secondShellId = Guid.NewGuid();
            var secondShellName = Guid.NewGuid().ToString();
            var secondShell = new Shell
            {
                Id = secondShellId,
                Name = secondShellName
            };

            var shells = new List<Shell>
            {
                firstShell,
                secondShell
            };

            var thirdShellId = Guid.NewGuid();
            var thirdShellName = Guid.NewGuid().ToString();
            var thirdShell = new Shell
            {
                Id = secondShellId,
                Name = thirdShellName
            };

            var fourthShellId = Guid.NewGuid();
            var fourthShellName = Guid.NewGuid().ToString();
            var fourthShell = new Shell
            {
                Id = fourthShellId,
                Name = fourthShellName
            };

            var newShells = new List<Shell>
            {
                thirdShell,
                fourthShell
            };

            shellRepository.Query()
                .Returns(shells.AsEnumerable());

            shellRepository.When(x => x.Save())
                .Do(x => { });

            shellRepository.Add(Arg.Do<Shell>(x => shells.Add(x)));
            shellRepository.Remove(Arg.Do<Shell>(x => shells.Remove(x)));

            var shellService = new ShellService(shellRepository);

            // Act
            shellService.Update(newShells);

            // Assert
            Assert.That(shells, Is.Not.Null);
            Assert.That(shells.Count(), Is.EqualTo(2));

            var firstResult = shells.FirstOrDefault(x => x.Id == secondShellId);

            Assert.That(firstResult.Name, Is.EqualTo(thirdShellName));

            var secondResult = shells.FirstOrDefault(x => x.Id == fourthShellId);

            Assert.That(secondResult.Name, Is.EqualTo(fourthShellName));
        }
    }
}
