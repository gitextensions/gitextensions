namespace GitUI.BranchTreePanel.Interfaces
{
    internal interface IGitRefActions
    {
        bool Checkout();

        bool CreateBranch();

        bool Merge();

        bool Rebase();

        bool Reset();
    }
}
