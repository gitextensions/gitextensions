using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GitUIPluginInterfaces;
using System.Windows.Forms;

namespace SyncToRemote
{
	public class SyncToRemotePlugin : GitPluginBase
	{
		//Constants
		static class SettingNames
		{
			public const string RemoteName = "Remote name";
		}

		//Fields
		private IGitModule git;
		private string remote;

		//Properties
		public override string Description
		{
			get { return "Sync to remote"; }
		}

		//Methods
		protected override void RegisterSettings()
		{
			base.RegisterSettings();
			Settings.AddSetting(SettingNames.RemoteName, "origin");
		}

		public override bool Execute(GitUIBaseEventArgs gitUiCommands)
		{
			git = gitUiCommands.GitModule;
			remote = Settings.GetSetting(SettingNames.RemoteName);

			string[] statusResults = RunGit("status --porcelain");
			if (statusResults.Length > 0)
			{
				string message = "Sync can only be done on a clean working directory. Would you like to clean and reset your working directory now?\nALL local changes will be discarded if you choose Yes.";
				string caption = "Clean working directory?";
				if (MessageBox.Show(message, caption, MessageBoxButtons.YesNo) == DialogResult.Yes)
				{
					RunGit("reset --hard");
					RunGit("clean -x -f");
				}
				else
				{
					// Cancel
					MessageBox.Show("Sync canceled.");
					return true;
				}
			}

			string[] fetchResults = RunGit("fetch -p");

			DeletePrunedBranches(gitUiCommands, fetchResults);
			CheckoutNewBranches(gitUiCommands, fetchResults);
			ResetBranches(gitUiCommands);

			RunGit("checkout master");

			MessageBox.Show("Sync complete.");

			return true;
		}

		private void CheckoutNewBranches(GitUIBaseEventArgs gitUiCommands, IEnumerable<string> fetchResults)
		{
			string[] remoteResults = RunGit("branch -r");

			foreach (string branchResult in remoteResults)
			{
				if (branchResult.Contains(" -> ") || branchResult.Contains("HEAD") || branchResult.Contains("master") || !branchResult.Contains(remote + "/"))
				{
					continue;
				}

				string branchName = branchResult.Substring(branchResult.LastIndexOf(remote + "/") + (remote + "/").Length);

				string[] branchExists = RunGit("branch --list " + branchName);

				if (branchExists.Length == 0 || string.IsNullOrWhiteSpace(branchExists[0]))
				{
					string message = string.Format("Remote branch {0} is not checked out. Do you want to checkout {0}?", branchName);
					string caption = string.Format("Checkout {0}?", branchName);
					if (MessageBox.Show(message, caption, MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RunGit("checkout --track " + branchResult);
					}
				}
			}
		}

		private void DeletePrunedBranches(GitUIBaseEventArgs gitUiCommands, IEnumerable<string> fetchResults)
		{
			List<string> prunedBranches = new List<string>();
			foreach (string fetchResult in fetchResults)
			{
				if (!fetchResult.Contains("[deleted]")) continue;

				string prunedBranch = fetchResult.Substring(fetchResult.LastIndexOf(remote + "/") + (remote + "/").Length);

				string[] branchExists = RunGit("branch --list " + prunedBranch);

				if (branchExists.Length > 0 && !string.IsNullOrWhiteSpace(branchExists[0]))
				{
					// Unset upstream configuration
					RunGit(string.Format("branch --unset-upstream {0}", prunedBranch));

					// See if they want to delete the branch now
					string message = string.Format("Branch {0} no longer exists in {1}. Do you want to delete your local branch {0}?", prunedBranch, remote);
					string caption = string.Format("Delete {0}?", prunedBranch);
					if (MessageBox.Show(message, caption, MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						RunGit("checkout master");
						RunGit(string.Format("branch -D {0}", prunedBranch));
					}
				}
			}
		}

		private void ResetBranches(GitUIBaseEventArgs gitUiCommands)
		{
			string[] branchResults = RunGit("branch --list");

			foreach (string branchResult in branchResults)
			{
				string branch = branchResult.Trim(new char[] { '*', ' ' });

				string[] isTracked = RunGit(string.Format("config branch.{0}.merge", branch));
				if (isTracked.Length == 0 || string.IsNullOrWhiteSpace(isTracked[0]))
				{
					// this is a local-only branch
					continue;
				}

				string localCommit = RunGit(string.Format("for-each-ref --format='%(objectname)' refs/heads/{0}", branch))[0];
				string remoteCommit = RunGit(string.Format("for-each-ref --format='%(objectname)' refs/remotes/{0}/{1}", remote, branch))[0];

				if (localCommit != remoteCommit)
				{
					string message = string.Format("Local branch {0} does not match {1}/{0}.\nDo you want to reset branch {0} to match {1}/{0}? ALL local changes will be discareded if you choose to do so.", branch, remote);
					string caption = string.Format("Reset branch {0} to {1}/{0}?", branch, remote);
					DialogResult proceed = MessageBox.Show(message, caption, MessageBoxButtons.YesNo);
					if (proceed == DialogResult.Yes)
					{
						RunGit(string.Format("checkout {0}", branch));
						RunGit(string.Format("reset --hard {0}/{1}", remote, branch));
					}
				}
			}
		}

		private string[] RunGit(string arguments)
		{
			return git.RunGit(arguments).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}
	}
}
