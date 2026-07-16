using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using WinFormsShims = GitExtensions.Shims.WinForms;

namespace GitUI.CommandsDialogs;

public sealed partial class FormDeleteTag : GitModuleForm
{
    public FormDeleteTag()
    {
        InitializeComponent();
        WireControls();
        InitializeComplete();
    }

    public FormDeleteTag(IGitUICommands commands, string? tag)
        : base(commands, enablePositionRestore: false)
    {
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
        // The existing process dialog performs the real remote push until the specialized
        // remote-process window and before/after push event-script host are ported.
        ArgumentString pushCommand = new GitArgumentBuilder("push")
        {
            remotesComboboxControl1.SelectedRemote.Quote(),
            $":refs/tags/{tagName}",
        };
        UICommands.StartGitCommandProcessDialog(this, pushCommand);
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
