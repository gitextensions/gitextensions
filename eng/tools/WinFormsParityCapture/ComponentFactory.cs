using GitExtensions.Extensibility;
using GitUI;
using GitUI.CommandsDialogs;

namespace WinFormsParityCapture;

internal static class ComponentFactory
{
    public static Control Create(string typeName, GitUICommands commands)
    {
        return typeName switch
        {
            "GitUI.CommandsDialogs.FormBrowse" => new FormBrowse(commands, new BrowseArguments()),
            "GitUI.CommandsDialogs.FormCommit" => new FormCommit(commands),
            "GitUI.CommandsDialogs.FormSettings" => new FormSettings(commands),
            _ => CreateParameterless(typeName)
        };
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
}
