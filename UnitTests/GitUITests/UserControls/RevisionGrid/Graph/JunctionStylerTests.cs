using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using FluentAssertions;
using GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Graph
{
    [TestFixture]
    public class JunctionStylerTests
    {
        private IJunctionColorProvider _junctionColorProvider;
        private JunctionStyler _junctionStyler;

        [SetUp]
        public void Setup()
        {
            _junctionColorProvider = Substitute.For<IJunctionColorProvider>();
            _junctionColorProvider.NonRelativeColor.Returns(x => Color.Azure);

            _junctionStyler = new JunctionStyler(_junctionColorProvider);
        }

        [Test]
        public void GetLaneBrush_should_set_black_color_for_empty_junction_list()
        {
            _junctionStyler.GetLaneBrush().Should().Be(Brushes.Black);
        }

        [Test]
        public void GetNodeBrush_should_return_black_brush_if_colors_cache_not_primed()
        {
            _junctionStyler.GetNodeBrush(Rectangle.Empty, false).Should().Be(Brushes.Black);
        }

        [Test]
        public void GetNodeBrush_should_return_SolidBrush_for_single_primed_color()
        {
            // prime a single color
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var brush = _junctionStyler.GetNodeBrush(Rectangle.Empty, false);

            brush.Should().BeOfType<SolidBrush>();
        }

        [Test]
        public void GetNodeBrush_should_return_SolidBrush_with_first_primed_color_if_highlighted()
        {
            // prime a single color
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var brush = _junctionStyler.GetNodeBrush(Rectangle.Empty, true);

            brush.Should().BeOfType<SolidBrush>();
            ((SolidBrush)brush).Color.Should().Be(_junctionStyler.GetJunctionColors().First());
        }

        [Test]
        public void GetNodeBrush_should_return_SolidBrush_with_NonRelativeColor_if_not_highlighted()
        {
            // prime a single color
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var brush = _junctionStyler.GetNodeBrush(Rectangle.Empty, false);

            brush.Should().BeOfType<SolidBrush>();
            ((SolidBrush)brush).Color.Should().Be(_junctionColorProvider.NonRelativeColor);
        }

        [Test]
        public void GetNodeBrush_should_return_LinearGradientBrush_if_mutiple_colors_primed()
        {
            // prime a two color
            var junctions = new List<Junction>
            {
                new Junction(new Node(ObjectId.WorkTreeId)),
                new Junction(new Node(ObjectId.IndexId))
            };
            _junctionStyler.UpdateJunctionColors(junctions, RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var brush = _junctionStyler.GetNodeBrush(new Rectangle(0, 0, 1, 1), false);

            brush.Should().BeOfType<LinearGradientBrush>();
        }

        [Test]
        public void UpdateJunctionColors_should_reset_between_calls()
        {
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
            _junctionStyler.GetJunctionColors().Count().Should().Be(1);
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
            _junctionStyler.GetJunctionColors().Count().Should().Be(1);
        }

        [Test]
        public void UpdateJunctionColors_should_set_black_color_for_empty_junction_list()
        {
            _junctionStyler.UpdateJunctionColors(new List<Junction>(), RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var colors = _junctionStyler.GetJunctionColors().ToList();
            colors.Count.Should().Be(1);
            colors[0].Should().Be(Color.Black);

            _junctionColorProvider.DidNotReceive().GetColor(Arg.Any<Junction>(), Arg.Any<RevisionGraphDrawStyleEnum>());
        }

        // NB: it would be simpler to use TestCase construct, however Junction type if marked as internal
        [Test]
        public void UpdateJunctionColors_should_prime_one_color_for_one_junction()
        {
            var junctions = new List<Junction> { new Junction(new Node(ObjectId.WorkTreeId)) };

            _junctionStyler.UpdateJunctionColors(junctions, RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var colors = _junctionStyler.GetJunctionColors().ToList();
            colors.Count.Should().Be(1);

            _junctionColorProvider.Received(1).GetColor(junctions[0], RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
        }

        [Test]
        public void UpdateJunctionColors_should_prime_two_colors_for_two_junctions()
        {
            var junctions = new List<Junction>
            {
                new Junction(new Node(ObjectId.WorkTreeId)),
                new Junction(new Node(ObjectId.IndexId))
            };

            _junctionStyler.UpdateJunctionColors(junctions, RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var colors = _junctionStyler.GetJunctionColors().ToList();
            colors.Count.Should().Be(2);

            _junctionColorProvider.Received(1).GetColor(junctions[0], RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
            _junctionColorProvider.Received(1).GetColor(junctions[1], RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
        }

        [Test]
        public void UpdateJunctionColors_should_prime_only_two_colors_for_three_or_more_junctions()
        {
            var junctions = new List<Junction>
            {
                new Junction(new Node(ObjectId.WorkTreeId)),
                new Junction(new Node(ObjectId.IndexId)),
                new Junction(new Node(ObjectId.CombinedDiffId))
            };

            _junctionStyler.UpdateJunctionColors(junctions, RevisionGraphDrawStyleEnum.DrawNonRelativesGray);

            var colors = _junctionStyler.GetJunctionColors().ToList();
            colors.Count.Should().Be(2);

            _junctionColorProvider.Received(1).GetColor(junctions[0], RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
            _junctionColorProvider.Received(1).GetColor(junctions[1], RevisionGraphDrawStyleEnum.DrawNonRelativesGray);
            _junctionColorProvider.DidNotReceive().GetColor(junctions[2], Arg.Any<RevisionGraphDrawStyleEnum>());
        }
    }
}