using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GitCommands
{
    public abstract class RevisionGraphInMemFilter
    {
        public abstract bool PassThru(GitRevision rev);
    }

    [Flags]
    public enum RefsFiltringOptions
    {
        Branches = 1,       // --branches
        Remotes = 2,        // --remotes
        Tags = 4,           // --tags
        Stashes = 8,        //
        All = 15,           // --all
        Boundary = 16,      // --boundary
        ShowGitNotes = 32,  // --not --glob=notes --not
        NoMerges = 64       // --no-merges
    }

    public class RevisionGraphUpdatedEventArgs : EventArgs
    {
		public RevisionGraphUpdatedEventArgs(GitRevision revision)
		{
			Revision = revision;
		}

        public readonly GitRevision Revision;
    }

	public class RevisionGraphBatchUpdatedEventArgs : EventArgs
	{
		public RevisionGraphBatchUpdatedEventArgs(GitRevision[] revisions)
		{
			Revisions = revisions;
		}

		public readonly GitRevision[] Revisions;
	}

    public abstract class RevisionGraph : IDisposable
    {
        public RevisionGraph(GitModule module)
        {
            _module = module;
            RefsOptions = RefsFiltringOptions.All | RefsFiltringOptions.Boundary;
        }

        ~RevisionGraph()
        {
            Dispose();
        }

        public void Dispose()
        {
            _backgroundLoader.Cancel();
        }

        private readonly AsyncLoader _backgroundLoader = new AsyncLoader();

        protected readonly GitModule _module;

        public event EventHandler<AsyncErrorEventArgs> Error
        {
            add
            {
                _backgroundLoader.LoadingError += value;
            }

            remove
            {
                _backgroundLoader.LoadingError -= value;
            }
        }

        public event EventHandler Updated;
        protected void OnUpdated(GitRevision revision)
        {
            if (Updated != null)
                Updated(this, new RevisionGraphUpdatedEventArgs(revision));
        }

		public event EventHandler BatchUpdated;
		protected void OnBatchUpdated(GitRevision[] revisions)
		{
			if (BatchUpdated != null)
				BatchUpdated(this, new RevisionGraphBatchUpdatedEventArgs(revisions));
		}

        public event EventHandler Exited;
        protected virtual void OnExited()
        {
            if (Exited != null)
                Exited(this, EventArgs.Empty);
        }

        public int RevisionCount { get; set; }

        public RefsFiltringOptions RefsOptions { get; set; }
        public string Filter { get; set; }
        public string BranchFilter { get; set; }
        public RevisionGraphInMemFilter InMemFilter { get; set; }

        protected IList<GitRef> GetRefs()
        {
            var result = _module.GetRefs(true);
            bool validWorkingDir = _module.IsValidGitWorkingDir();
            string selectedBranchName = validWorkingDir ? _module.GetSelectedBranch() : string.Empty;
            GitRef selectedRef = result.FirstOrDefault(head => head.Name == selectedBranchName);

            if (selectedRef != null)
            {
                selectedRef.Selected = true;

                var localConfigFile = _module.LocalConfigFile;

                var selectedHeadMergeSource =
                    result.FirstOrDefault(head => head.IsRemote
                                        && selectedRef.GetTrackingRemote(localConfigFile) == head.Remote
                                        && selectedRef.GetMergeWith(localConfigFile) == head.LocalName);

                if (selectedHeadMergeSource != null)
                    selectedHeadMergeSource.SelectedHeadMergeSource = true;
            }

            return result;
        }

        protected abstract void ProccessGitLog(CancellationToken taskState);

        public void Execute()
        {
             _backgroundLoader.Load(ProccessGitLog, OnExited);
        }
    }
}
