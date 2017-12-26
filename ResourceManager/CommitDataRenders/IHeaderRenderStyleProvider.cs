using System.Collections.Generic;
using System.Drawing;

namespace ResourceManager.CommitDataRenders
{
    public interface IHeaderRenderStyleProvider
    {
        Font GetFont(Graphics g);
        int GetMaxWidth();
        IEnumerable<int> GetTabStops();
    }
}