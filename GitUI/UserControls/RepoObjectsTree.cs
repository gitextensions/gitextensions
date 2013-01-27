using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.UserControls
{
    /// <summary>Tree-like structure for a repo's objects.</summary>
    public partial class RepoObjectsTree : UserControl
    {
        /// <summary>the <see cref="GitModule"/></summary>
        GitModule git;
        GitUICommands uiCommands;

        Lazy<TreeNode> nodeTags;
        Lazy<TreeNode> nodeBranches;
        Lazy<TreeNode> nodeStashes;

        static TaskScheduler uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public RepoObjectsTree()
        {
            InitializeComponent();

            nodeBranches = GetNodeLazy("branches");
            nodeTags = GetNodeLazy("tags");
            nodeStashes = GetNodeLazy("stashes");

            foreach (TreeNode node in treeMain.Nodes)
            {
                ApplyTreeNodeStyle(node);
            }
        }

        Lazy<TreeNode> GetNodeLazy(string node)
        {
            return new Lazy<TreeNode>(() => treeMain.Nodes.Find(node, false)[0]);
        }

        /// <summary>Sets up the objects tree for a new repo.</summary>
        public void NewRepo(GitModule git, GitUICommands uiCommands)
        {
            this.git = git;
            this.uiCommands = uiCommands;

            Reload();
            NewRepoBranches();
            NewRepoStashes();
        }

        /// <summary>Reloads the repo's objects tree.</summary>
        public void Reload()
        {
            // todo: async CancellationToken(s)
            // todo: task exception handling

            LoadBranches();
            LoadStashes();

            // update tree little by little OR when all data retrieved?

            //Task.Factory.ContinueWhenAll(
            //    new[] { taskBranches },
            //    tasks => treeMain.EndUpdate(),
            //    new CancellationToken(),
            //    TaskContinuationOptions.NotOnCanceled,
            //    uiScheduler);
        }

        /// <summary>Applies the style to the specified <see cref="TreeNode"/>.
        /// <remarks>Should be invoked from a more specific style.</remarks></summary>
        void ApplyTreeNodeStyle(TreeNode node)
        {
            node.NodeFont = Settings.Font;
            // ...
        }

        void ExpandAll_Click(object sender, EventArgs e)
        {
            treeMain.ExpandAll();
        }

        void CollapseAll_Click(object sender, EventArgs e)
        {
            treeMain.CollapseAll();
        }

        void OnNodeSelected(object sender, TreeViewEventArgs e)
        {

        }

        /// <summary>Performed on a <see cref="TreeNode"/> double-click.
        /// <remarks>Expand/Collapse still executes for any node with children.</remarks></summary>
        void OnNodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = e.Node;
            if (node.IsAncestorOf(nodeBranches.Value))
            {// branches/
                if (node.HasNoChildren())
                {// no children -> branch
                    // needs to go into Settings, but would probably like an option to:
                    // stash; checkout;
                    uiCommands.StartCheckoutBranchDialog(base.ParentForm, node.Text, false);
                }

            }
        }

        /// <summary>Watches a variable's dirty status. 
        /// Value is retrieved async, while updates are run on the UI thread.</summary>
        class ValueWatcher<T>
            where T : class
        {
            //public bool IsDirty { get; private set; }
            T _currentValue;
            readonly Action<T> _onChanged;
            readonly Func<T> _getValue;
            readonly Func<T, T, bool> _equals;

            public ValueWatcher(T initialValue, Func<T> getValue, Action<T> onChanged, Func<T, T, bool> equality)
            {
                _currentValue = initialValue;
                _onChanged = onChanged;
                _getValue = getValue;
                _equals = equality ?? Equals;
            }

            /// <summary>Passed between non-UI task and UI task.</summary>
            class DirtyResults<TValue>
                  where TValue : class
            {
                /// <summary>Indicates whether the value is dirty/needs updating.</summary>
                public bool IsDirty;
                /// <summary>The new value to update with.</summary>
                public TValue NewValue;
            }

            /// <summary>Gets the new values on a background thread, and if necessary, updates on the UI thread.</summary>
            public Task CheckUpdateAsync()
            {
                return
                    Task.Factory
                     .StartNew(
                         () =>
                         {
                             T newValue = _getValue();
                             if (_currentValue == null && newValue == null)
                             {// both still null
                                 return new DirtyResults<T> { IsDirty = false };
                             }

                             if ((_currentValue != null && newValue == null)
                                 ||
                                 (_currentValue == null && newValue != null)
                                 ||
                                 (_equals(_currentValue, newValue) == false)
                                 )
                             {// XOR null/not null OR not equal -> update
                                 return new DirtyResults<T> { IsDirty = true, NewValue = newValue };
                             }
                             return null;
                         })
                     .ContinueWith(
                         dirtyResults =>
                         {
                             var results = dirtyResults.Result;
                             if (results.IsDirty)
                             {
                                 Update(results.NewValue);
                             }
                         },
                         uiScheduler);
            }

            void Update(T newValue)
            {
                //IsDirty = true;
                _currentValue = newValue;
                _onChanged(_currentValue);
                //IsDirty = false;
            }
        }

        /// <summary>Watches a collection's dirty status. 
        /// Values are retrieved async, while updates are run on the UI thread.</summary>
        class ListWatcher<T> : ValueWatcher<ICollection<T>>
        {
            public ListWatcher(Func<ICollection<T>> getValues, Action<ICollection<T>> onChanged)
                : base(null, getValues, onChanged, (olds, news) => olds.SequenceEqual(news)) { }
        }
    }
}
