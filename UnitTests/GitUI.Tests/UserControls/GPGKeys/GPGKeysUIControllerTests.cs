using CommonTestUtils;
using GitCommands.Config;
using GitCommands.Git.Gpg;
using GitCommands.Gpg;
using GitExtUtils;
using GitUI.UserControls.GPGKeys;
using GitUIPluginInterfaces;
using NSubstitute;

namespace GitUITests.UserControls.GPGKeys
{
    [SetCulture("en-US")]
    [SetUICulture("en-US")]
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal class GPGKeysUIControllerTests
    {
        private GPGKeysUIController _uiController;
        private IGitModule _module;
        private IGitGpgController _gpgController;
        private IGPGSecretKeysParser _gpgSecretKeysParser;
        private MockExecutable _executable;
        private GpgKeyDisplayInfo[] _expectedKeys;

        [SetUp]
        public void Setup()
        {
            _executable = new MockExecutable();
            _module = Substitute.For<IGitModule>();
            _module.GitExecutable.Returns(_executable);
            _module.GpgExecutable.Returns(_executable);

            _gpgSecretKeysParser = new GpgSecretKeysParser();
            _gpgController = new GitGpgController(() => _module, _gpgSecretKeysParser);

            _expectedKeys = new GpgKeyDisplayInfo[]
            {
                new GpgKeyDisplayInfo("076D94226A5AF99D5911E6131C100BD531ED45B4",
                    "1C100BD531ED45B4",
                    "Test User Key <123@123.com>",
                    false),
                new GpgKeyDisplayInfo("076D94226A5AF99D5911E6131C100BD531ED45B4",
                    "1C100BD531ED45B4",
                    "Test User Key <123@123.com>",
                    true)
            };
            _uiController = new GPGKeysUIController(_gpgController);
        }

        [TearDown]
        public void TearDown()
        {
        }

        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "1C100BD531ED45B4")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200::e::+::cv25519::
fpr:::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 0, "1C100BD531ED45B4")] // invalid but looks like gpg output
        [TestCase("this is not valid gpg output", 0, "1C100BD531ED45B4")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:::::::::076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200:::::e:::+::cv25519::
fpr:::::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 1, "")]
        [TestCase(@"sec:u:255:22:1C100BD531ED45B4:1665235438:1728403200::u:::scESC:::+::ed25519:::0:
fpr:076D94226A5AF99D5911E6131C100BD531ED45B4:
grp:::::::397ECB64B371A9CE8DB742CACF8A66165399234D:
uid:u::1665235438::BFF780DF9E4541BED0ECBE237D0B6A86D73FD1F2::Test User Key <123@123.com>::::::::0:
ssb:u:255:18:54C01383C638A805:1665235438:1728403200::e::+::cv25519::
fpr:::::::EE2785B28CCB0C768B25C29054C01383C638A805:
grp:::::::E7CCC28181C1BE739B7BA856C99F0DAB4E6637B6:
", 0, "")] // invalid but looks like gpg output
        [TestCase("this is not valid gpg output", 0, "")]
        public async Task ValidateKetKeys(string output, int expectedCount, string defaultKey)
        {
            var gpgArgs = new ArgumentBuilder()
            {
                "-K", // Secret Keys
                "--with-colons",
                "--keyid-format LONG"
            };
            var defaultKeyArgs = new GitArgumentBuilder("config")
            {
                SettingKeyString.UserSigningKey
            };
            using var gppOutput = _executable.StageOutput(gpgArgs.ToString(), output);
            using var defultKeyOutput = _executable.StageOutput(defaultKeyArgs.ToString(), defaultKey);

            var keys = await _uiController.GetKeysAsync();

            Assert.IsNotNull(keys);
            Assert.That(keys.Count() == expectedCount);
            if (expectedCount > 0)
            {
                AssertEx.SequenceEqual(_expectedKeys.Where(k => k.IsDefault == !string.IsNullOrEmpty(defaultKey)), keys);
            }
        }
    }
}
