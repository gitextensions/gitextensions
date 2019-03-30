using System.Linq;
using GitUI.Script;
using NUnit.Framework;

namespace GitUITests.Script
{
    [TestFixture]
    internal sealed class ScriptOptionsTests
    {
        [Test]
        public void ScriptOptions_should_be_unique()
        {
            // Arrange
            // Act
            var uniqueScriptOptionsCount = ScriptOptions.All.Distinct()
                .Count();

            // Assert
            Assert.That(uniqueScriptOptionsCount, Is.EqualTo(ScriptOptions.All.Count));
        }
    }
}
