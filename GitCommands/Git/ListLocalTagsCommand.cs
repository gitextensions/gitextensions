using GitUIPluginInterfaces;

namespace GitCommands.Git
{
    public class ListLocalTagsCommand
    {
        private const string CommandLine = @"for-each-ref --format ""%(committerdate:raw)%(taggerdate:raw) %(objectname) %(refname)"" refs/tags/";

        private readonly IGitModule _gitModule;

        public ListLocalTagsCommand(IGitModule gitModule)
        {
            _gitModule = gitModule;
        }

        public void Execute()
        {
            Output = _gitModule.RunGitCmd(CommandLine);
        }

        public bool ResultedInAnError => GitModule.IsGitErrorMessage(Output);
        public string Output { get; private set; }
    }
}