using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class GitUIEventArgs : CancelEventArgs
    {
        private readonly IFilteredGitRefsProvider _refs;

        public GitUIEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands)
            : base(cancel: false)
        {
            OwnerForm = ownerForm;
            GitUICommands = gitUICommands;
            _refs = new FilteredGitRefsProvider(GitModule);
        }

        public IGitUICommands GitUICommands { get; }

        public IWin32Window? OwnerForm { get; }

        public IGitModule GitModule => GitUICommands.GitModule;

        public IReadOnlyList<IGitRef> GetRefs(RefsFilter filter) => _refs.GetRefs(filter);
    }
}
