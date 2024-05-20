namespace GitUI.ScriptsEngine;

internal interface IFilePromptCreator
{
    IUserInputPrompt Create();
}

internal class FilePromptCreator : IFilePromptCreator
{
    public IUserInputPrompt Create()
    {
        return new FormFilePrompt();
    }
}
