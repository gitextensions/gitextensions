namespace GitUI.BranchTreePanel.Interfaces
{
    internal interface IGitRefActions
    {
        string FullPath { get; }

        bool Checkout();

        bool CreateBranch();

        bool Merge();

        bool Rebase();

        bool Reset();
    }
}
