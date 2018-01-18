namespace GitUI.UserControls.RevisionGrid.Layouts
{
    public interface ILayoutRenderer
    {
        int Height { get; }
        bool IsCardLayout { get; }
        bool IsFilledBranchesLayout { get; }
        bool IsGraphLayout { get; }
        RevisionGridLayout Layout { get; }
    }

    public sealed class FilledBranchesSmallLayoutRenderer : ILayoutRenderer
    {
        public int Height => -1; // TODO: this is dynamically calculated
        public bool IsCardLayout => false;
        public bool IsFilledBranchesLayout => true;
        public bool IsGraphLayout => false;
        public RevisionGridLayout Layout => RevisionGridLayout.FilledBranchesSmall;
    }

    public sealed class FilledBranchesSmallWithGraphLayoutRenderer : ILayoutRenderer
    {
        public int Height => -1; // TODO: this is dynamically calculated
        public bool IsCardLayout => false;
        public bool IsFilledBranchesLayout => true;
        public bool IsGraphLayout => true;
        public RevisionGridLayout Layout => RevisionGridLayout.FilledBranchesSmallWithGraph;
    }

    public sealed class SmallLayoutRenderer : ILayoutRenderer
    {
        public int Height => 25;
        public bool IsCardLayout => false;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => false;
        public RevisionGridLayout Layout => RevisionGridLayout.Small;
    }

    public sealed class SmallWithGraphLayoutRenderer : ILayoutRenderer
    {
        public int Height => 25;
        public bool IsCardLayout => false;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => true;
        public RevisionGridLayout Layout => RevisionGridLayout.SmallWithGraph;
    }

    public sealed class CardLayoutRenderer : ILayoutRenderer
    {
        public int Height => 45;
        public bool IsCardLayout => true;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => false;
        public RevisionGridLayout Layout => RevisionGridLayout.Card;
    }

    public sealed class CardWithGraphLayoutRenderer : ILayoutRenderer
    {
        public int Height => 45;
        public bool IsCardLayout => true;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => true;
        public RevisionGridLayout Layout => RevisionGridLayout.CardWithGraph;
    }

    public sealed class LargeCardLayoutRenderer : ILayoutRenderer
    {
        public int Height { get; } = 70;
        public bool IsCardLayout => true;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => false;
        public RevisionGridLayout Layout => RevisionGridLayout.LargeCard;
    }

    public sealed class LargeCardWithGraphLayoutRenderer : ILayoutRenderer
    {
        public int Height => 70;
        public bool IsCardLayout => true;
        public bool IsFilledBranchesLayout => false;
        public bool IsGraphLayout => true;
        public RevisionGridLayout Layout => RevisionGridLayout.LargeCardWithGraph;
    }
}