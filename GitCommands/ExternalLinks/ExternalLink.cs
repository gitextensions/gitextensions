using System.Collections.Generic;
using GitCommands.Core;

namespace GitCommands.ExternalLinks
{
    public class ExternalLink : SimpleStructured
    {
        public string Caption { get; set; }
        public string Uri { get; set; }

        protected internal override IEnumerable<object> InlinedStructure()
        {
            yield return Caption;
            yield return Uri;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }
    }
}
