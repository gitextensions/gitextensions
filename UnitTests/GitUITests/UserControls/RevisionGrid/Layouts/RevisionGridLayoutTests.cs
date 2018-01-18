using System;
using System.Linq;
using FluentAssertions;
using GitUI.UserControls.RevisionGrid.Layouts;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Layouts
{
    [TestFixture]
    public class RevisionGridLayoutTests
    {
        // Typically enums do not require testing
        // However we allow users to cycle through the layouts and we want to ensure continouos numbering
        // We also want to ensure that if the enum is changed the develop is alerted to update
        // RevisionGridLayoutRendererFactoryTests class as it is coupled to this enum

        [Test]
        public void Ensure_continuous_numbering()
        {
            var values = Enum.GetValues(typeof(RevisionGridLayout));
            values.Cast<int>().ShouldAllBeEquivalentTo(Enumerable.Range(0, values.Length),
                "the enum must be started from 0 and must have continuous numbering");
        }

        [Test]
        public void Ensure_content_stability()
        {
            var values = Enum.GetValues(typeof(RevisionGridLayout));
            if (values.Length != 9)
            {
                Assert.Fail($"You must ensure the changes are reflected in {nameof(LayoutRendererFactoryTests)} because it is coupled to the enum content");
            }
        }
    }
}
