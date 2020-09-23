﻿using System.ComponentModel;

namespace GitUIPluginInterfaces
{
    // NB: The values are fed directly into git commands, casing is important!
    public enum GitRefsSortBy
    {
        [Description("Git default")]
        Default = 0,

        [Description("Author date")]
        authordate,

        [Description("Committer date")]
        committerdate,

        [Description("Creator date")]
        creatordate,

        [Description("Tagger date")]
        taggerdate,

        [Description("Alpha-numeric")]
        refname,

        [Description("Object size")]
        objectsize,

        [Description("Originating remote")]
        upstream,
    }

    public enum GitRefsSortOrder
    {
        [Description("A ↓ Z")]
        Ascending = 0,

        [Description("Z ↑ A")]
        Descending = 1,
    }
}
