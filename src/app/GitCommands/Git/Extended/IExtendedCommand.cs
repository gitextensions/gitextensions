#nullable enable

namespace GitCommands.Git.Extended;

public interface IExtendedCommand<TArguments>
{
    void Execute(TArguments arguments);
    bool Validate(TArguments arguments);
}
