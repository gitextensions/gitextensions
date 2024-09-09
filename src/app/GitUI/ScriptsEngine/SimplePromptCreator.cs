#nullable enable

namespace GitUI.ScriptsEngine;

internal interface ISimplePromptCreator
{
    IUserInputPrompt Create(string? title, string? label, string? defaultValue);
}

internal class SimplePromptCreator : ISimplePromptCreator
{
    public IUserInputPrompt Create(string? title, string? label, string? defaultValue)
    {
        return new SimplePrompt(title, label, defaultValue);
    }
}
