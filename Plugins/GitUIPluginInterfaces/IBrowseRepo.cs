namespace GitUIPluginInterfaces
{
    public interface IBrowseRepo
    {
        void GoToRef(string refName, bool showNoRevisionMsg, bool toggleSelection = false);
        void SetWorkingDir(string path);
    }
}
