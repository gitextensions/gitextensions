using System.Drawing;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class JunctionColorProviderTests
    {
        private JunctionColorProvider _provider;
        private Junction _junction;
        private bool _multicolorBranches;
        private Color _graphColor;

        [SetUp]
        public void Setup()
        {
            _multicolorBranches = AppSettings.MulticolorBranches;
            _graphColor = AppSettings.GraphColor;

            _provider = new JunctionColorProvider();
            _junction = new Junction(new Node(ObjectId.WorkTreeId));
        }

        [TearDown]
        public void TearDown()
        {
            AppSettings.MulticolorBranches = _multicolorBranches;
            AppSettings.GraphColor = _graphColor;
        }

        [Test]
        public void GetColor_should_return_empty_color_for_non_relative_if_DrawNonRelativesGray()
        {
            _provider.GetColor(_junction, GitUI.RevisionGraphDrawStyleEnum.DrawNonRelativesGray)
                .Should().Be(_provider.NonRelativeColor);
        }

        [Test]
        public void GetColor_should_return_empty_color_for_non_highlighted_if_HighlightSelected()
        {
            _junction.IsHighlighted = false;
            _provider.GetColor(_junction, GitUI.RevisionGraphDrawStyleEnum.HighlightSelected)
                .Should().Be(_provider.NonRelativeColor);
        }

        [Test]
        public void GetColor_should_return_user_configured_color_if_not_MulticolorBranches()
        {
            AppSettings.MulticolorBranches = false;
            _junction.IsHighlighted = true;

            _provider.GetColor(_junction, GitUI.RevisionGraphDrawStyleEnum.HighlightSelected)
                .Should().Be(AppSettings.GraphColor);
        }

        [Test]
        public void GetColor_should_return_predefined_color_if_junction_color_has_been_calcualed()
        {
            _junction.ColorIndex = 1;

            _provider.GetColor(_junction, GitUI.RevisionGraphDrawStyleEnum.Normal)
                .Should().Be(JunctionColorProvider.PresetGraphColors[1]);
        }

        [TestCase(-1)]
        [TestCase(100)]
        public void GetColor_should_return_first_predefined_color_if_junction_color_index_outside_boundaries(int colorIndex)
        {
            _junction.ColorIndex = colorIndex;

            _provider.GetColor(_junction, GitUI.RevisionGraphDrawStyleEnum.Normal)
                .Should().Be(JunctionColorProvider.PresetGraphColors[0]);
        }
    }
}
