using System.IO;
// Copyright (C) 2006-2008 Jim Tilander. See COPYING for and README for more details.
using EnvDTE;
using EnvDTE80;

namespace GitPlugin.Commands
{
    class ToolbarCommand<ItemCommandT> : CommandBase
        where ItemCommandT : ItemCommandBase, new()
    {
        readonly bool _runInDocumentContext;

        public ToolbarCommand(bool runInDocumentContext = false)
        {
            _runInDocumentContext = runInDocumentContext;
        }

        public override void OnCommand(DTE2 application, OutputWindowPane pane, bool runInDocumentContext)
        {
            new ItemCommandT().OnCommand(application, pane, _runInDocumentContext);
        }

        static public string ResolveFileNameWithCase(string fullpath)
        {
            string dirname = Path.GetDirectoryName(fullpath);
            string basename = Path.GetFileName(fullpath).ToLower();
            DirectoryInfo info = new DirectoryInfo(dirname);
            FileInfo[] files = info.GetFiles();

            foreach (FileInfo file in files)
            {
                if (file.Name.ToLower() == basename)
                {
                    return file.FullName;
                }
            }

            // Should never happen...
            return fullpath;
        }





        public override bool IsEnabled(DTE2 application)
        {
            return new ItemCommandT().IsEnabled(application);
        }

        public override CommandBase CreateCopyWithDocumentContext()
        {
            return new ToolbarCommand<ItemCommandT>(runInDocumentContext: true);
        }
    }

}
