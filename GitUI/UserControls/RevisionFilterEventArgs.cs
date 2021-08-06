namespace GitUI.UserControls
{
    internal class RevisionFilterEventArgs
    {
        // TODO: consider using a bit vector or an enum

        public RevisionFilterEventArgs(RevisionFilter filter)
        {
            Filter = filter;
        }

        public RevisionFilter Filter { get; }
    }
}
