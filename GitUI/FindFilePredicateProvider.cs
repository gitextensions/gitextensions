using System;
using GitCommands;

namespace GitUI
{
    public interface IFindFilePredicateProvider
    {
        Func<string, bool> Get(string name, string workingDir);
    }

    public sealed class FindFilePredicateProvider : IFindFilePredicateProvider
    {
        public Func<string, bool> Get(string name, string workingDir)
        {
            var pattern = name.ToPosixPath();
            var dir = workingDir.ToPosixPath();

            if (pattern.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
            {
                pattern = pattern.Substring(dir.Length).TrimStart('/');
                return fileName => fileName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase);
            }

            // Method Contains have no override with StringComparison parameter
            return fileName => fileName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
