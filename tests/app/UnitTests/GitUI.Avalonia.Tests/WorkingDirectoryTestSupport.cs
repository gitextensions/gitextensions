using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GitCommands;
using GitCommands.UserRepositoryHistory;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.Menus;
using GitUI.Compat;
using NSubstitute;
using ToolStripDropDownItem = GitUI.Compat.WinFormsControls.ToolStripDropDownItem;
using ToolStripMenuItem = GitUI.Compat.WinFormsControls.ToolStripMenuItem;

namespace GitExtensionsTests;

// parity-scaffolding: inspect the native popup and feed controlled history into its real producer.
internal static class WorkingDirectoryTestSupport
{
    private static readonly ConditionalWeakTable<WorkingDirectoryToolStripSplitButton, State> States = new();
    private static readonly List<State> Active = [];
    private static Func<string, string, IProcess>? _previousStart;
    private static bool _intercepting;
    private static readonly List<string> Directories = [];

    public static string CreateRepositoryPath(string name)
    {
        string root = Path.Join(Path.GetTempPath(), "WorkingDirectoryTests-" + Guid.NewGuid().ToString("N"));
        string path = Path.Join(root, name);
        Directory.CreateDirectory(Path.Join(path, ".git"));
        Directories.Add(root);
        return path;
    }

    public static Accessor GetTestAccessor(this WorkingDirectoryToolStripSplitButton control) => new(control);

    public static void Reset()
    {
        if (_intercepting)
        {
            GitExtensionsProcess.StartForTesting = _previousStart;
            _intercepting = false;
        }

        foreach (State state in Active)
        {
            ((IDisposable)state.Start).Dispose();
        }

        Active.Clear();
        foreach (string path in Directories)
        {
            TestDirectory.Delete(path);
        }

        Directories.Clear();
    }

    // Existing isolated input fixtures supply history data; the real Browse regression supplies all production owners.
    public static void InitializeInputFixture(WorkingDirectoryToolStripSplitButton control,
        Func<IGitUICommands> commands, IRepositoryHistoryUIService history)
    {
        State state = States.GetValue(control, item => new State(item));
        state.History = history;
        state.Initialize(commands);
    }

    internal readonly struct Accessor(WorkingDirectoryToolStripSplitButton control)
    {
        private State State => States.GetValue(control, item => new State(item));

        public MenuFlyout Menu => Get<MenuFlyout>("_menu");
        public ToolStripDropDownItem CategorisedRepositoriesMenu
        {
            get
            {
                object implementation = typeof(WorkingDirectoryToolStripSplitButton)
                    .GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
                return (ToolStripDropDownItem)implementation.GetType()
                    .GetField("_tsmiCategorisedRepos", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(implementation)!;
            }
        }

        public TextBox Filter => Get<TextBox>("_txtFilter");
        public MenuItem FilterHost => Get<MenuItem>("_filterHost");
        public NativeToolStripDropDownLayout.Group Layout => Get<NativeToolStripDropDownLayout>("_dropDownLayout").Root;

        public void FillDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            State state = State;
            state.Snapshot = state.CreateSnapshot(recent, favourites);
            state.UseInfoTags = true;
            Populate();
        }

        public void FillDropDown(RepositoryHistorySnapshot snapshot)
        {
            State.Snapshot = snapshot;
            State.UseInfoTags = false;
            Populate();
        }

        public void RefreshContent(string path, IList<Repository> recent)
        {
            int minimum = AppSettings.RecentReposComboMinWidth;
            ShorteningRecentRepoPathStrategy shortening = AppSettings.ShorteningRecentRepoPathStrategy;
            using AppSettingsTestScope settings = new();
            AppSettings.RecentReposComboMinWidth = minimum;
            AppSettings.ShorteningRecentRepoPathStrategy = shortening;
            ThreadHelper.JoinableTaskFactory.Run(() => RepositoryHistoryManager.Locals.SaveRecentHistoryAsync(recent));
            State state = State;
            state.Module.WorkingDir.Returns(path);
            state.Initialize();
            Window? window = null;
            if (TopLevel.GetTopLevel(control) is null)
            {
                window = new Window { Content = control };
                window.Show();
            }

            try
            {
                control.RefreshContent();
            }
            finally
            {
                window?.Close();
            }
        }

        public void ApplyFilterForTesting() => Dispatcher.UIThread.RunJobs();

        public void ShowDropDown(IList<Repository> favourites, IList<Repository> recent)
        {
            PrepareDropDown(favourites, recent);
            Menu.ShowAt(control);
        }

        public void PrepareDropDown(IList<Repository> favourites, IList<Repository> recent) => FillDropDown(favourites, recent);
        public void PrepareDropDown(RepositoryHistorySnapshot snapshot) => FillDropDown(snapshot);

        public void SetRepositoryActions(Action<string> current, Action<string> launched)
        {
            State.Current = current;
            if (!_intercepting)
            {
                _previousStart = GitExtensionsProcess.StartForTesting;
                _intercepting = true;
            }

            GitExtensionsProcess.StartForTesting = (_, path) =>
            {
                launched(path);
                return Substitute.For<IProcess>();
            };
        }

        public IInvalidRepositoryRemover InvalidRemover => State.InvalidRemover;

        public void OpenRepository(string path, bool openInNewInstance)
        {
            FillDropDown(new RepositoryHistorySnapshot([new RepositoryHistoryEntry(new Repository(path), path, null, false, false)], []));
            MenuItem item = Menu.Items.OfType<MenuItem>().Single(item => item.Tag is RepositoryHistoryEntry);
            NativeRepositoryMenu.Activate(openInNewInstance ? KeyModifiers.Control : KeyModifiers.None,
                () => item.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)));
        }

        private void Populate()
        {
            State state = State;
            state.Initialize();
            object implementation = typeof(WorkingDirectoryToolStripSplitButton).GetField("_implementation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
            implementation.GetType().GetMethod("FillDropDown", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(implementation, [control]);
        }

        private T Get<T>(string name) => (T)typeof(WorkingDirectoryToolStripSplitButton)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(control)!;
    }

    // Replace only external storage; the singleton manager and original history logic still run.
    internal sealed class HistoryStorageScope : IDisposable
    {
        private readonly FieldInfo _field = typeof(LocalRepositoryManager).GetField("_repositoryStorage", BindingFlags.Instance | BindingFlags.NonPublic)!;
        private readonly IRepositoryStorage _original;

        public HistoryStorageScope()
        {
            _original = (IRepositoryStorage)_field.GetValue(RepositoryHistoryManager.Locals)!;
            Storage.Load(Arg.Any<string>()).Returns(call => _original.Load(call.Arg<string>()));
            Storage.When(store => store.Save(Arg.Any<string>(), Arg.Any<IEnumerable<Repository>>()))
                .Do(call => _original.Save(call.Arg<string>(), call.Arg<IEnumerable<Repository>>()));
            _field.SetValue(RepositoryHistoryManager.Locals, Storage);
        }

        public IRepositoryStorage Storage { get; } = Substitute.For<IRepositoryStorage>();

        public void Dispose() => _field.SetValue(RepositoryHistoryManager.Locals, _original);
    }

    private sealed class State
    {
        private readonly WorkingDirectoryToolStripSplitButton _control;
        private readonly IRepositoryCurrentBranchNameCache _branches = Substitute.For<IRepositoryCurrentBranchNameCache>();
        private bool _initialized;
        private RepositoryHistorySnapshot _opening = new([], []);

        public State(WorkingDirectoryToolStripSplitButton control)
        {
            _control = control;
            IGitExecutorProvider provider = Substitute.For<IGitExecutorProvider>();
            provider.GetExecutor(Arg.Any<string>()).Returns(call =>
            {
                string path = call.Arg<string>();
                IGitExecutor executor = Substitute.For<IGitExecutor>();
                executor.WorkingDir.Returns(path);
                executor.GetGitDirectory().Returns(Path.Join(path, ".git"));
                return executor;
            });
            Producer = new RepositoryHistoryUIService(provider, _branches, InvalidRemover);
            Producer.GitModuleChanged += (_, e) => Current?.Invoke(e.GitModule.WorkingDir);
            Active.Add(this);
        }

        public IGitModule Module { get; } = Substitute.For<IGitModule>();
        public RepositoryHistoryUIService Producer { get; }
        public IInvalidRepositoryRemover InvalidRemover { get; } = Substitute.For<IInvalidRepositoryRemover>();
        public StartToolStripMenuItem Start { get; } = new();
        public IRepositoryHistoryUIService? History { get; set; }
        public RepositoryHistorySnapshot? Snapshot { get; set; }
        public bool UseInfoTags { get; set; }
        public Action<string>? Current { get; set; }

        public RepositoryHistorySnapshot CreateSnapshot(IList<Repository> recent, IList<Repository> favourites)
            => RepositoryHistoryTestHelper.CreateSnapshot(recent, favourites, _branches.GetCachedBranchName);

        public void Initialize(Func<IGitUICommands>? getCommands = null)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            IGitUICommands commands = Substitute.For<IGitUICommands>();
            commands.Module.Returns(Module);
            IRepositoryHistoryUIService history = History ?? Substitute.For<IRepositoryHistoryUIService>();
            history.When(service => service.PopulateFavouriteRepositoriesMenu(Arg.Any<ToolStripDropDownItem>())).Do(call =>
            {
                _opening = Snapshot ?? RepositoryHistoryTestHelper.GetSnapshot(history);
                ToolStripDropDownItem container = call.Arg<ToolStripDropDownItem>();
                container.Items.Clear();
                foreach (RepositoryHistoryEntry entry in _opening.Favourites)
                {
                    _branches.GetCachedBranchName(entry.Repository.Path).Returns(entry.BranchName);
                }

                RepositoryHistoryTestHelper.PopulateFavouriteFixture(Producer, container, _opening.Favourites);
                Remap(container, _opening.Favourites);
            });
            history.When(service => service.PopulateRecentRepositoriesMenu(Arg.Any<ToolStripDropDownItem>())).Do(call =>
            {
                ToolStripDropDownItem container = call.Arg<ToolStripDropDownItem>();
                foreach (RepositoryHistoryEntry entry in _opening.Recent)
                {
                    _branches.GetCachedBranchName(entry.Repository.Path).Returns(entry.BranchName);
                }

                RepositoryHistoryTestHelper.PopulateRecentFixture(Producer, container, _opening);
                Remap(container, _opening.Recent);
            });
            ToolStripMenuItem close = new()
            {
                Header = "_Close (go to Dashboard)",
                Icon = new Image { Source = GitUI.Properties.Images.DashboardFolderGit },
            };
            IGitUICommands actualCommands = getCommands?.Invoke() ?? commands;
            actualCommands.GetService(typeof(IRepositoryHistoryUIService)).Returns(Producer);
            actualCommands.GetService(typeof(IGitExecutorProvider)).Returns(Substitute.For<IGitExecutorProvider>());
            Start.Initialize(() => actualCommands);
            _control.Initialize(() => actualCommands, history, Start, close);
        }

        private void Remap(MenuItem container, IReadOnlyList<RepositoryHistoryEntry> entries)
        {
            foreach (MenuItem item in container.Items.OfType<MenuItem>())
            {
                if (item.Tag is Repository repository)
                {
                    RepositoryHistoryEntry entry = entries.First(entry => ReferenceEquals(entry.Repository, repository));
                    item.Tag = UseInfoTags
                        ? new RecentRepoInfo(repository, topRepo: false, anchored: entry.IsAnchored) { Caption = entry.Caption }
                        : entry;
                }
                else
                {
                    Remap(item, entries);
                }
            }
        }
    }
}
