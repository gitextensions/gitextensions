namespace GitUI
{
    public sealed class WindowsThumbnailToolbarButtons
    {
        public WindowsThumbnailToolbarButtons(WindowsThumbnailToolbarButton commit,
            WindowsThumbnailToolbarButton pull,
            WindowsThumbnailToolbarButton push,
            WindowsThumbnailToolbarButton closeAll)
        {
            CloseAll = closeAll;
            Commit = commit;
            Pull = pull;
            Push = push;
        }

        public WindowsThumbnailToolbarButton CloseAll { get; }
        public WindowsThumbnailToolbarButton Commit { get; }
        public WindowsThumbnailToolbarButton Pull { get; }
        public WindowsThumbnailToolbarButton Push { get; }
    }
}
