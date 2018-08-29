using System.Collections.Generic;

namespace GitUI
{
    public interface IControlPositionProvider
    {
        IEnumerable<WindowPosition> GetPositions();
    }
}
