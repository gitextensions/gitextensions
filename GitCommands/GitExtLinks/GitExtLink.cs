using System;
using System.Collections.Generic;
using System.Text;
using GitCommands.Core;

namespace GitCommands.GitExtLinks
{
    public class GitExtLink: SimpleStructured
    {
        public string Caption { get; set; }
        public string URI { get; set; }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Caption;
            yield return URI;
        }

        public override int GetHashCode()
        {
            return URI.GetHashCode();
        }
    }
}
