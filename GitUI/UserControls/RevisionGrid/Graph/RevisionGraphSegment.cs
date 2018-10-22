using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid.Graph
{
    public class RevisionGraphSegment
    {
        public RevisionGraphSegment(RevisionGraphRevision parent, RevisionGraphRevision child)
        {
            Parent = parent;
            Child = child;
        }

        public int StartScore => Child.Score;

        public int EndScore => Parent.Score;

        public RevisionGraphRevision Parent { get; private set; }
        public RevisionGraphRevision Child { get; private set; }
    }
}
