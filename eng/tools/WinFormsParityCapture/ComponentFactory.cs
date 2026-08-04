using GitCommands;
using GitCommands.Git;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;
using GitUI.CommitInfo;
using GitUI.UserControls;
using GitUI.UserControls.RevisionGrid;
using GitUI.UserControls.Settings;
using GitUIPluginInterfaces;

namespace WinFormsParityCapture;

internal static class ComponentFactory
{
    public static Control Create(CaptureComponentPlan component, GitUICommands commands)
    {
        Control control = component.TypeName switch
        {
            "GitUI.CommandsDialogs.FormBrowse" => new FormBrowse(commands, new BrowseArguments()),
            "GitUI.CommandsDialogs.FormCommit" => new FormCommit(commands),
            "GitUI.CommandsDialogs.FormStash" => new FormStash(commands),
            "GitUI.CommandsDialogs.FormSettings" => new FormSettings(commands),
            "GitUI.CommitInfo.CommitInfo" => CreateCommitInfo(),
            "GitUI.CommitInfo.CommitInfoHeader" => CreateCommitInfoHeader(),
            "GitUI.UserControls.RevisionGrid.EmptyRepoControl" => new EmptyRepoControl(),

            // parity-scaffolding: Hosts the internal modeless editor-search dialog without changing GitUI visibility.
            "GitUI.FormFindInCommitFilesGitGrep" => CreateWithCommands(component.TypeName, commands),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.ColorsSettingsPage" =>
                new ColorsSettingsPage(GitUICommands.EmptyServiceProvider),
            _ => CreateParameterless(component.TypeName)
        };
        PrepareInitialSize(control);
        foreach ((string fieldName, string text) in component.TextValues)
        {
            if (FindFieldValue(control, fieldName) is not Control target)
            {
                throw new InvalidDataException($"Text seed field '{fieldName}' was not found on {component.TypeName}.");
            }

            target.Text = text;
        }

        return control;
    }

    // parity-scaffolding: Code-only controls have no Designer-owned size when hosted standalone.
    private static void PrepareInitialSize(Control control)
    {
        control.Size = control switch
        {
            WaitSpinner => new Size(48, 48),
            WatermarkComboBox or CaseSensitiveComboBox => new Size(250, 23),
            _ => control.Size
        };
    }

    // parity-scaffolding: Populates the same commit-details state used by the Avalonia capture host.
    private static CommitInfo CreateCommitInfo()
    {
        return new CommitInfo { ShowBranchesAsLinks = true };
    }

    // parity-scaffolding: Populates the standalone header with the tranche's representative revision.
    private static CommitInfoHeader CreateCommitInfoHeader()
    {
        return new CommitInfoHeader();
    }

    // parity-scaffolding: Runs control logic only after WinForms has created the capture host handle.
    public static void PrepareAfterHandle(Control control, IGitUICommands commands)
    {
        CaptureCommandsSource source = new(commands);
        switch (control)
        {
            case CommitInfo commitInfo:
                commitInfo.UICommandsSource = source;
                commitInfo.Revision = CreateRevision(commands);
                break;
            case CommitInfoHeader commitInfoHeader:
                commitInfoHeader.UICommandsSource = source;
                commitInfoHeader.ShowCommitInfo(CreateRevision(commands), [commands.Module.RevParse("HEAD~1")]);
                break;
            case BranchSelector branchSelector:
                branchSelector.UICommandsSource = source;
                branchSelector.Initialize(remote: false, containObjectIds: null);
                break;
            case InteractiveGitActionControl interactiveGitActionControl:
                interactiveGitActionControl.UICommandsSource = source;
                InvokeNonPublic(
                    interactiveGitActionControl,
                    "SetGitAction",
                    InteractiveGitActionControl.GitAction.Rebase,
                    false);
                break;
            case SettingsCheckBox settingsCheckBox:
                settingsCheckBox.Text = "Enable representative setting";
                settingsCheckBox.ToolTipText = "Representative setting information";
                break;
            case WaitSpinner waitSpinner:
                waitSpinner.IsAnimating = false;
                SetNonPublicField(waitSpinner, "_progress", 7);
                waitSpinner.Invalidate();
                break;
            case LoadingControl loadingControl:
                loadingControl.IsAnimating = false;
                WaitSpinner loadingSpinner = (WaitSpinner?)FindFieldValue(loadingControl, "_waitSpinner")
                    ?? throw new InvalidOperationException("LoadingControl did not create its WaitSpinner.");
                SetNonPublicField(loadingSpinner, "_progress", 7);
                loadingSpinner.Invalidate();
                break;
            case WatermarkComboBox watermarkComboBox:
                watermarkComboBox.Watermark = "Filter files using a regular expression...";
                break;
            case CaseSensitiveComboBox caseSensitiveComboBox:
                caseSensitiveComboBox.Items.AddRange(["Main", "main", "release/1.0"]);
                caseSensitiveComboBox.Text = "main";
                break;
        }
    }

    // parity-scaffolding: Seeds private original state without adding product-facing capture hooks.
    private static void SetNonPublicField(object target, string fieldName, object value)
    {
        System.Reflection.FieldInfo field = target.GetType().GetField(
            fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().FullName}.");
        field.SetValue(target, value);
    }

    // parity-scaffolding: Drives an original private state transition through its own implementation.
    private static void InvokeNonPublic(object target, string methodName, params object[] arguments)
    {
        System.Reflection.MethodInfo method = target.GetType().GetMethod(
            methodName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Method '{methodName}' was not found on {target.GetType().FullName}.");
        method.Invoke(target, arguments);
    }

    // parity-scaffolding: Keeps both commit-details capture surfaces on one deterministic model.
    private static GitRevision CreateRevision(IGitUICommands commands)
    {
        IGitModule module = commands.Module;
        ObjectId objectId = module.GetCurrentCheckout();
        IReadOnlyList<IGitRef> refs = module.GetRefs(RefsFilter.NoFilter);
        long unixTime = new DateTimeOffset(2026, 7, 17, 10, 30, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        return new GitRevision(objectId)
        {
            Author = "Avalonia Contributor",
            AuthorEmail = "avalonia@example.com",
            AuthorUnixTime = unixTime,
            Committer = "Git Extensions Team",
            CommitterEmail = "team@gitextensions.org",
            CommitUnixTime = unixTime,
            Subject = "Establish the Avalonia application shell",
            Body = "Establish the Avalonia application shell\n\nRepresentative content used by the visual parity screenshot harness.",
            ParentIds = [module.RevParse("HEAD~1")],
            Refs = refs.Where(gitRef => gitRef.ObjectId == objectId).ToArray(),
        };
    }

    private static object? FindFieldValue(object owner, string fieldName)
    {
        for (Type? type = owner.GetType(); type is not null; type = type.BaseType)
        {
            System.Reflection.FieldInfo? field = type.GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.DeclaredOnly);
            if (field is not null)
            {
                return field.GetValue(owner);
            }
        }

        return null;
    }

    private static Control CreateParameterless(string typeName)
    {
        Type type = Type.GetType($"{typeName}, GitUI", throwOnError: true)!;
        if (!typeof(Control).IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{typeName} is not a Windows Forms control.");
        }

        return (Control?)Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"{typeName} could not be constructed.");
    }

    private static Control CreateWithCommands(string typeName, GitUICommands commands)
    {
        Type type = Type.GetType($"{typeName}, GitUI", throwOnError: true)!;
        return (Control?)Activator.CreateInstance(
            type,
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            args: [commands],
            culture: null)
            ?? throw new InvalidOperationException($"{typeName} could not be constructed.");
    }

    // parity-scaffolding: Adapts the capture worker's commands to GitModuleControl ownership.
    private sealed class CaptureCommandsSource(IGitUICommands commands) : IGitUICommandsSource
    {
        public event EventHandler<GitUICommandsChangedEventArgs>? UICommandsChanged
        {
            add { }
            remove { }
        }

        public IGitUICommands UICommands { get; } = commands;
    }
}
