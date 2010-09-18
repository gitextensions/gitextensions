using System;
namespace GitUI.Editor
{
    interface IFileViewer
    {
        void EnableDiffContextMenu(bool enable);
        event EventHandler<EventArgs> ExtraDiffArgumentsChanged;
        void Find();
        string GetExtraDiffArguments();
        string GetText();
        bool IgnoreWhitespaceChanges { get; set; }
        bool IsReadOnly { get; set; }
        int NumberOfVisibleLines { get; set; }
        void SaveCurrentScrollPos();
        int ScrollPos { get; set; }
        bool ShowEntireFile { get; set; }
        bool TreatAllFilesAsText { get; set; }
        void ViewCurrentChanges(string fileName, bool staged);
        void ViewFile(string fileName);
        void ViewGitItem(string fileName, string guid);
        void ViewGitItemRevision(string fileName, string guid);
        void ViewPatch(Func<string> loadPatchText);
        void ViewPatch(string text);
        void ViewText(string fileName, string text);
    }
}
