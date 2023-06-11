using FluentAssertions;
using GitCommands.Config;

namespace GitCommandsTests.Settings
{
    [TestFixture]
    internal sealed class ConfigSectionTests
    {
        private static readonly string _keyName = Guid.NewGuid().ToString();
        private static readonly string _sectionName = Guid.NewGuid().ToString();

        private static ConfigSection _configSection;

        [SetUp]
        public void SetUp()
        {
            _configSection = new ConfigSection(_sectionName, forceCaseSensitive: true);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _configSection = null;
        }

        [Test]
        public void should_remove_setting([Values(null, "")] string noValue)
        {
            string value = Guid.NewGuid().ToString();
            string defaultValue = Guid.NewGuid().ToString();
            _configSection.HasValue(_keyName).Should().BeFalse();
            _configSection.SetValue(_keyName, value);
            _configSection.HasValue(_keyName).Should().BeTrue();
            _configSection.GetValue(_keyName, defaultValue).Should().Be(value);

            string invalidKey = "foobar";
            _configSection.GetValue(invalidKey, defaultValue).Should().Be(defaultValue);
            _configSection.HasValue(invalidKey).Should().BeFalse();

            _configSection.HasValue(_keyName).Should().BeTrue();
            _configSection.SetValue(_keyName, noValue);
            _configSection.HasValue(_keyName).Should().BeFalse();
            _configSection.GetValue(_keyName, defaultValue).Should().Be(defaultValue);
            _configSection.HasValue(_keyName).Should().BeFalse();
        }
    }
}
