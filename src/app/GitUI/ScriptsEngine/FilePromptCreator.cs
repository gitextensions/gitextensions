namespace GitUI.ScriptsEngine;

internal interface IFilePromptCreator
{
    IUserInputPrompt Create();
}

internal sealed class FilePromptCreator : IFilePromptCreator
{
    public IUserInputPrompt Create()
    {
        return new FormFilePrompt();
    }
}
