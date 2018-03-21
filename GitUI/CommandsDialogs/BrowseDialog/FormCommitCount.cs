using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitCommands.Statistics;

namespace GitUI.CommandsDialogs.BrowseDialog
{
    public partial class FormCommitCount : GitModuleForm
    {
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
            Loading.Visible = true;

            CommitCount.Text = "";
            var dict = new Dictionary<string, HashSet<string>>();
            var (items, _) = CommitCounter.GroupAllCommitsByContributor(Module);
            if (cbIncludeSubmodules.Checked)
            {
                IList<string> submodules = Module.GetSubmodulesLocalPaths();
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

                CommitCount.Text += string.Format("{0,6} - {1}{2}\r\n", count, name, submodulesList);
            }

            Loading.Visible = false;
        }
    }
}