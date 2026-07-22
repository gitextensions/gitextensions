using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using GitCommands;
using GitCommands.Config;
using GitCommands.Git;
using GitExtensions.Extensibility.Configurations;
using GitExtensions.Extensibility.Git;
using GitExtensions.Extensibility.Translations;
using GitExtUtils;
using GitExtUtils.GitUI;
using GitUI.CommandsDialogs.SubmodulesDialog;
using GitUI.Compat;
using GitUI.HelperDialogs;
using ResourceManager;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

// Twin of GitUI/CommandsDialogs/FormSubmodules.cs. The WinForms BindingSource and
// DataGridView become one explicitly populated ListBox; selection updates the same-named
// detail controls while the original submodule command/configuration paths stay intact.
public sealed partial class FormSubmodules : GitModuleForm
{
    private readonly SplitterManager _splitterManager = new(new AppSettingsPath("FormSubmodules"));
    private readonly TranslationString _removeSelectedSubmodule = new("Are you sure you want remove the selected submodule?");
    private readonly TranslationString _removeSelectedSubmoduleCaption = new("Remove");

    private readonly CancellationTokenSequence _loadSequence = new();
    private readonly List<IGitSubmoduleInfo> _modules = [];
    private bool _splitterRegistered;

    public FormSubmodules()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormSubmodules(IGitUICommands commands)
        : base(commands, enablePositionRestore: true)
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    private void WireControls()
    {
        Submodules.ItemTemplate = new FuncDataTemplate<IGitSubmoduleInfo>(
            (submodule, _) =>
            {
                Grid row = new() { ColumnDefinitions = new ColumnDefinitions("*,100") };
                TextBlock name = new() { Text = submodule?.Name ?? string.Empty, Margin = new Avalonia.Thickness(6, 2) };
                TextBlock status = new() { Text = submodule?.Status ?? string.Empty, Margin = new Avalonia.Thickness(6, 2) };
                Grid.SetColumn(status, 1);
                row.Children.Add(name);
                row.Children.Add(status);
                return row;
            },
            supportsRecycling: false);

        AddSubmodule.Click += AddSubmoduleClick;
        Submodules.SelectionChanged += (_, _) => UpdateDetails();
        SynchronizeSubmodule.Click += SynchronizeSubmoduleClick;
        UpdateSubmodule.Click += UpdateSubmoduleClick;
        RemoveSubmodule.Click += RemoveSubmoduleClick;
        Pull.Click += Pull_Click;
        UpdateDetails();
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        if (!TryGetUICommands(out _))
        {
            return;
        }

        if (!_splitterRegistered)
        {
            _splitterManager.AddSplitter(splitContainer1, nameof(splitContainer1), defaultDistance: 222);
            _splitterManager.RestoreSplitters();
            _splitterRegistered = true;
        }

        Initialize();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_splitterRegistered)
        {
            _splitterManager.SaveSplitters();
        }

        Cursor = null;
        _loadSequence.Dispose();
        base.OnClosed(e);
    }

    private void AddSubmoduleClick(object? sender, EventArgs e)
    {
        using (FormAddSubmodule formAddSubmodule = new(UICommands))
        {
            formAddSubmodule.ShowDialog(this);
        }

        Initialize();
    }

    private void Initialize()
    {
        string? oldLocalPath = (Submodules.SelectedItem as IGitSubmoduleInfo)?.LocalPath;
        CancellationToken cancellationToken = _loadSequence.Next();
        Cursor = new Cursor(StandardCursorType.Wait);
        ApplyModules([], oldLocalPath);

        ThreadHelper.FileAndForget(async () =>
        {
            List<IGitSubmoduleInfo> modules;
            try
            {
                modules = await Task.Run(
                    () => Module.GetSubmodulesInfo()
                        .WhereNotNull()
                        .Select(submodule =>
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            return submodule;
                        })
                        .ToList(),
                    cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await this.SwitchToMainThreadAsync();
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Cursor = null;
            ApplyModules(modules, oldLocalPath);
        });
    }

    private void ApplyModules(IReadOnlyList<IGitSubmoduleInfo> modules, string? preferredLocalPath)
    {
        _modules.Clear();
        _modules.AddRange(modules);
        Submodules.ItemsSource = _modules.ToArray();
        Submodules.SelectedItem = _modules.FirstOrDefault(module => module.LocalPath == preferredLocalPath)
            ?? _modules.FirstOrDefault();
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        IGitSubmoduleInfo? submodule = Submodules.SelectedItem as IGitSubmoduleInfo;
        SubModuleName.Text = submodule?.Name ?? string.Empty;
        SubModuleRemotePath.Text = submodule?.RemotePath ?? string.Empty;
        SubModuleLocalPath.Text = submodule?.LocalPath ?? string.Empty;
        SubModuleCommit.Text = submodule?.CurrentCommitId.ToString() ?? string.Empty;
        SubModuleBranch.Text = submodule?.Branch ?? string.Empty;
        SubModuleStatus.Text = submodule?.Status ?? string.Empty;
        Pull.IsEnabled = submodule is not null;
        RemoveSubmodule.IsEnabled = submodule is not null;
    }

    private void SynchronizeSubmoduleClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.SubmoduleSync(SubModuleLocalPath.Text), Module.WorkingDir, input: null, useDialogSettings: true);
            Initialize();
        }
    }

    private void UpdateSubmoduleClick(object? sender, EventArgs e)
    {
        using (WaitCursorScope.Enter())
        {
            FormProcess.ShowDialog(this, UICommands, arguments: Commands.SubmoduleUpdate(SubModuleLocalPath.Text), Module.WorkingDir, input: null, useDialogSettings: true);
            Initialize();
        }
    }

    private void RemoveSubmoduleClick(object? sender, EventArgs e)
    {
        if (Submodules.SelectedItem is not IGitSubmoduleInfo
            || MessageBoxes.Show(
                this,
                _removeSelectedSubmodule.Text,
                _removeSelectedSubmoduleCaption.Text,
                WinFormsShims.MessageBoxButtons.YesNo,
                WinFormsShims.MessageBoxIcon.Warning) != WinFormsShims.DialogResult.Yes)
        {
            return;
        }

        using (WaitCursorScope.Enter())
        {
            string localPath = SubModuleLocalPath.Text ?? string.Empty;
            string name = SubModuleName.Text ?? string.Empty;
            Module.UnstageFile(localPath);

            ISubmodulesConfigFile submoduleConfigFile;
            try
            {
                submoduleConfigFile = Module.GetSubmodulesConfigFile();
            }
            catch (GitConfigurationException ex)
            {
                MessageBoxes.ShowGitConfigurationExceptionMessage(this, ex);
                return;
            }

            submoduleConfigFile.RemoveConfigSection("submodule \"" + name + "\"");
            if (submoduleConfigFile.ConfigSections.Count > 0)
            {
                submoduleConfigFile.Save();
                Module.StageFile(".gitmodules");
            }
            else
            {
                Module.UnstageFile(".gitmodules");
            }

            Module.RemoveConfigSection("submodule", name);
            Initialize();
        }
    }

    private void Pull_Click(object? sender, EventArgs e)
    {
        if (Submodules.SelectedItem is not IGitSubmoduleInfo)
        {
            return;
        }

        IGitModule submodule = Module.GetSubmodule(SubModuleLocalPath.Text);
        UICommands.WithGitModule(submodule).StartPullDialog(this);

        using (WaitCursorScope.Enter())
        {
            Initialize();
        }
    }

    public override void AddTranslationItems(ITranslation translation)
    {
        base.AddTranslationItems(translation);
        translation.AddTranslationItem(nameof(FormSubmodules), nameof(nameDataGridViewTextBoxColumn), "HeaderText", "Name");
        translation.AddTranslationItem(nameof(FormSubmodules), nameof(Status), "HeaderText", "Status");
    }

    public override void TranslateItems(ITranslation translation)
    {
        base.TranslateItems(translation);
        TranslateHeader(translation, nameDataGridViewTextBoxColumn, nameof(nameDataGridViewTextBoxColumn), "Name");
        TranslateHeader(translation, Status, nameof(Status), "Status");
    }

    private static void TranslateHeader(ITranslation translation, Border header, string fieldName, string defaultText)
    {
        string? text = translation.TranslateItem(nameof(FormSubmodules), fieldName, "HeaderText", () => defaultText);
        if (!string.IsNullOrEmpty(text) && header.Child is TextBlock textBlock)
        {
            textBlock.Text = text;
        }
    }

    internal TestAccessor GetTestAccessor() => new(this);

    internal readonly struct TestAccessor(FormSubmodules form)
    {
        public ListBox Submodules => form.Submodules;
        public TextBox Name => form.SubModuleName;
        public TextBox RemotePath => form.SubModuleRemotePath;
        public TextBox LocalPath => form.SubModuleLocalPath;
        public TextBox Commit => form.SubModuleCommit;
        public TextBox Branch => form.SubModuleBranch;
        public TextBox Status => form.SubModuleStatus;
        public Button Remove => form.RemoveSubmodule;
        public Button Pull => form.Pull;

        public void SetModules(IReadOnlyList<IGitSubmoduleInfo> modules, string? preferredLocalPath = null)
            => form.ApplyModules(modules, preferredLocalPath);
    }
}
