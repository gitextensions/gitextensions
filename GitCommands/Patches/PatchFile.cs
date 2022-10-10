﻿using System.Diagnostics;
using GitUIPluginInterfaces;

namespace GitCommands.Patches
{
    [DebuggerDisplay("PatchFile( {" + nameof(Subject) + "} )")]
    public class PatchFile
    {
        public string? FullName { get; set; }

        public string? Action { get; set; }
        public string? Name { get; set; }
        public ObjectId? ObjectId { get; set; }

        public string? Author { get; set; }

        public string? Subject { get; set; }

        public string? Date { get; set; }

        public bool IsNext { get; set; }

        public bool IsSkipped { get; set; }
        public bool IsApplied { get; set; }

        public string Status
        {
            get
            {
                if (IsSkipped)
                {
                    return "Skipped";
                }

                if (IsApplied)
                {
                    return "Applied";
                }

                if (IsNext)
                {
                    return "Applying...";
                }

                if (!string.IsNullOrEmpty(FullName) && !File.Exists(FullName))
                {
                    return "Applied";
                }

                return "";
            }
        }
    }
}
