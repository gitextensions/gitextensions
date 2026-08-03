using GitExtensions.Extensibility;
using GitExtensions.ParityCapture;
using GitUI;
using GitUI.CommandsDialogs;
using GitUI.CommandsDialogs.SettingsDialog.Pages;

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

            // parity-scaffolding: Hosts the internal modeless editor-search dialog without changing GitUI visibility.
            "GitUI.FormFindInCommitFilesGitGrep" => CreateWithCommands(component.TypeName, commands),
            "GitUI.CommandsDialogs.SettingsDialog.Pages.ColorsSettingsPage" =>
                new ColorsSettingsPage(GitUICommands.EmptyServiceProvider),
            _ => CreateParameterless(component.TypeName)
        };
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
}
