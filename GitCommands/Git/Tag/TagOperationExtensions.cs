using System;

namespace GitCommands.Git.Tag
{
    public static class TagOperationExtensions
    {
        public static bool CanProvideMessage(this TagOperation operation)
        {
            switch (operation)
            {
                case TagOperation.Lightweight:
                    return false;
                case TagOperation.Annotate:
                case TagOperation.SignWithDefaultKey:
                case TagOperation.SignWithSpecificKey:
                    return true;
                default:
                    throw new NotSupportedException($"Invalid tag operation: {operation}");
            }
        }
    }
}