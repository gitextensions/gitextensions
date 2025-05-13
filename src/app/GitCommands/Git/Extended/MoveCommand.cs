#nullable enable

using GitCommands.Utils;
using GitExtensions.Extensibility;
using GitExtUtils;

namespace GitCommands.Git.Extended;

#pragma warning disable SA1313 // ParameterNamesMustBeginWithLowerCaseLetter
public sealed class MoveCommand(IExecutable _gitExecutable) : IExtendedCommand<MoveCommand.Arguments>
#pragma warning restore SA1313 // ParameterNamesMustBeginWithLowerCaseLetter
{
    public record Arguments(bool IsFolder, string OldName, string NewName);

    private static void CreateDestinationFolders(Arguments arguments, string workingDir)
    {
        // single file: "old" -> "new"
        // folder: "old" -> "new/" renames the folder
        //         "old" -> "new" results in "new/old"
        if (!arguments.IsFolder)
        {
            CreateDestinationFolder(Path.GetDirectoryName(arguments.NewName));
        }
        else if (arguments.NewName.EndsWith('/'))
        {
            CreateDestinationFolder(Path.GetDirectoryName(Path.GetDirectoryName(arguments.NewName)));
        }
        else
        {
            CreateDestinationFolder(arguments.NewName);
        }

        return;

        void CreateDestinationFolder(string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            string fullPath = Path.Combine(workingDir, relativePath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
    }

    public void Execute(Arguments arguments)
    {
        CreateDestinationFolders(arguments, _gitExecutable.WorkingDir);
        _gitExecutable.Execute(GetArgumentString(arguments));
    }

    public static ArgumentString GetArgumentString(Arguments arguments)
    {
        return new GitArgumentBuilder("mv")
        {
            arguments.OldName.Quote(),
            arguments.NewName.Quote()
        };
    }

    public bool Validate(Arguments arguments)
    {
        if (arguments.OldName == arguments.NewName)
        {
            return false;
        }

        // Git does not support changing only the case of folders in Windows
        return !arguments.IsFolder || string.Compare(arguments.OldName, arguments.NewName, ignoreCase: true) != 0 || !IsRunningOnNativeWindows();

        bool IsRunningOnNativeWindows() => EnvUtils.RunningOnWindows() && !PathUtil.IsWslPath(_gitExecutable.WorkingDir);
    }
}
