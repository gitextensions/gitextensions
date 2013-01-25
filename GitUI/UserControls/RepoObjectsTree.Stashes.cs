using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI.UserControls
{
    // "stashes"
    public partial class RepoObjectsTree
    {
        Task LoadStashes()
        {
            return Task.Factory
                       .StartNew(() => git.GetStashes())
                       .ContinueWith(stashesTask => GetStashes(stashesTask.Result));
            
            throw new NotImplementedException();

        }

        void GetStashes(IList<GitStash> stashes)
        {//
            throw new NotImplementedException();
        }
    }
}
