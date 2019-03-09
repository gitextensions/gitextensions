namespace GitUI.Browsing
{
    internal interface ICanGoToRef
    {
        void GoToRef(string refName, bool showNoRevisionMsg);
    }
}
