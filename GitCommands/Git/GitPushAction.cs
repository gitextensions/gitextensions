using System;
using System.Collections.Generic;
using System.Text;

namespace GitCommands
{
	public class GitPushAction
	{
		private string _localBranch;
		private string _remoteBranch;
		private bool _force;
		private bool _delete;

		/// <summary>
		/// Push a local branch to a remote one, optionally forcing a non-fast-forward commit.
		/// </summary>
		/// <param name="fromBranch"></param>
		/// <param name="toBranch"></param>
		/// <param name="force"></param>
		public GitPushAction(string fromBranch, string toBranch, bool force)
		{
			_localBranch = fromBranch;
			_remoteBranch = toBranch;
			_force = force;
		}

		/// <summary>
		/// Push a delete of a remote branch.
		/// </summary>
		/// <param name="deleteBranch"></param>
		public GitPushAction(string deleteBranch)
		{
			_remoteBranch = deleteBranch;
			_delete = true;
		}

		public string Format()
		{
			if (_delete)
				return ":" + _remoteBranch;

			return (_force ? "+" : "") + _localBranch + ":" + _remoteBranch;
		}
	}
}
