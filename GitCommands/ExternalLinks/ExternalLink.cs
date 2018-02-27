using System.Collections.Generic;
using GitCommands.Core;

namespace GitCommands.ExternalLinks
{
    public class ExternalLink : SimpleStructured
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
