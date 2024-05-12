using FluentAssertions;
using GitCommands;
using GitUI.Hotkey;
using GitUI.ScriptsEngine;
using NSubstitute;
using ResourceManager;

namespace GitUITests.Hotkey
{
    [TestFixture]
    public class HotkeySettingsManagerTests
    {
        private HotkeySettingsManager _settingsManager;

        [SetUp]
        public void SetUp()
        {
            IScriptsManager scriptsManager = Substitute.For<IScriptsManager>();
            scriptsManager.GetScripts().Returns([]);

            _settingsManager = new(scriptsManager);
        }

        [Test]
        public void MergeEqualSettings()
        {
            // arrange

            HotkeySettings[] defaultHotkeySettingsArray = CreateHotkeySettings(2);
            HotkeySettings[] loadedHotkeySettingsArray = CreateHotkeySettings(2);

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);
            HotkeySettings[] expected = CreateHotkeySettings(2);

            defaultHotkeySettingsArray.SequenceEqual(expected).Should().BeTrue();
        }

        public void SequenceEqualOnDifferentSettings()
        {
            HotkeySettings[] defaultHotkeySettingsArray = CreateHotkeySettings(2);
            HotkeySettings[] loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[0].Commands[0].KeyData = Keys.C;

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeFalse();
        }

        [Test]
        public void SequenceEqualOnEqualSettings()
        {
            HotkeySettings[] defaultHotkeySettingsArray = CreateHotkeySettings(2);
            HotkeySettings[] loadedHotkeySettingsArray = CreateHotkeySettings(2);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        [Test]
        public void MergeLoadedSettings()
        {
            // arrange

            HotkeySettings[] defaultHotkeySettingsArray = CreateHotkeySettings(2);
            HotkeySettings[] loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[0].Commands[0].KeyData = Keys.C;

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);

            defaultHotkeySettingsArray.SequenceEqual(loadedHotkeySettingsArray).Should().BeTrue();
        }

        [Test]
        public void MergeLoadedDiffSizeSettings()
        {
            // arrange

            HotkeySettings[] defaultHotkeySettingsArray = CreateHotkeySettings(3);
            HotkeySettings[] loadedHotkeySettingsArray = CreateHotkeySettings(2);
            loadedHotkeySettingsArray[1].Commands[1].KeyData = Keys.C;

            HotkeySettingsManager.MergeIntoDefaultSettings(defaultHotkeySettingsArray, loadedHotkeySettingsArray);
            HotkeySettings[] expected = CreateHotkeySettings(3);
            expected[1].Commands[1].KeyData = loadedHotkeySettingsArray[1].Commands[1].KeyData;

            defaultHotkeySettingsArray.SequenceEqual(expected).Should().BeTrue();
        }

        [Test]
        public async Task Can_save_settings()
        {
            string originalHotkeys = AppSettings.SerializedHotkeys;

            try
            {
                _settingsManager.SaveSettings(CreateHotkeySettings(2));

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
