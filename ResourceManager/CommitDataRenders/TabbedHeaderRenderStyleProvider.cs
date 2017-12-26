using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GitCommands;

namespace ResourceManager.CommitDataRenders
{
    public sealed class TabbedHeaderRenderStyleProvider : IHeaderRenderStyleProvider
    {
        public Font GetFont(Graphics g)
        {
            return AppSettings.Font;
        }

        public int GetMaxWidth()
        {
            return 16;
        }

        public IEnumerable<int> GetTabStops()
        {
            var strings = new[]
            {
                Strings.GetAuthorText(),
                Strings.GetAuthorDateText(),
                Strings.GetCommitterText(),
                Strings.GetCommitDateText(),
                Strings.GetCommitHashText(),
                Strings.GetChildrenText(),
                Strings.GetParentsText()
            };

            int tabStop = 0;
            foreach (string s in strings)
            {
                tabStop = Math.Max(tabStop, TextRenderer.MeasureText(s + "  ", AppSettings.Font).Width);
            }

            // simulate a two column layout even when there's more then one tab used
            return new[] { tabStop, tabStop + 1, tabStop + 2, tabStop + 3 };
        }
    }
}