using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitCommands.Submodules
{
    public class DetailedSubmoduleInfo
    {
        public bool IsDirty { get; set; }
        public SubmoduleStatus? Status { get; set; }
        public string AddedAndRemovedText { get; set; }
    }
}
