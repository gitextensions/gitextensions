using System;
using System.Collections.Concurrent;
using System.Linq;
using GitCommands;

namespace GitUI.UserControls.RevisionGrid.Layouts
{
    public interface ILayoutRendererFactory
    {
        /// <summary>
        /// Returns the renderer for the specified layout.
        /// </summary>
        /// <param name="layout">The chosen layout.</param>
        /// <returns>The renderer for the specified layout.</returns>
        ILayoutRenderer Get(RevisionGridLayout layout);

        /// <summary>
        /// Returns the currently configured layout renderer.
        /// </summary>
        /// <returns>The renderer for the currently configured layout.</returns>
        ILayoutRenderer GetCurrent();

        /// <summary>
        /// Cycles to the next available layout renderer.
        /// </summary>
        /// <returns>The next available renderer after the currently configured layout.</returns>
        ILayoutRenderer GetNext();
    }

    public sealed class LayoutRendererFactory : ILayoutRendererFactory
    {
        private static readonly ConcurrentDictionary<RevisionGridLayout, ILayoutRenderer> LayoutRenderers = new ConcurrentDictionary<RevisionGridLayout, ILayoutRenderer>();
        private static readonly int MaxLayoutNumber;

        static LayoutRendererFactory()
        {
            MaxLayoutNumber = Enum.GetValues(typeof(RevisionGridLayout)).Cast<int>().Max();
        }


        /// <summary>
        /// Returns the renderer for the specified layout.
        /// </summary>
        /// <param name="layout">The chosen layout.</param>
        /// <returns>The renderer for the specified layout.</returns>
        public ILayoutRenderer Get(RevisionGridLayout layout)
        {
            switch (layout)
            {
                case RevisionGridLayout.Card: return LayoutRenderers.GetOrAdd(layout, new CardLayoutRenderer());
                case RevisionGridLayout.CardWithGraph: return LayoutRenderers.GetOrAdd(layout, new CardWithGraphLayoutRenderer());
                case RevisionGridLayout.FilledBranchesSmall: return LayoutRenderers.GetOrAdd(layout, new FilledBranchesSmallLayoutRenderer());
                case RevisionGridLayout.FilledBranchesSmallWithGraph: return LayoutRenderers.GetOrAdd(layout, new FilledBranchesSmallWithGraphLayoutRenderer());
                case RevisionGridLayout.LargeCard: return LayoutRenderers.GetOrAdd(layout, new LargeCardLayoutRenderer());
                case RevisionGridLayout.LargeCardWithGraph: return LayoutRenderers.GetOrAdd(layout, new LargeCardWithGraphLayoutRenderer());
                case RevisionGridLayout.Small: return LayoutRenderers.GetOrAdd(layout, new SmallLayoutRenderer());
                //case RevisionGridLayout.SmallWithGraph: 
                default: return LayoutRenderers.GetOrAdd(layout, new SmallWithGraphLayoutRenderer());
            }
        }

        /// <summary>
        /// Returns the currently configured layout renderer.
        /// </summary>
        /// <returns>The renderer for the currently configured layout.</returns>
        public ILayoutRenderer GetCurrent()
        {
            return Get(GetCurrentLayout());
        }

        /// <summary>
        /// Cycles to the next available layout renderer.
        /// </summary>
        /// <returns>The next available renderer after the currently configured layout.</returns>
        public ILayoutRenderer GetNext()
        {
            var layout = (int)GetCurrentLayout();
            layout++;
            if (layout > MaxLayoutNumber)
            {
                layout = 1;
            }
            return Get((RevisionGridLayout)layout);
        }


        private RevisionGridLayout GetCurrentLayout()
        {
            var layout = Enum.IsDefined(typeof(RevisionGridLayout), AppSettings.RevisionGraphLayout)
                ? (RevisionGridLayout)AppSettings.RevisionGraphLayout
                : RevisionGridLayout.SmallWithGraph;
            return layout;
        }
    }
}