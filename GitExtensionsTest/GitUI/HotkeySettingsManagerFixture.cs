using NUnit.Framework;
using FluentAssertions;
using GitUI.Hotkey;
using ResourceManager;

namespace GitExtensionsTest.GitUI
{
    [TestFixture]
    public class HotkeySettingsManagerFixture
    {
        [Test]
        public void DidDefaultSettingsChangeTest1()
        {
            // arrange
            HotkeySettings hotkeySettings1 = new HotkeySettings("settings1", new HotkeyCommand(1, "C1"), new HotkeyCommand(2, "C2"));
            HotkeySettings hotkeySettings2 = new HotkeySettings("settings2", new HotkeyCommand(3, "C3"), new HotkeyCommand(4, "C4"));

            var defaultHotkeySettingsArray = new HotkeySettings[2] { hotkeySettings1, hotkeySettings2 };
            var loadedHotkeySettingsArray = new HotkeySettings[2] { hotkeySettings1, hotkeySettings2 };

            // act, assert
            HotkeySettingsManager.DidDefaultSettingsChange(defaultHotkeySettingsArray, loadedHotkeySettingsArray).Should().BeFalse();

            // arrange: add one more settings items
            defaultHotkeySettingsArray = new HotkeySettings[3] { hotkeySettings1, hotkeySettings2, hotkeySettings2 };

            // act, assert
            HotkeySettingsManager.DidDefaultSettingsChange(defaultHotkeySettingsArray, loadedHotkeySettingsArray).Should().BeTrue();

            // act (null handling), assert
            HotkeySettingsManager.DidDefaultSettingsChange(null, defaultHotkeySettingsArray).Should().BeTrue();

            // act (null handling), assert
            HotkeySettingsManager.DidDefaultSettingsChange(defaultHotkeySettingsArray, null).Should().BeTrue();

            // act (null handling), assert
            HotkeySettingsManager.DidDefaultSettingsChange(null, null).Should().BeTrue();
        }

        [Test(Description="Move one command from one settings to another should also be detected in 'changed settings'")]
        public void DidDefaultSettingsChangeTest2()
        {
            // arrange
            var defaultHotkeySettingsArray = new HotkeySettings[2] {
                new HotkeySettings("settings1", new HotkeyCommand(1, "C1")),
                new HotkeySettings("settings2", new HotkeyCommand(3, "C3"), new HotkeyCommand(2, "C2"), new HotkeyCommand(4, "C4"))
            };

            var loadedHotkeySettingsArray = new HotkeySettings[2] {
                new HotkeySettings("settings1", new HotkeyCommand(1, "C1"), new HotkeyCommand(4, "C4")),
                new HotkeySettings("settings2", new HotkeyCommand(3, "C3"), new HotkeyCommand(2, "C2"))
            };

            // act, assert
            HotkeySettingsManager.DidDefaultSettingsChange(defaultHotkeySettingsArray, loadedHotkeySettingsArray).Should().BeTrue();
        }
    }
}
