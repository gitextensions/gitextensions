using GitCommands.Services;

namespace GitCommandsTests;

public sealed class NUnitMessageBoxService : IMessageBoxService
{
    public Task ShowInfoMessageAsync(string? text, string? caption)
    {
        TestContext.Progress.WriteLine(
            $"[MessageBox] Caption: {caption ?? string.Empty} | Text: {text ?? string.Empty}"
        );

        return Task.CompletedTask;
    }
}
