using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.HelperDialogs;
using GitUI.ScriptsEngine;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormDeleteTag : GitModuleForm
{
    private readonly Func<ArgumentString, bool>? _remoteProcessRunner;

    public FormDeleteTag()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormDeleteTag(IGitUICommands commands, string? tag)
        : this(commands, tag, remoteProcessRunner: null)
    {
    }

    internal FormDeleteTag(IGitUICommands commands, string? tag, Func<ArgumentString, bool>? remoteProcessRunner)
        : base(commands, enablePositionRestore: false)
    {
        _remoteProcessRunner = remoteProcessRunner;
        InitializeComponent();
        remotesComboboxControl1.UICommandsSource = this;
        WireControls();
        AcceptButton = Ok;
        InitializeComplete();
        Tag = tag;
    }

    protected override void OnRuntimeLoad(EventArgs e)
    {
        base.OnRuntimeLoad(e);

        Tags.ItemsSource = Module.GetRefs(RefsFilter.Tags);
        Tags.Text = Tag as string;
        remotesComboboxControl1.SelectedRemote = Module.GetCurrentRemote();
        EnableOrDisableRemotesCombobox();
    }

    private void WireControls()
    {
        Tags.ItemTemplate = new FuncDataTemplate<IGitRef>(
            (tag, _) => new TextBlock { Text = tag?.LocalName ?? string.Empty },
            supportsRecycling: false);
        Ok.Click += OkClick;
        deleteTag.IsCheckedChanged += deleteTag_CheckedChanged;
    }

    private void OkClick(object? sender, EventArgs e)
    {
        try
        {
            string tagName = Tags.Text ?? string.Empty;
            Module.DeleteTag(tagName);

            if (deleteTag.IsChecked == true && !string.IsNullOrEmpty(tagName))
            {
                RemoveRemoteTag(tagName);
            }

            DialogResult = WinFormsShims.DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }

    private void RemoveRemoteTag(string tagName)
    {
        string remote = remotesComboboxControl1.SelectedRemote;
        ArgumentString pushCommand = new GitArgumentBuilder("push")
        {
            remote.Quote(),
            $":refs/tags/{tagName}",
        };

        bool success = ScriptsRunner.RunEventScripts(ScriptEvent.BeforePush, this);
        if (!success)
        {
            return;
        }

        if (_remoteProcessRunner is not null)
        {
            success = _remoteProcessRunner(pushCommand);
            if (success && !Module.InTheMiddleOfAction())
            {
                ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
            }

            return;
        }

        using FormRemoteProcess form = new(UICommands, pushCommand)
        {
            Remote = remote,
        };
        form.ShowDialog(this);

        if (!Module.InTheMiddleOfAction() && !form.ErrorOccurred())
        {
            ScriptsRunner.RunEventScripts(ScriptEvent.AfterPush, this);
        }
    }

    private void deleteTag_CheckedChanged(object? sender, EventArgs e)
    {
        EnableOrDisableRemotesCombobox();
    }

    private void EnableOrDisableRemotesCombobox()
    {
        remotesComboboxControl1.IsEnabled = deleteTag.IsChecked == true;
    }
}
