using FluentAssertions;
using GitCommands;
using GitUI.Hotkey;
using ResourceManager;

namespace GitUITests.Hotkey
{
    [TestFixture]
    public class HotkeySettingsManagerTests
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

        [Test]
        public void SequenceEqualOnEqualSettings()
        {
            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        [Test]
        public void MergeLoadedSettings()
        {
            // arrange

            var defaultHotkeySettingsArray = CreateHotkeySettings(2);
            var loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[0].Commands[0].KeyData = Keys.C;

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        [Test]
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

        [Test]
        public async Task Can_save_settings()
        {
            string originalHotkeys = AppSettings.SerializedHotkeys;

            try
            {
                HotkeySettingsManager.SaveSettings(CreateHotkeySettings(2));

                // Verify as a string, as the xml verifier ignores line breaks.
                await Verifier.Verify(AppSettings.SerializedHotkeys);
            }
            finally
            {
                AppSettings.SerializedHotkeys = originalHotkeys;
            }
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
