﻿using System.ComponentModel.Composition;
using GitUIPluginInterfaces;
using ResourceManager;

namespace GitImpact
{
    [Export(typeof(IGitPlugin))]
    public class GitImpactPlugin : GitPluginBase, IGitPluginForRepository
    {
        public GitImpactPlugin()
        {
            SetNameAndDescription("Impact Graph");
            Translate();
        }

        #region IGitPlugin Members

        public override bool Execute(GitUIEventArgs args)
        {
            if (string.IsNullOrEmpty(args.GitModule.WorkingDir))
            {
                return false;
            }

            using (var form = new FormImpact(args.GitModule))
            {
                form.ShowDialog(args.OwnerForm);
            }

            return false;
        }

        #endregion
    }
}
