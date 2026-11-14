using System.Reflection;
using Avalonia.Controls;
using GitCommands.UserRepositoryHistory;
using GitUI;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;

namespace GitExtensionsTests;

// parity-scaffolding: inspect retained native popup state without a shipping TestAccessor.
internal static class WorkingDirectoryTestSupport
{
    public static Accessor GetTestAccessor(this WorkingDirectoryToolStripSplitButton control) => new(control);

    internal readonly struct Accessor(WorkingDirectoryToolStripSplitButton control)
    {
        public MenuFlyout Menu => Get<MenuFlyout>("_menu");

        public TextBox Filter => Get<TextBox>("_txtFilter");

        public MenuItem FilterHost => Get<MenuItem>("_filterHost");

        public NativeToolStripDropDownLayout.Group Layout => Get<NativeToolStripDropDownLayout>("_dropDownLayout").Root;

        public void FillDropDown(IList<Repository> favourites, IList<Repository> recent)
            => Invoke("FillDropDown", [typeof(IList<Repository>), typeof(IList<Repository>)], [favourites, recent]);

        public void FillDropDown(RepositoryHistorySnapshot snapshot)
            => Invoke("FillDropDown", [typeof(RepositoryHistorySnapshot)], [snapshot]);

        public void ApplyFilterForTesting() => Invoke("ApplyFilter", [], []);

        public void ShowDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            PrepareDropDown(favourites, recent);
            Menu.ShowAt(control);
        }

        public void PrepareDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            FillDropDown(favourites, recent);
            Set("_dropDownPreparedForTest", true);
        }

        public void PrepareDropDown(RepositoryHistorySnapshot snapshot)
        {
            FillDropDown(snapshot);
            Set("_dropDownPreparedForTest", true);
        }

        public void RefreshContent(string path, IList<Repository> recent)
            => Invoke("RefreshContent", [typeof(string), typeof(IList<Repository>)], [path, recent]);

        public void SetRepositoryActions(Action<string> setWorkingDirectory, Action<string> launchRepository)
        {
            Set("_setWorkingDirectory", setWorkingDirectory);
            Set("_launchRepository", launchRepository);
        }

        public void SetOpenRepositoryAction(Action openRepository) => Set("_openRepository", openRepository);

        public void OpenRepository(string path, bool openInNewInstance)
            => Invoke("OpenRepository", [typeof(string), typeof(bool)], [path, openInNewInstance]);

        private T Get<T>(string name) => (T)typeof(WorkingDirectoryToolStripSplitButton)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;

        private void Set(string name, object value) => typeof(WorkingDirectoryToolStripSplitButton)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(control, value);

        private void Invoke(string name, Type[] types, object[] args) => typeof(WorkingDirectoryToolStripSplitButton)
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic, types)!.Invoke(control, args);
    }
}
