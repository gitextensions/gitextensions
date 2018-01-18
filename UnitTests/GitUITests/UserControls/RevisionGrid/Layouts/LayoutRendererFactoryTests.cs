using System;
using System.Collections.Generic;
using FluentAssertions;
using GitCommands;
using GitUI.UserControls.RevisionGrid.Layouts;
using JetBrains.Annotations;
using NUnit.Framework;

namespace GitUITests.UserControls.RevisionGrid.Layouts
{
    [TestFixture]
    public class LayoutRendererFactoryTests
    {
        private static readonly Dictionary<RevisionGridLayout, Type> LayoutRendererMap = new Dictionary<RevisionGridLayout, Type>
        {
            { RevisionGridLayout.Default, typeof(SmallWithGraphLayoutRenderer) },
            { RevisionGridLayout.FilledBranchesSmall, typeof(FilledBranchesSmallLayoutRenderer) },
            { RevisionGridLayout.FilledBranchesSmallWithGraph, typeof(FilledBranchesSmallWithGraphLayoutRenderer) },
            { RevisionGridLayout.Small, typeof(SmallLayoutRenderer) },
            { RevisionGridLayout.SmallWithGraph, typeof(SmallWithGraphLayoutRenderer) },
            { RevisionGridLayout.Card, typeof(CardLayoutRenderer) },
            { RevisionGridLayout.CardWithGraph, typeof(CardWithGraphLayoutRenderer) },
            { RevisionGridLayout.LargeCard, typeof(LargeCardLayoutRenderer) },
            { RevisionGridLayout.LargeCardWithGraph, typeof(LargeCardWithGraphLayoutRenderer) },
        };

        private LayoutRendererFactory _factory;


        [SetUp]
        public void Setup()
        {
            AppSettings.RevisionGraphLayout = 0;

            _factory = new LayoutRendererFactory();
        }


        [TestCaseSource(nameof(GetData))]
        public void Get_should_return_appropriate_renderer_for_layoutrenderer(RevisionGridLayout layout, Type expected)
        {
            _factory.Get(layout).Should().BeOfType(expected);
        }

        [Test]
        public void Get_should_return_default_renderer_for_unknown_layoutrenderer()
        {
            _factory.Get((RevisionGridLayout)(-1)).Should().BeOfType(LayoutRendererMap[RevisionGridLayout.Default]);
        }

        [Test]
        public void GetCurrent_should_return_user_specified_layoutrenderer()
        {
            AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmallWithGraph;

            _factory.GetCurrent().Should().BeOfType(LayoutRendererMap[RevisionGridLayout.FilledBranchesSmallWithGraph]);
        }

        [Test]
        public void GetNext_should_return_next_available_layoutrenderer()
        {
            AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.FilledBranchesSmallWithGraph;

            _factory.GetNext().Should().BeOfType(LayoutRendererMap[RevisionGridLayout.Small]);
        }

        [Test]
        public void GetNext_should_return_first_layoutrenderer_if_current_is_last()
        {
            AppSettings.RevisionGraphLayout = (int)RevisionGridLayout.LargeCardWithGraph;

            var renderer = _factory.GetNext();

            renderer.Should().BeOfType(LayoutRendererMap[(RevisionGridLayout)1]);
            renderer.Should().BeOfType(LayoutRendererMap[RevisionGridLayout.FilledBranchesSmall]);
        }


        [UsedImplicitly]
        private static IEnumerable<TestCaseData> GetData
        {
            get
            {
                var index = 0;
                foreach (var key in LayoutRendererMap.Keys)
                {
                    index++;
                    var expected = LayoutRendererMap[key];
                    yield return new TestCaseData(key, expected).SetName($"{index:#0}. {key} -> {expected.Name}");
                }
            }
        }
    }
}
