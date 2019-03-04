namespace GitUIPluginInterfaces
{
    public interface IBrowseRepo
    {
        void GoToRef(string refName, bool showNoRevisionMsg);
        void SetWorkingDir(string path);
    }
}
