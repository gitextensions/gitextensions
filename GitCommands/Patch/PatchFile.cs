using System.Diagnostics;
using System.IO;

namespace PatchApply
{
    [DebuggerDisplay("PatchFile( {Subject} )")]
    public class PatchFile
    {
        public string FullName { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public string Subject { get; set; }

        public string Date { get; set; }

        public bool IsNext { get; set; }

        public bool IsSkipped { get; set; }

        public string Status
        {
            get
            {
                if (IsNext)
                {
                    return "Next to apply";
                }
                if (IsSkipped)
                {
                    return "Skipped";
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
