using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public class GetRefsGitCommand
    {
        private readonly IGitModule _gitModule;
        private readonly bool _tags;
        private readonly bool _branches;

        public GetRefsGitCommand(IGitModule gitModule, bool tags = true, bool branches = true)
        {
            _gitModule = gitModule;
            _tags = tags;
            _branches = branches;
        }

        public string Execute()
        {
            if (_tags && _branches)
            {
                return _gitModule.RunGitCmd("show-ref --dereference", GitModule.SystemEncoding);
            }

            if (_tags)
            {
                return _gitModule.RunGitCmd("show-ref --tags", GitModule.SystemEncoding);
            }

            if (_branches)
            {
                return _gitModule.RunGitCmd(@"for-each-ref --sort=-committerdate refs/heads/ --format=""%(objectname) %(refname)""", GitModule.SystemEncoding);
            }

            return "";
        }
    }
}