using System.Linq;
using System.Windows.Forms;
using FluentAssertions;
using GitUI.Hotkey;
using NUnit.Framework;
using ResourceManager;

namespace GitUITests.Hotkey
{
    [TestFixture]
    public class HotkeySettingsManagerFixture
    {
        [Test]
        public void MergeEqualSettings()
        {
            // arrange

            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);
            var expected = CreateHotkeySettings(2);

            defaultHotkeySettingsArray.SequenceEqual(expected).Should().BeTrue();
        }

        public void SequenceEqualOnDifferentSettings()
        {
            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[0].Commands[0].KeyData = Keys.C;

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeFalse();
        }

        public void SequenceEqualOnEqualSettings()
        {
            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        public void MergeLoadedSettings()
        {
            // arrange

            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[0].Commands[0].KeyData = Keys.C;

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        public void MergeLoadedDiffSizeSettings()
        {
            // arrange

            var defaultHotkeySettingsArray = CreateHotkeySettings(3);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[1].Commands[1].KeyData = Keys.C;

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);
            var expected = CreateHotkeySettings(3);
            expected[1].Commands[1].KeyData = loadedHotkeySettingsArray[1].Commands[1].KeyData;

            defaultHotkeySettingsArray.SequenceEqual(expected).Should().BeTrue();
        }

        private static HotkeySettings[] CreateHotkeySettings(int count)
        {
            return Enumerable.Range(1, count).Select(i =>
                new HotkeySettings("settings" + i,
                    new HotkeyCommand(1, "C1") { KeyData = Keys.A },
                    new HotkeyCommand(2, "C2") { KeyData = Keys.B })).ToArray();
        }
    }
}
