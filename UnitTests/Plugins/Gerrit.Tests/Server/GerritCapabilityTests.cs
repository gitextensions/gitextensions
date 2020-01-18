using System.Linq;
using Gerrit.Server;
using NUnit.Framework;

namespace GerritTests.Server
{
    public static class GerritCapabilityTests
    {
        [Test]
        public static void PublishTypes_for_Version2_15_has_expected_list_of_values()
        {
            RunTest(GerritCapabilities.Version2_15, new[] { "", "wip", "private" });
        }

        [Test]
        public static void PublishTypes_for_OlderVersion_has_expected_list_of_values()
        {
            RunTest(GerritCapabilities.OldestVersion, new[] { "", "drafts" });
        }

        private static void RunTest(GerritCapabilities capability, string[] expectedValues)
        {
            var publishTypes = capability.PublishTypes
                .Select(x => x.Value)
                .ToArray();

            Assert.That(publishTypes, Is.EqualTo(expectedValues));
        }
    }
}
