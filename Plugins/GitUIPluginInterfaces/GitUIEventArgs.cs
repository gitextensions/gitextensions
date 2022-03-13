using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace GitUIPluginInterfaces
{
    public class GitUIEventArgs : CancelEventArgs
    {
        private readonly IFilteredGitRefsProvider _getRefs;

        public GitUIEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, Lazy<IReadOnlyList<IGitRef>> getRefs)
            : base(cancel: false)
        {
            OwnerForm = ownerForm;
            GitUICommands = gitUICommands;
            _getRefs = new FilteredGitRefsProvider(getRefs);
        }

        public GitUIEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands)
            : base(cancel: false)
        {
            OwnerForm = ownerForm;
            GitUICommands = gitUICommands;
            _getRefs = new FilteredGitRefsProvider(GitModule);
        }

        public IGitUICommands GitUICommands { get; }

        public IWin32Window? OwnerForm { get; }

        public IGitModule GitModule => GitUICommands.GitModule;

        public IReadOnlyList<IGitRef> GetRefs(RefsFilter filter) => _getRefs.GetRefs(filter);
    }
}
