using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Statistics;
using Microsoft.VisualStudio.Threading;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormCommitCount : GitModuleForm
    {
        private CancellationTokenSource _backgroundLoaderTokenSource = new CancellationTokenSource();

        public FormCommitCount(GitUICommands commands)
            : base(commands)
        {
            InitializeComponent();
            Loading.Image = Properties.Resources.loadingpanel;
            Translate();
        }

        private void FormCommitCountLoad(object sender, EventArgs e)
        {
            FetchData();
        }

        private void cbIncludeSubmodules_CheckedChanged(object sender, EventArgs e)
        {
            FetchData();
        }

        private void FetchData()
        {
            _backgroundLoaderTokenSource.Cancel();
            _backgroundLoaderTokenSource = new CancellationTokenSource();
            var token = _backgroundLoaderTokenSource.Token;

            ThreadHelper.JoinableTaskFactory.RunAsync(
                async () =>
                {
                    Loading.Visible = true;

                    CommitCount.Text = "";

                    var includeSubmodules = cbIncludeSubmodules.Checked;

                    await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                    string text = "";

                    var dict = new Dictionary<string, HashSet<string>>();
                    var (items, _) = CommitCounter.GroupAllCommitsByContributor(Module);

                    if (includeSubmodules)
                    {
                        var submodules = Module.GetSubmodulesLocalPaths();

                        foreach (var submoduleName in submodules)
                        {
                            GitModule submodule = Module.GetSubmodule(submoduleName);
                            if (submodule.IsValidGitWorkingDir())
                            {
                                var (submoduleItems, _) = CommitCounter.GroupAllCommitsByContributor(submodule);
                                foreach (var (name, count) in submoduleItems)
                                {
                                    if (!dict.ContainsKey(name))
                                    {
                                        dict.Add(name, new HashSet<string>());
                                    }

                                    dict[name].Add(submodule.SubmoduleName);
                                    if (items.ContainsKey(name))
                                    {
                                        items[name] += count;
                                    }
                                    else
                                    {
                                        items.Add(name, count);
                                    }
                                }
                            }
                        }
                    }

                    var sortedItems = from pair in items
                        orderby pair.Value descending
                        select pair;

                    foreach (var (name, count) in sortedItems)
                    {
                        string submodulesList = "";
                        if (dict.ContainsKey(name))
                        {
                            var sub = dict[name];
                            if (sub.Count == 1)
                            {
                                foreach (var item in dict[name])
                                {
                                    submodulesList = " [" + item + "]";
                                }
                            }
                            else
                            {
                                submodulesList = " [" + sub.Count + " submodules]";
                            }
                        }

                        text += string.Format("{0,6} - {1}{2}\r\n", count, name, submodulesList);
                    }

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);

                    if (!token.IsCancellationRequested)
                    {
                        CommitCount.Text = text;
                        Loading.Visible = false;
                    }
                });
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _backgroundLoaderTokenSource.Cancel();
            _backgroundLoaderTokenSource.Dispose();
            base.OnFormClosed(e);
        }
    }
}